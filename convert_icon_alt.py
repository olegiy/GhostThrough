import os
from weasyprint import HTML
from PIL import Image
import base64

def convert_svg_to_ico_weasy(svg_path, ico_path):
    print(f"Converting {svg_path} to {ico_path} using WeasyPrint...")
    
    temp_png = "temp_icon.png"
    
    # WeasyPrint can render HTML/SVG
    # We'll wrap the SVG in a small HTML to ensure dimensions
    with open(svg_path, 'r', encoding='utf-8') as f:
        svg_content = f.read()
    
    html_content = f"""
    <html>
    <body style="margin:0; padding:0; overflow:hidden;">
        {svg_content}
    </body>
    </html>
    """
    
    # Render to PNG
    HTML(string=html_content).write_png(temp_png, resolution=300)
    
    # Load PNG with Pillow
    img = Image.open(temp_png)
    
    # Save as ICO
    icon_sizes = [(16, 16), (24, 24), (32, 32), (48, 48), (64, 64), (128, 128), (256, 256)]
    img.save(ico_path, format='ICO', sizes=icon_sizes)
    
    img.close()
    if os.path.exists(temp_png):
        os.remove(temp_png)
    
    print("Success!")

if __name__ == "__main__":
    base_dir = r"c:\Users\olegiy\PeekThrough"
    svg_file = os.path.join(base_dir, "resources", "icons", "icon.svg")
    ico_file = os.path.join(base_dir, "resources", "icons", "icon.ico")
    
    try:
        convert_svg_to_ico_weasy(svg_file, ico_file)
    except Exception as e:
        print(f"Error: {e}")
