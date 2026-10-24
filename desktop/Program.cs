using System;
using System.Threading;
using System.Windows.Forms;
using SmartVidkonAutomation.Helpers;

namespace SmartVidkonAutomation
{
    static class Program
    {
        private static Mutex _appMutex = null;

        [STAThread]
        static void Main()
        {
            // 0. Kunci Direktori Root Aplikasi
            try { AppPaths.SetAppDir(Application.StartupPath); } catch { }

            // 1. Single Instance Mutex
            bool createdNew = false;
            _appMutex = new Mutex(true, "SmartVidkonAutomation_Mutex", out createdNew);

            if (!createdNew)
            {
                MessageBox.Show(
                    "Aplikasi Smart Vidkon Automation sudah berjalan di sistem ini.",
                    "Peringatan Single Instance",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            // 2. Aktivasi Visual Styles
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 3. Validasi & Inisialisasi Lisensi Offline
            if (!LicenseManager.CheckLicense(true))
            {
                // Pengguna membatalkan aktivasi atau lisensi tidak valid
                return;
            }

            // 4. Peluncuran Form Utama
            try
            {
                Application.Run(new MainForm());
            }
            finally
            {
                if (_appMutex != null)
                {
                    _appMutex.ReleaseMutex();
                    _appMutex.Dispose();
                    _appMutex = null;
                }
            }
        }
    }
}
