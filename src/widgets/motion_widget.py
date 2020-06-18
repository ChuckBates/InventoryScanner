import RPi.GPIO as GPIO
import screen

sensor_pin = 11

def check_for_motion():
    GPIO.setmode(GPIO.BOARD)
    GPIO.setup(sensor_pin, GPIO.IN) 
    if GPIO.input(sensor_pin)==GPIO.HIGH:
        screen.wake()
    elif GPIO.input(sensor_pin)==GPIO.LOW:
        screen.sleep()