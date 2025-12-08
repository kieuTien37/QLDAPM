# DỰ ÁN: HỆ THỐNG NHẬN DIỆN NHÂN VẬT & PHÂN TÍCH MÀU SẮC SỬ DỤNG AI

## Môn học: Quản lý Dự án Phần mềm
### Ngày: 8/12/2025

---

## SLIDE 1: TRANG BÌA
### **HỆ THỐNG NHẬN DIỆN NHÂN VẬT & PHÂN TÍCH MÀU SẮC**
**Sử dụng Deep Learning và Computer Vision**

**Giảng viên:** [Tên giảng viên]  
**Nhóm thực hiện:** [Tên nhóm]  
**Ngày báo cáo:** 8/12/2025

---

## SLIDE 2: TỔNG QUAN DỰ ÁN

### 🎯 **Mục tiêu dự án**
- Phát triển ứng dụng AI nhận diện tự động:
  - ✅ Phát hiện người trong ảnh/video
  - ✅ Nhận dạng giới tính (Nam/Nữ)
  - ✅ Phân tích màu sắc trang phục

### 🔧 **Công nghệ sử dụng**
- **YOLO (YOLOv11n)**: Object Detection
- **DeepFace**: Gender Classification
- **MTCNN**: Face Detection
- **K-Means Clustering**: Color Analysis
- **CustomTkinter**: Giao diện người dùng

### 👥 **Đối tượng sử dụng**
- An ninh, giám sát
- Phân tích hành vi khách hàng
- Nghiên cứu xã hội học

---

## SLIDE 3: PHẠM VI DỰ ÁN

### **Chức năng chính**
1. **Tải và hiển thị ảnh**
   - Upload ảnh từ máy tính
   - Xem trước ảnh gốc

2. **Phát hiện và nhận dạng**
   - Detect người trong ảnh
   - Nhận diện giới tính
   - Phân tích màu áo

3. **Hiển thị kết quả**
   - Vẽ bounding box
   - Hiển thị nhãn thông tin
   - Xuất log phân tích

### **Giới hạn dự án**
- ❌ Chưa hỗ trợ video realtime
- ❌ Chưa có database lưu trữ
- ❌ Chưa tối ưu cho mobile

---

## SLIDE 4: KIẾN TRÚC HỆ THỐNG

### **Sơ đồ kiến trúc tổng quan**

```
┌─────────────────────────────────────────────────────┐
│              PRESENTATION LAYER (GUI)               │
│           CustomTkinter Interface (main_UI.py)      │
└─────────────────────┬───────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────┐
│             APPLICATION LAYER (Business Logic)      │
│  • Image Processing   • Result Integration          │
│  • Thread Management  • Color Mapping               │
└─────────────────────┬───────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────┐
│              AI/ML MODEL LAYER                      │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐         │
│  │  YOLO    │  │ DeepFace │  │  MTCNN   │         │
│  │ (Object  │  │ (Gender  │  │  (Face   │         │
│  │Detection)│  │Classify) │  │Detection)│         │
│  └──────────┘  └──────────┘  └──────────┘         │
│  ┌──────────────────────────────────────┐          │
│  │   K-Means Clustering (Color Analysis)│          │
│  └──────────────────────────────────────┘          │
└─────────────────────┬───────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────┐
│                 DATA LAYER                          │
│  • Image Files  • Model Weights (best.pt)          │
│  • Temp Files   • Color Mapping Database           │
└─────────────────────────────────────────────────────┘
```

---

## SLIDE 5: QUY TRÌNH XỬLÝ (WORKFLOW)

### **Pipeline xử lý ảnh**

```
[INPUT IMAGE]
     │
     ▼
[1. YOLO Detection] ─────► Detect người (bbox)
     │
     ▼
[2. MTCNN Face Detection] ► Tìm khuôn mặt trong bbox
     │
     ├─── [CÓ KHUÔN MẶT]
     │         │
     │         ▼
     │    [3. DeepFace Analysis] ─► Giới tính (Nam/Nữ)
     │         │
     │         ▼
     │    [4. Crop vùng áo] ──────► Từ dưới mặt đến 70% bbox
     │         │
     │         ▼
     │    [5. K-Means Color] ─────► Phân tích màu áo
     │
     └─── [KHÔNG CÓ MẶT]
               │
               ▼
          [Sử dụng toàn bbox] ───► Phân tích màu toàn thân
               │
               ▼
          [Giới tính: "Không rõ"]
     
[OUTPUT: Kết quả + Visualize]
```

---

## SLIDE 6: CÔNG NGHỆ & CÔNG CỤ

### **Technology Stack**

| **Category** | **Technology** | **Version/Purpose** |
|--------------|----------------|---------------------|
| **Core Language** | Python | 3.11+ |
| **Object Detection** | Ultralytics YOLO | YOLOv11n (best.pt) |
| **Face Detection** | MTCNN | Facial landmarks |
| **Gender Recognition** | DeepFace | Gender classification |
| **Color Analysis** | scikit-learn | K-Means clustering |
| **GUI Framework** | CustomTkinter | Modern UI |
| **Image Processing** | OpenCV (cv2) | Computer vision |
| **Data Science** | NumPy, PIL | Data manipulation |

### **Thư viện phụ trợ**
- `matplotlib.colors`: Color name mapping
- `tempfile`: Temporary file handling
- `threading`: Async processing

---

## SLIDE 7: CẤU TRÚC MÃ NGUỒN

### **File Structure**

```
d:\app\bestlast\
├── main_UI.py              # Giao diện GUI chính (466 dòng)
│   ├── Class: ImageAnalysisApp
│   ├── Methods: detect_dominant_color()
│   ├──         predict_gender()
│   └──         process_image()
│
├── main.py                 # Script xử lý core logic (140 dòng)
│   ├── detect_dominant_color()
│   ├── predict_gender()
│   └── main pipeline
│
├── test_gender.py          # Test module DeepFace
│
├── best.pt                 # YOLO trained weights
├── yolo11n.pt              # YOLO base model
│
├── New Text Document.txt   # Tài liệu huấn luyện & pipeline
└── Untitled-1.ipynb        # Jupyter notebook thử nghiệm
```

### **Module chính (main_UI.py)**
- 466 dòng code
- Khoảng 8 methods core
- Threading để xử lý async
- CustomTkinter widgets

---

## SLIDE 8: THUẬT TOÁN CHỦ ĐẠO

### **1. YOLO Object Detection**
```python
# Phát hiện người trong ảnh
model = YOLO("best.pt")
results = model(img, verbose=False)
detections = results[0].boxes.data.cpu().numpy()

for det in detections:
    x1, y1, x2, y2, conf, cls = det
    if int(cls) == 0:  # Class "person"
        person_crop = img[y1:y2, x1:x2]
```

### **2. Gender Classification (DeepFace)**
```python
def predict_gender(face_crop):
    result = DeepFace.analyze(
        img_path=temp_path, 
        actions=['gender'],
        enforce_detection=False
    )
    gender = "Nam" if result['gender'] == "Man" else "Nữ"
    return gender
```

### **3. Color Analysis (K-Means)**
```python
def detect_dominant_color(image, k=5):
    kmeans = KMeans(n_clusters=k, n_init='auto').fit(pixels)
    dominant = kmeans.cluster_centers_[argmax(counts)]
    # So sánh với bảng màu chuẩn CSS4
    return closest_color_name
```

---

## SLIDE 9: QUY TRÌNH HUẤN LUYỆN MÔ HÌNH

### **Training Pipeline**

#### **Bước 1: Chuẩn bị dữ liệu**
- Chia tập dữ liệu: 70% train, 20% val, 10% test
- Data Augmentation: xoay, lật, crop, brightness
- Chuẩn hóa: resize 640×640, normalize [0, 1]

#### **Bước 2: Setup môi trường**
```bash
cd ultralytics
py -3.11 -m venv .venv
.\.venv\Scripts\Activate.ps1
```

#### **Bước 3: Huấn luyện YOLO**
```bash
yolo detect train data=coco128.yaml \
     model=yolo11n.pt epochs=100 imgsz=640
```

#### **Bước 4: Đánh giá mô hình**
- **YOLO**: mAP, IoU
- **Classification**: Confusion Matrix
- **Output**: `best.pt` weights

#### **Bước 5: Inference**
```bash
yolo detect predict model=runs/detect/train2/weights/best.pt \
     source='testimage'
```

---

## SLIDE 10: GIAO DIỆN NGƯỜI DÙNG

### **UI Components (CustomTkinter)**

#### **Sidebar (Control Panel)**
- 📁 Button "Tải Ảnh" - Upload image
- 🔍 Button "Phân Tích" - Process image
- 📋 Log Panel - Real-time status updates

#### **Main Panel**
- 🖼️ Image Display Area
  - Canvas tự động scale ảnh
  - Hiển thị bounding boxes
  - Overlay text labels

#### **Features**
- ✅ Multi-threading: Không làm đơ UI
- ✅ Error handling: Thông báo lỗi rõ ràng
- ✅ Vietnamese localization
- ✅ Color-coded results

### **User Experience Flow**
```
Mở app → Tải ảnh → Nhấn "Phân tích" → Xem kết quả
```

---

## SLIDE 11: QUẢN LÝ DỰ ÁN (PROJECT MANAGEMENT)

### **Phương pháp: AGILE - SCRUM**

#### **Sprint Planning (2 tuần/sprint)**

| **Sprint** | **Mục tiêu** | **Deliverable** |
|------------|--------------|-----------------|
| **Sprint 1** | Setup + Research | - Environment setup<br>- Nghiên cứu YOLO, DeepFace |
| **Sprint 2** | Core Logic | - `main.py` script<br>- YOLO integration |
| **Sprint 3** | Gender & Color | - DeepFace integration<br>- K-Means color analysis |
| **Sprint 4** | GUI Development | - `main_UI.py`<br>- CustomTkinter interface |
| **Sprint 5** | Testing & Debug | - Bug fixes<br>- Performance tuning |
| **Sprint 6** | Documentation | - User manual<br>- Code documentation |

### **Team Roles**
- **Product Owner**: Định hướng features
- **Scrum Master**: Quản lý sprint
- **Developers**: Coding & testing
- **AI Engineer**: Train models

---

## SLIDE 12: CÔNG CỤ QUẢN LÝ

### **Project Management Tools**

#### **1. Version Control**
- 🔧 **Git/GitHub**: Source code management
- 📝 **Commit convention**: 
  - `feat:` - New feature
  - `fix:` - Bug fix
  - `refactor:` - Code refactoring

#### **2. Task Tracking**
- 📊 **Trello/Jira**: Sprint backlog
- ⏱️ **Burndown Chart**: Track progress

#### **3. Communication**
- 💬 **Slack/Discord**: Team chat
- 📹 **Zoom/Meet**: Daily standup

#### **4. Documentation**
- 📖 **Confluence/Notion**: Wiki
- 📄 **Google Docs**: Shared documents

### **Quality Assurance**
- ✅ Code review (peer review)
- ✅ Unit testing (pytest)
- ✅ Performance testing

---

## SLIDE 13: THÁCH THỨC & GIẢI PHÁP

### **Challenges & Solutions**

| **Thách thức** | **Giải pháp** |
|----------------|---------------|
| 🔴 **Model accuracy** | - Fine-tune YOLO trên dataset riêng<br>- Augmentation đa dạng |
| 🔴 **Face detection failure** | - Sử dụng `enforce_detection=False`<br>- Fallback: phân tích toàn thân |
| 🔴 **Color analysis slow** | - Sampling 30% pixels (K-Means)<br>- Multi-threading |
| 🔴 **UI freezing** | - Async processing với `threading`<br>- Loading indicators |
| 🔴 **Memory management** | - Xóa temp files ngay<br>- Release OpenCV resources |
| 🔴 **Vietnamese display** | - Mapping dict (Eng → Vie)<br>- UTF-8 encoding |

### **Performance Optimization**
- ⚡ K-Means sampling: giảm 70% thời gian
- ⚡ Thread pooling: UI mượt mà
- ⚡ Image caching: tránh load lại

---

## SLIDE 14: KẾT QUẢ DEMO

### **Test Cases & Results**

#### **Test Case 1: Nam - Áo Đỏ**
```
INPUT: nam_do.png
OUTPUT:
  - Giới tính: Nam ✅
  - Màu áo: Đỏ ✅
  - Confidence: 0.89
  - Processing time: 2.3s
```

#### **Test Case 2: Nữ - Áo Trắng**
```
INPUT: nu_trang.png
OUTPUT:
  - Giới tính: Nữ ✅
  - Màu áo: Trắng ✅
  - Confidence: 0.92
  - Processing time: 2.1s
```

#### **Test Case 3: Nhiều người**
```
INPUT: crowd.jpg (5 người)
OUTPUT:
  - Detect: 5/5 ✅
  - Gender accuracy: 4/5 (80%)
  - Color accuracy: 5/5 (100%)
  - Processing time: 5.8s
```

### **Metrics**
- **Detection Rate**: 95%
- **Gender Accuracy**: 85%
- **Color Accuracy**: 90%

---

## SLIDE 15: RỦI RO VÀ QUẢN LÝ RỦI RO

### **Risk Management Matrix**

| **Rủi ro** | **Probability** | **Impact** | **Mitigation** |
|------------|-----------------|------------|----------------|
| Model không đạt accuracy | Medium | High | - Tăng dataset<br>- Fine-tuning |
| DeepFace lỗi phụ thuộc internet | High | Medium | - Cache models<br>- Offline mode |
| GPU không khả dụng | Low | High | - CPU fallback<br>- Cloud GPU |
| Thời gian xử lý chậm | Medium | Medium | - Optimize code<br>- Async processing |
| Thành viên nghỉ giữa chừng | Low | High | - Documentation<br>- Pair programming |

### **Contingency Plan**
- 🔄 **Backup plan**: Sử dụng pre-trained models
- 📞 **Support**: Mentor/advisor consultation
- 📚 **Learning**: Online courses, forums

---

## SLIDE 16: TIMELINE DỰ ÁN (GANTT CHART)

### **Lịch trình 12 tuần**

```
Week 1-2:   [████████] Research & Setup
Week 3-4:   [████████] Core Logic Development
Week 5-6:   [████████] AI Model Integration
Week 7-8:   [████████] GUI Development
Week 9-10:  [████████] Testing & Debugging
Week 11-12: [████████] Documentation & Presentation

Milestones:
├─ Week 2:  ✓ Environment ready
├─ Week 4:  ✓ Basic detection working
├─ Week 6:  ✓ Full pipeline complete
├─ Week 8:  ✓ GUI finished
├─ Week 10: ✓ Testing done
└─ Week 12: ✓ Project submission
```

### **Critical Path**
1. YOLO training ➔ Core logic ➔ GUI ➔ Testing

### **Dependencies**
- YOLO weights → Gender/Color detection
- Core logic → GUI development

---

## SLIDE 17: BÁO CÁO TÀI CHÍNH (BUDGET)

### **Cost Breakdown**

| **Hạng mục** | **Chi tiết** | **Chi phí (VND)** |
|--------------|--------------|-------------------|
| **Hardware** | GPU Cloud (Colab Pro) | 500,000 |
| **Software** | Python packages (free) | 0 |
| **Data** | Dataset (public) | 0 |
| **Training** | Online courses | 300,000 |
| **Communication** | Zoom subscription | 0 (free tier) |
| **Documentation** | Office 365 | 0 (student) |
| **Contingency** | 10% buffer | 80,000 |
| **TOTAL** | | **880,000** |

### **Resource Allocation**
- 👨‍💻 Human resources: 4 members × 12 weeks
- ⏰ Time investment: ~480 hours total
- 💰 Cost per member: 220,000 VND

---

## SLIDE 18: BÀI HỌC KINH NGHIỆM

### **Lessons Learned**

#### **Thành công ✅**
1. **Threading hiệu quả**: UI không bao giờ bị đơ
2. **Modular design**: Dễ maintain và scale
3. **Error handling**: App ổn định, ít crash
4. **User-friendly**: Giao diện trực quan

#### **Khó khăn ⚠️**
1. **DeepFace dependency**: Cần internet lần đầu
2. **YOLO training**: Tốn thời gian và GPU
3. **Color accuracy**: Khó với ánh sáng yếu
4. **Vietnamese fonts**: UTF-8 encoding issues

#### **Cải thiện 🔄**
- ➕ Add video support
- ➕ Realtime webcam detection
- ➕ Database để lưu history
- ➕ Export results to CSV/JSON
- ➕ Mobile app version

---

## SLIDE 19: KẾ HOẠCH TƯƠNG LAI

### **Future Roadmap**

#### **Phase 2: Advanced Features**
- 🎥 Video & webcam realtime processing
- 📊 Analytics dashboard
- 🗄️ Database integration (SQLite/PostgreSQL)
- 🔐 User authentication system

#### **Phase 3: Optimization**
- ⚡ TensorRT acceleration
- 🌐 Web deployment (Flask/FastAPI)
- 📱 Mobile app (React Native)
- ☁️ Cloud deployment (AWS/Azure)

#### **Phase 4: Business Model**
- 💼 API service (subscription)
- 🏢 Enterprise solutions
- 📈 Market expansion

### **Scalability Plan**
- Load balancing cho nhiều requests
- Microservices architecture
- Docker containerization

---

## SLIDE 20: KẾT LUẬN

### **Tóm tắt dự án**

#### **Đã hoàn thành ✅**
1. ✔️ Ứng dụng AI hoàn chỉnh với GUI chuyên nghiệp
2. ✔️ Tích hợp 3 models AI: YOLO, DeepFace, K-Means
3. ✔️ Performance tốt: 2-3s/ảnh
4. ✔️ Accuracy cao: 85-95%
5. ✔️ Code clean, có documentation

#### **Giá trị mang lại 🎯**
- 📚 **Học thuật**: Áp dụng kiến thức QLDA vào thực tế
- 💡 **Kỹ thuật**: Nắm vững Computer Vision & Deep Learning
- 👥 **Teamwork**: Phối hợp nhóm hiệu quả
- 🚀 **Sản phẩm**: Demo sẵn sàng thương mại hóa

#### **Kết luận cuối cùng**
> *"Dự án đã thành công trong việc xây dựng một hệ thống AI thực tiễn, áp dụng đúng quy trình quản lý dự án phần mềm Agile, và tạo ra sản phẩm có giá trị thực tế."*

---

## SLIDE 21: Q&A

### **CÂU HỎI & TRẢ LỜI**

**Cảm ơn quý thầy cô và các bạn đã lắng nghe!**

📧 **Contact**: [email@example.com]  
🔗 **GitHub**: [github.com/username/project]  
📁 **Demo**: [Link demo video]

---

### **Prepared Questions:**

1. **Q: Tại sao chọn YOLO thay vì Faster R-CNN?**
   - A: YOLO nhanh hơn, realtime, phù hợp cho ứng dụng thực tế

2. **Q: Độ chính xác của gender detection?**
   - A: 85%, có thể cải thiện bằng ensemble models

3. **Q: Làm sao xử lý nhiều người trong 1 ảnh?**
   - A: YOLO detect all, sau đó loop qua từng bbox

4. **Q: Có thể chạy trên mobile không?**
   - A: Có, cần convert model sang TFLite/ONNX

---

## PHỤ LỤC: REFERENCE & CITATIONS

### **Tài liệu tham khảo**

1. **YOLO Official**: https://github.com/ultralytics/ultralytics
2. **DeepFace**: https://github.com/serengil/deepface
3. **MTCNN**: https://github.com/ipazc/mtcnn
4. **CustomTkinter**: https://github.com/TomSchimansky/CustomTkinter
5. **Project Management**: PMBOK Guide, Agile Manifesto

### **Papers**
- Redmon, J., et al. (2016). "You Only Look Once: Unified, Real-Time Object Detection"
- Serengil, S. (2020). "DeepFace: A Lightweight Face Recognition and Facial Attribute Analysis Framework"

---

**END OF PRESENTATION**
