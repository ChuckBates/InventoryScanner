import display_config
from guizero import Text, PushButton, Box

def spin_up_new_item_confirm(parent, commands):
    global question
    question = Text(parent, text='Item not known!\r\rWould you like to enter missing values?')
    question.font = display_config.text_font
    question.text_size = display_config.text_size
    question.text_color = display_config.text_color
    question.height = 4
    global buttons_box
    buttons_box = Box(parent)

    yes_button = PushButton(buttons_box, command=commands[0], text='Yes', align='left', width=5)
    yes_button.font = display_config.text_font
    yes_button.text_color = '#00FF21'
    yes_button.text_size = display_config.button_text_size
    
    no_button = PushButton(buttons_box, command=commands[1], text='No', align='right', width=5)
    no_button.font = display_config.text_font
    no_button.text_color = '#B60000'
    no_button.text_size = display_config.button_text_size

def hide_new_item_confirm():
    question.hide()
    buttons_box.hide()

def show_new_item_confirm():
    question.show()
    buttons_box.show()