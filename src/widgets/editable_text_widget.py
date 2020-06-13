import display_config
from guizero import Text, TextBox

def spin_up_editable_texts(parent, command):
    texts = []
    global item_name_text
    item_name_text = Text(parent, width='fill', color=display_config.text_color, font=display_config.text_font, size=display_config.text_size, grid=[2,3], align='left')
    global item_quantity_text
    item_quantity_text = Text(parent, width='fill', color=display_config.text_color, font=display_config.text_font, size=display_config.text_size, grid=[2,4], align='left')
    global item_size_text
    item_size_text = Text(parent, width='fill', color=display_config.text_color, font=display_config.text_font, size=display_config.text_size, grid=[2,5], align='left')

    global item_name_text_box
    item_name_text_box = TextBox(parent, width=45, grid=[2,3], align='left')
    item_name_text_box.text_color = display_config.text_color
    item_name_text_box.font = display_config.text_font
    item_name_text_box.text_size = display_config.text_size
    item_name_text_box.when_clicked = command

    global item_quantity_text_box
    item_quantity_text_box = TextBox(parent, width=10, grid=[2,4], align='left')
    item_quantity_text_box.text_color = display_config.text_color
    item_quantity_text_box.font = display_config.text_font
    item_quantity_text_box.text_size = display_config.text_size
    item_quantity_text_box.when_clicked = command

    global item_size_text_box
    item_size_text_box = TextBox(parent, width=10, grid=[2,5], align='left')
    item_size_text_box.text_color = display_config.text_color
    item_size_text_box.font = display_config.text_font
    item_size_text_box.text_size = display_config.text_size
    item_size_text_box.when_clicked = command

def get_values():
    result = {}
    result['item_name_text_box'] = item_name_text_box.value,
    result['item_quantity_text_box'] = item_quantity_text_box.value,
    result['item_size_text_box'] = item_size_text_box.value
    return result
    
def hide_text_boxes():
    item_name_text_box.clear()
    item_name_text_box.hide()
    item_quantity_text_box.clear()
    item_quantity_text_box.hide()
    item_size_text_box.clear()
    item_size_text_box.hide()
    # new_item_confirm_button.hide()
    
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
    # app.update()

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

    # app.update()

def clear_text():
    item_name_text.value = ''
    item_quantity_text.value = ''
    item_size_text.value = ''
    
def populate_text(item_info):
    if 'name' in item_info:
        item_name_text.value = item_info['name']
    if 'quantity' in item_info:
        item_quantity_text.value = item_info['quantity']
    if 'size' in item_info and 'uom' in item_info:
        item_size_text.value = str(item_info['size']) + item_info['uom']