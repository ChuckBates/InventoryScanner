import pymongo

mycol = ''
def load():
    myclient = pymongo.MongoClient('mongodb://localhost:27017/')
    mydb = myclient['inventory_database']
    global mycol
    mycol = mydb['inventory']

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
    
    query = {'_id': inventory_item['_id']}
    mydict = { 
        '$set': {
            'name': inventory_item['name'],
            'quantity': inventory_item['quantity'],
            'size': inventory_item['size'],
            'uom': inventory_item['uom'],
            'image': inventory_item['image']
        }
    }
    mycol.update_one(query, mydict, upsert=True)

def find(_id):
    document_count = mycol.count_documents({'_id': _id})
    if document_count == 0:
        return None
    documents = mycol.find({'_id': _id})
    return documents[0]

def delete(_id):
    mycol.delete_one({"_id": _id})