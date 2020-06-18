import sys
import pytest
sys.path.insert(1, '../src/device')

import inventory_repository as repo

repo.load()

class Test_when_round_tripping_inventory:
    _id = "123456"
    inventory_item = {
        '_id': _id,
        'name': 'item_name',
        'quantity': 1,
        'size': 8,
        'uom': 'oz',
        'image': 'https://cdn.com/item.jpg'
    }

    repo.save(inventory_item)
    saved_inventory_item = repo.find(_id)

    def test_it_should_have_the_correct_id(self):
        assert self.saved_inventory_item['_id'] == self._id
    def test_it_should_have_the_correct_name(self):
        assert self.saved_inventory_item['name'] == self.inventory_item['name']
    def test_it_should_have_the_correct_quantity(self):
        assert self.saved_inventory_item['quantity'] == self.inventory_item['quantity']
    def test_it_should_have_the_correct_size(self):
        assert self.saved_inventory_item['size'] == self.inventory_item['size']
    def test_it_should_have_the_correct_uom(self):
        assert self.saved_inventory_item['uom'] == self.inventory_item['uom']
    def test_it_should_have_the_correct_image(self):
        assert self.saved_inventory_item['image'] == self.inventory_item['image']

    repo.delete(_id)

class Test_when_saving_and_required_fields_are_missing:
    def test_it_should_raise_an_error_on_id_missing(self):
        inventory_item = {}
        with pytest.raises(ValueError) as e:
            repo.save(inventory_item)

        assert 'Missing required field: _id' in str(e.value)
    
    def test_it_should_raise_an_error_on_name_missing(self):
        inventory_item = {'_id': '123456'}
        with pytest.raises(ValueError) as e:
            repo.save(inventory_item)

        assert 'Missing required field: name' in str(e.value)
    
    def test_it_should_raise_an_error_on_quantity_missing(self):
        inventory_item = {'_id': '123456', 'name': 'item_name'}
        with pytest.raises(ValueError) as e:
            repo.save(inventory_item)

        assert 'Missing required field: quantity' in str(e.value)
    
    def test_it_should_raise_an_error_on_size_missing(self):
        inventory_item = {'_id': '123456', 'name': 'item_name', 'quantity': 1}
        with pytest.raises(ValueError) as e:
            repo.save(inventory_item)

        assert 'Missing required field: size' in str(e.value)
    
    def test_it_should_raise_an_error_on_uom_missing(self):
        inventory_item = {'_id': '123456', 'name': 'item_name', 'quantity': 1, 'size': 2}
        with pytest.raises(ValueError) as e:
            repo.save(inventory_item)

        assert 'Missing required field: uom' in str(e.value)

class Test_when_finding_and_the_item_is_not_found:
    def test_it_should_return_none(self):
        result = repo.find('')

        assert result == None