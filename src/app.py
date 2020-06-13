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
import editable_text_widget
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
    editable_text_widget.populate_text(item_info)

    if 'image' in item_info:
        global found_item_image
        found_item_image = item_info['image']
        picture_widget.set_picture(item_info['image'])
    
def handle_partial_item_lookup(item):
    keyboard_widget.show_key_pad()
    item_found(item)
    editable_text_widget.swap_to_text_box(item)

def save_new_item():
    values = editable_text_widget.get_values()
    new_item = {}
    new_item['_id'] = barcode_widget.look_up_value()
    new_item['name'] = values['item_name_text_box']
    new_item['quantity'] = int(values['item_quantity_text_box'])
    new_item['image'] = found_item_image
    extract_response = string_tools.extract_size_and_uom(values['item_size_text_box'])
    if extract_response['successful'] is True:
        new_item['size'] = extract_response['size']
        new_item['uom'] = extract_response['uom']
        repo.save(new_item)

        reset_display()

def scan_in_key_pressed(event_data):
    reset_timer_to_reset_display()
    if 'Scan barcode to begin' in event_data.widget.value:
        event_data.widget.clear()
    if event_data.key == '\r':
        editable_text_widget.clear_text()
        update_item_display('scan_in')

def scan_out_key_pressed(event_data):
    reset_timer_to_reset_display()
    if 'Scan barcode to begin' in event_data.widget.value:
        event_data.widget.clear()
    if event_data.key == '\r':
        editable_text_widget.clear_text()
        update_item_display('scan_out')

def look_up_key_pressed(event_data):
    reset_timer_to_reset_display()
    if 'Scan barcode to begin' in event_data.widget.value:
        event_data.widget.clear()
    if event_data.key == '\r':
        editable_text_widget.clear_text()
        update_item_display('look_up')

def reset_display():
    item_info_box.hide()
    spacer_widget.hide_spacers()
    picture_widget.hide_picture()
    keyboard_widget.hide_key_pad()
    button_widget.show_main_buttons()
    picture_widget.reset_picture()

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
    editable_text_widget.clear_text()
    item_info_box.show()
    editable_text_widget.swap_to_text_label()
    button_widget.hide_edit_button()
    spacer_widget.show_spacers()
    picture_widget.show_picture()
    button_widget.hide_main_buttons()
    reset_timer_to_reset_display()
    picture_widget.reset_picture()
    keyboard_widget.reset_key_pad()

def reset_timer_to_reset_display():
    app.cancel(reset_display)
    app.after(10000, reset_display)

def spin_up_key_pad(event_data):
    app.cancel(reset_display)
    button_widget.show_edit_button()
    keyboard_widget.spin_up_keypad(event_data.widget)

def spin_up_barcodes(parent):
    barcodes = barcode_widget.get_barcodes(parent)
    commands = [scan_in_key_pressed, scan_out_key_pressed, look_up_key_pressed]
    iteration = 0
    for barcode in barcodes:
        barcode.when_key_pressed = commands[iteration]
        iteration+=1

app = App(title='Inventory Scanner')

main_buttons_commands = [set_display_to_scan_in, set_display_to_scan_out, set_display_to_look_up]
buttons = button_widget.spin_up_main_buttons(app, main_buttons_commands)

spacer_widget.spin_up_spacers(app)

item_info_box = Box(app, width='fill', layout='grid')
spin_up_barcodes(item_info_box)

label_widget.spin_up_item_labels(item_info_box)

editable_text_widget.spin_up_editable_texts(item_info_box, spin_up_key_pad)
editable_text_widget.hide_text_boxes()

button_widget.spin_up_edit_button(item_info_box, save_new_item)
button_widget.hide_edit_button()

picture_widget.spin_up_picture(app)
keyboard_widget.spin_up_key_pad_box(app)
reset_display()

app.bg = '#011627'
# app.set_full_screen()
app.display()