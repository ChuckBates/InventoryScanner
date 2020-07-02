from guizero import Box, Text

def spin_up_top_bottom_spacers(parent):
    global top_space_box
    top_space_box = Box(parent, width='fill', align='top')
    global top_space_text
    top_space_text = Text(top_space_box, height='fill', width='fill', text='')

    global bottom_space_box
    bottom_space_box = Box(parent, width='fill', align='bottom')
    global bottom_space_text
    bottom_space_text = Text(bottom_space_box, height='fill', width='fill', text='')

def hide_top_bottom_spacers():
    if 'top_space_box' in globals():
        top_space_box.hide()
    if 'bottom_space_box' in globals():
        bottom_space_box.hide()

def show_top_bottom_spacers():    
    top_space_box.show()
    bottom_space_box.show()
    
def update_top_bottom_spacer_width(parent):
    top_space_text.height = get_top_bottom_spacer_height(parent)
    bottom_space_text.height = get_top_bottom_spacer_height(parent)
    
def get_top_bottom_spacer_height(parent):
    return int(int(parent.tk.winfo_height() / 4) / 16)