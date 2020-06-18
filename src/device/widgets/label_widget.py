import display_config
from guizero import Text

def spin_up_item_labels(parent):
    item_name_label = Text(parent, text='Name:', color=display_config.alt_text_color, font=display_config.text_font, size=display_config.text_size, grid=[1,3], align='left')
    item_quantity_label = Text(parent, text='Quantity:', color=display_config.alt_text_color, font=display_config.text_font, size=display_config.text_size, grid=[1,4], align='left')
    item_size_label = Text(parent, text='Size:', color=display_config.alt_text_color, font=display_config.text_font, size=display_config.text_size, grid=[1,5], align='left')