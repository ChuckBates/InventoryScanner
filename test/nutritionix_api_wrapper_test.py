import sys

sys.path.insert(1, '../src/device')

import nutritionix_api_wrapper as api_wrapper

class Test_when_calling_nutritionix_and_it_was_successful:
    def test_it_should_call_the_correct_url(self):
        assert api_wrapper.url == 'https://trackapi.nutritionix.com/v2/search/item'
    
    api_response = api_wrapper.find('015400015288')

    def test_it_should_return_success(self):
        assert self.api_response['successful'] == True
    def test_it_should_return_name(self):
        assert self.api_response['item']['_id'] == '015400015288'
    def test_it_should_return_name(self):
        assert self.api_response['item']['name'] == 'Western Family Tomato Sauce'
    def test_it_should_return_image(self):
        assert self.api_response['item']['image'] == 'https://nutritionix-api.s3.amazonaws.com/54f7e93624aa687c0f260f0c.jpeg'

class Test_when_calling_nutritionix_and_the_upc_is_unknown: 
    api_response = api_wrapper.find('123456')

    def test_it_should_return_unsuccessful(self):          
        assert self.api_response['successful'] == False
    def test_it_should_return_upc_unknown_error(self):          
        assert self.api_response['error'] == 'UPC is unknown'