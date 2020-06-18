from subprocess import call

def wake():
    command = 'xset -display :0 dpms force on'
    call(command, shell=True)

def sleep():
    command = 'xset -display :0 dpms force off'
    call(command, shell=True)