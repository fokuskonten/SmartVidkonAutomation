using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace SmartVidkonAutomation.Helpers
{
    /// <summary>
    /// Sentral Resolusi Jalur Portabel dan Mandiri Smart Vidkon Automation.
    /// 100% Independen, Bebas Path Hardcoded Developer, Siap Digunakan Pengguna Umum.
    /// Kompatibel penuh dengan C# 5.0 (.NET Framework 4.8 csc.exe).
    /// </summary>
    public static class AppPaths
    {
        private static string _appDir = null;

        /// <summary>
        /// Mengatur direktori root aplikasi secara eksplisit.
        /// </summary>
        public static void SetAppDir(string dir)
        {
            if (!string.IsNullOrEmpty(dir))
            {
                _appDir = dir.TrimEnd('\\', '/');
            }
        }

        /// <summary>
        /// Direktori root aplikasi mandiri tempat binary executable berada.
        /// </summary>
        public static string AppDir
        {
            get
            {
                if (_appDir == null)
                {
                    try
                    {
                        string loc = Assembly.GetExecutingAssembly().Location;
                        if (!string.IsNullOrEmpty(loc))
                        {
                            string dir = Path.GetDirectoryName(Path.GetFullPath(loc));
                            if (!string.IsNullOrEmpty(dir) && dir.IndexOf("System32", StringComparison.OrdinalIgnoreCase) < 0)
                            {
                                _appDir = dir;
                            }
                        }
                    }
                    catch { }

                    if (string.IsNullOrEmpty(_appDir))
                    {
                        try
                        {
                            string startup = Application.StartupPath;
                            if (!string.IsNullOrEmpty(startup) && startup.IndexOf("System32", StringComparison.OrdinalIgnoreCase) < 0)
                            {
                                _appDir = startup;
                            }
                        }
                        catch { }
                    }

                    if (string.IsNullOrEmpty(_appDir))
                    {
                        try
                        {
                            string cur = Environment.CurrentDirectory;
                            if (!string.IsNullOrEmpty(cur) && cur.IndexOf("System32", StringComparison.OrdinalIgnoreCase) < 0)
                            {
                                _appDir = cur;
                            }
                        }
                        catch { }
                    }

                    if (string.IsNullOrEmpty(_appDir))
                    {
                        _appDir = AppDomain.CurrentDomain.BaseDirectory;
                    }

                    _appDir = _appDir.TrimEnd('\\', '/');
                }
                return _appDir;
            }
        }

        // Subdirektori Utama Aplikasi (Sintaks C# 5 Klasik)
        public static string BinDir { get { return Path.Combine(AppDir, "bin"); } }
        public static string ConfigDir { get { return EnsureDir(Path.Combine(AppDir, "config")); } }
        public static string DatabaseDir { get { return EnsureDir(Path.Combine(AppDir, "database")); } }
        public static string AssetsDir { get { return EnsureDir(Path.Combine(AppDir, "assets")); } }
        public static string BgmDir { get { return EnsureDir(Path.Combine(AssetsDir, "bgm")); } }
        public static string FootageDir { get { return EnsureDir(Path.Combine(AssetsDir, "footage")); } }
        public static string LutsDir { get { return EnsureDir(Path.Combine(AssetsDir, "luts")); } }
        public static string BrandingDir { get { return EnsureDir(Path.Combine(AssetsDir, "branding")); } }
        public static string ProjectsDir { get { return EnsureDir(Path.Combine(AppDir, "projects")); } }
        public static string EngineDir { get { return Path.Combine(AppDir, "engine"); } }
        public static string DocsDir { get { return Path.Combine(AppDir, "docs"); } }

        // Berkas Eksekusi & Konfigurasi Mandiri
        public static string NodeExe
        {
            get
            {
                string local = Path.Combine(BinDir, "node.exe");
                return File.Exists(local) ? local : "node";
            }
        }

        public static string FfmpegExe
        {
            get
            {
                string local = Path.Combine(BinDir, "ffmpeg.exe");
                return File.Exists(local) ? local : "ffmpeg";
            }
        }

        public static string FfprobeExe
        {
            get
            {
                string local = Path.Combine(BinDir, "ffprobe.exe");
                return File.Exists(local) ? local : "ffprobe";
            }
        }

        public static string PipelineScript { get { return Path.Combine(EngineDir, "run_production_pipeline.js"); } }
        public static string ScanScript { get { return Path.Combine(EngineDir, "scan_and_check_footage.js"); } }
        public static string AppIconPath { get { return Path.Combine(AppDir, "SmartVidkon.ico"); } }
        public static string LicenseKeyPath { get { return Path.Combine(ConfigDir, "license.key"); } }
        public static string AppConfigPath { get { return Path.Combine(ConfigDir, "app_config.json"); } }
        public static string ApiVaultPath { get { return Path.Combine(ConfigDir, "studio_api_vault.json"); } }
        public static string MediaVaultPath { get { return Path.Combine(ConfigDir, "studio_media_vault.json"); } }
        public static string FootageDbPath { get { return Path.Combine(DatabaseDir, "footage_catalog.db"); } }
        public static string CatalogSummaryPath { get { return Path.Combine(DatabaseDir, "katalog_vidcon_summary.json"); } }

        private static string EnsureDir(string path)
        {
            try
            {
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            }
            catch { }
            return path;
        }
    }
}
