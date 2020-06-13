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