barcodeDefs = {}

def load():
    with open('../barcodes.txt', encoding='utf8') as f:
        barcodes = f.read().splitlines()
        for barcode in barcodes:
            parts = barcode.split(',')
            barcodeDefs[parts[0]] = parts[1]
    return

def scan():
    return True