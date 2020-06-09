import inventory_repository as repo
import nutritionix_api_wrapper as api_wrapper

def scan(barcode):
    scan_response = {}
    item = repo.find(barcode)
    if item == None:
        api_response = api_wrapper.find(barcode)
        if api_response['successful'] == True:
            scan_response['status'] = 'partial'
            scan_response['item'] = api_response['item']
        else:            
            scan_response['status'] = 'not found'
            scan_response['item'] = {'_id': barcode}
    else:
        scan_response['status'] = 'successful'
        scan_response['item'] = item
    return scan_response