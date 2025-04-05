import sys
import json
import cv2
import mediapipe as mp

# Minimal key landmark indices
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

def detect_landmarks(image_path):
    mp_face_mesh = mp.solutions.face_mesh
    face_mesh = mp_face_mesh.FaceMesh(
        static_image_mode=True,
        max_num_faces=1,
        refine_landmarks=True  # needed for pupil tracking
    )

    image = cv2.imread(image_path)
    if image is None:
        print(json.dumps({ "landmarks": [] }))
        return

    height, width, _ = image.shape
    rgb_image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
    results = face_mesh.process(rgb_image)

    output = []
    if results.multi_face_landmarks:
        face = results.multi_face_landmarks[0]
        for name, idx in KEY_INDICES.items():
            if idx >= len(face.landmark):
                continue
            lm = face.landmark[idx]
            x = lm.x * width
            y = lm.y * height
            output.append({ "name": name, "x": x, "y": y })

    print(json.dumps({ "landmarks": output }))
    sys.stdout.flush()

if __name__ == "__main__":
    img_path = sys.argv[1]
    detect_landmarks(img_path)
