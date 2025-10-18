using System.Diagnostics;
using System.Windows.Forms;

namespace SnakeBite.ModPages
{
    public partial class NoInstalledPage : UserControl
    {
        public NoInstalledPage()
        {
            InitializeComponent();
        }

        private void linkLabelSnakeBiteModsList_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(GamePaths.SBWMSearchURLPath);
        }
    }
}
