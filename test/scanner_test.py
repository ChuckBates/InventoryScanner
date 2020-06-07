import sys
sys.path.insert(1, '../src')

import scanner

class TestLoadBarcodeDefinitions:
    def test_load(self):
        assert len(scanner.barcodeDefs) == 0
        scanner.load()
        assert len(scanner.barcodeDefs) > 0