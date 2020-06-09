import inventory_lookup
from guizero import App, Text, TextBox, PushButton

def update_item_info():
    print(barcode.value)
    found_item = inventory_lookup.find(barcode.value)
    if found_item['status'] == 'successful':
        item_found(found_item['item'])
    else:
        item_name.value = 'Unknown item'
    barcode.value = ''

def item_found(item_info):
    item_name.value = item_info['name']
    item_quantity.value = item_info['quantity']
    item_size.value = str(item_info['size']) + item_info['uom']

def clear_info():
    item_name.value = ''
    item_quantity.value = ''
    item_size.value = ''

def key_pressed(event_data):
    if 'Scan barcode to begin' in event_data.widget.value:
        event_data.widget.clear()
    if event_data.key == '\r':
        clear_info()
        update_item_info()

app = App(layout='grid', title='Inventory Scanner')
item_name_label = Text(app, text='Name:', grid=[0,3], align='left')
item_name = Text(app, grid=[1,3])
item_quantity_label = Text(app, text='Quantity:', grid=[0,4], align='left')
item_quantity = Text(app, grid=[1,4])
item_size_label = Text(app, text='Size:', grid=[0,5], align='left')
item_size = Text(app, grid=[1,5])

barcode = TextBox(app, grid=[0,1], width=50)
barcode.when_key_pressed = key_pressed
barcode.append('Scan barcode to begin')
barcode.focus()
app.display()