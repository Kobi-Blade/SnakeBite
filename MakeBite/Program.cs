using SnakeBite;
using System;
using System.Windows.Forms;

namespace MakeBite
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Debug.Clear();

            Debug.LogLine(string.Format(
                "MakeBite {0}\n" +
                "{1}\n" +
                "-------------------------",
                Tools.GetMBVersion(),
                Environment.OSVersion.VersionString));

            Application.Run(new formMain());
        }
    }
}