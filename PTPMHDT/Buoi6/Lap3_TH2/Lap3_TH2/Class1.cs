using System;
using System.Drawing;
using System.Windows.Forms;

namespace ThucHanh2
{
    public class MainForm : Form
    {
        // input controls
        TextBox txtHoTen, txtLop, txtDiaChi;
        DateTimePicker dtpNgaySinh;

        // action buttons
        Button btnThem, btnSua, btnXoa, btnThoat;

        // list view
        ListView lv;

        public MainForm()
        {
            Text = "Danh sách sinh viên";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(760, 520);
            Font = new Font("Segoe UI", 9F);

            BuildHeader();
            BuildInputPanel();
            BuildButtons();
            BuildListView();
        }

        void BuildHeader()
        {
            var lblTitle = new Label
            {
                Text = "DANH MỤC SINH VIÊN",
                ForeColor = Color.RoyalBlue,
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 60
            };
            Controls.Add(lblTitle);
        }

        void BuildInputPanel()
        {
            var grp = new GroupBox
            {
                Text = "Thông tin sinh viên:",
                Left = 16,
                Top = 70,
                Width = ClientSize.Width - 32,
                Height = 130,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            Controls.Add(grp);

            // labels
            var lblHoTen = new Label { Text = "Họ tên:", Left = 16, Top = 28, AutoSize = true };
            var lblLop = new Label { Text = "Lớp:", Left = 400, Top = 28, AutoSize = true };
            var lblNgay = new Label { Text = "Ngày sinh:", Left = 16, Top = 64, AutoSize = true };
            var lblDiaChi = new Label { Text = "Địa chỉ:", Left = 400, Top = 64, AutoSize = true };

            grp.Controls.Add(lblHoTen);
            grp.Controls.Add(lblLop);
            grp.Controls.Add(lblNgay);
            grp.Controls.Add(lblDiaChi);

            // inputs
            txtHoTen = new TextBox { Left = 90, Top = 24, Width = 280, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            txtLop = new TextBox { Left = 440, Top = 24, Width = 280, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            dtpNgaySinh = new DateTimePicker { Left = 90, Top = 60, Width = 280, Format = DateTimePickerFormat.Short };
            txtDiaChi = new TextBox { Left = 440, Top = 60, Width = 280, Anchor = AnchorStyles.Top | AnchorStyles.Right };

            grp.Controls.AddRange(new Control[] { txtHoTen, txtLop, dtpNgaySinh, txtDiaChi });
        }

        void BuildButtons()
        {
            var grp = new GroupBox
            {
                Text = "Chức năng:",
                Left = 16,
                Top = 210,
                Width = ClientSize.Width - 32,
                Height = 70,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            Controls.Add(grp);

            btnThem = new Button { Text = "Thêm", Left = 40, Top = 28, Width = 90 };
            btnSua = new Button { Text = "Sửa", Left = 150, Top = 28, Width = 90 };
            btnXoa = new Button { Text = "Xóa", Left = 260, Top = 28, Width = 90 };
            btnThoat = new Button { Text = "Thoát", Left = grp.Width - 120, Top = 28, Width = 90, Anchor = AnchorStyles.Top | AnchorStyles.Right };

            btnThem.Click += BtnThem_Click;
            btnSua.Click += BtnSua_Click;
            btnXoa.Click += BtnXoa_Click;
            btnThoat.Click += (s, e) => Close();

            grp.Controls.AddRange(new Control[] { btnThem, btnSua, btnXoa, btnThoat });

            // reposition on resize
            grp.Resize += (s, e) => { btnThoat.Left = grp.Width - 120; };
        }

        void BuildListView()
        {
            var grp = new GroupBox
            {
                Text = "Thông tin chung sinh viên:",
                Left = 16,
                Top = 290,
                Width = ClientSize.Width - 32,
                Height = ClientSize.Height - 306,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            Controls.Add(grp);

            lv = new ListView
            {
                View = View.Details,
                GridLines = true,
                FullRowSelect = true,
                HideSelection = false,
                Dock = DockStyle.Fill
            };
            lv.Columns.Add("Họ tên", 220);
            lv.Columns.Add("Ngày sinh", 110);
            lv.Columns.Add("Lớp", 100);
            lv.Columns.Add("Địa chỉ", 300);

            lv.SelectedIndexChanged += Lv_SelectedIndexChanged;

            grp.Controls.Add(lv);
        }

        // ====== Events ======
        void BtnThem_Click(object sender, EventArgs e)
        {
            var hoten = (txtHoTen.Text ?? "").Trim();
            if (hoten.Length == 0)
            {
                MessageBox.Show("Họ tên không được rỗng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtHoTen.Focus();
                return;
            }

            string ngay = dtpNgaySinh.Value.ToString("dd/MM/yyyy");
            string lop = (txtLop.Text ?? "").Trim();
            string dia = (txtDiaChi.Text ?? "").Trim();

            var item = new ListViewItem(hoten);
            item.SubItems.Add(ngay);
            item.SubItems.Add(lop);
            item.SubItems.Add(dia);

            lv.Items.Add(item);
            ClearInputs();
        }

        void BtnSua_Click(object sender, EventArgs e)
        {
            if (lv.SelectedItems.Count == 0)
            {
                MessageBox.Show("Hãy chọn 1 dòng để sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var hoten = (txtHoTen.Text ?? "").Trim();
            if (hoten.Length == 0)
            {
                MessageBox.Show("Họ tên không được rỗng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtHoTen.Focus();
                return;
            }

            var it = lv.SelectedItems[0];
            it.Text = hoten;
            it.SubItems[1].Text = dtpNgaySinh.Value.ToString("dd/MM/yyyy");
            it.SubItems[2].Text = (txtLop.Text ?? "").Trim();
            it.SubItems[3].Text = (txtDiaChi.Text ?? "").Trim();
        }

        void BtnXoa_Click(object sender, EventArgs e)
        {
            if (lv.SelectedItems.Count == 0)
            {
                MessageBox.Show("Hãy chọn 1 dòng để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (MessageBox.Show("Bạn có chắc muốn xóa dòng đã chọn?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                lv.Items.Remove(lv.SelectedItems[0]);
                ClearInputs();
            }
        }

        void Lv_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lv.SelectedItems.Count == 0) return;
            var it = lv.SelectedItems[0];
            txtHoTen.Text = it.Text;
            DateTime d;
            if (DateTime.TryParse(it.SubItems[1].Text, out d))
                dtpNgaySinh.Value = d;
            txtLop.Text = it.SubItems[2].Text;
            txtDiaChi.Text = it.SubItems[3].Text;
        }

        void ClearInputs()
        {
            txtHoTen.Clear();
            txtLop.Clear();
            txtDiaChi.Clear();
            dtpNgaySinh.Value = DateTime.Today;
            txtHoTen.Focus();
        }
    }
}
