from PIL import Image
import os

def check_img(path):
    try:
        img = Image.open(path)
        print(f"Image: {os.path.basename(path)}")
        print(f"Size: {img.size}")
        bbox = img.getbbox()
        if bbox:
            print(f"Content Bounding Box: {bbox}")
            width = bbox[2] - bbox[0]
            height = bbox[3] - bbox[1]
            print(f"Content Size: {width}x{height}")
            
            # Calculate fill ratio
            total_area = img.size[0] * img.size[1]
            content_area = width * height
            print(f"Content Fill Ratio: {content_area / total_area:.2f}")
        print("-" * 20)
    except Exception as e:
        print(f"Error: {e}")

paths = [
    r"C:\Users\ASUS LAPTOP\Desktop\UTS\Mobile App\Assignment\UTS-Transit\UTSTransit\UTSTransit\Resources\Images\map_icon.png",
    r"C:\Users\ASUS LAPTOP\Desktop\UTS\Mobile App\Assignment\UTS-Transit\UTSTransit\UTSTransit\Resources\Images\schedule_icon.png"
]

for p in paths:
    check_img(p)
