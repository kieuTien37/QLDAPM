using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace Buoi81150080144NguyenThuyKieu
{
    public class FormXoa : Form
    {
        // Đổi chuỗi kết nối theo máy bạn nếu cần
        private readonly string strCon =
            @"Data Source=BOP;Initial Catalog=buoi6_tranthingochuyen;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";
        private SqlConnection sqlCon;

        private Label lblTitle;
        private GroupBox grpList;
        private ListView lsvSV;
        private ColumnHeader colMaSV, colTenSV, colGioiTinh, colNgaySinh, colQueQuan, colMaLop;
        private Button btnXoa;    // dock ở đáy để luôn nhìn thấy
        private string _maSVSelected;

        public FormXoa()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Áp dụng 3 - Xóa dữ liệu (Parameter)";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(900, 480);

            lblTitle = new Label
            {
                Text = "XÓA DỮ LIỆU CÓ DÙNG PARAMETER",
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Gainsboro,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            Controls.Add(lblTitle);

            grpList = new GroupBox { Text = "Danh sách sinh viên:", Dock = DockStyle.Fill, Padding = new Padding(6) };
            lsvSV = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            lsvSV.SelectedIndexChanged += (s, e) =>
            {
                if (lsvSV.SelectedItems.Count == 0)
                {
                    _maSVSelected = null;
                    btnXoa.Enabled = false;
                }
                else
                {
                    _maSVSelected = lsvSV.SelectedItems[0].SubItems[0].Text.Trim();
                    btnXoa.Enabled = true;
                }
            };

            colMaSV = new ColumnHeader { Text = "Mã SV", Width = 110 };
            colTenSV = new ColumnHeader { Text = "Tên SV", Width = 220 };
            colGioiTinh = new ColumnHeader { Text = "Giới tính", Width = 80 };
            colNgaySinh = new ColumnHeader { Text = "Ngày sinh", Width = 110 };
            colQueQuan = new ColumnHeader { Text = "Quê quán", Width = 200 };
            colMaLop = new ColumnHeader { Text = "Mã lớp", Width = 100 };
            lsvSV.Columns.AddRange(new[] { colMaSV, colTenSV, colGioiTinh, colNgaySinh, colQueQuan, colMaLop });

            grpList.Controls.Add(lsvSV);
            Controls.Add(grpList);

            btnXoa = new Button { Text = "Xóa (Parameter)", Dock = DockStyle.Bottom, Height = 46, Enabled = false };
            btnXoa.Click += BtnXoa_Click;
            Controls.Add(btnXoa);

            Load += (s, e) => LoadList();
        }

        // ===== Helpers =====
        private void Open()
        {
            if (sqlCon == null) sqlCon = new SqlConnection(strCon);
            if (sqlCon.State == ConnectionState.Closed) sqlCon.Open();
        }
        private void CloseConn()
        {
            if (sqlCon != null && sqlCon.State == ConnectionState.Open) sqlCon.Close();
        }

        private void LoadList()
        {
            try
            {
                Open();
                using (var cmd = new SqlCommand(
                    "SELECT MaSV, TenSV, GioiTinh, NgaySinh, QueQuan, MaLop FROM dbo.SinhVien ORDER BY MaSV", sqlCon))
                using (var rd = cmd.ExecuteReader())
                {
                    lsvSV.Items.Clear();
                    while (rd.Read())
                    {
                        var item = new ListViewItem(rd.GetString(0));
                        item.SubItems.Add(rd.GetString(1));
                        item.SubItems.Add(rd.IsDBNull(2) ? "" : rd.GetString(2));
                        item.SubItems.Add(rd.IsDBNull(3) ? "" : rd.GetDateTime(3).ToString("dd-MM-yyyy"));
                        item.SubItems.Add(rd.IsDBNull(4) ? "" : rd.GetString(4));
                        item.SubItems.Add(rd.IsDBNull(5) ? "" : rd.GetString(5));
                        lsvSV.Items.Add(item);
                    }
                }
                _maSVSelected = null;
                btnXoa.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hiển thị: " + ex.Message);
            }
            finally { CloseConn(); }
        }

        // ===== DELETE with parameters =====
        private void BtnXoa_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_maSVSelected))
            {
                MessageBox.Show("Hãy chọn sinh viên cần xóa.");
                return;
            }

            var ok = MessageBox.Show($"Xóa sinh viên '{_maSVSelected}'?", "Xác nhận",
                                     MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (ok != DialogResult.Yes) return;

            try
            {
                Open();
                const string sql = "DELETE FROM dbo.SinhVien WHERE MaSV = @MaSV";
                using (var cmd = new SqlCommand(sql, sqlCon))
                {
                    cmd.Parameters.Add("@MaSV", SqlDbType.NVarChar, 20).Value = _maSVSelected;
                    int kq = cmd.ExecuteNonQuery();
                    MessageBox.Show(kq > 0 ? "Đã xóa thành công!" : "Không có dòng nào bị xóa.");
                }
                LoadList();
            }
            catch (SqlException ex) when (ex.Number == 547) // bị ràng buộc FK
            {
                MessageBox.Show("Không thể xóa do ràng buộc dữ liệu (khóa ngoại).", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa: " + ex.Message);
            }
            finally { CloseConn(); }
        }
    }
}
