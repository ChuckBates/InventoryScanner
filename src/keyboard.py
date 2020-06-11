import tkinter
from functools import partial

def click(btn):
    s = 'Button %s clicked' % btn
    root.title(s)

root = tkinter.Tk()

lf = tkinter.LabelFrame(root, text=' keypad ', bd=3)
lf.pack(padx=15, pady=10)

btn_list = [
    'q', 'w', 'e', 'r', 't', 'y', 'u', 'i', 'o', 'p',
    'a', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l',
    '^', 'z', 'x', 'c', 'v', 'b', 'n', 'm', '<x|',
    '123', ',', '___________________', '.', '<-¬'
]

first_row = [
    'q', 'w', 'e', 'r', 't', 'y', 'u', 'i', 'o', 'p'
]

second_row = [
    'a', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l'
]

third_row = [
    '^', 'z', 'x', 'c', 'v', 'b', 'n', 'm', '<x|'
]

fourth_row = [
    '123', ',', '___________________', '.', '<-¬'
]

row_one_column=0
row_two_column=0
row_three_column=0
row_four_column=0
n=0

btn = list(range(len(btn_list)+3))
for label in btn_list:
    cmd = partial(click, label)

    key_width=7
    if label in first_row:
        btn[n] = tkinter.Button(lf, text=label, width=key_width, command=cmd)
        btn[n].grid(row=1, column=row_one_column)
        row_one_column+=1
    elif label in second_row:        
        btn[n] = tkinter.Button(lf, text=label, width=key_width, command=cmd)
        btn[n].grid(row=2, column=row_two_column, columnspan=2)
        row_two_column+=1
    elif label in third_row:
        btn[n] = tkinter.Button(lf, text=label, width=key_width, command=cmd)
        btn[n].grid(row=3, column=row_three_column, columnspan=2)
        row_three_column+=1
    elif label in fourth_row:
        if label == '___________________':
            btn[n] = tkinter.Button(lf, text=label, width=32, command=cmd)
            btn[n].grid(row=4, column=row_four_column, columnspan=6)
            row_four_column+=4
        else:
            btn[n] = tkinter.Button(lf, text=label, width=key_width, command=cmd)
            btn[n].grid(row=4, column=row_four_column, columnspan=3)
            row_four_column+=1

root.mainloop()