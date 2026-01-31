import os
from PIL import Image

def convert_png_to_ico(png_path, ico_path):
    print(f"Converting {png_path} to {ico_path}...")
    
    # Load PNG with Pillow
    img = Image.open(png_path)
    
    # ICO files should contain multiple sizes for best OS display
    icon_sizes = [(16, 16), (24, 24), (32, 32), (48, 48), (64, 64), (128, 128), (256, 256)]
    
    # Save as ICO
    img.save(ico_path, format='ICO', sizes=icon_sizes)
    
    img.close()
    print("Success!")

if __name__ == "__main__":
    base_dir = r"c:\Users\olegiy\PeekThrough"
    png_file = os.path.join(base_dir, "resources", "icons", "icon.png")
    ico_file = os.path.join(base_dir, "resources", "icons", "icon.ico")
    
    try:
        convert_png_to_ico(png_file, ico_file)
    except Exception as e:
        print(f"Error: {e}")
