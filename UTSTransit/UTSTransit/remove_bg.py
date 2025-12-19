from PIL import Image
import os

def remove_white_bg(path):
    try:
        print(f"Processing {path}...")
        img = Image.open(path).convert("RGBA")
        datas = img.getdata()

        newData = []
        for item in datas:
            # Change all white (also shades of whites) to transparent
            # Threshold 240 is usually good for clean white backgrounds
            if item[0] > 240 and item[1] > 240 and item[2] > 240:
                newData.append((255, 255, 255, 0))
            else:
                newData.append(item)

        img.putdata(newData)
        img.save(path, "PNG")
        print(f"Successfully removed background from {path}")
    except Exception as e:
        print(f"Error processing {path}: {e}")

paths = [
    r"C:\Users\ASUS LAPTOP\Desktop\UTS\Mobile App\Assignment\UTS-Transit\UTSTransit\UTSTransit\Resources\Images\hostel_icon.png",
    r"C:\Users\ASUS LAPTOP\Desktop\UTS\Mobile App\Assignment\UTS-Transit\UTSTransit\UTSTransit\Resources\Images\campus_icon.png"
]

for p in paths:
    remove_white_bg(p)
