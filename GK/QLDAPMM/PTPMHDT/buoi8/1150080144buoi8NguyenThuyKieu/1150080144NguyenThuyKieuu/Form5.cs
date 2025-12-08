using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace Buoi81150080144NguyenThuyKieu
{
    public class Form5 : Form
    {
        // ===== CHUỖI KẾT NỐI (sửa nếu cần) =====
        private readonly string strCon =
            @"Data Source=BOP;Initial Catalog=buoi6_tranthingochuyen;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";

        private SqlConnection sqlCon;

        // ===== UI =====
        private Label lblTitle;
        private GroupBox grpList;
        private ListView lsvDanhSach;
        private ColumnHeader colMaSV, colTenSV, colGioiTinh, colNgaySinh, colQueQuan, colMaLop;
        private Button btnXoa;            // Dock ở đáy form

        // ===== STATE =====
        private string _maSVSelected = null;

        public Form5()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Xóa dữ liệu";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(900, 480);

            // Tiêu đề
            lblTitle = new Label
            {
                Text = "XÓA DỮ LIỆU",
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Gainsboro,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            Controls.Add(lblTitle);

            // Danh sách
            grpList = new GroupBox { Text = "Danh sách sinh viên:", Dock = DockStyle.Fill, Padding = new Padding(6) };
            lsvDanhSach = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            lsvDanhSach.SelectedIndexChanged += lsvDanhSach_SelectedIndexChanged;

            colMaSV = new ColumnHeader { Text = "Mã SV", Width = 110 };
            colTenSV = new ColumnHeader { Text = "Tên SV", Width = 200 };
            colGioiTinh = new ColumnHeader { Text = "Giới tính", Width = 80 };
            colNgaySinh = new ColumnHeader { Text = "Ngày sinh", Width = 110 };
            colQueQuan = new ColumnHeader { Text = "Quê quán", Width = 180 };
            colMaLop = new ColumnHeader { Text = "Mã lớp", Width = 100 };
            lsvDanhSach.Columns.AddRange(new[] { colMaSV, colTenSV, colGioiTinh, colNgaySinh, colQueQuan, colMaLop });

            grpList.Controls.Add(lsvDanhSach);
            Controls.Add(grpList);

            // Nút xóa – DƯỚI CÙNG
            btnXoa = new Button
            {
                Text = "Xóa dữ liệu",
                Dock = DockStyle.Bottom,   // luôn hiển thị ở đáy
                Height = 48,
                Enabled = false
            };
            btnXoa.Click += btnXoa_Click;
            Controls.Add(btnXoa);

            // Sự kiện load
            Load += Form5_Load;
        }

        // ===== DB helpers =====
        private void MoKetNoi()
        {
            if (sqlCon == null) sqlCon = new SqlConnection(strCon);
            if (sqlCon.State == ConnectionState.Closed) sqlCon.Open();
        }
        private void DongKetNoi()
        {
            if (sqlCon != null && sqlCon.State == ConnectionState.Open) sqlCon.Close();
        }

        // ===== Load form / danh sách =====
        private void Form5_Load(object sender, EventArgs e) => HienThiDSSinhVien();

        private void HienThiDSSinhVien()
        {
            try
            {
                MoKetNoi();
                using (var cmd = new SqlCommand(
                    "SELECT MaSV, TenSV, GioiTinh, NgaySinh, QueQuan, MaLop FROM [dbo].[SinhVien] ORDER BY MaSV", sqlCon))
                using (var rd = cmd.ExecuteReader())
                {
                    lsvDanhSach.Items.Clear();
                    while (rd.Read())
                    {
                        var item = new ListViewItem(rd.GetString(0));
                        item.SubItems.Add(rd.GetString(1));
                        item.SubItems.Add(rd.IsDBNull(2) ? "" : rd.GetString(2));
                        item.SubItems.Add(rd.IsDBNull(3) ? "" : rd.GetDateTime(3).ToString("dd-MM-yyyy"));
                        item.SubItems.Add(rd.IsDBNull(4) ? "" : rd.GetString(4));
                        item.SubItems.Add(rd.IsDBNull(5) ? "" : rd.GetString(5));
                        lsvDanhSach.Items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hiển thị: " + ex.Message);
            }
            finally { DongKetNoi(); }
        }

        // ===== Chọn dòng =====
        private void lsvDanhSach_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lsvDanhSach.SelectedItems.Count == 0)
            {
                _maSVSelected = null;
                btnXoa.Enabled = false;
                return;
            }

            _maSVSelected = lsvDanhSach.SelectedItems[0].SubItems[0].Text.Trim();
            btnXoa.Enabled = true;
        }

        // ===== Xóa (Parameter) =====
        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_maSVSelected))
            {
                MessageBox.Show("Bạn chưa chọn sinh viên để xóa.");
                return;
            }

            var confirm = MessageBox.Show(
                $"Bạn có chắc muốn xóa sinh viên '{_maSVSelected}' không?",
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            try
            {
                MoKetNoi();
                const string sql = "DELETE FROM [dbo].[SinhVien] WHERE MaSV = @MaSV";
                using (var cmd = new SqlCommand(sql, sqlCon))
                {
                    cmd.Parameters.Add("@MaSV", SqlDbType.NVarChar, 20).Value = _maSVSelected;
                    int kq = cmd.ExecuteNonQuery();
                    if (kq > 0)
                    {
                        MessageBox.Show("Xóa dữ liệu thành công!");
                        HienThiDSSinhVien();
                        _maSVSelected = null;
                        btnXoa.Enabled = false;
                    }
                    else
                    {
                        MessageBox.Show("Không có dòng nào bị xóa.");
                    }
                }
            }
            catch (SqlException ex) when (ex.Number == 547) // FK constraint
            {
                MessageBox.Show("Không thể xóa do ràng buộc khóa ngoại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa: " + ex.Message);
            }
            finally { DongKetNoi(); }
        }
    }
}
