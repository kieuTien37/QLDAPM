using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace 1150080144NguyenThuyKieu
{
    public class Form3 : Form
    {
        // ===== CHUỖI KẾT NỐI =====
        private readonly string strCon =
            @"Data Source=BOP;Initial Catalog=buoi6_tranthingochuyen;Integrated Security=True;Encrypt=False";

        private SqlConnection sqlCon;

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

        public Form3()
        {
            InitializeComponent();
        }

        // ===== UI (thay cho Designer) =====
        private void InitializeComponent()
        {
            // Form
            Text = "Sửa dữ liệu";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(900, 520);

            // Title
            lblTitle = new Label
            {
                Text = "Sửa dữ liệu không dùng Parameter",
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.Gainsboro
            };
            Controls.Add(lblTitle);

            // Top choose class
            grpTop = new GroupBox { Dock = DockStyle.Top, Height = 70 };
            lblChonMaLop = new Label { Text = "Chọn mã lớp:", Location = new Point(20, 30) };
            cbMaLop = new ComboBox
            {
                Location = new Point(110, 26),
                Width = 220,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cbMaLop.SelectedIndexChanged += cbMaLop_SelectedIndexChanged;
            grpTop.Controls.Add(lblChonMaLop);
            grpTop.Controls.Add(cbMaLop);
            Controls.Add(grpTop);

            // Left list
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

            // Right panel
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

            btnSuaThongTin = new Button { Text = "Sửa thông tin", Location = new Point(xInput, y), Width = w };
            btnSuaThongTin.Click += btnSuaThongTin_Click;
            grpRight.Controls.AddRange(new Control[] { txtMaSV, txtTenSV, cbGioiTinh, dtpNgaySinh, txtQueQuan, txtMaLop, btnSuaThongTin });
            Controls.Add(grpRight);

            Load += Form1_Load;
        }

        // ===== DB helpers =====
        private void MoKetNoi()
        {
            if (sqlCon == null) sqlCon = new SqlConnection(strCon);
            if (sqlCon.State == System.Data.ConnectionState.Closed) sqlCon.Open();
        }
        private void DongKetNoi()
        {
            if (sqlCon != null && sqlCon.State == System.Data.ConnectionState.Open) sqlCon.Close();
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
                        cbMaLop.Items.Add($"{ma} - {ten}");
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

        // ===== LOAD danh sách SV theo lớp =====
        private void HienThiDSSVTheoLop(string maLop)
        {
            try
            {
                MoKetNoi();
                string sql = "SELECT MaSV, TenSV, GioiTinh, NgaySinh, QueQuan, MaLop " +
                             "FROM [dbo].[SinhVien] WHERE MaLop = '" + maLop + "' ORDER BY MaSV"; // theo đề: không dùng parameter

                using (var cmd = new SqlCommand(sql, sqlCon))
                using (var rd = cmd.ExecuteReader())
                {
                    lsvDanhSach.Items.Clear();
                    while (rd.Read())
                    {
                        string maSV = rd.GetString(0);
                        string tenSV = rd.GetString(1);
                        string gt = rd.IsDBNull(2) ? "" : rd.GetString(2);
                        string ns = rd.IsDBNull(3) ? "" : rd.GetDateTime(3).ToString("dd/MM/yyyy");
                        string qq = rd.IsDBNull(4) ? "" : rd.GetString(4);
                        string ml = rd.IsDBNull(5) ? "" : rd.GetString(5);

                        var item = new ListViewItem(maSV);
                        item.SubItems.Add(tenSV);
                        item.SubItems.Add(gt);
                        item.SubItems.Add(ns);
                        item.SubItems.Add(qq);
                        item.SubItems.Add(ml);
                        lsvDanhSach.Items.Add(item);
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
        private void Form1_Load(object sender, EventArgs e)
        {
            HienThiDSMaLop();
        }

        private void cbMaLop_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbMaLop.SelectedIndex == -1) return;
            string left = cbMaLop.SelectedItem.ToString();
            string maLop = left.Split('-')[0].Trim();
            HienThiDSSVTheoLop(maLop);
            ClearInputs();
        }

        private void lsvDanhSach_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lsvDanhSach.SelectedItems.Count == 0) return;
            var lvi = lsvDanhSach.SelectedItems[0];

            txtMaSV.Text = lvi.SubItems[0].Text;
            txtTenSV.Text = lvi.SubItems[1].Text;
            cbGioiTinh.SelectedIndex = -1;
            cbGioiTinh.Text = lvi.SubItems[2].Text;

            // dd/MM/yyyy
            var dt = lvi.SubItems[3].Text;
            DateTime parsed;
            if (DateTime.TryParseExact(dt, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out parsed))
                dtpNgaySinh.Value = parsed;

            txtQueQuan.Text = lvi.SubItems[4].Text;
            txtMaLop.Text = lvi.SubItems[5].Text;
        }

        private void btnSuaThongTin_Click(object sender, EventArgs e)
        {
            try
            {
                string maSV = txtMaSV.Text.Trim();
                string tenSV = txtTenSV.Text.Trim();
                string gioiTinh = cbGioiTinh.Text.Trim();
                string ngaySinh = dtpNgaySinh.Value.ToString("yyyy/MM/dd");
                string queQuan = txtQueQuan.Text.Trim();
                string maLop = txtMaLop.Text.Trim();

                if (string.IsNullOrEmpty(maSV))
                {
                    MessageBox.Show("Hãy chọn 1 sinh viên trong danh sách để sửa.");
                    return;
                }

                MoKetNoi();
                // KHÔNG dùng parameter theo đề (lưu ý: thực tế nên dùng parameter để an toàn)
                string sql =
                    "UPDATE [dbo].[SinhVien] " +
                    "SET MaSV = '" + maSV + "', " +
                    "    TenSV = N'" + tenSV + "', " +
                    "    GioiTinh = N'" + gioiTinh + "', " +
                    "    NgaySinh = CAST('" + ngaySinh + "' AS DATETIME), " +
                    "    QueQuan = N'" + queQuan + "', " +
                    "    MaLop = '" + maLop + "' " +
                    "WHERE MaSV = '" + maSV + "'";

                using (var cmd = new SqlCommand(sql, sqlCon))
                {
                    int kq = cmd.ExecuteNonQuery();
                    if (kq > 0)
                    {
                        MessageBox.Show("Cập nhật thành công!");
                        // refresh lại list theo lớp đang chọn
                        string currentMaLop = (cbMaLop.SelectedIndex >= 0)
                            ? cbMaLop.SelectedItem.ToString().Split('-')[0].Trim()
                            : maLop;
                        HienThiDSSVTheoLop(currentMaLop);
                    }
                    else
                    {
                        MessageBox.Show("Cập nhật không thành công!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật: " + ex.Message);
            }
            finally { DongKetNoi(); }
        }

        private void ClearInputs()
        {
            txtMaSV.Clear();
            txtTenSV.Clear();
            cbGioiTinh.SelectedIndex = -1;
            dtpNgaySinh.Value = DateTime.Today;
            txtQueQuan.Clear();
            txtMaLop.Clear();
        }
    }
}
