import psycopg2
import json

def load():
    global config
    config = json.load(open('../../config.json'))
    global pg_connection
    pg_connection = psycopg2.connect(host=config['postgres_host'], database=config['postgres_database'], user=config['postgres_username'], password=config['postgres_password'])
    global cursor
    cursor = pg_connection.cursor()

def unload():
    cursor.close()

def save(inventory_item):    
    if '_id' not in inventory_item:
        raise ValueError('Missing required field: _id')
    if 'name' not in inventory_item:
        raise ValueError('Missing required field: name')
    if 'quantity' not in inventory_item:
        raise ValueError('Missing required field: quantity')
    if 'size' not in inventory_item:
        raise ValueError('Missing required field: size')
    if 'uom' not in inventory_item:
        raise ValueError('Missing required field: uom')

    if find(inventory_item['_id']) is not None:
        update(inventory_item)
    else:
        insert(inventory_item)

def update(inventory_item):
    sql = f"UPDATE inventory SET name = %s, quantity = %s, size = %s, uom = %s, image = %s WHERE barcode = %s"
    cursor.execute(sql, (inventory_item['name'], inventory_item['quantity'], inventory_item['size'], inventory_item['uom'], inventory_item['image'], inventory_item['_id']))
    pg_connection.commit()

def insert(inventory_item):
    sql = "INSERT INTO inventory (barcode, name, quantity, size, uom, image) VALUES ('"+inventory_item["_id"]+"', '"+inventory_item["name"]+"', '"+str(inventory_item["quantity"])+"', '"+str(inventory_item["size"])+"', '"+inventory_item["uom"]+"', '"+inventory_item["image"]+"')"
    cursor.execute(sql)
    pg_connection.commit()

def find(_id):
    sql = "SELECT * FROM inventory WHERE barcode = %s"
    cursor.execute(sql, (_id,))
    pg_connection.commit()

    result = cursor.fetchone()
    if result is None:
        return None
    else:
        return {
            '_id': result[0],
            'name': result[1],
            'quantity': result[2],
            'size': result[3],
            'uom': result[4],
            'image': result[5]
        }

def delete(_id):
    sql = f"DELETE FROM inventory WHERE barcode = %s"
    cursor.execute(sql, (_id,))
    pg_connection.commit()