import sys
import cv2
import numpy as np
import os

def pad_image_to_dimensions(image_path, output_path=None):
    image = cv2.imread(image_path, cv2.IMREAD_UNCHANGED)
    if image is None:
        print("Could not read image.")
        return

    # Handle alpha channel if present
    if image.shape[2] == 4:
        b, g, r, a = cv2.split(image)
        mask = a > 0
        if np.count_nonzero(mask) == 0:
            print("Image has only transparent pixels.")
            return
        avg_b = int(np.mean(b[mask]))
        avg_g = int(np.mean(g[mask]))
        avg_r = int(np.mean(r[mask]))
        avg_color = (avg_b, avg_g, avg_r, 255)
        # Replace transparent areas with average color
        b[~mask] = avg_b
        g[~mask] = avg_g
        r[~mask] = avg_r
        a[~mask] = 255
        image = cv2.merge([b, g, r, a])
    else:
        # No alpha channel
        avg_color_per_channel = image.mean(axis=0).mean(axis=0)
        avg_color = tuple([int(c) for c in avg_color_per_channel])

    # Check original dimensions
    height, width = image.shape[:2]
    if height != 150 or width != 150:
        print(f"Image must be 150x150. Got {width}x{height}")
        return

    # Padding values
    top, bottom, left, right = 30, 12, 180, 180

    # Pad with avg_color
    padded_image = cv2.copyMakeBorder(
        image,
        top=top, bottom=bottom,
        left=left, right=right,
        borderType=cv2.BORDER_CONSTANT,
        value=avg_color
    )

    if output_path is None:
        output_path = os.path.join(os.path.dirname(image_path), "material.png")

    cv2.imwrite(output_path, padded_image)
    print(f"Padded image saved to: {output_path}")

if __name__ == "__main__":
    img_path = sys.argv[1]
    pad_image_to_dimensions(img_path)
