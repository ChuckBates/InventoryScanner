#!/bin/bash

DISPLAY=:0 xrandr --output DSI-1 --rotate right
DISPLAY=:0 xinput set-prop 'FT5406 memory based driver' 'Coordinate Transformation Matrix' 0 1 0 -1 0 1 0 0 1

