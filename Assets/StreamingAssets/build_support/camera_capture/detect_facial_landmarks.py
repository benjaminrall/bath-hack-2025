import sys
import json
import cv2
import mediapipe as mp
import numpy as np
import os

# Key landmark indices
KEY_INDICES = {
    "left_pupil": 468,
    "right_pupil": 473,
    "left_nostril": 97,
    "right_nostril": 326,
    "mouth_left": 61,
    "mouth_right": 291,
    "mouth_top": 13,
    "mouth_bottom": 14
}

def get_head_bounding_box(landmarks, width, height,
                          margin_left=0.3, margin_right=0.3,
                          margin_top=1.0, margin_bottom=0.2):
    xs = [lm.x * width for lm in landmarks]
    ys = [lm.y * height for lm in landmarks]

    min_x, max_x = min(xs), max(xs)
    min_y, max_y = min(ys), max(ys)

    w = max_x - min_x
    h = max_y - min_y

    # Apply **asymmetric** margins to better capture the head
    x1 = int(max(0, min_x - w * margin_left))
    x2 = int(min(width, max_x + w * margin_right))
    y1 = int(max(0, min_y - h * margin_top))      # more above for hair
    y2 = int(min(height, max_y + h * margin_bottom))  # less below to avoid shoulders

    return x1, y1, x2, y2


def pad_to_square(image):
    height, width = image.shape[:2]
    size = max(width, height)
    pad_vert = (size - height) // 2
    pad_horiz = (size - width) // 2

    # Pad with transparent pixels
    padded = cv2.copyMakeBorder(
        image,
        pad_vert, size - height - pad_vert,
        pad_horiz, size - width - pad_horiz,
        borderType=cv2.BORDER_CONSTANT,
        value=[0, 0, 0, 0]  # Transparent padding
    )
    return padded, pad_horiz, pad_vert, size / width, size / height


def mask_and_crop_head(image, segmentation_mask, face_landmarks, output_path):
    h, w, _ = image.shape

    # Create binary mask from segmentation (keep region > 0.1 confidence)
    condition = segmentation_mask > 0.1
    mask = (condition.astype(np.uint8)) * 255
    mask = cv2.GaussianBlur(mask, (15, 15), 0)

    # Convert image to BGRA and apply mask as alpha
    image_bgra = cv2.cvtColor(image, cv2.COLOR_BGR2BGRA)
    image_bgra[:, :, 3] = mask

    # Crop image and landmarks to head region
    x1, y1, x2, y2 = get_head_bounding_box(face_landmarks.landmark, w, h)
    cropped_image = image_bgra[y1:y2, x1:x2]

    # Pad to square
    square_image, pad_x, pad_y, scale_x, scale_y = pad_to_square(cropped_image)

    # Resize to 150x150
    resized_image = cv2.resize(square_image, (150, 150), interpolation=cv2.INTER_AREA)

    # Save final image
    cv2.imwrite(output_path, resized_image)

    # Return original offset and scale info for landmark adjustment
    return x1, y1, pad_x, pad_y, scale_x, scale_y


def detect_landmarks(image_path):
    mp_face_mesh = mp.solutions.face_mesh
    mp_selfie_segmentation = mp.solutions.selfie_segmentation

    image = cv2.imread(image_path)
    if image is None:
        print(json.dumps({ "landmarks": [] }))
        return

    height, width, _ = image.shape
    rgb_image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)

    # Segment head (includes hair/beard)
    with mp_selfie_segmentation.SelfieSegmentation(model_selection=1) as selfie_seg:
        seg_result = selfie_seg.process(rgb_image)

    # Get facial landmarks
    with mp_face_mesh.FaceMesh(
        static_image_mode=True,
        max_num_faces=1,
        refine_landmarks=True
    ) as face_mesh:
        results = face_mesh.process(rgb_image)

        if not results.multi_face_landmarks:
            print(json.dumps({ "landmarks": [] }))
            return

        face = results.multi_face_landmarks[0]

        # Mask and crop image
        out_path = os.path.join(os.path.dirname(image_path), "face_masked.png")
        offset_x, offset_y, pad_x, pad_y, scale_x, scale_y = mask_and_crop_head(
            image, seg_result.segmentation_mask, face, out_path
        )

        # Adjust landmarks to cropped, padded and resized space
        output = []
        for name, idx in KEY_INDICES.items():
            if idx >= len(face.landmark):
                continue
            lm = face.landmark[idx]
            x = (lm.x * width - offset_x + pad_x) * scale_x * (150 / max(scale_x * (width - offset_x), scale_y * (height - offset_y)))
            y = (lm.y * height - offset_y + pad_y) * scale_y * (150 / max(scale_x * (width - offset_x), scale_y * (height - offset_y)))
            output.append({ "name": name, "x": x, "y": y })

        print(json.dumps({ "landmarks": output }))
        sys.stdout.flush()

if __name__ == "__main__":
    img_path = sys.argv[1]
    detect_landmarks(img_path)
