# Mobile Messaging Delivery Strategy

## Overview

This document outlines the architecture and delivery strategy for pushing asynchronous messages from RabbitMQ to mobile client applications in a reliable, ordered, and offline-tolerant manner.

---

## Goals

- Real-time delivery to online clients
- Offline message buffering with eventual delivery
- Guaranteed **message order**
- Per-client message stream isolation
- Lightweight, mobile-friendly transport (WebSocket)

---

## Architecture Diagram

```
[RabbitMQ] 
    |
[.NET Core Backend] <---> [Redis (per-client cache)]
    |
[WebSocket Endpoint]
    |
[Mobile Client App]
```

---

## Components

### .NET Core Backend

- Subscribes to RabbitMQ events using **EasyNetQ**
- Exposes a WebSocket endpoint at `/ws?clientId={id}`
- Manages active WebSocket sessions via `WebSocketConnectionManager`
- Stores messages in Redis when the client is offline
- Flushes Redis messages in order when the client reconnects

### Redis

- Stores **per-client message queues**
- Message list key format: `wsqueue:{clientId}`
- Used only if the client is **not currently connected**
- Messages expire or are pruned by age/acknowledgment

### Mobile Client

- Connects via WebSocket using a unique `clientId`
- Receives **missed messages first** (from Redis)
- Then switches to **real-time streaming**
- May send optional heartbeats (`{"type": "ping"}`) to maintain connection state

---

## Message Flow

### Online Client

1. RabbitMQ publishes message
2. Backend receives and checks `IsClientConnected(clientId)`
3. Message is pushed via WebSocket

### Offline Client

1. RabbitMQ publishes message
2. Backend checks `!IsClientConnected(clientId)`
3. Message is appended to `wsqueue:{clientId}` in Redis

### Client Reconnects

1. Client opens WebSocket with `clientId`
2. Backend:
    - Flushes Redis queue to client (in order)
    - Starts streaming real-time messages

---

## Redis Data Model

```text
Key: wsqueue:{clientId}
Type: List

Example values:
[
  { "timestamp": 1712740801, "payload": { ... } },
  { "timestamp": 1712740802, "payload": { ... } }
]
```

- TTL and pruning rules applied as needed
- Optionally track `lastSeen` timestamp in a separate key

---

## Connection Management

- `WebSocketConnectionManager` tracks client sessions using:
  ```csharp
  ConcurrentDictionary<string, WebSocket>
  ```
- Handles `AddClient`, `RemoveClient`, and `IsClientConnected`
- Disconnections detected via `WebSocket.CloseAsync` or timeout

---

## Considerations

- Messages must include a clear `clientId` or routing key
- Redis should be configured for memory limits + TTL for stale queues
- Backend must be resilient to reconnect bursts (rate-limiting or backoff)
- Consider persisting `lastDeliveredMessageId` per client if duplicate delivery must be avoided

---

## Future Enhancements

- Add message acknowledgment per client
- Implement at-least-once or exactly-once delivery
- Persist message history in Postgres for audit trails
- Support group/broadcast messages with multi-client delivery