import inventory_lookup
from guizero import App, Text, TextBox, PushButton, Box

def update_item_display():
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
    reset_timer_to_reset_display()
    if 'Scan barcode to begin' in event_data.widget.value:
        event_data.widget.clear()
    if event_data.key == '\r':
        clear_info()
        update_item_display()

def scan_in_item():
    set_display_to_scan()

def scan_out_item():
    set_display_to_scan()

def look_up_item():
    set_display_to_scan()

def reset_display():
    item_info_box.hide()
    buttons_box.show()

def set_display_to_scan():    
    item_info_box.show()
    buttons_box.hide()
    barcode.focus()
    reset_timer_to_reset_display()

def reset_timer_to_reset_display():
    buttons_box.cancel(reset_display)
    buttons_box.after(10000, reset_display)

text_color = 'white'
text_font = 'Consolas'
text_size = 10
app = App(title='Inventory Scanner')

buttons_box = Box(app, width='fill', height='fill', layout='auto')
scan_in_button = PushButton(buttons_box, text='SCAN IN', command=scan_in_item, height='fill', width='fill')
scan_in_button.font = text_font
scan_in_button.text_color = text_color
scan_in_button.text_size = text_size
scan_out_button = PushButton(buttons_box, text='SCAN OUT', command=scan_out_item, height='fill', width='fill')
scan_out_button.font = text_font
scan_out_button.text_color = text_color
scan_out_button.text_size = text_size
look_up_button = PushButton(buttons_box, text='LOOKUP', command=look_up_item, height='fill', width='fill')
look_up_button.font = text_font
look_up_button.text_color = text_color
look_up_button.text_size = text_size

item_info_box = Box(app, width='fill', height='fill', layout='auto')
barcode = TextBox(item_info_box, width=23)
barcode.text_color = text_color
barcode.font = text_font
barcode.when_key_pressed = key_pressed
barcode.append('Scan barcode to begin')

item_name_label = Text(item_info_box, text='Name:', color=text_color, font=text_font, size=text_size)
item_name = Text(item_info_box, width='fill', color=text_color, font=text_font, size=text_size)
item_quantity_label = Text(item_info_box, text='Quantity:', color=text_color, font=text_font, size=text_size)
item_quantity = Text(item_info_box, width='fill', color=text_color, font=text_font, size=text_size)
item_size_label = Text(item_info_box, text='Size:', color=text_color, font=text_font, size=text_size)
item_size = Text(item_info_box, width='fill', color=text_color, font=text_font, size=text_size)

reset_display()

app.bg = '#011627'
# app.set_full_screen()
app.display()