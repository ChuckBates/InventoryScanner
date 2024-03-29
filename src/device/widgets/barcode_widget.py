import display_config
from guizero import TextBox

def get_barcodes(parent, commands):
    global scan_in_barcode
    scan_in_barcode = TextBox(parent, width=22, grid=[1,2,2,1], align='left')
    scan_in_barcode.text_color = display_config.text_color
    scan_in_barcode.font = display_config.text_font
    scan_in_barcode.text_size = display_config.text_size
    scan_in_barcode.when_key_pressed = commands[0]

    global scan_out_barcode
    scan_out_barcode = TextBox(parent, width=22, grid=[1,2,2,1], align='left')
    scan_out_barcode.text_color = display_config.text_color
    scan_out_barcode.font = display_config.text_font
    scan_out_barcode.text_size = display_config.text_size
    scan_out_barcode.when_key_pressed = commands[1]

    global look_up_barcode
    look_up_barcode = TextBox(parent, width=22, grid=[1,2,2,1], align='left')
    look_up_barcode.text_color = display_config.text_color
    look_up_barcode.font = display_config.text_font
    look_up_barcode.text_size = display_config.text_size
    look_up_barcode.when_key_pressed = commands[2]

def disable():
    scan_in_barcode.disable()
    scan_out_barcode.disable()
    look_up_barcode.disable()

def enable():
    scan_in_barcode.enable()
    scan_out_barcode.enable()
    look_up_barcode.enable()

def scan_in_value():
    return scan_in_barcode.value

def scan_out_value():
    return scan_out_barcode.value

def look_up_value():
    return look_up_barcode.value

def get_value():
    if len(look_up_barcode.value) > 0:
        return look_up_barcode.value
    elif len(scan_in_barcode.value) > 0:
        return scan_in_barcode.value
    else:
        return scan_out_barcode.value

def reset_values():
    scan_in_barcode.value = ''
    scan_out_barcode.value = ''
    look_up_barcode.value = ''

def set_to_scan_in():    
    scan_out_barcode.hide()
    look_up_barcode.hide()

    scan_in_barcode.show()
    scan_in_barcode.clear()
    scan_in_barcode.focus()
    scan_in_barcode.append('Scan barcode to begin')

def set_to_scan_out():    
    scan_in_barcode.hide()
    look_up_barcode.hide()

    scan_out_barcode.show()
    scan_out_barcode.clear()
    scan_out_barcode.focus()
    scan_out_barcode.append('Scan barcode to begin')

def set_to_look_up():    
    scan_out_barcode.hide()
    scan_in_barcode.hide()

    look_up_barcode.show()
    look_up_barcode.clear()
    look_up_barcode.focus()
    look_up_barcode.append('Scan barcode to begin')