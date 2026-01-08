using System;
using System.Windows.Forms;

namespace ConsoleApp
{
    internal static class Program
    {
        /// <summary>
        ///  預設進入點，啟動主視窗。
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
