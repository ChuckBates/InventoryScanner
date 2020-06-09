import inventory_repository as repo
import nutritionix_api_wrapper as api_wrapper

def find(barcode):
    find_response = {}
    item = repo.find(barcode)
    if item == None:
        api_response = api_wrapper.find(barcode)
        if api_response['successful'] == True:
            find_response['status'] = 'partial'
            find_response['item'] = api_response['item']
        else:            
            find_response['status'] = 'not found'
            find_response['item'] = {'_id': barcode}
    else:
        find_response['status'] = 'successful'
        find_response['item'] = item
    return find_response