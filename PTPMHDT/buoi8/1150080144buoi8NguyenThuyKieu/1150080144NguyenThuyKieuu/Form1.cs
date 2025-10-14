using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace 1150080144NguyenThuyKieu
{
    public class Form1 : Form
    {
        // ======== CHUỖI KẾT NỐI SQL SERVER (theo bạn) ========
        private readonly string strCon =
            @"Data Source=BOP;Initial Catalog=buoi6_tranthingochuyen;Integrated Security=True;Trust Server Certificate=True";

        private SqlConnection sqlCon;

        // ======== KHAI BÁO CONTROL ========
        private Label lblTitle;
        private GroupBox grpNhap;
        private Label lblMaSV, lblTenSV, lblGioiTinh, lblNgaySinh, lblQueQuan, lblMaLop;
        private TextBox txtMaSV, txtTenSV, txtQueQuan, txtMaLop;
        private ComboBox cbGioiTinh;
        private DateTimePicker dtpNgaySinh;
        private Button btnThemSinhVien;

        private GroupBox grpDanhSach;
        private ListView lsvDanhSachSV;
        private ColumnHeader colMaSV, colTenSV, colGioiTinh, colNgaySinh, colQueQuan, colMaLop;

        public Form1()
        {
            InitializeComponent();   // Tự ta định nghĩa, không cần Designer.cs
        }

        // ======== UI KHỞI TẠO (thay cho Designer) ========
        private void InitializeComponent()
        {
            // Form
            this.Text = "Thêm dữ liệu không dùng Parameter";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ClientSize = new Size(980, 520);

            // Title
            lblTitle = new Label
            {
                Text = "Thêm dữ liệu không dùng Parameter",
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Gainsboro,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            Controls.Add(lblTitle);

            // Group nhập
            grpNhap = new GroupBox { Text = "Nhập thông tin:", Location = new Point(12, 55), Size = new Size(360, 420) };
            Controls.Add(grpNhap);

            lblMaSV = new Label { Text = "Mã sinh viên:", Location = new Point(15, 35) };
            lblTenSV = new Label { Text = "Tên sinh viên:", Location = new Point(15, 85) };
            lblGioiTinh = new Label { Text = "Giới tính:", Location = new Point(15, 135) };
            lblNgaySinh = new Label { Text = "Ngày sinh:", Location = new Point(15, 185) };
            lblQueQuan = new Label { Text = "Quê quán:", Location = new Point(15, 235) };
            lblMaLop = new Label { Text = "Mã lớp:", Location = new Point(15, 285) };

            txtMaSV = new TextBox { Location = new Point(120, 32), Width = 210 };
            txtTenSV = new TextBox { Location = new Point(120, 82), Width = 210 };
            cbGioiTinh = new ComboBox { Location = new Point(120, 132), Width = 210, DropDownStyle = ComboBoxStyle.DropDownList };
            cbGioiTinh.Items.AddRange(new object[] { "Nam", "Nữ", "Khác" });
            dtpNgaySinh = new DateTimePicker { Location = new Point(120, 182), Width = 210, Format = DateTimePickerFormat.Short };
            txtQueQuan = new TextBox { Location = new Point(120, 232), Width = 210 };
            txtMaLop = new TextBox { Location = new Point(120, 282), Width = 210 };

            btnThemSinhVien = new Button { Text = "Thêm sinh viên", Location = new Point(120, 340), Width = 210 };
            btnThemSinhVien.Click += btnThemSinhVien_Click;

            grpNhap.Controls.AddRange(new Control[]
            {
                lblMaSV,lblTenSV,lblGioiTinh,lblNgaySinh,lblQueQuan,lblMaLop,
                txtMaSV,txtTenSV,cbGioiTinh,dtpNgaySinh,txtQueQuan,txtMaLop,btnThemSinhVien
            });

            // Group danh sách
            grpDanhSach = new GroupBox { Text = "Danh sách sinh viên:", Location = new Point(385, 55), Size = new Size(580, 420) };
            Controls.Add(grpDanhSach);

            lsvDanhSachSV = new ListView { View = View.Details, FullRowSelect = true, GridLines = true, Dock = DockStyle.Fill };
            colMaSV = new ColumnHeader { Text = "Mã SV", Width = 80 };
            colTenSV = new ColumnHeader { Text = "Tên SV", Width = 150 };
            colGioiTinh = new ColumnHeader { Text = "Giới tính", Width = 80 };
            colNgaySinh = new ColumnHeader { Text = "Ngày sinh", Width = 100 };
            colQueQuan = new ColumnHeader { Text = "Quê quán", Width = 100 };
            colMaLop = new ColumnHeader { Text = "Mã lớp", Width = 70 };
            lsvDanhSachSV.Columns.AddRange(new[] { colMaSV, colTenSV, colGioiTinh, colNgaySinh, colQueQuan, colMaLop });
            grpDanhSach.Controls.Add(lsvDanhSachSV);

            // Event Load
            this.Load += Form1_Load;
        }

        // ======== DB helpers ========
        private void MoKetNoi()
        {
            if (sqlCon == null) sqlCon = new SqlConnection(strCon);
            if (sqlCon.State == System.Data.ConnectionState.Closed) sqlCon.Open();
        }
        private void DongKetNoi()
        {
            if (sqlCon != null && sqlCon.State == System.Data.ConnectionState.Open) sqlCon.Close();
        }

        // ======== Events ========
        private void Form1_Load(object sender, EventArgs e)
        {
            HienThiDanhSach();
        }

        private void btnThemSinhVien_Click(object sender, EventArgs e)
        {
            try
            {
                string maSV = txtMaSV.Text.Trim();
                string tenSV = txtTenSV.Text.Trim();
                string gioiTinh = cbGioiTinh.Text.Trim();
                string ngaySinhSql = dtpNgaySinh.Value.ToString("yyyy-MM-dd");
                string queQuan = txtQueQuan.Text.Trim();
                string maLop = txtMaLop.Text.Trim();

                if (string.IsNullOrEmpty(maSV) || string.IsNullOrEmpty(tenSV))
                {
                    MessageBox.Show("Mã SV và Tên SV không được để trống!");
                    return;
                }

                MoKetNoi();
                // Theo yêu cầu bài: không dùng parameter
                string sql = "INSERT INTO SinhVien(MaSV,TenSV,GioiTinh,NgaySinh,QueQuan,MaLop) " +
                             $"VALUES ('{maSV}', N'{tenSV}', N'{gioiTinh}', '{ngaySinhSql}', N'{queQuan}', '{maLop}')";

                using (var cmd = new SqlCommand(sql, sqlCon))
                {
                    int kq = cmd.ExecuteNonQuery();
                    if (kq > 0)
                    {
                        MessageBox.Show("Thêm sinh viên thành công!");
                        ClearInputs();
                        HienThiDanhSach();
                    }
                    else MessageBox.Show("Không có dòng nào được thêm!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thêm: " + ex.Message);
            }
            finally { DongKetNoi(); }
        }

        private void HienThiDanhSach()
        {
            try
            {
                MoKetNoi();
                using (var cmd = new SqlCommand("SELECT MaSV,TenSV,GioiTinh,NgaySinh,QueQuan,MaLop FROM SinhVien", sqlCon))
                using (var rd = cmd.ExecuteReader())
                {
                    lsvDanhSachSV.Items.Clear();
                    while (rd.Read())
                    {
                        var item = new ListViewItem(rd.GetString(0));
                        item.SubItems.Add(rd.GetString(1));
                        item.SubItems.Add(rd.GetString(2));
                        item.SubItems.Add(rd.GetDateTime(3).ToString("dd/MM/yyyy"));
                        item.SubItems.Add(rd.GetString(4));
                        item.SubItems.Add(rd.GetString(5));
                        lsvDanhSachSV.Items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hiển thị: " + ex.Message);
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
            txtMaSV.Focus();
        }
    }
}
