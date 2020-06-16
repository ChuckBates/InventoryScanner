import sys
import os
from pathlib import Path
sys.path.insert(1, '../src')
sys.path.insert(1, '../src/tools')

import inventory_lookup
import inventory_repository as repo

_id = "123456"

repo.load()

inventory_item = {
    '_id': _id,
    'name': 'item_name',
    'quantity': 1,
    'size': 8,
    'uom': 'oz',
    'image': 'https://cdn.com/item.jpg'
}

repo.save(inventory_item)

class Test_when_scanning_a_barcode_and_it_is_in_cache:
    result = inventory_lookup.find(_id)
    def test_it_should_return_successful(self):
        assert self.result['status'] == 'successful'
    def test_it_should_return_the_inventory_item(self):
        assert self.result['item']['name'] == inventory_item['name']
        assert self.result['item']['quantity'] == inventory_item['quantity']
        
    repo.delete(_id)

class Test_when_scanning_a_barcode_and_it_is_not_in_cache:
    result = inventory_lookup.find('851045005013')
    def test_it_should_return_partial(self):
        assert self.result['status'] == 'partial'
    def test_it_should_return_the_partial_item(self):
        assert self.result['item']['name'] == 'Three Jerks Beef Jerky, Chipotle Adobo'
        assert self.result['item']['image'] == str(Path(f'{os.getcwd()}/images/851045005013.png'))
        assert ('quantity' in self.result['item']) == False

class Test_clean_up:
    def test_it_should_clean_up(self):
        image_file = Path(f'{os.getcwd()}/images/851045005013.png')
        os.remove(image_file)
        assert image_file.is_file() is False