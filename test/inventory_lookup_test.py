import sys
sys.path.insert(1, '../src')

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
    def test_it_should_return_the_inventory_item(self):
        result = inventory_lookup.find(_id)

        assert result['name'] == inventory_item['name']
        
        repo.delete(_id)

class Test_when_scanning_a_barcode_and_it_is_not_in_cache:
    def test_it_should_return_the_nutritionix_result(self):
        result = inventory_lookup.find('851045005013')

        assert result['name'] == 'Three Jerks Beef Jerky, Chipotle Adobo'