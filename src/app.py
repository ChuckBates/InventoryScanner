import inventory_lookup
import inventory_repository as repo
import keyboard
import string_tools
import picture_widget
import label_widget
import display_config
from guizero import App, Text, TextBox, PushButton, Box, Picture

def update_item_display(mode):
    if mode == 'look_up':
        handle_item_look_up(inventory_lookup.find(look_up_barcode.value))    
    elif mode == 'scan_in':
        handle_item_scan_in(inventory_lookup.find(scan_in_barcode.value))
    elif mode == 'scan_out':
        handle_item_scan_out(inventory_lookup.find(scan_out_barcode.value))

def handle_item_look_up(item):
    if item['status'] == 'successful':
        item_found(item['item'])
    elif item['status'] == 'partial':
        handle_partial_item_lookup(item['item'])
    else:
        item_name_text.value = 'Unknown item'

def handle_item_scan_in(item):
    if item['status'] == 'successful':
        item['item']['quantity'] = item['item']['quantity'] + 1
        repo.save(item['item'])
        item_found(item['item'])
    scan_in_barcode.value = ''

def handle_item_scan_out(item):
    if item['status'] == 'successful':
        item['item']['quantity'] = item['item']['quantity'] - 1
        repo.save(item['item'])
        item_found(item['item'])
    scan_out_barcode.value = ''

def item_found(item_info):
    if 'name' in item_info:
        item_name_text.value = item_info['name']
    if 'quantity' in item_info:
        item_quantity_text.value = item_info['quantity']
    if 'size' in item_info and 'uom' in item_info:
        item_size_text.value = str(item_info['size']) + item_info['uom']

    if 'image' in item_info:
        global found_item_image
        found_item_image = item_info['image']
        spin_up_picture(item_info['image'])
    
def handle_partial_item_lookup(item):
    key_pad_box.show()
    item_found(item)
    swap_to_text_box(item)

def save_new_item():
    new_item = {}
    new_item['_id'] = look_up_barcode.value
    new_item['name'] = item_name_text_box.value
    new_item['quantity'] = int(item_quantity_text_box.value)
    new_item['image'] = found_item_image
    extract_response = string_tools.extract_size_and_uom(item_size_text_box.value)
    if extract_response['successful'] is True:
        new_item['size'] = extract_response['size']
        new_item['uom'] = extract_response['uom']
        repo.save(new_item)

        reset_display()

def clear_info():
    item_name_text.value = ''
    item_quantity_text.value = ''
    item_size_text.value = ''

def scan_in_key_pressed(event_data):
    reset_timer_to_reset_display()
    if 'Scan barcode to begin' in event_data.widget.value:
        event_data.widget.clear()
    if event_data.key == '\r':
        clear_info()
        update_item_display('scan_in')

def scan_out_key_pressed(event_data):
    reset_timer_to_reset_display()
    if 'Scan barcode to begin' in event_data.widget.value:
        event_data.widget.clear()
    if event_data.key == '\r':
        clear_info()
        update_item_display('scan_out')

def look_up_key_pressed(event_data):
    reset_timer_to_reset_display()
    if 'Scan barcode to begin' in event_data.widget.value:
        event_data.widget.clear()
    if event_data.key == '\r':
        clear_info()
        update_item_display('look_up')

def scan_in_item():
    scan_in_barcode.show()
    scan_in_barcode.clear()
    scan_out_barcode.hide()
    look_up_barcode.hide()
    set_display_to_scan_in()

def scan_out_item():
    scan_in_barcode.hide()
    scan_out_barcode.show()
    scan_out_barcode.clear()
    look_up_barcode.hide()
    set_display_to_scan_out()

def look_up_item():
    scan_in_barcode.hide()
    scan_out_barcode.hide()
    look_up_barcode.show()
    look_up_barcode.clear()
    set_display_to_look_up()

def reset_display():
    item_info_box.hide()
    left_space_box.hide()
    right_space_box.hide()
    item_picture_box.hide()
    key_pad_box.hide()
    buttons_box.show()
    reset_image()

def get_spacer_width():
    return int(int(app.tk.winfo_width() / 4) / 8)

def get_picture_width():
    space_width = int(int(app.tk.winfo_width() / 4) / 8)
    return app.tk.winfo_width() - (space_width * 2)

def set_display_to_scan_in(): 
    set_display_to_blank()   
    scan_in_barcode.focus()
    scan_in_barcode.append('Scan barcode to begin')
    app.update()

def set_display_to_scan_out(): 
    set_display_to_blank()   
    scan_out_barcode.focus()
    scan_out_barcode.append('Scan barcode to begin')
    app.update()

def set_display_to_look_up(): 
    set_display_to_blank()   
    look_up_barcode.focus()
    look_up_barcode.append('Scan barcode to begin')
    app.update()

def set_display_to_blank():
    left_space_text.width = get_spacer_width()
    right_space_text.width = get_spacer_width()
    clear_info()
    item_info_box.show()
    swap_to_text_label()
    left_space_box.show()
    right_space_box.show()
    item_picture_box.show()
    buttons_box.hide()
    reset_timer_to_reset_display()
    reset_image()
    reset_key_pad()

def reset_timer_to_reset_display():
    buttons_box.cancel(reset_display)
    buttons_box.after(10000, reset_display)

def reset_image():
    item_picture.image = picture_widget.get_default_picture()
    item_picture.config(image = picture_widget.get_default_picture())

def spin_up_picture(image):
    global item_picture
    if 'item_picture' in globals():
        item_picture.destroy()
    item_picture = picture_widget.get_picture(item_picture_box.tk, image)

def spin_up_key_pad(event_data):
    buttons_box.cancel(reset_display)
    new_item_confirm_button.show()
    reset_key_pad()
    global key_pad
    key_pad = keyboard.get_keypad(key_pad_box.tk, event_data.widget)

def reset_key_pad():
    if 'key_pad' in globals():
        key_pad.destroy()

def swap_to_text_box(item):
    hide_texts()
    new_name = ''
    if 'name' in item:
        new_name = item['name']
    item_name_text_box.show()
    item_name_text_box.value = new_name
    
    new_quantity = ''
    if 'quantity' in item:
        new_quantity = item['quantity']
    item_quantity_text_box.show()
    item_quantity_text_box.value = new_quantity
    
    new_size = ''
    if 'size' in item:
        new_size = item['size']
    item_size_text_box.show()
    item_size_text_box.value = new_size

    app.update()

def hide_text_boxes():
    item_name_text_box.clear()
    item_name_text_box.hide()
    item_quantity_text_box.clear()
    item_quantity_text_box.hide()
    item_size_text_box.clear()
    item_size_text_box.hide()
    new_item_confirm_button.hide()

def hide_texts():
    item_name_text.clear()
    item_name_text.hide()
    item_quantity_text.clear()
    item_quantity_text.hide()
    item_size_text.clear()
    item_size_text.hide()

def swap_to_text_label():    
    hide_text_boxes()
    item_name_text.show()
    item_quantity_text.show()
    item_size_text.show()
    app.update()

app = App(title='Inventory Scanner')

buttons_box = Box(app, width='fill', height='fill', layout='auto')
scan_in_button = PushButton(buttons_box, text='SCAN IN', command=scan_in_item, height='fill', width='fill')
scan_in_button.font = display_config.text_font
scan_in_button.text_color = '#00FF21'
scan_in_button.text_size = display_config.button_text_size
scan_out_button = PushButton(buttons_box, text='SCAN OUT', command=scan_out_item, height='fill', width='fill')
scan_out_button.font = display_config.text_font
scan_out_button.text_color = '#B60000'
scan_out_button.text_size = display_config.button_text_size
look_up_button = PushButton(buttons_box, text='LOOKUP', command=look_up_item, height='fill', width='fill')
look_up_button.font = display_config.text_font
look_up_button.text_color = '#FFD800'
look_up_button.text_size = display_config.button_text_size

left_space_box = Box(app, height='fill', align='left')
left_space_text = Text(left_space_box, height='fill', width='fill', text='')

right_space_box = Box(app, height='fill', align='right')
right_space_text = Text(right_space_box, height='fill', width='fill', text='')

item_info_box = Box(app, width='fill', layout='grid')

scan_in_barcode = TextBox(item_info_box, width=22, grid=[1,2,2,1], align='left')
scan_in_barcode.text_color = display_config.text_color
scan_in_barcode.font = display_config.text_font
scan_in_barcode.text_size = display_config.text_size
scan_in_barcode.when_key_pressed = scan_in_key_pressed

scan_out_barcode = TextBox(item_info_box, width=22, grid=[1,2,2,1], align='left')
scan_out_barcode.text_color = display_config.text_color
scan_out_barcode.font = display_config.text_font
scan_out_barcode.text_size = display_config.text_size
scan_out_barcode.when_key_pressed = scan_out_key_pressed

look_up_barcode = TextBox(item_info_box, width=22, grid=[1,2,2,1], align='left')
look_up_barcode.text_color = display_config.text_color
look_up_barcode.font = display_config.text_font
look_up_barcode.text_size = display_config.text_size
look_up_barcode.when_key_pressed = look_up_key_pressed

label_widget.spin_up_item_labels(item_info_box)

item_name_text = Text(item_info_box, width='fill', color=display_config.text_color, font=display_config.text_font, size=display_config.text_size, grid=[2,3], align='left')
item_quantity_text = Text(item_info_box, width='fill', color=display_config.text_color, font=display_config.text_font, size=display_config.text_size, grid=[2,4], align='left')
item_size_text = Text(item_info_box, width='fill', color=display_config.text_color, font=display_config.text_font, size=display_config.text_size, grid=[2,5], align='left')

item_name_text_box = TextBox(item_info_box, width=45, grid=[2,3], align='left')
item_name_text_box.text_color = display_config.text_color
item_name_text_box.font = display_config.text_font
item_name_text_box.text_size = display_config.text_size
item_name_text_box.when_clicked = spin_up_key_pad

item_quantity_text_box = TextBox(item_info_box, width=10, grid=[2,4], align='left')
item_quantity_text_box.text_color = display_config.text_color
item_quantity_text_box.font = display_config.text_font
item_quantity_text_box.text_size = display_config.text_size
item_quantity_text_box.when_clicked = spin_up_key_pad

item_size_text_box = TextBox(item_info_box, width=10, grid=[2,5], align='left')
item_size_text_box.text_color = display_config.text_color
item_size_text_box.font = display_config.text_font
item_size_text_box.text_size = display_config.text_size
item_size_text_box.when_clicked = spin_up_key_pad

new_item_confirm_button = PushButton(item_info_box, text='DONE', command=save_new_item, width='fill', grid=[2,6])
new_item_confirm_button.font = display_config.text_font
new_item_confirm_button.text_color = '#00FF21'
new_item_confirm_button.text_size = display_config.text_size
new_item_confirm_button.hide()

hide_text_boxes()

item_picture_box = Box(app, width='fill', align='top')

spin_up_picture('')

key_pad_box = Box(app, width='fill', align='bottom')

reset_display()

app.bg = '#011627'
# app.set_full_screen()
app.display()