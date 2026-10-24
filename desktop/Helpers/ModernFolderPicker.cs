using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SmartVidkonAutomation.Helpers
{
    /// <summary>
    /// Dialog Pemilih Folder Modern Windows Explorer (IFileOpenDialog FOS_PICKFOLDERS).
    /// Menggantikan FolderBrowserDialog pohon (treeview) kuno dengan dialog Windows Explorer standar
    /// yang memiliki navigasi Quick Access, bilah alamat, pencarian, dan tampilan folder modern.
    /// Kompatibel penuh dengan C# 5.0 (.NET Framework 4.8 csc.exe) dan Windows 7/8/10/11.
    /// </summary>
    public class ModernFolderPicker
    {
        public string Title { get; set; }
        public string InitialDirectory { get; set; }
        public string OkButtonLabel { get; set; }
        public string SelectedPath { get; private set; }

        public ModernFolderPicker()
        {
            Title = "Pilih Folder";
            OkButtonLabel = "Pilih Folder";
            SelectedPath = string.Empty;
        }

        public DialogResult ShowDialog(IWin32Window owner = null)
        {
            IntPtr ownerHwnd = owner != null ? owner.Handle : IntPtr.Zero;

            // 1. Coba gunakan dialog Windows Explorer modern via COM IFileOpenDialog
            try
            {
                IFileOpenDialog dialog = (IFileOpenDialog)new FileOpenDialogRCW();

                uint options;
                dialog.GetOptions(out options);
                // FOS_PICKFOLDERS (0x20) + FOS_FORCEFILESYSTEM (0x40) + FOS_NOVALIDATE (0x100)
                options |= 0x00000020 | 0x00000040;
                dialog.SetOptions(options);

                if (!string.IsNullOrEmpty(Title))
                {
                    dialog.SetTitle(Title);
                }

                if (!string.IsNullOrEmpty(OkButtonLabel))
                {
                    dialog.SetOkButtonLabel(OkButtonLabel);
                }

                if (!string.IsNullOrEmpty(InitialDirectory) && Directory.Exists(InitialDirectory))
                {
                    IShellItem initialItem;
                    Guid iid = new Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE");
                    if (SHCreateItemFromParsingName(InitialDirectory, IntPtr.Zero, ref iid, out initialItem) == 0 && initialItem != null)
                    {
                        dialog.SetFolder(initialItem);
                    }
                }

                int hr = dialog.Show(ownerHwnd);
                if (hr == 0) // S_OK
                {
                    IShellItem resultItem;
                    dialog.GetResult(out resultItem);
                    if (resultItem != null)
                    {
                        IntPtr pszString;
                        // SIGDN_FILESYSPATH (0x80058000)
                        resultItem.GetDisplayName(0x80058000, out pszString);
                        if (pszString != IntPtr.Zero)
                        {
                            SelectedPath = Marshal.PtrToStringUni(pszString);
                            Marshal.FreeCoTaskMem(pszString);
                            return DialogResult.OK;
                        }
                    }
                }
                else if (hr == unchecked((int)0x800704C7)) // HRESULT ERROR_CANCELLED (User tekan Batal / Cancel)
                {
                    return DialogResult.Cancel;
                }
            }
            catch
            {
                // 2. Fallback aman ke FolderBrowserDialog standar jika lingkungan COM tidak merespons
                using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                {
                    fbd.Description = Title;
                    fbd.SelectedPath = InitialDirectory;
                    fbd.ShowNewFolderButton = false;
                    if (fbd.ShowDialog(owner) == DialogResult.OK)
                    {
                        SelectedPath = fbd.SelectedPath;
                        return DialogResult.OK;
                    }
                    return DialogResult.Cancel;
                }
            }

            return DialogResult.Cancel;
        }

        #region Native COM Interop Definitions (Windows Shell IFileDialog)

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
        private static extern int SHCreateItemFromParsingName(
            [MarshalAs(UnmanagedType.LPWStr)] string pszPath,
            IntPtr pbc,
            ref Guid riid,
            [MarshalAs(UnmanagedType.Interface)] out IShellItem ppv);

        [ComImport]
        [Guid("DC1C5A9C-E88A-4DDE-A5A1-60F82A20AEF7")]
        [ClassInterface(ClassInterfaceType.None)]
        private class FileOpenDialogRCW
        {
        }

        [ComImport]
        [Guid("D57C7288-D4AD-4768-BE02-9D969532D960")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IFileOpenDialog
        {
            [PreserveSig]
            int Show(IntPtr parent);

            void SetFileTypes(uint cFileTypes, IntPtr rgFilterSpec);
            void SetFileTypeIndex(uint iFileType);
            void GetFileTypeIndex(out uint piFileType);
            void Advise(IntPtr pfde, out uint pdwCookie);
            void Unadvise(uint dwCookie);
            void SetOptions(uint fos);
            void GetOptions(out uint pfos);
            void SetDefaultFolder(IShellItem psi);
            void SetFolder(IShellItem psi);
            void GetFolder(out IShellItem ppsi);
            void GetCurrentSelection(out IShellItem ppsi);
            void SetFileName([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string pszName);
            void SetTitle([MarshalAs(UnmanagedType.LPWStr)] string pszTitle);
            void SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string pszText);
            void SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
            void GetResult(out IShellItem ppsi);
            void AddPlace(IShellItem psi, int fdap);
            void SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);
            void Close(int hr);
            void SetClientGuid(ref Guid guid);
            void ClearClientData();
            void SetFilter(IntPtr pFilter);
            void GetResults(out IntPtr ppenum);
            void GetSelectedItems(out IntPtr ppsai);
        }

        [ComImport]
        [Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellItem
        {
            void BindToHandler(IntPtr pbc, ref Guid bhid, ref Guid riid, out IntPtr ppv);
            void GetParent(out IShellItem ppsi);
            void GetDisplayName(uint sigdnName, out IntPtr ppszName);
            void GetAttributes(uint sfgaoMask, out uint psfgaoAttribs);
            void Compare(IShellItem psi, uint hint, out int piOrder);
        }

        #endregion
    }
}
