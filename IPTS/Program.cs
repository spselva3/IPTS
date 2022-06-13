using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IPTS
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            frmLogin Login = new frmLogin();
            if (Login.ShowDialog() == DialogResult.OK)
                //Application.Run(new frmChallanPrint());
                Application.Run(new frmMain());
        }
    }
}
