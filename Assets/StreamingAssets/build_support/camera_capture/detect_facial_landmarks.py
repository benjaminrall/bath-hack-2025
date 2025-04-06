import sys
import cv2
import mediapipe as mp
import numpy as np
import os

def get_head_bounding_box(landmarks, width, height,
                          margin_left=0.3, margin_right=0.3,
                          margin_top=1.0, margin_bottom=0.2):
    xs = [lm.x * width for lm in landmarks]
    ys = [lm.y * height for lm in landmarks]

    min_x, max_x = min(xs), max(xs)
    min_y, max_y = min(ys), max(ys)

    w = max_x - min_x
    h = max_y - min_y

    x1 = int(max(0, min_x - w * margin_left))
    x2 = int(min(width, max_x + w * margin_right))
    y1 = int(max(0, min_y - h * margin_top))
    y2 = int(min(height, max_y + h * margin_bottom))

    return x1, y1, x2, y2

def pad_to_square(image):
    height, width = image.shape[:2]
    size = max(width, height)
    pad_vert = (size - height) // 2
    pad_horiz = (size - width) // 2

    padded = cv2.copyMakeBorder(
        image,
        pad_vert, size - height - pad_vert,
        pad_horiz, size - width - pad_horiz,
        borderType=cv2.BORDER_CONSTANT,
        value=[0, 0, 0, 0]
    )
    return padded

def mask_and_crop_head(image, segmentation_mask, face_landmarks, output_path):
    h, w, _ = image.shape

    condition = segmentation_mask > 0.1
    mask = (condition.astype(np.uint8)) * 255
    mask = cv2.GaussianBlur(mask, (15, 15), 0)

    image_bgra = cv2.cvtColor(image, cv2.COLOR_BGR2BGRA)
    image_bgra[:, :, 3] = mask

    x1, y1, x2, y2 = get_head_bounding_box(face_landmarks.landmark, w, h)
    cropped_image = image_bgra[y1:y2, x1:x2]

    square_image = pad_to_square(cropped_image)
    resized_image = cv2.resize(square_image, (150, 150), interpolation=cv2.INTER_AREA)

    cv2.imwrite(output_path, resized_image)

def process_image(image_path):
    mp_face_mesh = mp.solutions.face_mesh
    mp_selfie_segmentation = mp.solutions.selfie_segmentation

    image = cv2.imread(image_path)
    if image is None:
        return

    rgb_image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)

    with mp_selfie_segmentation.SelfieSegmentation(model_selection=1) as selfie_seg:
        seg_result = selfie_seg.process(rgb_image)

    with mp_face_mesh.FaceMesh(
        static_image_mode=True,
        max_num_faces=1,
        refine_landmarks=True
    ) as face_mesh:
        results = face_mesh.process(rgb_image)

        if not results.multi_face_landmarks:
            return

        face = results.multi_face_landmarks[0]
        out_path = os.path.join(os.path.dirname(image_path), "icon.png")
        mask_and_crop_head(image, seg_result.segmentation_mask, face, out_path)

if __name__ == "__main__":
    img_path = sys.argv[1]
    process_image(img_path)
