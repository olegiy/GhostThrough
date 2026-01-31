import os
from svglib.svglib import svg2rlg
from reportlab.graphics import renderPM
from PIL import Image

def convert_svg_to_ico(svg_path, ico_path):
    print(f"Converting {svg_path} to {ico_path}...")
    
    # Temporary PNG file
    temp_png = "temp_icon.png"
    
    # Load SVG and convert to Drawing object
    drawing = svg2rlg(svg_path)
    
    # Render Drawing object to PNG
    # We'll render at a larger size to ensure quality for multi-size ICO
    renderPM.drawToFile(drawing, temp_png, fmt="PNG")
    
    # Load PNG with Pillow
    img = Image.open(temp_png)
    
    # ICO files should contain multiple sizes for best OS display
    icon_sizes = [(16, 16), (24, 24), (32, 32), (48, 48), (64, 64), (128, 128), (256, 256)]
    
    # Save as ICO
    img.save(ico_path, format='ICO', sizes=icon_sizes)
    
    # Clean up
    img.close()
    if os.path.exists(temp_png):
        os.remove(temp_png)
    
    print("Success!")

if __name__ == "__main__":
    base_dir = r"c:\Users\olegiy\PeekThrough"
    svg_file = os.path.join(base_dir, "resources", "icons", "icon.svg")
    ico_file = os.path.join(base_dir, "resources", "icons", "icon.ico")
    
    convert_svg_to_ico(svg_file, ico_file)
