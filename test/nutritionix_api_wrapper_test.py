import sys
import pytest

sys.path.insert(1, '../src')

import nutritionix_api_wrapper as api_wrapper

class Test_when_calling_nutritionix_and_it_was_successful:
    def test_it_should_call_the_correct_url(self):
        assert api_wrapper.url == 'https://trackapi.nutritionix.com/v2/search/item'
    
    item = api_wrapper.find('015400015288')

    def test_it_should_return_name(self):
        assert self.item['name'] == 'Western Family Tomato Sauce'
    def test_it_should_return_image(self):
        assert self.item['image'] == 'https://nutritionix-api.s3.amazonaws.com/54f7e93624aa687c0f260f0c.jpeg'

class Test_when_calling_nutritionix_and_the_upc_is_unknown:
    def test_it_should_raise_an_error(self):        
        with pytest.raises(ValueError) as e:
            api_wrapper.find('123456')
        
        assert 'UPC is unknown' in str(e.value) 