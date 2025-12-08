import customtkinter as ctk
import tkinter as tk
from tkinter import filedialog, messagebox
import cv2
import numpy as np
from PIL import Image, ImageTk
from ultralytics import YOLO
from sklearn.cluster import KMeans
from deepface import DeepFace
from mtcnn import MTCNN
import tempfile
import os
import warnings
import matplotlib.colors as mcolors
import threading

# Tắt cảnh báo từ các thư viện
warnings.filterwarnings("ignore")

# Cấu hình CustomTkinter
ctk.set_appearance_mode("System")  # "System", "Dark", "Light"
ctk.set_default_color_theme("blue")

class ImageAnalysisApp(ctk.CTk):
    """Ứng dụng GUI phân tích ảnh sử dụng CustomTkinter và các mô hình AI."""

    def __init__(self):
        super().__init__()

        # --- Cấu hình Cửa sổ ---
        self.title("AI Image Analysis - Nhận Dạng Nhân Vật & Màu Áo")
        self.geometry("1100x700")
        self.grid_columnconfigure(1, weight=1)
        self.grid_rowconfigure(0, weight=1)

        # --- Khởi tạo Model AI và Cấu hình CV ---
        try:
            # Tải mô hình YOLO để phát hiện người
            self.yolo_model = YOLO("best.pt")
            # Tải MTCNN để phát hiện khuôn mặt (cho DeepFace)
            self.face_detector = MTCNN()
        except Exception as e:
            messagebox.showerror("Lỗi Khởi tạo AI", f"Không thể tải mô hình AI. Vui lòng kiểm tra file model và kết nối mạng. Lỗi: {e}")
            self.destroy()
            return

        # Chuyển dict màu chuẩn sang RGB
        self.COLOR_NAMES = {}
        for name, hex in mcolors.CSS4_COLORS.items():
            rgb = np.array(mcolors.to_rgb(hex)) * 255
            self.COLOR_NAMES[name] = rgb.astype(int)

        # --- Biến trạng thái ---
        self.image_path = None
        self.original_image_cv = None
        self.processed_image_tk = None
        self.is_processing = False

        # --- Tạo Layout ---
        self.create_sidebar()
        self.create_main_panel()
        
        # Hiển thị thông báo khởi động ban đầu
        self.update_log("Hệ thống AI đã sẵn sàng. Vui lòng tải ảnh.")

    # =========================================================================
    # CÁC HÀM XỬ LÝ DỮ LIỆU CHUYÊN SÂU (Core Logic)
    # =========================================================================

    def detect_dominant_color(self, image, k=5):
        """Nhận diện màu chi tiết dựa trên KMeans và so sánh với bảng màu chuẩn"""
        if image is None or image.size == 0:
            return "Không rõ"
        
        # Chỉ lấy 30% pixel ngẫu nhiên để tăng tốc độ K-Means
        h, w, _ = image.shape
        sampling_ratio = 0.3
        num_pixels = h * w
        sample_size = int(num_pixels * sampling_ratio)
        
        if num_pixels < 1000: # Xử lý ảnh quá nhỏ
             sample_size = num_pixels

        img_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB).reshape((-1, 3))
        
        if sample_size < k:
            return "Quá nhỏ"
            
        # Lấy mẫu ngẫu nhiên
        indices = np.random.choice(img_rgb.shape[0], sample_size, replace=False)
        sampled_pixels = img_rgb[indices]

        try:
            kmeans = KMeans(n_clusters=k, n_init='auto', random_state=42).fit(sampled_pixels)
            cluster_centers = kmeans.cluster_centers_.astype(int)
            counts = np.bincount(kmeans.labels_)
            dominant = cluster_centers[np.argmax(counts)]
        except Exception as e:
            print(f"Lỗi K-Means: {e}")
            return "Không rõ"

        # So sánh với bảng màu chuẩn
        min_dist = float('inf')
        closest_color_name = "Không rõ"
        
        # Danh sách màu chuẩn tiếng Việt
        color_map_vn = {
            "white": "Trắng", "black": "Đen", "red": "Đỏ", "blue": "Xanh biển",
            "green": "Xanh lá", "yellow": "Vàng", "purple": "Tím", "pink": "Hồng",
            "orange": "Cam", "brown": "Nâu", "gray": "Xám", "cyan": "Xanh lơ",
            "magenta": "Đỏ tía"
        }

        for name, rgb in self.COLOR_NAMES.items():
            dist = np.linalg.norm(dominant - rgb)
            if dist < min_dist:
                min_dist = dist
                closest_color_name = color_map_vn.get(name.lower(), name)
        
        return closest_color_name

    def predict_gender(self, face_crop):
        """Nhận diện giới tính bằng DeepFace"""
        
        # DeepFace cần lưu tạm file để phân tích
        with tempfile.NamedTemporaryFile(suffix=".jpg", delete=False) as tmp:
            temp_path = tmp.name
            cv2.imwrite(temp_path, face_crop)
        
        gender = "Không rõ"
        try:
            # Ngưỡng phát hiện DeepFace thấp hơn để tăng khả năng thành công
            result = DeepFace.analyze(img_path=temp_path, actions=['gender'], enforce_detection=False)
            if isinstance(result, list): result = result[0]
            
            # Xử lý kết quả trả về từ DeepFace (có thể là dict hoặc string)
            gender_data = result.get("gender")
            if isinstance(gender_data, dict):
                # Lấy giới tính có tỷ lệ tin cậy cao nhất
                predicted_gender = max(gender_data, key=gender_data.get)
            else:
                predicted_gender = str(gender_data)
            
            # Dịch sang tiếng Việt
            gender = "Nam" if predicted_gender.lower() == "man" or predicted_gender.lower() == "male" else "Nữ"
            
        except Exception as e:
            # print(f"Lỗi DeepFace: {e}")
            pass # Bỏ qua lỗi nếu không tìm thấy mặt hoặc DeepFace thất bại
        finally:
            os.remove(temp_path)
            
        return gender

    def process_image(self):
        """Hàm chính xử lý ảnh và tạo kết quả trực quan."""
        if not self.image_path:
            messagebox.showerror("Lỗi", "Vui lòng tải lên một hình ảnh trước.")
            return

        if self.is_processing:
            self.update_log("Đang xử lý. Vui lòng đợi...")
            return

        self.is_processing = True
        self.process_button.configure(state="disabled", text="Đang Xử Lý...")
        self.update_log("Bắt đầu phân tích hình ảnh...")

        # Chạy xử lý nặng trong luồng riêng để UI không bị đóng băng
        threading.Thread(target=self._run_analysis_in_thread, daemon=True).start()

    def _run_analysis_in_thread(self):
        try:
            # Đọc lại ảnh gốc bằng CV2
            img = cv2.imread(self.image_path)
            if img is None:
                raise ValueError("Không đọc được ảnh, kiểm tra đường dẫn!")
            
            # Chuẩn bị ảnh cho kết quả vẽ
            img_result = img.copy()
            text_results = []
            
            # 1. Phát hiện khuôn mặt (MTCNN)
            faces = self.face_detector.detect_faces(img)

            # 2. Phát hiện người (YOLO)
            results = self.yolo_model(img, verbose=False)
            detections = results[0].boxes.data.cpu().numpy()

            output = []

            for i, det in enumerate(detections):
                x1, y1, x2, y2, conf, cls = det
                if int(cls) != 0:  # Chỉ xử lý class 'person' (cls=0)
                    continue
                x1, y1, x2, y2 = map(int, [x1, y1, x2, y2])
                
                person_crop = img[y1:y2, x1:x2]
                if person_crop is None or person_crop.size == 0:
                    continue

                # --- Tìm khuôn mặt liên quan trong bbox người ---
                face_bbox = None
                for f in faces:
                    fx, fy, fw, fh = f['box']
                    # Kiểm tra xem face bbox có nằm trong person bbox không
                    if fx >= x1 and fy >= y1 and fx + fw <= x2 and fy + fh <= y2:
                        face_bbox = (fx, fy, fw, fh)
                        break

                gender = "Không rõ"
                color = "Không rõ"
                shirt_box = (x1, y1, x2, y2) # Mặc định là toàn bộ bbox
                
                if face_bbox:
                    fx, fy, fw, fh = face_bbox
                    # Crop mặt cho DeepFace
                    face_crop = img[fy:fy+fh, fx:fx+fw]
                    gender = self.predict_gender(face_crop)
                    
                    # Xác định vùng áo: từ dưới mặt xuống
                    shirt_y1 = fy + fh + int(0.1 * fh) # 10% chiều cao mặt
                    shirt_y2 = y1 + int(0.7 * (y2 - y1)) # 70% chiều cao người
                    
                    # Đảm bảo vùng crop hợp lệ
                    if shirt_y1 < shirt_y2:
                        shirt_crop = img[shirt_y1:shirt_y2, x1:x2]
                        color = self.detect_dominant_color(shirt_crop)
                        shirt_box = (x1, shirt_y1, x2, shirt_y2)
                    else:
                        # Nếu không xác định được vùng áo rõ ràng, dùng toàn thân
                        color = self.detect_dominant_color(person_crop)
                        
                else:
                    # Nếu không tìm thấy mặt, dùng toàn bộ bbox người để dự đoán màu áo
                    color = self.detect_dominant_color(person_crop)
                
                output.append({
                    "id": i + 1,
                    "giới_tính": gender,
                    "màu_áo": color,
                    "bbox": (x1, y1, x2, y2),
                    "face_box": face_bbox,
                    "shirt_box": shirt_box
                })
            
            # --- Vẽ kết quả lên ảnh ---
            for o in output:
                x1, y1, x2, y2 = o['bbox']
                
                # 1. Vẽ Bounding Box Người (Xanh Lá)
                cv2.rectangle(img_result, (x1, y1), (x2, y2), (0, 255, 0), 2)
                
                # 2. Tạo nhãn
                text = f"{o['giới_tính']}, Áo {o['màu_áo']}"
                
                # 3. Vẽ Bbox Áo (Hồng)
                sx1, sy1, sx2, sy2 = o['shirt_box']
                # cv2.rectangle(img_result, (sx1, sy1), (sx2, sy2), (255, 0, 255), 2)
                
                # 4. Vẽ Bbox Khuôn Mặt (Xanh Dương)
                if o['face_box']:
                    fx, fy, fw, fh = o['face_box']
                    cv2.rectangle(img_result, (fx, fy), (fx + fw, fy + fh), (255, 0, 0), 2)

                # 5. Đặt Text Label
                text_size, _ = cv2.getTextSize(text, cv2.FONT_HERSHEY_SIMPLEX, 0.7, 2)
                text_w, text_h = text_size
                # Background cho text
                # cv2.rectangle(img_result, (x1, y1 - text_h - 10), (x1 + text_w + 10, y1), (0, 255, 0), -1)
                # cv2.putText(img_result, text, (x1 + 5, y1 - 5), cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 0, 0), 2)
                
                # Thêm vào kết quả text
                text_results.append(f"Người {o['id']}: Giới tính: {o['giới_tính']}, Màu áo: {o['màu_áo']}")
                
            self.update_log(f"Phân tích hoàn tất. Tìm thấy {len(output)} người.")
            self.display_image_result(img_result)
            self.display_text_results(text_results)

        except Exception as e:
            self.update_log(f"Đã xảy ra lỗi trong quá trình xử lý: {e}", is_error=True)
            messagebox.showerror("Lỗi Xử Lý", f"Đã xảy ra lỗi: {e}")
        finally:
            self.is_processing = False
            self.process_button.configure(state="normal", text="Xử Lý Ảnh")

    # =========================================================================
    # THIẾT LẬP GIAO DIỆN (GUI)
    # =========================================================================

    def create_sidebar(self):
        """Tạo khung điều khiển bên trái."""
        self.sidebar_frame = ctk.CTkFrame(self, width=250, corner_radius=10)
        self.sidebar_frame.grid(row=0, column=0, rowspan=4, sticky="nsew", padx=10, pady=10)
        self.sidebar_frame.grid_rowconfigure(5, weight=1)

        # Tiêu đề
        self.logo_label = ctk.CTkLabel(self.sidebar_frame, text="AI Vision Tool", font=ctk.CTkFont(size=20, weight="bold"))
        self.logo_label.grid(row=0, column=0, padx=20, pady=(20, 10))

        # Nút Tải Ảnh
        self.upload_button = ctk.CTkButton(self.sidebar_frame, text="Tải Lên Hình Ảnh", command=self.upload_image, 
                                           height=40, font=ctk.CTkFont(size=14, weight="bold"), fg_color="#1F538D")
        self.upload_button.grid(row=1, column=0, padx=20, pady=10)
        
        # Nút Xử Lý Ảnh
        self.process_button = ctk.CTkButton(self.sidebar_frame, text="Xử Lý Ảnh", command=self.process_image,
                                            height=40, font=ctk.CTkFont(size=14, weight="bold"), state="disabled",
                                            fg_color="#3B82F6", hover_color="#2563EB")
        self.process_button.grid(row=2, column=0, padx=20, pady=(10, 30))

        # Khung kết quả text
        self.result_label = ctk.CTkLabel(self.sidebar_frame, text="KẾT QUẢ PHÂN TÍCH:", font=ctk.CTkFont(size=14, weight="bold"))
        self.result_label.grid(row=3, column=0, padx=20, pady=(10, 0), sticky="w")
        
        self.result_textbox = ctk.CTkTextbox(self.sidebar_frame, width=220, height=200, corner_radius=10)
        self.result_textbox.grid(row=4, column=0, padx=10, pady=10, sticky="nsew")
        self.result_textbox.insert("0.0", "--- Chờ kết quả ---")
        self.result_textbox.configure(state="disabled")

        # Khung log
        self.log_label = ctk.CTkLabel(self.sidebar_frame, text="LOG HỆ THỐNG:", font=ctk.CTkFont(size=14, weight="bold"))
        self.log_label.grid(row=5, column=0, padx=20, pady=(10, 0), sticky="sw")
        
        self.log_textbox = ctk.CTkTextbox(self.sidebar_frame, width=220, height=100, corner_radius=10)
        self.log_textbox.grid(row=6, column=0, padx=10, pady=10, sticky="s")
        self.log_textbox.insert("0.0", "Sẵn sàng...")
        self.log_textbox.configure(state="disabled")

    def create_main_panel(self):
        """Tạo khung hiển thị ảnh bên phải."""
        self.main_frame = ctk.CTkFrame(self, corner_radius=10)
        self.main_frame.grid(row=0, column=1, sticky="nsew", padx=(0, 10), pady=10)
        self.main_frame.grid_rowconfigure(0, weight=1)
        self.main_frame.grid_columnconfigure(0, weight=1)

        self.image_display_frame = ctk.CTkFrame(self.main_frame, fg_color="transparent")
        self.image_display_frame.grid(row=0, column=0, sticky="nsew", padx=10, pady=10)
        self.image_display_frame.grid_rowconfigure(0, weight=1)
        self.image_display_frame.grid_columnconfigure(0, weight=1)

        # Nhãn hiển thị ảnh
        self.image_label = ctk.CTkLabel(self.image_display_frame, text="TẢI ẢNH LÊN ĐỂ BẮT ĐẦU", 
                                        font=ctk.CTkFont(size=24, weight="bold"), 
                                        text_color="#A0A0A0")
        self.image_label.grid(row=0, column=0, sticky="nsew")

        # Bật/tắt chế độ tự động điều chỉnh kích thước ảnh
        self.main_frame.bind("<Configure>", self.on_frame_resize)


    # =========================================================================
    # CÁC HÀM TƯƠNG TÁC GIAO DIỆN
    # =========================================================================

    def update_log(self, message, is_error=False):
        """Cập nhật nhật ký hệ thống."""
        self.log_textbox.configure(state="normal")
        color = "red" if is_error else "#00A0A0"
        self.log_textbox.insert("end", f"\n> {message}", "color_tag")
        self.log_textbox.tag_config("color_tag", foreground=color)
        self.log_textbox.see("end")
        self.log_textbox.configure(state="disabled")

    def upload_image(self):
        """Mở hộp thoại để chọn và tải ảnh."""
        file_path = filedialog.askopenfilename(
            filetypes=[("Image files", "*.png;*.jpg;*.jpeg")]
        )
        if file_path:
            self.image_path = file_path
            self.update_log(f"Đã tải ảnh: {os.path.basename(file_path)}")
            self.process_button.configure(state="normal")
            
            # Hiển thị ảnh gốc trước
            img_cv = cv2.imread(self.image_path)
            self.display_image_result(img_cv, is_original=True)
            self.result_textbox.configure(state="normal")
            self.result_textbox.delete("0.0", "end")
            self.result_textbox.insert("0.0", "Đã tải ảnh. Bấm 'Xử Lý Ảnh' để tiếp tục.")
            self.result_textbox.configure(state="disabled")


    def display_image_result(self, img_cv, is_original=False):
        """Chuyển ảnh CV2 sang CTkImage và hiển thị, có điều chỉnh kích thước."""
        
        # Chuyển BGR sang RGB
        img_rgb = cv2.cvtColor(img_cv, cv2.COLOR_BGR2RGB)
        
        # Chuyển mảng NumPy sang đối tượng PIL Image
        img_pil = Image.fromarray(img_rgb)
        
        # Điều chỉnh kích thước ảnh để vừa với khung
        self.resize_image_for_display(img_pil)
        
        if is_original:
            self.image_label.configure(text="")
            
    def resize_image_for_display(self, img_pil):
        """Điều chỉnh kích thước ảnh (PIL) để vừa với khung hình hiện tại."""
        
        if not self.image_display_frame.winfo_exists():
            return
            
        frame_width = self.image_display_frame.winfo_width() - 20
        frame_height = self.image_display_frame.winfo_height() - 20

        if frame_width <= 0 or frame_height <= 0:
            # Trường hợp khởi tạo, lấy kích thước mặc định
            frame_width = 800
            frame_height = 600
        
        # Giữ tỷ lệ khung hình
        original_width, original_height = img_pil.size
        
        ratio_w = frame_width / original_width
        ratio_h = frame_height / original_height
        
        ratio = min(ratio_w, ratio_h)
        
        new_width = int(original_width * ratio)
        new_height = int(original_height * ratio)
        
        # Đảm bảo ảnh không bị phóng to quá mức so với ảnh gốc
        if ratio > 1:
            new_width = original_width
            new_height = original_height

        img_resized = img_pil.resize((new_width, new_height), Image.Resampling.LANCZOS)
        
        self.processed_image_tk = ImageTk.PhotoImage(img_resized)
        
        self.image_label.configure(image=self.processed_image_tk, text="")
        self.image_label.image = self.processed_image_tk # Giữ tham chiếu

    def on_frame_resize(self, event):
        """Xử lý sự kiện khi khung hình thay đổi kích thước."""
        if self.processed_image_tk and self.image_path:
            # Tải lại ảnh (hoặc ảnh đã xử lý) để điều chỉnh kích thước
            try:
                img_cv = cv2.imread(self.image_path)
                if img_cv is not None:
                    # Chạy lại logic hiển thị để điều chỉnh kích thước
                    self.display_image_result(img_cv)
            except Exception as e:
                print(f"Lỗi khi resize ảnh: {e}")

    def display_text_results(self, results_list):
        """Hiển thị kết quả dạng văn bản trong textbox bên lề."""
        self.result_textbox.configure(state="normal")
        self.result_textbox.delete("0.0", "end")
        
        if not results_list:
            self.result_textbox.insert("0.0", "Không tìm thấy người nào trong ảnh.")
        else:
            self.result_textbox.insert("0.0", "--- KẾT QUẢ CHI TIẾT ---\n")
            for result in results_list:
                self.result_textbox.insert("end", result + "\n")
        
        self.result_textbox.configure(state="disabled")

if __name__ == "__main__":
    app = ImageAnalysisApp()
    if not hasattr(app, 'yolo_model'):
        exit() 
    app.mainloop()