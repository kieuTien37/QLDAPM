using System;
using System.Windows.Forms;

namespace Buoi81150080144NguyenThuyKieu
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());   // chạy áp dụng 3
        }
    }
}
