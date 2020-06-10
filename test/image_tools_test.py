import sys
sys.path.insert(1, '../src')

import image_tools
import os
from pathlib import Path

image_file = Path(os.getcwd() + '/images/015400015288.jpeg')
image_url = 'https://nutritionix-api.s3.amazonaws.com/54f7e93624aa687c0f260f0c.jpeg'
png_image_file = Path(f'{os.getcwd()}/images/015400015288.png')

class Test_when_retrieving_an_image_successfully:
    result = image_tools.download_image('015400015288', image_url)
    def test_it_should_download_the_image(self):
        assert image_file.is_file() is True

    def test_it_should_return_successful(self):
        assert self.result['successful'] is True

    def test_it_should_return_image_file_path(self):
        assert self.result['image_path'] == str(png_image_file)

class Test_when_retrieving_an_image_and_it_is_unsuccessful:
    def test_it_should_return_unsuccessful(self):
        result = image_tools.download_image('123456', 'http://not.a.real.domain/image.jpeg')
        assert result['successful'] is False

class Test_when_converting_an_image_to_png:
    def test_it_should_convert_successfully(self):
        result = image_tools.convert_to_png(image_file)
        assert png_image_file.is_file() is True
        assert result == str(png_image_file)

class Test_when_converting_an_image_to_png_and_it_is_already:
    def test_it_should_return_the_same_file(self):
        result = image_tools.convert_to_png(png_image_file)
        assert result == png_image_file

class Test_clean_up:
    def test_it_should_clean_up(self):
        os.remove(image_file)
        assert image_file.is_file() is False
        os.remove(png_image_file)
        assert png_image_file.is_file() is False
