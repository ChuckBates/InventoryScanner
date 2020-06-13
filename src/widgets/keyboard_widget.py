import tkinter
import characters
from functools import partial

def click(btn):
    if btn == '123':
        reset_pad()
        paint_number_pad()
    elif btn == 'abc':
        reset_pad()
        paint_letter_pad()
    else:
        text_container.append(btn)

def reset_pad():
    for child in label_frame.winfo_children():
        child.destroy()

def get_keypad(parent, tc):    
    tc.clear()
    global text_container
    text_container = tc
    global label_frame
    label_frame = tkinter.LabelFrame(parent, text=' keypad ', bd=3)
    label_frame.pack(padx=15, pady=10)

    paint_letter_pad()
    return label_frame

def paint_letter_pad():
    row_one_column=0
    row_two_column=0
    row_three_column=0
    row_four_column=0

    btn = list(range(len(characters.letter_btn_list)+3))
    for label in characters.letter_btn_list:
        cmd = partial(click, label)

        key_width=7
        if label in characters.letter_first_row:
            btn = tkinter.Button(label_frame, text=label, width=key_width, command=cmd)
            btn.grid(row=1, column=row_one_column)
            row_one_column+=1
        elif label in characters.letter_second_row:        
            btn = tkinter.Button(label_frame, text=label, width=key_width, command=cmd)
            btn.grid(row=2, column=row_two_column, columnspan=2)
            row_two_column+=1
        elif label in characters.letter_third_row:
            btn = tkinter.Button(label_frame, text=label, width=key_width, command=cmd)
            btn.grid(row=3, column=row_three_column, columnspan=2)
            row_three_column+=1
        elif label in characters.letter_fourth_row:
            if label == ' ':
                btn = tkinter.Button(label_frame, text=label, width=32, command=cmd)
                btn.grid(row=4, column=row_four_column, columnspan=6)
                row_four_column+=4
            else:
                btn = tkinter.Button(label_frame, text=label, width=key_width, command=cmd)
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

        key_width=15
        if label in characters.number_first_row:
            btn = tkinter.Button(label_frame, text=label, width=key_width, command=cmd)
            btn.grid(row=1, column=num_row_one_column)
            num_row_one_column+=1
        elif label in characters.number_second_row:        
            btn = tkinter.Button(label_frame, text=label, width=key_width, command=cmd)
            btn.grid(row=2, column=num_row_two_column)
            num_row_two_column+=1
        elif label in characters.number_third_row:
            btn = tkinter.Button(label_frame, text=label, width=key_width, command=cmd)
            btn.grid(row=3, column=num_row_three_column)
            num_row_three_column+=1
        elif label in characters.number_fourth_row:
            btn = tkinter.Button(label_frame, text=label, width=key_width, command=cmd)
            btn.grid(row=4, column=num_row_four_column)
            num_row_four_column+=1