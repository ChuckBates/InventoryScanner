import sys
sys.path.insert(1, '../src')

import scanner

class TestLoadBarcodeDefinitions:
    def test_load(self):
        assert len(scanner.barcodeDefs) == 0
        scanner.load()
        assert len(scanner.barcodeDefs) > 0

class TestLookupBarcode:
    def test_lookup_unknown_barcode(self):
        unknown = '000000'
        assert scanner.lookup(unknown) == 'Unknown Barcode'
    def test_lookup_known_barcode(self):
        known = '720103002748'
        assert scanner.lookup(known) == 'Ashbys flavorful assortment of teas 1.92oz'