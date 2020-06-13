from tkinter import Label, PhotoImage

def get_default_picture():
    return PhotoImage(file='')

def get_picture(parent, image):
    picture = PhotoImage(file=image)
    item_picture = Label(parent)
    item_picture.image = picture
    item_picture.config(image = picture, background='#011627')
    item_picture.pack()
    return item_picture