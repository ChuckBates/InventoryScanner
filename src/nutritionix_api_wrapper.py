import requests
import json

url = 'https://trackapi.nutritionix.com/v2/search/item'
config = json.load(open('../config.json'))

def find(barcode_id):
    response = requests.get(url, params={'upc': barcode_id}, headers={
        'x-app-id': config['nutritionix_app_id'],
        'x-app-key': config['nutritionix_app_key']
    })

    if response.status_code is not 200:
        raise ValueError('UPC is unknown')

    json = response.json()['foods'][0]
    item = {
        'name': json['brand_name'] + ' ' + json['food_name'],
        'image': json['photo']['thumb']
        }
    return item

    