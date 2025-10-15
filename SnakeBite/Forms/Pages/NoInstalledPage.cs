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

        private void linkLabelSnakeBiteModsList_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) // opens the [SBWM] search filter on nexus mods, randomly sorted.
        {
            _ = Process.Start(GamePaths.SBWMSearchURLPath);
        }
    }
}
