from tkinter import Label, PhotoImage
from guizero import Box

def get_default_picture():
    return PhotoImage(file='')

def spin_up_picture(parent):
    global item_picture_box
    item_picture_box = Box(parent, width='fill', align='top')
    
    global item_picture
    item_picture = Label(item_picture_box.tk)
    item_picture.image = get_default_picture()
    item_picture.config(image = get_default_picture(), background='#011627')
    item_picture.pack()
    return item_picture

def set_picture(image):
    reset_picture()
    picture = PhotoImage(file=image)
    item_picture.image = picture
    item_picture.config(image = picture)

def hide_picture():
    item_picture_box.hide()

def show_picture():
    item_picture_box.show()

def reset_picture():
    item_picture.image = get_default_picture()
    item_picture.config(image = get_default_picture())