import sys
import cv2
import numpy as np
import os

def pad_image_to_dimensions(image_path, output_path=None):
    image = cv2.imread(image_path, cv2.IMREAD_UNCHANGED)
    if image is None:
        print("Could not read image.")
        return

    # Check image size
    height, width = image.shape[:2]
    if height != 150 or width != 150:
        print(f"Image must be 150x150. Got {width}x{height}")
        return

    # Determine average color (non-transparent pixels only if alpha channel exists)
    if image.shape[2] == 4:
        b, g, r, a = cv2.split(image)
        mask = a > 0
        if np.count_nonzero(mask) == 0:
            print("Image has only transparent pixels.")
            return
        avg_b = int(np.mean(b[mask]))
        avg_g = int(np.mean(g[mask]))
        avg_r = int(np.mean(r[mask]))
        avg_color = (avg_b, avg_g, avg_r)
        # Convert image to BGR (drop alpha)
        image = cv2.merge([b, g, r])
    else:
        avg_color_per_channel = image.mean(axis=0).mean(axis=0)
        avg_color = tuple([int(c) for c in avg_color_per_channel])

    # Create blank canvas with average color
    canvas_height, canvas_width = 192, 510
    canvas = np.full((canvas_height, canvas_width, 3), avg_color, dtype=np.uint8)

    # Compute placement coordinates
    top, left = 30, 180  # Same offsets from original padding

    # Paste the image onto the canvas
    canvas[top:top+150, left:left+150] = image

    if output_path is None:
        output_path = os.path.join(os.path.dirname(image_path), "material.png")

    cv2.imwrite(output_path, canvas)
    print(f"Padded image saved to: {output_path}")

if __name__ == "__main__":
    img_path = sys.argv[1]
    pad_image_to_dimensions(img_path)
