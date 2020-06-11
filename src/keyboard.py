import tkinter
from functools import partial

def click(btn):
    s = 'Button %s clicked' % btn
    root.title(s)

    if btn == '123':
        reset_pad()
        paint_number_pad()
    if btn == 'abc':
        reset_pad()
        paint_letter_pad()

def reset_pad():
    for child in lf.winfo_children():
        child.destroy()

root = tkinter.Tk()

lf = tkinter.LabelFrame(root, text=' keypad ', bd=3)
lf.pack(padx=15, pady=10)

number_btn_list = [
    '7', '8', '9',
    '4', '5', '6', 
    '1', '2', '3', 
    'abc', '0', '<x|', '<-¬'
]

number_first_row = [
    '7', '8', '9',
]

number_second_row = [
    '4', '5', '6',
]

number_third_row = [
    '1', '2', '3',
]

number_fourth_row = [
    'abc', '0', '<x|', '<-¬'
]

letter_btn_list = [
    'q', 'w', 'e', 'r', 't', 'y', 'u', 'i', 'o', 'p',
    'a', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l',
    '^', 'z', 'x', 'c', 'v', 'b', 'n', 'm', '<x|',
    '123', ',', '___________________', '.', '<-¬'
]

letter_first_row = [
    'q', 'w', 'e', 'r', 't', 'y', 'u', 'i', 'o', 'p'
]

letter_second_row = [
    'a', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l'
]

letter_third_row = [
    '^', 'z', 'x', 'c', 'v', 'b', 'n', 'm', '<x|'
]

letter_fourth_row = [
    '123', ',', '___________________', '.', '<-¬'
]

def paint_letter_pad():
    row_one_column=0
    row_two_column=0
    row_three_column=0
    row_four_column=0

    btn = list(range(len(letter_btn_list)+3))
    for label in letter_btn_list:
        cmd = partial(click, label)

        key_width=7
        if label in letter_first_row:
            btn = tkinter.Button(lf, text=label, width=key_width, command=cmd)
            btn.grid(row=1, column=row_one_column)
            row_one_column+=1
        elif label in letter_second_row:        
            btn = tkinter.Button(lf, text=label, width=key_width, command=cmd)
            btn.grid(row=2, column=row_two_column, columnspan=2)
            row_two_column+=1
        elif label in letter_third_row:
            btn = tkinter.Button(lf, text=label, width=key_width, command=cmd)
            btn.grid(row=3, column=row_three_column, columnspan=2)
            row_three_column+=1
        elif label in letter_fourth_row:
            if label == '___________________':
                btn = tkinter.Button(lf, text=label, width=32, command=cmd)
                btn.grid(row=4, column=row_four_column, columnspan=6)
                row_four_column+=4
            else:
                btn = tkinter.Button(lf, text=label, width=key_width, command=cmd)
                btn.grid(row=4, column=row_four_column, columnspan=3)
                row_four_column+=1
                
def paint_number_pad():
    num_row_one_column=0
    num_row_two_column=0
    num_row_three_column=0
    num_row_four_column=0

    btn = list(range(len(number_btn_list)))
    for label in number_btn_list:
        cmd = partial(click, label)

        key_width=7
        if label in number_first_row:
            btn = tkinter.Button(lf, text=label, width=key_width, command=cmd)
            btn.grid(row=1, column=num_row_one_column)
            num_row_one_column+=1
        elif label in number_second_row:        
            btn = tkinter.Button(lf, text=label, width=key_width, command=cmd)
            btn.grid(row=2, column=num_row_two_column)
            num_row_two_column+=1
        elif label in number_third_row:
            btn = tkinter.Button(lf, text=label, width=key_width, command=cmd)
            btn.grid(row=3, column=num_row_three_column)
            num_row_three_column+=1
        elif label in number_fourth_row:
            btn = tkinter.Button(lf, text=label, width=key_width, command=cmd)
            btn.grid(row=4, column=num_row_four_column)
            num_row_four_column+=1

paint_letter_pad()
root.mainloop()