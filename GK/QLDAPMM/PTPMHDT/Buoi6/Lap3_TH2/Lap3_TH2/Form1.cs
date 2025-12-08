using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Lap3_TH2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtHoTen.Text))
            {
                MessageBox.Show("Họ tên không được rỗng!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ListViewItem item = new ListViewItem(txtHoTen.Text);
            item.SubItems.Add(txtLop.Text);
            item.SubItems.Add(dtNgaysinh.Value.ToString("dd/MM/yyyy"));
            item.SubItems.Add(txtDiachi.Text);

            listView1.Items.Add(item);
            ClearTextBoxes();
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn một dòng để xóa!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            listView1.Items.Remove(listView1.SelectedItems[0]);
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn một dòng để sửa!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ListViewItem selectedItem = listView1.SelectedItems[0];
            selectedItem.Text = txtHoTen.Text;
            selectedItem.SubItems[1].Text = txtLop.Text;
            selectedItem.SubItems[2].Text = dtNgaysinh.Value.ToString("dd/MM/yyyy");
            selectedItem.SubItems[3].Text = txtDiachi.Text;
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listView1.SelectedItems[0];
                txtHoTen.Text = selectedItem.Text;
                txtLop.Text = selectedItem.SubItems[1].Text;
                dtNgaysinh.Value = DateTime.ParseExact(selectedItem.SubItems[2].Text, "dd/MM/yyyy", null);
                txtDiachi.Text = selectedItem.SubItems[3].Text;
            }
        }

        private void ClearTextBoxes()
        {
            txtHoTen.Text = "";
            txtLop.Text = "";
            dtNgaysinh.Value = DateTime.Now;
            txtDiachi.Text = "";
        }
    }
}
