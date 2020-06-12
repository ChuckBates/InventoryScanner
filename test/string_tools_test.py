import sys
sys.path.insert(1, '../src')

import string_tools

class Test_when_extracting_size_and_uom_successfully:
    result = string_tools.extract_size_and_uom('8oz')

    def test_it_should_return_successful(self):
        assert self.result['successful'] is True

    def test_it_should_return_the_size(self):
        assert self.result['size'] == 8

    def test_it_should_return_the_uom(self):
        assert self.result['uom'] == 'oz'

class Test_when_extracting_size_and_uom_and_size_are_multi_digit:
    result = string_tools.extract_size_and_uom('854g')
    
    def test_it_should_return_successful(self):
        assert self.result['successful'] is True
    
    def test_it_should_return_the_size(self):
        assert self.result['size'] == 854

    def test_it_should_return_the_uom(self):
        assert self.result['uom'] == 'g'

class Test_when_extracting_size_and_uom_and_there_are_outside_whitespace():
    result = string_tools.extract_size_and_uom(' 8oz ')

    def test_it_should_return_successful(self):
        assert self.result['successful'] is True

    def test_it_should_return_the_size(self):
        assert self.result['size'] == 8

    def test_it_should_return_the_uom(self):
        assert self.result['uom'] == 'oz'

class Test_when_extracting_size_and_uom_and_there_are_inside_whitespace():
    result = string_tools.extract_size_and_uom('8 oz')

    def test_it_should_return_successful(self):
        assert self.result['successful'] is True

    def test_it_should_return_the_size(self):
        assert self.result['size'] == 8

    def test_it_should_return_the_uom(self):
        assert self.result['uom'] == 'oz'

class Test_when_extracting_size_and_uom_and_there_are_multiple_digit_groups():
    result = string_tools.extract_size_and_uom('8 6 oz')

    def test_it_should_not_return_successful(self):
        assert self.result['successful'] is False

class Test_when_extracting_size_and_uom_and_there_are_multiple_separated_digits():
    result = string_tools.extract_size_and_uom('8 6oz')

    def test_it_should_not_return_successful(self):
        assert self.result['successful'] is False

class Test_when_extracting_size_and_uom_and_there_are_no_digits():
    result = string_tools.extract_size_and_uom('oz')

    def test_it_should_not_return_successful(self):
        assert self.result['successful'] is False

class Test_when_extracting_size_and_uom_and_there_are_no_chars_after_digits():
    result = string_tools.extract_size_and_uom('8')

    def test_it_should_not_return_successful(self):
        assert self.result['successful'] is False