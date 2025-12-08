from mtcnn import MTCNN
import cv2
from deepface import DeepFace

img = cv2.imread("nu_trang.png")
detector = MTCNN()
faces = detector.detect_faces(img)

for f in faces:
    x, y, w, h = f['box']
    face_crop = img[y:y+h, x:x+w]
    result = DeepFace.analyze(face_crop, actions=['gender'], enforce_detection=False)
    print(result[0]['gender'], x, y, w, h)
