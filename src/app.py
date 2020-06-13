import sys
sys.path.append('widgets')
sys.path.append('tools')

import inventory_lookup
import inventory_repository as repo
import keyboard_widget
import string_tools
import picture_widget
import label_widget
import button_widget
import spacer_widget
import barcode_widget
import display_config
from guizero import App, Text, TextBox, PushButton, Box, Picture

def update_item_display(mode):
    if mode == 'look_up':
        handle_item_look_up(inventory_lookup.find(barcode_widget.look_up_value()))    
    elif mode == 'scan_in':
        handle_item_scan_in(inventory_lookup.find(barcode_widget.scan_in_value()))
    elif mode == 'scan_out':
        handle_item_scan_out(inventory_lookup.find(barcode_widget.scan_out_value()))

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
    barcode_widget.reset_values()

def handle_item_scan_out(item):
    if item['status'] == 'successful':
        item['item']['quantity'] = item['item']['quantity'] - 1
        repo.save(item['item'])
        item_found(item['item'])
    barcode_widget.reset_values()

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
    new_item['_id'] = barcode_widget.look_up_value()
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

def reset_display():
    item_info_box.hide()
    spacer_widget.hide_spacers()
    item_picture_box.hide()
    key_pad_box.hide()
    buttons_box.show()
    reset_image()

def get_picture_width():
    space_width = int(int(app.tk.winfo_width() / 4) / 8)
    return app.tk.winfo_width() - (space_width * 2)

def set_display_to_scan_in(): 
    barcode_widget.set_to_scan_in()
    set_display_to_blank()   
    app.update()

def set_display_to_scan_out(): 
    barcode_widget.set_to_scan_out()
    set_display_to_blank()   
    app.update()

def set_display_to_look_up(): 
    barcode_widget.set_to_look_up()
    set_display_to_blank()   
    app.update()

def set_display_to_blank():
    spacer_widget.update_spacer_width(app)
    clear_info()
    item_info_box.show()
    swap_to_text_label()
    spacer_widget.show_spacers()
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
    key_pad = keyboard_widget.get_keypad(key_pad_box.tk, event_data.widget)

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

def spin_up_barcodes(parent):
    barcodes = barcode_widget.get_barcodes(parent)
    commands = [scan_in_key_pressed, scan_out_key_pressed, look_up_key_pressed]
    iteration = 0
    for barcode in barcodes:
        barcode.when_key_pressed = commands[iteration]
        iteration+=1

def spin_up_main_buttons(parent):
    buttons = button_widget.get_main_buttons(parent)
    commands = [set_display_to_scan_in, set_display_to_scan_out, set_display_to_look_up]
    iteration = 0
    for button in buttons:
        button.update_command(commands[iteration])
        iteration+=1

app = App(title='Inventory Scanner')

buttons_box = Box(app, width='fill', height='fill', layout='auto')
spin_up_main_buttons(buttons_box)

spacer_widget.spin_up_spacers(app)

item_info_box = Box(app, width='fill', layout='grid')
spin_up_barcodes(item_info_box)

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