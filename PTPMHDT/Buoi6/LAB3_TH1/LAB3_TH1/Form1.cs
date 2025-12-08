using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace FastFoodPDA
{
    public class FormPda : Form
    {
        private readonly Dictionary<string, int> order = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> menuItems = new List<string>()
        {
            "Cơm chiên trứng","Bánh mỳ ốp la","Coca","Lipton",
            "Ốc rang muối","Khoai tây chiên","7 up","Cam",
            "Mỳ xào hải sản","Cá viên chiên","Pepsi","Cafe",
            "Buger bò nướng","Đùi gà rán","Bún bò Huế"
        };

        private DataGridView dgv;
        private ComboBox cboTable;
        private Button btnOrder, btnDelete;

        public FormPda()
        {
            Text = "Quán ăn nhanh AAA";
            ClientSize = new Size(600, 500);
            BuildUI();
        }

        private void BuildUI()
        {
            cboTable = new ComboBox { Left = 200, Top = 230, Width = 120 };
            cboTable.Items.AddRange(new[] { "Bàn 1", "Bàn 2", "Bàn 3", "Bàn 4" });
            cboTable.SelectedIndex = 0;
            Controls.Add(cboTable);

            btnDelete = new Button { Text = "Xóa", Left = 50, Top = 230, Width = 80 };
            btnDelete.Click += BtnDelete_Click;
            Controls.Add(btnDelete);

            btnOrder = new Button { Text = "Order", Left = 350, Top = 230, Width = 80 };
            btnOrder.Click += BtnOrder_Click;
            Controls.Add(btnOrder);

            dgv = new DataGridView
            {
                Left = 20,
                Top = 270,
                Width = 550,
                Height = 200,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            dgv.Columns.Add("colItem", "Món");
            dgv.Columns.Add("colQty", "Số lượng");
            Controls.Add(dgv);

            int x = 20, y = 40, count = 0;
            foreach (string item in menuItems)
            {
                var b = new Button { Text = item, Tag = item, Left = x, Top = y, Width = 100, Height = 35 };
                b.Click += MenuBtn_Click;
                Controls.Add(b);

                x += 110; count++;
                if (count % 5 == 0) { x = 20; y += 45; }
            }
        }

        private void MenuBtn_Click(object sender, EventArgs e)
        {
            Button b = sender as Button;
            if (b == null || b.Tag == null) return;

            string item = b.Tag.ToString();
            int q;
            if (order.TryGetValue(item, out q))
                order[item] = q + 1;
            else
                order[item] = 1;

            RefreshGrid();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgv.SelectedRows.Count == 0) return;
            string name = dgv.SelectedRows[0].Cells[0].Value.ToString();
            if (order.ContainsKey(name))
            {
                order.Remove(name);
                RefreshGrid();
            }
        }

        private void BtnOrder_Click(object sender, EventArgs e)
        {
            if (order.Count == 0) { MessageBox.Show("Chưa chọn món."); return; }

            string table = cboTable.SelectedItem.ToString();
            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Orders");
            Directory.CreateDirectory(folder);
            string path = Path.Combine(folder, $"Order_{table}_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

            try
            {
                using (var sw = new StreamWriter(path, false, new UTF8Encoding(false)))
                {
                    sw.WriteLine("Bàn: " + table);
                    sw.WriteLine("Món\tSố lượng");
                    foreach (var kv in order)
                        sw.WriteLine(kv.Key + "\t" + kv.Value);
                }
                MessageBox.Show("Đã ghi file order tại:\n" + path);
                order.Clear();
                RefreshGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể ghi file: " + ex.Message, "Lỗi");
            }
        }

        private void RefreshGrid()
        {
            dgv.Rows.Clear();
            foreach (var kv in order)
                dgv.Rows.Add(kv.Key, kv.Value);
        }
    }
}
