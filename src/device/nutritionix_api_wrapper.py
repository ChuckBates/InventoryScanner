import requests
import json

url = 'https://trackapi.nutritionix.com/v2/search/item'
config = json.load(open('../config.json'))

def find(barcode_id):
    response = requests.get(url, params={'upc': barcode_id}, headers={
        'x-app-id': config['nutritionix_app_id'],
        'x-app-key': config['nutritionix_app_key']
    })

    api_response = {}

    if response.status_code != 200:
        api_response['successful'] = False
        api_response['error'] = 'Unknown error: ' + str(response.status_code)
        api_response['item'] = {}
        if response.status_code == 404:
            api_response['error'] = 'UPC is unknown'        
        if response.status_code == 401:
            api_response['error'] = 'Rate limit exceeded'  
        return api_response

    json = response.json()['foods'][0]
    item = {
        '_id': barcode_id,
        'name': json['brand_name'] + ' ' + json['food_name'],
        'image': json['photo']['thumb']
        }
    api_response['successful'] = True
    api_response['item'] = item

    return api_response

    