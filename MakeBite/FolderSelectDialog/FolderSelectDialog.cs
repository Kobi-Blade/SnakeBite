using System;
using System.Windows.Forms;

namespace FolderSelect
{
    public class FolderSelectDialog
    {
        private readonly System.Windows.Forms.OpenFileDialog ofd = null;
        public FolderSelectDialog()
        {
            ofd = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "Folders|\n",
                AddExtension = false,
                CheckFileExists = false,
                DereferenceLinks = true,
                Multiselect = false
            };
        }

        #region Properties
        public string InitialDirectory
        {
            get => ofd.InitialDirectory; set => ofd.InitialDirectory = value == null || value.Length == 0 ? Environment.CurrentDirectory : value;
        }
        public string Title
        {
            get => ofd.Title; set => ofd.Title = value ?? "Select a folder";
        }
        public string FileName => ofd.FileName;

        #endregion Properties

        #region Methods
        public bool ShowDialog()
        {
            return ShowDialog(IntPtr.Zero);
        }
        public bool ShowDialog(IntPtr hWndOwner)
        {
            bool flag = false;

            if (Environment.OSVersion.Version.Major >= 6)
            {
                Reflector r = new Reflector("System.Windows.Forms");

                uint num = 0;
                Type typeIFileDialog = r.GetType("FileDialogNative.IFileDialog");
                object dialog = r.Call(ofd, "CreateVistaDialog");
                r.Call(ofd, "OnBeforeVistaDialog", dialog);

                uint options = (uint)r.CallAs(typeof(System.Windows.Forms.FileDialog), ofd, "GetOptions");
                options |= (uint)r.GetEnum("FileDialogNative.FOS", "FOS_PICKFOLDERS");
                r.CallAs(typeIFileDialog, dialog, "SetOptions", options);

                object pfde = r.New("FileDialog.VistaDialogEvents", ofd);
                object[] parameters = new object[] { pfde, num };
                r.CallAs2(typeIFileDialog, dialog, "Advise", parameters);
                num = (uint)parameters[1];
                try
                {
                    int num2 = (int)r.CallAs(typeIFileDialog, dialog, "Show", hWndOwner);
                    flag = 0 == num2;
                }
                finally
                {
                    r.CallAs(typeIFileDialog, dialog, "Unadvise", num);
                    GC.KeepAlive(pfde);
                }
            }
            else
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog
                {
                    Description = Title,
                    SelectedPath = InitialDirectory,
                    ShowNewFolderButton = false
                };
                if (fbd.ShowDialog(new WindowWrapper(hWndOwner)) != DialogResult.OK)
                {
                    return false;
                }

                ofd.FileName = fbd.SelectedPath;
                flag = true;
            }

            return flag;
        }

        #endregion Methods
    }
    public class WindowWrapper : System.Windows.Forms.IWin32Window
    {
        public WindowWrapper(IntPtr handle)
        {
            Handle = handle;
        }
        public IntPtr Handle { get; }
    }
}