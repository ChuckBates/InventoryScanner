from guizero import Box, Text

def spin_up_spacers(parent):
    global left_space_box
    left_space_box = Box(parent, height='fill', align='left')
    global left_space_text
    left_space_text = Text(left_space_box, height='fill', width='fill', text='')

    global right_space_box
    right_space_box = Box(parent, height='fill', align='right')
    global right_space_text
    right_space_text = Text(right_space_box, height='fill', width='fill', text='')

def hide_spacers():
    left_space_box.hide()
    right_space_box.hide()

def show_spacers():    
    left_space_box.show()
    right_space_box.show()

def update_spacer_width(parent):
    left_space_text.width = get_spacer_width(parent)
    right_space_text.width = get_spacer_width(parent)
    
def get_spacer_width(parent):
    return int(int(parent.tk.winfo_width() / 4) / 8)