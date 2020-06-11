import inventory_lookup
import inventory_repository as repo
from guizero import App, Text, TextBox, PushButton, Box, Picture
from tkinter import Label, PhotoImage

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
    else:
        item_name.value = 'Unknown item'
    look_up_barcode.value = ''

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
    item_name.value = item_info['name']
    item_quantity.value = item_info['quantity']
    item_size.value = str(item_info['size']) + item_info['uom']

    new_image = PhotoImage(file=item_info['image'])
    item_picture.config(width=get_picture_width())
    item_picture.image = new_image
    item_picture.config(image = new_image)
    item_picture.pack()

def clear_info():
    item_name.value = ''
    item_quantity.value = ''
    item_size.value = ''
    reset_image()

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
    scan_out_barcode.hide()
    look_up_barcode.hide()
    set_display_to_scan_in()

def scan_out_item():
    scan_in_barcode.hide()
    scan_out_barcode.show()
    look_up_barcode.hide()
    set_display_to_scan_out()

def look_up_item():
    scan_in_barcode.hide()
    scan_out_barcode.hide()
    look_up_barcode.show()
    set_display_to_look_up()

def reset_display():
    item_info_box.hide()
    left_space_box.hide()
    right_space_box.hide()
    item_picture_box.hide()
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
    left_space_box.show()
    right_space_box.show()
    item_picture_box.show()
    buttons_box.hide()
    reset_timer_to_reset_display()
    reset_image()

def reset_timer_to_reset_display():
    buttons_box.cancel(reset_display)
    buttons_box.after(10000, reset_display)

def reset_image():
    item_picture.image = picture
    item_picture.config(image = picture)

text_color = 'white'
alt_text_color = '#ADDB67'
text_font = 'Consolas'
text_size = 20
button_text_size = 40
app = App(title='Inventory Scanner')

buttons_box = Box(app, width='fill', height='fill', layout='auto')
scan_in_button = PushButton(buttons_box, text='SCAN IN', command=scan_in_item, height='fill', width='fill')
scan_in_button.font = text_font
scan_in_button.text_color = '#00FF21'
scan_in_button.text_size = button_text_size
scan_out_button = PushButton(buttons_box, text='SCAN OUT', command=scan_out_item, height='fill', width='fill')
scan_out_button.font = text_font
scan_out_button.text_color = '#B60000'
scan_out_button.text_size = button_text_size
look_up_button = PushButton(buttons_box, text='LOOKUP', command=look_up_item, height='fill', width='fill')
look_up_button.font = text_font
look_up_button.text_color = '#FFD800'
look_up_button.text_size = button_text_size

left_space_box = Box(app, height='fill', align='left')
left_space_text = Text(left_space_box, height='fill', width='fill', text='')

right_space_box = Box(app, height='fill', align='right')
right_space_text = Text(right_space_box, height='fill', width='fill', text='')

item_info_box = Box(app, width='fill', layout='grid')

scan_in_barcode = TextBox(item_info_box, width=22, grid=[1,2,2,1], align='left')
scan_in_barcode.text_color = text_color
scan_in_barcode.font = text_font
scan_in_barcode.text_size = text_size
scan_in_barcode.when_key_pressed = scan_in_key_pressed

scan_out_barcode = TextBox(item_info_box, width=22, grid=[1,2,2,1], align='left')
scan_out_barcode.text_color = text_color
scan_out_barcode.font = text_font
scan_out_barcode.text_size = text_size
scan_out_barcode.when_key_pressed = scan_out_key_pressed

look_up_barcode = TextBox(item_info_box, width=22, grid=[1,2,2,1], align='left')
look_up_barcode.text_color = text_color
look_up_barcode.font = text_font
look_up_barcode.text_size = text_size
look_up_barcode.when_key_pressed = look_up_key_pressed

item_name_label = Text(item_info_box, text='Name:', color=alt_text_color, font=text_font, size=text_size, grid=[1,3], align='left')
item_quantity_label = Text(item_info_box, text='Quantity:', color=alt_text_color, font=text_font, size=text_size, grid=[1,4], align='left')
item_size_label = Text(item_info_box, text='Size:', color=alt_text_color, font=text_font, size=text_size, grid=[1,5], align='left')

item_name = Text(item_info_box, width='fill', color=text_color, font=text_font, size=text_size, grid=[2,3], align='left')
item_quantity = Text(item_info_box, width='fill', color=text_color, font=text_font, size=text_size, grid=[2,4], align='left')
item_size = Text(item_info_box, width='fill', color=text_color, font=text_font, size=text_size, grid=[2,5], align='left')

key_pad_box = Box(app, width='fill')
key_pad = keyboard.get_keypad(key_pad_box.tk)

item_picture_box = Box(app, height='fill', align='left')

picture = PhotoImage(file='')
item_picture = Label(item_picture_box.tk)
item_picture.image = picture
item_picture.config(image = picture)

reset_display()

app.bg = '#011627'
# app.set_full_screen()
app.display()