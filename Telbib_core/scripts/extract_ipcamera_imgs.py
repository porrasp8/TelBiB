import requests
from PIL import Image
from io import BytesIO
import os
import time

#-- Current image frmae URL
camera_url = "http://192.168.0.193:8080/shot.jpg"

#-- Save folder for the images
storage_directory = "captured_images"
os.makedirs(storage_directory, exist_ok=True)

# Cantidad de imágenes a capturar
num_images_to_capture = 100

def download_and_save_image(index):
        
    #-- Current frame download
    response = requests.get(camera_url)
    response.raise_for_status()

    #-- Open image
    image_data = BytesIO(response.content)
    img = Image.open(image_data)

    #-- Save it
    #img.save(os.path.join(storage_directory, f"image_{index}.png"))
    img.save(os.path.join(storage_directory, f"image_current.png"))
    print(f"Image {index} captured and saved.")

# Capturar un número específico de imágenes
for i in range(num_images_to_capture):
    download_and_save_image(i)
    #time.sleep(1)

print("Capture process completed.")
