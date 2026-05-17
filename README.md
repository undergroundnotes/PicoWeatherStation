# PicoWeatherStation

Visualizer contains the monogame server to collect data from the server and create a weather map. 

Device contains the code and such for the weather station device

Server contains the server code

todo: explaination/charts/results


NEW code for devices

import machine
import time
import math
import network
import ntptime
import urequests
import bme280

# USER Inputs

DEVICE_ID = "PICO_WEATHER_01"
#API_KEY = "key_abc123"

WIFI_SSID = "WIFI NAME"
WIFI_PASSWORD = "WIFI PASSWORD"

SERVER_URL = "http://url"

N_MEAS = 60
TS = 1


# Wind sensor geometry

length = 0.08
path_travelled = 2 * math.pi * length
WIND_THRESHOLD = 10000


# WIFI

def connect_wifi():
    wlan = network.WLAN(network.STA_IF)
    wlan.active(True)
    wlan.connect(WIFI_SSID, WIFI_PASSWORD)

    print("Connecting WiFi...")

    while not wlan.isconnected():
        time.sleep(1)

    print("Connected:", wlan.ifconfig())


# INTERNET TIME

def sync_time():
    print("Syncing time")
    ntptime.settime()
    print("Time synced")


def get_time_string():
    t = time.localtime()
    return "{}-{:02d}-{:02d} {:02d}:{:02d}:{:02d}".format(
        t[0], t[1], t[2], t[3], t[4], t[5]
    )


# SERVER FUNCTION (SECURE)

def send_to_server(serial, temp, hum, press, wind, tstr):

    payload = {
        "device_id": DEVICE_ID,
        #"api_key": API_KEY,
        "serial": serial,
        "temperature": temp,
        "humidity": hum,
        "pressure": press,
        "wind_speed": wind,
        "time": tstr
    }

    try:

        print("Sending:", payload)

        response = urequests.post(SERVER_URL, json=payload)

        print("Server response:", response.text)

        response.close()

    except Exception as e:

        print("Server send failed:", e)


# WIND SENSOR

wind_adc = machine.ADC(26)

def get_wind_speed():
    rotation_counter = 0
    start = time.time()
    prev_state = 1

    duration = 5  # seconds

    while time.time() - start < duration:
        reading = wind_adc.read_u16()
        state = 1 if reading >= WIND_THRESHOLD else 0

        # Detect falling edge
        if prev_state == 1 and state == 0:
            rotation_counter += 1
            time.sleep_ms(5)  # debounce

        prev_state = state

    rotations = rotation_counter

    # Speed
    speed_m_per_s = (rotations * path_travelled) / duration
    speed_kmh = speed_m_per_s * 3.6

    return speed_kmh


# BME280 SENSOR

spi = machine.SPI(0,
                  baudrate=1000000,
                  polarity=0,
                  phase=0,
                  sck=machine.Pin(18),
                  mosi=machine.Pin(19),
                  miso=machine.Pin(16))

cs = machine.Pin(17, machine.Pin.OUT)
cs.value(1)

sensor = bme280.BME280(spi=spi, cs=cs)


# FILE LOGGING

def log_data(device, serial, temp, hum, press, wind, tstr, file):

    line = "{},{},{:.2f},{:.2f},{:.2f},{:.2f},{}\n".format(
        device, serial, temp, hum, press, wind, tstr
    )

    file.write(line)
    file.flush()


# MAIN PROGRAM

connect_wifi()
sync_time()

file = open("weather_log.txt", "w")

file.write("DeviceID,Serial,TempC,Humidity,Pressure,WindSpeed_kmh,Time\n")

serial = 1

for i in range(N_MEAS):

    # Read BME280
    temp, press, hum = sensor.read_compensated_data()

    temp = round(temp, 2)
    press = round(press, 2)
    hum = round(hum, 2)

    # Wind speed
    wind = get_wind_speed()

    # Time
    tstr = get_time_string()

    # Print
    print(DEVICE_ID, serial, temp, hum, press, wind, tstr)

    # Save locally
    log_data(DEVICE_ID, serial, temp, hum, press, wind, tstr, file)

    # Send to server
    send_to_server(serial, temp, hum, press, wind, tstr)

    serial += 1
    time.sleep(TS)

file.close()

print("Logging complete")

