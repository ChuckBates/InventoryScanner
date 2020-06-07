import sys
sys.path.insert(1, '../src')

import scanner

def test_it_should_pass():
    assert scanner.scan() is True