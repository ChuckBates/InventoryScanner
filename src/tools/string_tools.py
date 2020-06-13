

def extract_size_and_uom(value):
    extract_response = {}
    stripped_value = value.strip()
    if count_digit_groups(stripped_value) > 1:
        extract_response['successful'] = False
        return extract_response

    size = ''
    for char in list(stripped_value):
        if char.isdigit():
            size += char
        if char.isalpha() or char.isspace():
            break
    
    num_digits = count_digits(stripped_value)
    if num_digits != len(size) or num_digits == 0:
        extract_response['successful'] = False
        return extract_response

    remainder = stripped_value.replace(size, '').strip()
    if len(remainder) == 0:
        extract_response['successful'] = False
        return extract_response

    extract_response['successful'] = True
    extract_response['size'] = int(size)
    extract_response['uom'] = remainder
    return extract_response

def count_digit_groups(value):
    result = []
    for char in value.split():
        if char.isdigit():
            result.append(int(char))
    return len(result)

def count_digits(value):
    result = []
    for char in list(value):
        if char.isdigit():
            result.append(int(char))
    return len(result)