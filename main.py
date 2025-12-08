import cv2
import numpy as np
from ultralytics import YOLO
from sklearn.cluster import KMeans
from deepface import DeepFace
from mtcnn import MTCNN
import tempfile
import os
import warnings
import matplotlib.colors as mcolors

warnings.filterwarnings("ignore")

# Chuyển dict màu chuẩn sang RGB
COLOR_NAMES = {}
for name, hex in mcolors.CSS4_COLORS.items():
    rgb = np.array(mcolors.to_rgb(hex)) * 255
    COLOR_NAMES[name] = rgb.astype(int)

def detect_dominant_color(image, k=5):
    """Nhận diện màu chi tiết dựa trên KMeans và so sánh với bảng màu chuẩn"""
    if image is None or image.size == 0:
        return "không rõ"
    img = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
    img = img.reshape((-1, 3))
    kmeans = KMeans(n_clusters=k, n_init=10).fit(img)
    cluster_centers = kmeans.cluster_centers_.astype(int)
    counts = np.bincount(kmeans.labels_)
    dominant = cluster_centers[np.argmax(counts)]
    # So sánh với bảng màu chuẩn
    min_dist = float('inf')
    closest_color = "khác"
    for name, rgb in COLOR_NAMES.items():
        dist = np.linalg.norm(dominant - rgb)
        if dist < min_dist:
            min_dist = dist
            closest_color = name
    return closest_color

def predict_gender(face_crop):
    """Nhận diện giới tính bằng DeepFace"""
    with tempfile.NamedTemporaryFile(suffix=".jpg", delete=False) as tmp:
        temp_path = tmp.name
        cv2.imwrite(temp_path, face_crop)
    try:
        result = DeepFace.analyze(img_path=temp_path, actions=['gender'], enforce_detection=False)
        if isinstance(result, list): result = result[0]
        gender_label = result.get("gender", "Unknown")
        if isinstance(gender_label, dict):
            gender = max(gender_label, key=gender_label.get)
        else:
            gender = gender_label
        return gender
    except:
        return "không rõ"
    finally:
        os.remove(temp_path)

# ===== Load YOLO detect người =====
model = YOLO(r"yolo11n.pt")  # hoặc yolov8n.pt

# ===== Đọc ảnh =====
img = cv2.imread(r"nam_do.png")
if img is None:
    raise ValueError("Không đọc được ảnh, kiểm tra đường dẫn!")

# MTCNN detect face
face_detector = MTCNN()
faces = face_detector.detect_faces(img)

# YOLO detect người
results = model(img)
detections = results[0].boxes.data.cpu().numpy()

output = []

for det in detections:
    x1, y1, x2, y2, conf, cls = det
    if int(cls) != 0:  # Chỉ person
        continue
    x1, y1, x2, y2 = map(int, [x1, y1, x2, y2])
    person_crop = img[y1:y2, x1:x2]
    if person_crop is None or person_crop.size == 0:
        continue

    # tìm face trong bbox người
    face_bbox = None
    for f in faces:
        fx, fy, fw, fh = f['box']
        if fx >= x1 and fy >= y1 and fx+fw <= x2 and fy+fh <= y2:
            face_bbox = (fx, fy, fw, fh)
            break

    # crop mặt nếu có
    if face_bbox:
        fx, fy, fw, fh = face_bbox
        face_crop = img[fy:fy+fh, fx:fx+fw]
        gender = predict_gender(face_crop)
        # Vùng áo: từ dưới mặt đến cuối bbox người
        shirt_y1 = fy + fh + 10
        shirt_y2 = y1 + int(0.7 * (y2 - y1))
        shirt_crop = img[shirt_y1:shirt_y2, x1:x2]
        color = detect_dominant_color(shirt_crop)
        shirt_box = (x1, shirt_y1, x2, shirt_y2)
    else:
        gender = "không rõ"
        color = detect_dominant_color(person_crop)
        shirt_box = (x1, y1, x2, y2)  # dùng toàn bbox nếu không có mặt

    output.append({
        "giới_tính": gender,
        "màu_áo": color,
        "bbox": (x1, y1, x2, y2),
        "face_box": face_bbox,
        "shirt_box": shirt_box
    })

# ===== In kết quả =====
for i, o in enumerate(output, 1):
    print(f"Người {i}: {o['giới_tính']}, áo {o['màu_áo']}")

# ===== Vẽ kết quả =====
for o in output:
    x1, y1, x2, y2 = o['bbox']
    cv2.rectangle(img, (x1, y1), (x2, y2), (0, 255, 0), 2)  # bbox người
    text = f"{o['giới_tính']}, ao {o['màu_áo']}"
    cv2.putText(img, text, (x1, y1-10), cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 255, 0), 2)

    # vẽ face nếu có
    if o['face_box']:
        fx, fy, fw, fh = o['face_box']
        cv2.rectangle(img, (fx, fy), (fx+fw, fy+fh), (255, 0, 0), 2)

    # vẽ bbox áo
    sx1, sy1, sx2, sy2 = o['shirt_box']
    cv2.rectangle(img, (sx1, sy1), (sx2, sy2), (255, 0, 255), 2)

cv2.imshow("Kết quả nhận dạng giới tính & màu áo", img)
cv2.waitKey(0)
cv2.destroyAllWindows()