import inventory_lookup
from guizero import App, Text, TextBox, PushButton, Box, Picture
from tkinter import Label, PhotoImage

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

    new_image = PhotoImage(file=item_info['image'])
    item_picture.image = new_image
    item_picture.config(image = new_image)
    item_picture.pack()

def clear_info():
    item_name.value = ''
    item_quantity.value = ''
    item_size.value = ''
    reset_image()

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
    left_space_box.hide()
    item_picture_box.hide()
    buttons_box.show()
    reset_image()

def set_display_to_scan():    
    left_space_text.width = int(int(app.tk.winfo_width() / 4) / 8)
    clear_info()
    item_info_box.show()
    left_space_box.show()
    item_picture_box.show()
    buttons_box.hide()
    barcode.focus()
    reset_timer_to_reset_display()
    reset_image()
    app.update()

def reset_timer_to_reset_display():
    buttons_box.cancel(reset_display)
    buttons_box.after(10000, reset_display)

def reset_image():
    item_picture.image = picture
    item_picture.config(image = picture)
    
    app.update()

text_color = 'white'
text_font = 'Consolas'
text_size = 20
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

left_space_box = Box(app, height='fill', align='left')
left_space_text = Text(left_space_box, height='fill', width='fill', text='')

item_info_box = Box(app, width='fill', height='fill', layout='grid')
barcode = TextBox(item_info_box, width=23, grid=[2,2])
barcode.text_color = text_color
barcode.font = text_font
barcode.text_size = text_size
barcode.when_key_pressed = key_pressed
barcode.append('Scan barcode to begin')

item_name_label = Text(item_info_box, text='Name:', color=text_color, font=text_font, size=text_size, grid=[1,3])
item_name = Text(item_info_box, width='fill', color=text_color, font=text_font, size=text_size, grid=[2,3])
item_quantity_label = Text(item_info_box, text='Quantity:', color=text_color, font=text_font, size=text_size, grid=[1,4])
item_quantity = Text(item_info_box, width='fill', color=text_color, font=text_font, size=text_size, grid=[2,4])
item_size_label = Text(item_info_box, text='Size:', color=text_color, font=text_font, size=text_size, grid=[1,5])
item_size = Text(item_info_box, width='fill', color=text_color, font=text_font, size=text_size, grid=[2,5])

item_picture_box = Box(app)

picture = PhotoImage(file='')
item_picture = Label(item_picture_box.tk)
item_picture.image = picture
item_picture.config(image = picture)

reset_display()

app.bg = '#011627'
app.set_full_screen()
app.display()