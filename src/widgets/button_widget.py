import display_config
from guizero import PushButton

def get_main_buttons(parent):    
    buttons = []
    scan_in_button = PushButton(parent, text='SCAN IN', height='fill', width='fill')
    scan_in_button.font = display_config.text_font
    scan_in_button.text_color = '#00FF21'
    scan_in_button.text_size = display_config.button_text_size
    buttons.append(scan_in_button)

    scan_out_button = PushButton(parent, text='SCAN OUT', height='fill', width='fill')
    scan_out_button.font = display_config.text_font
    scan_out_button.text_color = '#B60000'
    scan_out_button.text_size = display_config.button_text_size
    buttons.append(scan_out_button)

    look_up_button = PushButton(parent, text='LOOKUP', height='fill', width='fill')
    look_up_button.font = display_config.text_font
    look_up_button.text_color = '#FFD800'
    look_up_button.text_size = display_config.button_text_size
    buttons.append(look_up_button)

    return buttons

def spin_up_edit_button(parent, command):
    global new_item_confirm_button
    new_item_confirm_button = PushButton(parent, text='DONE', command=command, width='fill', grid=[2,6])
    new_item_confirm_button.font = display_config.text_font
    new_item_confirm_button.text_color = '#00FF21'
    new_item_confirm_button.text_size = display_config.text_size
    new_item_confirm_button.hide()

def hide_edit_button():
    new_item_confirm_button.hide()

def show_edit_button():
    new_item_confirm_button.show()