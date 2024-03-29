import tkinter
import characters
import display_config
from functools import partial
from guizero import Box

def click(btn):
    if btn == '123':
        destroy_pad()
        paint_number_pad()
    elif btn == 'abc':
        destroy_pad()
        paint_letter_pad()
        map_enter_key(enter_command)
    elif btn == '⌫':
        index = text_container.tk.index(tkinter.INSERT)
        text_container.tk.delete(index-1)
    elif btn == '↑':
        global case
        if is_upper_case():
            case = 'lower'
            shift_letters('lower')
        else:
            case = 'upper'
            shift_letters('upper')
    else:
        value = btn
        if is_upper_case():
            value = value.upper()
        text_container.tk.insert(tkinter.INSERT, value)

def is_upper_case():
    return case == 'upper'

def destroy_pad():
    for child in label_frame.winfo_children():
        child.destroy()

def reset_key_pad():    
    if 'label_frame' in globals():
        label_frame.destroy()

def show_key_pad():
    key_pad_box.show()

def hide_key_pad():
    key_pad_box.hide()

def spin_up_key_pad_box(parent):
    global key_pad_box   
    key_pad_box = Box(parent, width='fill', align='bottom')
    key_pad_box.bg = '#011627'

def spin_up_keypad(tc): 
    global text_container
    text_container = tc
    reset_key_pad()
    global label_frame
    label_frame = tkinter.LabelFrame(key_pad_box.tk, bg='#011627', relief='flat')
    label_frame.pack(padx=0, pady=0)

    global case
    case = 'lower'
    paint_letter_pad()
    return label_frame

def map_enter_key(command):
    global enter_command
    enter_command = command
    for child in label_frame.winfo_children():
        if child['text'] == '↵':
            child.config(command=command)
            child.update()
            break

def shift_letters(direction):
    for child in label_frame.winfo_children():
        current = child['text']
        if direction == 'upper':
            child.config(text=current.upper())
        else: 
            child.config(text=current.lower())

def paint_letter_pad():
    row_one_column=0
    row_two_column=0
    row_three_column=0
    row_four_column=0

    btn = list(range(len(characters.letter_lower_btn_list)))
    for label in characters.letter_lower_btn_list:
        cmd = partial(click, label)

        key_width=2
        key_height=2
        key_relief='groove'
        btn = tkinter.Button(label_frame, bd=0, text=label, width=key_width, height=key_height, command=cmd, bg="#011627", fg=display_config.text_color, font=display_config.text_font, relief=key_relief)
        if label in characters.letter_lower_first_row:
            btn.grid(row=1, column=row_one_column)
            row_one_column+=1
        elif label in characters.letter_lower_second_row:        
            btn.grid(row=2, column=row_two_column, columnspan=2)
            row_two_column+=1
        elif label in characters.letter_lower_third_row:
            btn.grid(row=3, column=row_three_column, columnspan=2)
            row_three_column+=1
        elif label in characters.letter_fourth_row:
            if label == ' ':
                btn = tkinter.Button(label_frame, bd=0, text=label, width=17, height=key_height, command=cmd, bg="#011627", fg=display_config.text_color, font=display_config.text_font, relief=key_relief)
                btn.grid(row=4, column=row_four_column, columnspan=6)
                row_four_column+=4
            else:
                btn.grid(row=4, column=row_four_column, columnspan=3)
                row_four_column+=1
                
def paint_number_pad():
    num_row_one_column=0
    num_row_two_column=0
    num_row_three_column=0
    num_row_four_column=0

    btn = list(range(len(characters.number_btn_list)))
    for label in characters.number_btn_list:
        cmd = partial(click, label)

        key_width=10     
        key_height=2   
        btn = tkinter.Button(label_frame, bd=0, text=label, width=key_width, height=key_height, command=cmd, bg="#011627", fg=display_config.text_color, font=display_config.text_font, relief='groove')
        if label in characters.number_first_row:
            btn.grid(row=1, column=num_row_one_column)
            num_row_one_column+=1
        elif label in characters.number_second_row:        
            btn.grid(row=2, column=num_row_two_column)
            num_row_two_column+=1
        elif label in characters.number_third_row:
            btn.grid(row=3, column=num_row_three_column)
            num_row_three_column+=1
        elif label in characters.number_fourth_row:
            btn.grid(row=4, column=num_row_four_column)
            num_row_four_column+=1