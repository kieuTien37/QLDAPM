using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Buoi81150080144NguyenThuyKieu
{
    public class Form4 : Form   // code-only UI
    {
        // ===== CHUỖI KẾT NỐI =====
        private readonly string strCon =
            @"Data Source=BOP;Initial Catalog=buoi6_tranthingochuyen;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";

        private SqlConnection sqlCon;

        // ===== BIẾN TRẠNG THÁI =====
        private string _maSVOld = null; // giữ MaSV gốc đang chọn (phòng khi đổi mã)

        // ===== KHAI BÁO CONTROL =====
        private Label lblTitle;

        private GroupBox grpTop;
        private Label lblChonMaLop;
        private ComboBox cbMaLop;

        private GroupBox grpLeft;
        private ListView lsvDanhSach;
        private ColumnHeader colMaSV, colTenSV, colGioiTinh, colNgaySinh, colQueQuan, colMaLop;

        private GroupBox grpRight;
        private TextBox txtMaSV, txtTenSV, txtQueQuan, txtMaLop;
        private ComboBox cbGioiTinh;
        private DateTimePicker dtpNgaySinh;
        private Button btnSuaThongTin;

        public Form4()
        {
            InitializeComponent();
        }

        // ===== UI (thay Designer) =====
        private void InitializeComponent()
        {
            // Form
            Text = "Form4 - Sửa dữ liệu (Parameter)";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(900, 520);

            // Title
            lblTitle = new Label
            {
                Text = "SỬA DỮ LIỆU CÓ DÙNG PARAMETER",
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.Gainsboro
            };
            Controls.Add(lblTitle);

            // Top (chọn lớp)
            grpTop = new GroupBox { Dock = DockStyle.Top, Height = 70 };
            lblChonMaLop = new Label { Text = "Chọn mã lớp:", Location = new Point(20, 30) };
            cbMaLop = new ComboBox
            {
                Location = new Point(110, 26),
                Width = 260,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cbMaLop.SelectedIndexChanged += cbMaLop_SelectedIndexChanged;
            grpTop.Controls.Add(lblChonMaLop);
            grpTop.Controls.Add(cbMaLop);
            Controls.Add(grpTop);

            // Left (danh sách SV)
            grpLeft = new GroupBox
            {
                Text = "Danh sách sinh viên:",
                Location = new Point(10, 120),
                Size = new Size(520, 380)
            };
            lsvDanhSach = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            lsvDanhSach.SelectedIndexChanged += lsvDanhSach_SelectedIndexChanged;

            colMaSV = new ColumnHeader { Text = "Mã SV", Width = 80 };
            colTenSV = new ColumnHeader { Text = "Tên SV", Width = 150 };
            colGioiTinh = new ColumnHeader { Text = "Giới tính", Width = 75 };
            colNgaySinh = new ColumnHeader { Text = "Ngày sinh", Width = 100 };
            colQueQuan = new ColumnHeader { Text = "Quê quán", Width = 90 };
            colMaLop = new ColumnHeader { Text = "Mã lớp", Width = 70 };
            lsvDanhSach.Columns.AddRange(new[] { colMaSV, colTenSV, colGioiTinh, colNgaySinh, colQueQuan, colMaLop });

            grpLeft.Controls.Add(lsvDanhSach);
            Controls.Add(grpLeft);

            // Right (chi tiết + sửa)
            grpRight = new GroupBox
            {
                Text = "Thông tin sinh viên:",
                Location = new Point(540, 120),
                Size = new Size(350, 380)
            };

            int xLabel = 20, xInput = 120, y = 40, gap = 40, w = 200;

            grpRight.Controls.Add(new Label { Text = "Mã SV:", Location = new Point(xLabel, y) });
            txtMaSV = new TextBox { Location = new Point(xInput, y - 3), Width = w };
            y += gap;

            grpRight.Controls.Add(new Label { Text = "Tên SV:", Location = new Point(xLabel, y) });
            txtTenSV = new TextBox { Location = new Point(xInput, y - 3), Width = w };
            y += gap;

            grpRight.Controls.Add(new Label { Text = "Giới tính:", Location = new Point(xLabel, y) });
            cbGioiTinh = new ComboBox { Location = new Point(xInput, y - 3), Width = w, DropDownStyle = ComboBoxStyle.DropDownList };
            cbGioiTinh.Items.AddRange(new object[] { "Nam", "Nữ", "Khác" });
            y += gap;

            grpRight.Controls.Add(new Label { Text = "Ngày sinh:", Location = new Point(xLabel, y) });
            dtpNgaySinh = new DateTimePicker { Location = new Point(xInput, y - 3), Width = w, Format = DateTimePickerFormat.Short };
            y += gap;

            grpRight.Controls.Add(new Label { Text = "Quê quán:", Location = new Point(xLabel, y) });
            txtQueQuan = new TextBox { Location = new Point(xInput, y - 3), Width = w };
            y += gap;

            grpRight.Controls.Add(new Label { Text = "Mã lớp:", Location = new Point(xLabel, y) });
            txtMaLop = new TextBox { Location = new Point(xInput, y - 3), Width = w };
            y += gap + 10;

            btnSuaThongTin = new Button { Text = "Sửa (Parameter)", Location = new Point(xInput, y), Width = w };
            btnSuaThongTin.Click += btnSuaThongTin_Click;

            grpRight.Controls.AddRange(new Control[] { txtMaSV, txtTenSV, cbGioiTinh, dtpNgaySinh, txtQueQuan, txtMaLop, btnSuaThongTin });
            Controls.Add(grpRight);

            // Form load
            Load += Form4_Load;
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

        // ===== LOAD form =====
        private void Form4_Load(object sender, EventArgs e)
        {
            HienThiDSMaLop();
        }

        // ===== LOAD danh sách mã lớp =====
        private void HienThiDSMaLop()
        {
            try
            {
                MoKetNoi();
                using (var cmd = new SqlCommand("SELECT MaLop, TenLop FROM [dbo].[Lop] ORDER BY MaLop", sqlCon))
                using (var rd = cmd.ExecuteReader())
                {
                    cbMaLop.Items.Clear();
                    while (rd.Read())
                    {
                        string ma = rd.GetString(0);
                        string ten = rd.IsDBNull(1) ? "" : rd.GetString(1);
                        // hiện "Ma - Ten" nếu có tên, không thì chỉ mã
                        var display = string.IsNullOrWhiteSpace(ten) ? ma : $"{ma} - {ten}";
                        cbMaLop.Items.Add(display);
                    }
                }
                if (cbMaLop.Items.Count > 0) cbMaLop.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách lớp: " + ex.Message);
            }
            finally { DongKetNoi(); }
        }

        // ===== LOAD danh sách SV theo lớp (Parameter) =====
        private void HienThiDSSVTheoLop(string maLop)
        {
            try
            {
                MoKetNoi();
                var sql = @"SELECT MaSV, TenSV, GioiTinh, NgaySinh, QueQuan, MaLop
                            FROM [dbo].[SinhVien]
                            WHERE MaLop = @MaLop
                            ORDER BY MaSV";

                using (var cmd = new SqlCommand(sql, sqlCon))
                {
                    cmd.Parameters.Add("@MaLop", SqlDbType.NVarChar, 20).Value =
                        string.IsNullOrWhiteSpace(maLop) ? (object)DBNull.Value : maLop;

                    using (var rd = cmd.ExecuteReader())
                    {
                        lsvDanhSach.Items.Clear();
                        while (rd.Read())
                        {
                            var item = new ListViewItem(rd.GetString(0));
                            item.SubItems.Add(rd.GetString(1));
                            item.SubItems.Add(rd.IsDBNull(2) ? "" : rd.GetString(2));
                            item.SubItems.Add(rd.IsDBNull(3) ? "" : rd.GetDateTime(3).ToString("dd/MM/yyyy"));
                            item.SubItems.Add(rd.IsDBNull(4) ? "" : rd.GetString(4));
                            item.SubItems.Add(rd.IsDBNull(5) ? "" : rd.GetString(5));
                            lsvDanhSach.Items.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách SV: " + ex.Message);
            }
            finally { DongKetNoi(); }
        }

        // ===== EVENTS =====
        private void cbMaLop_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbMaLop.SelectedIndex == -1) return;

            // tách mã lớp an toàn
            var s = cbMaLop.SelectedItem.ToString();
            var maLop = s.Contains(" - ")
                ? s.Split(new[] { " - " }, StringSplitOptions.None)[0].Trim()
                : s.Trim();

            HienThiDSSVTheoLop(maLop);
            ClearInputs();
        }

        private void lsvDanhSach_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lsvDanhSach.SelectedItems.Count == 0) return;
            var lvi = lsvDanhSach.SelectedItems[0];

            _maSVOld = lvi.SubItems[0].Text;     // lưu khoá cũ
            txtMaSV.Text = lvi.SubItems[0].Text;
            txtTenSV.Text = lvi.SubItems[1].Text;

            cbGioiTinh.SelectedIndex = -1;
            cbGioiTinh.Text = lvi.SubItems[2].Text;

            var dt = lvi.SubItems[3].Text; // dd/MM/yyyy
            if (DateTime.TryParseExact(dt, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var parsed))
                dtpNgaySinh.Value = parsed;

            txtQueQuan.Text = lvi.SubItems[4].Text;
            txtMaLop.Text = lvi.SubItems[5].Text;
        }

        // ===== UPDATE (Parameter) =====
        private void btnSuaThongTin_Click(object sender, EventArgs e)
        {
            try
            {
                string maSV = txtMaSV.Text.Trim();
                string tenSV = txtTenSV.Text.Trim();
                string gioiTinh = cbGioiTinh.Text.Trim();
                DateTime? ngaySinh = dtpNgaySinh.Value.Date;
                string queQuan = txtQueQuan.Text.Trim();
                string maLop = txtMaLop.Text.Trim();

                if (string.IsNullOrEmpty(_maSVOld))
                {
                    MessageBox.Show("Hãy chọn 1 sinh viên trong danh sách để sửa.");
                    return;
                }
                if (string.IsNullOrEmpty(maSV) || string.IsNullOrEmpty(tenSV))
                {
                    MessageBox.Show("Mã SV và Tên SV không được để trống!");
                    return;
                }

                MoKetNoi();

                string sql = @"
                    UPDATE [dbo].[SinhVien]
                    SET    MaSV     = @MaSV,
                           TenSV    = @TenSV,
                           GioiTinh = @GioiTinh,
                           NgaySinh = @NgaySinh,
                           QueQuan  = @QueQuan,
                           MaLop    = @MaLop
                    WHERE  MaSV     = @MaSVOld;";

                using (var cmd = new SqlCommand(sql, sqlCon))
                {
                    cmd.Parameters.Add("@MaSV", SqlDbType.NVarChar, 20).Value = maSV;
                    cmd.Parameters.Add("@TenSV", SqlDbType.NVarChar, 100).Value = tenSV;
                    cmd.Parameters.Add("@GioiTinh", SqlDbType.NVarChar, 10).Value =
                        string.IsNullOrWhiteSpace(gioiTinh) ? (object)DBNull.Value : gioiTinh;
                    cmd.Parameters.Add("@NgaySinh", SqlDbType.Date).Value =
                        (object)ngaySinh ?? DBNull.Value;
                    cmd.Parameters.Add("@QueQuan", SqlDbType.NVarChar, 100).Value =
                        string.IsNullOrWhiteSpace(queQuan) ? (object)DBNull.Value : queQuan;
                    cmd.Parameters.Add("@MaLop", SqlDbType.NVarChar, 20).Value =
                        string.IsNullOrWhiteSpace(maLop) ? (object)DBNull.Value : maLop;

                    cmd.Parameters.Add("@MaSVOld", SqlDbType.NVarChar, 20).Value = _maSVOld;

                    int kq = cmd.ExecuteNonQuery();
                    if (kq > 0)
                    {
                        MessageBox.Show("Cập nhật (Parameter) thành công!");

                        // refresh theo lớp đang chọn
                        string currentMaLop = "";
                        if (cbMaLop.SelectedIndex >= 0)
                        {
                            var s = cbMaLop.SelectedItem.ToString();
                            currentMaLop = s.Contains(" - ")
                                ? s.Split(new[] { " - " }, StringSplitOptions.None)[0].Trim()
                                : s.Trim();
                        }
                        else
                        {
                            currentMaLop = maLop;
                        }
                        HienThiDSSVTheoLop(currentMaLop);

                        // nếu đổi mã, cập nhật khoá cũ
                        _maSVOld = maSV;
                    }
                    else
                    {
                        MessageBox.Show("Không có dòng nào được cập nhật.");
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("SQL lỗi: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khác: " + ex.Message);
            }
            finally { DongKetNoi(); }
        }

        private void ClearInputs()
        {
            _maSVOld = null;
            txtMaSV.Clear();
            txtTenSV.Clear();
            cbGioiTinh.SelectedIndex = -1;
            dtpNgaySinh.Value = DateTime.Today;
            txtQueQuan.Clear();
            txtMaLop.Clear();
        }
    }
}
