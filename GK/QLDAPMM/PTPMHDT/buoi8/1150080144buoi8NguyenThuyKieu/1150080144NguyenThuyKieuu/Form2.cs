using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace buoi4_TranThiNgocHuyen
{
    public class Form2 : Form   // KHÔNG cần partial vì không dùng Designer
    {
        // Chuỗi kết nối — sửa lại nếu cần
        private readonly string strCon =
            @"Data Source=BOP;Initial Catalog=buoi6_tranthingochuyen;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";

        private SqlConnection sqlCon;

        // ===== UI controls =====
        private Label lblTitle;
        private GroupBox grpNhap, grpDanhSach;
        private Label lblMaSV, lblTenSV, lblGioiTinh, lblNgaySinh, lblQueQuan, lblMaLop;
        private TextBox txtMaSV, txtTenSV, txtQueQuan, txtMaLop;
        private ComboBox cbGioiTinh;
        private DateTimePicker dtpNgaySinh;
        private Button btnThem;

        private ListView lsvDanhSach;
        private ColumnHeader colMaSV, colTenSV, colGioiTinh, colNgaySinh, colQueQuan, colMaLop;

        public Form2()
        {
            InitializeComponent();      // tự định nghĩa ở dưới
        }

        // ===== Tạo giao diện (thay cho Designer) =====
        private void InitializeComponent()
        {
            Text = "Thêm dữ liệu (Parameter)";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(980, 520);

            // Title
            lblTitle = new Label
            {
                Text = "Thêm dữ liệu CÓ dùng Parameter",
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.Gainsboro
            };
            Controls.Add(lblTitle);

            // Nhập
            grpNhap = new GroupBox { Text = "Nhập thông tin:", Location = new Point(12, 55), Size = new Size(360, 420) };
            Controls.Add(grpNhap);

            lblMaSV = new Label { Text = "Mã SV:", Location = new Point(15, 35) };
            lblTenSV = new Label { Text = "Tên SV:", Location = new Point(15, 85) };
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

            btnThem = new Button { Text = "Thêm (Parameter)", Location = new Point(120, 340), Width = 210 };
            btnThem.Click += btnThem_Click;

            grpNhap.Controls.AddRange(new Control[]
            {
                lblMaSV, lblTenSV, lblGioiTinh, lblNgaySinh, lblQueQuan, lblMaLop,
                txtMaSV, txtTenSV, cbGioiTinh, dtpNgaySinh, txtQueQuan, txtMaLop, btnThem
            });

            // Danh sách
            grpDanhSach = new GroupBox { Text = "Danh sách sinh viên:", Location = new Point(385, 55), Size = new Size(580, 420) };
            Controls.Add(grpDanhSach);

            lsvDanhSach = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = true };
            colMaSV = new ColumnHeader { Text = "Mã SV", Width = 80 };
            colTenSV = new ColumnHeader { Text = "Tên SV", Width = 150 };
            colGioiTinh = new ColumnHeader { Text = "Giới tính", Width = 80 };
            colNgaySinh = new ColumnHeader { Text = "Ngày sinh", Width = 100 };
            colQueQuan = new ColumnHeader { Text = "Quê quán", Width = 100 };
            colMaLop = new ColumnHeader { Text = "Mã lớp", Width = 70 };
            lsvDanhSach.Columns.AddRange(new[] { colMaSV, colTenSV, colGioiTinh, colNgaySinh, colQueQuan, colMaLop });
            grpDanhSach.Controls.Add(lsvDanhSach);

            // Sự kiện Form Load
            Load += Form2_Load;
        }

        // ===== Kết nối =====
        private void MoKetNoi()
        {
            if (sqlCon == null) sqlCon = new SqlConnection(strCon);
            if (sqlCon.State == ConnectionState.Closed) sqlCon.Open();
        }
        private void DongKetNoi()
        {
            if (sqlCon != null && sqlCon.State == ConnectionState.Open) sqlCon.Close();
        }

        // ===== Events =====
        private void Form2_Load(object sender, EventArgs e)
        {
            HienThiDanhSach();
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            string maSV = txtMaSV.Text.Trim();
            string tenSV = txtTenSV.Text.Trim();
            string gioiTinh = cbGioiTinh.Text.Trim();
            DateTime? ngaySinh = dtpNgaySinh.Value.Date;
            string queQuan = txtQueQuan.Text.Trim();
            string maLop = txtMaLop.Text.Trim();

            if (string.IsNullOrEmpty(maSV) || string.IsNullOrEmpty(tenSV))
            {
                MessageBox.Show("Mã SV và Tên SV không được để trống!");
                return;
            }

            const string sql = @"
                INSERT INTO [dbo].[SinhVien] (MaSV, TenSV, GioiTinh, NgaySinh, QueQuan, MaLop)
                VALUES (@MaSV, @TenSV, @GioiTinh, @NgaySinh, @QueQuan, @MaLop);";

            try
            {
                MoKetNoi();
                using (var cmd = new SqlCommand(sql, sqlCon))
                {
                    cmd.Parameters.Add("@MaSV", SqlDbType.NVarChar, 20).Value = maSV;
                    cmd.Parameters.Add("@TenSV", SqlDbType.NVarChar, 100).Value = tenSV;
                    cmd.Parameters.Add("@GioiTinh", SqlDbType.NVarChar, 10).Value = (object)gioiTinh ?? DBNull.Value;
                    cmd.Parameters.Add("@NgaySinh", SqlDbType.Date).Value = (object)ngaySinh ?? DBNull.Value;
                    cmd.Parameters.Add("@QueQuan", SqlDbType.NVarChar, 100).Value = (object)queQuan ?? DBNull.Value;
                    cmd.Parameters.Add("@MaLop", SqlDbType.NVarChar, 20).Value = (object)maLop ?? DBNull.Value;

                    int kq = cmd.ExecuteNonQuery();
                    MessageBox.Show(kq > 0 ? "Thêm (Parameter) thành công!" : "Không có dòng nào được thêm.");
                }

                ClearInputs();
                HienThiDanhSach();
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

        private void HienThiDanhSach()
        {
            try
            {
                MoKetNoi();
                using (var cmd = new SqlCommand("SELECT MaSV,TenSV,GioiTinh,NgaySinh,QueQuan,MaLop FROM [dbo].[SinhVien] ORDER BY MaSV", sqlCon))
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
