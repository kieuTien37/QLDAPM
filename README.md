# HỆ THỐNG NHẬN DIỆN NHÂN VẬT & PHÂN TÍCH MÀU SẮC

Ứng dụng AI sử dụng Deep Learning và Computer Vision để nhận diện người, giới tính và màu sắc trang phục.

## Tính năng chính

- 🔍 Phát hiện người trong ảnh (YOLO)
- 👤 Nhận dạng giới tính (DeepFace)
- 🎨 Phân tích màu sắc trang phục (K-Means)
- 🖥️ Giao diện GUI thân thiện (CustomTkinter)

## Công nghệ sử dụng

- **YOLO v11**: Object Detection
- **DeepFace**: Gender Classification
- **MTCNN**: Face Detection
- **K-Means**: Color Analysis
- **CustomTkinter**: Modern UI
- **OpenCV**: Image Processing

## Cài đặt

```bash
# Tạo môi trường ảo
python -m venv .venv
.\.venv\Scripts\Activate.ps1

# Cài đặt thư viện
pip install ultralytics deepface mtcnn customtkinter opencv-python pillow scikit-learn matplotlib numpy
```

## Sử dụng

### Chạy GUI
```bash
python main_UI.py
```

### Chạy script console
```bash
python main.py
```

## Cấu trúc dự án

```
├── main_UI.py              # Giao diện GUI chính
├── main.py                 # Script xử lý core
├── test_gender.py          # Test module DeepFace
├── best.pt                 # YOLO trained weights
├── yolo11n.pt              # YOLO base model
├── Presentation_Slides.md  # Slide thuyết trình
└── README.md               # File này
```

## Hướng dẫn sử dụng

1. Chạy ứng dụng `main_UI.py`
2. Click "Tải Ảnh" để chọn ảnh
3. Click "Phân Tích" để xử lý
4. Xem kết quả hiển thị trên ảnh

## Môn học

**Quản lý Dự án Phần mềm**  
Ngày: 8/12/2025

## License

MIT License
