import display_config
from guizero import PushButton, Box

def spin_up_main_buttons(parent, commands):    
    global buttons_box
    buttons_box = Box(parent, width='fill', height='fill', layout='auto')

    scan_in_button = PushButton(buttons_box, text='SCAN IN', height='fill', width='fill')
    scan_in_button.font = display_config.text_font
    scan_in_button.text_color = '#00FF21'
    scan_in_button.text_size = display_config.button_text_size
    scan_in_button.update_command(commands[0])

    scan_out_button = PushButton(buttons_box, text='SCAN OUT', height='fill', width='fill')
    scan_out_button.font = display_config.text_font
    scan_out_button.text_color = '#B60000'
    scan_out_button.text_size = display_config.button_text_size
    scan_out_button.update_command(commands[1])

    look_up_button = PushButton(buttons_box, text='LOOKUP', height='fill', width='fill')
    look_up_button.font = display_config.text_font
    look_up_button.text_color = '#FFD800'
    look_up_button.text_size = display_config.button_text_size
    look_up_button.update_command(commands[2])

def spin_up_edit_button(parent, command):
    global new_item_confirm_button
    new_item_confirm_button = PushButton(parent, text='DONE', command=command, width='fill', grid=[2,6])
    new_item_confirm_button.font = display_config.text_font
    new_item_confirm_button.text_color = '#00FF21'
    new_item_confirm_button.text_size = display_config.text_size
    new_item_confirm_button.hide()

def hide_main_buttons():
    buttons_box.hide()

def show_main_buttons():
    buttons_box.show()

def hide_edit_button():
    new_item_confirm_button.hide()

def show_edit_button():
    new_item_confirm_button.show()