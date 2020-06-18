from guizero import Box, Text

def spin_up_side_spacers(parent):
    global left_space_box
    left_space_box = Box(parent, height='fill', align='left')
    global left_space_text
    left_space_text = Text(left_space_box, height='fill', width='fill', text='')

    global right_space_box
    right_space_box = Box(parent, height='fill', align='right')
    global right_space_text
    right_space_text = Text(right_space_box, height='fill', width='fill', text='')

def spin_up_top_bottom_spacers(parent):
    global top_space_box
    top_space_box = Box(parent, width='fill', align='top')
    global top_space_text
    top_space_text = Text(top_space_box, height='fill', width='fill', text='')

    global bottom_space_box
    bottom_space_box = Box(parent, width='fill', align='bottom')
    global bottom_space_text
    bottom_space_text = Text(bottom_space_box, height='fill', width='fill', text='')

def hide_side_spacers():
    left_space_box.hide()
    right_space_box.hide()

def hide_top_bottom_spacers():
    if 'top_space_box' in globals():
        top_space_box.hide()
    if 'bottom_space_box' in globals():
        bottom_space_box.hide()

def show_side_spacers():    
    left_space_box.show()
    right_space_box.show()

def show_top_bottom_spacers():    
    top_space_box.show()
    bottom_space_box.show()

def update_side_spacer_width(parent):
    left_space_text.width = get_side_spacer_width(parent)
    right_space_text.width = get_side_spacer_width(parent)
    
def get_side_spacer_width(parent):
    return int(int(parent.tk.winfo_width() / 4) / 8)

def update_top_bottom_spacer_width(parent):
    top_space_text.height = get_top_bottom_spacer_height(parent)
    bottom_space_text.height = get_top_bottom_spacer_height(parent)
    
def get_top_bottom_spacer_height(parent):
    return int(int(parent.tk.winfo_height() / 4) / 16)