using System;
using System.Text;
using System.Windows.Forms;

namespace SnakeBite.ModPages
{
    public partial class LogPage : UserControl
    {
        public StringBuilder logStringBuilder = new StringBuilder();
        private volatile bool stopTimer = false;

        private readonly object thisLock = new object();

        public LogPage()
        {
            InitializeComponent();

            logStringBuilder.Capacity = 200 * 6000;

            Console.SetOut(new MultiTextWriter(new LogTextBoxWriter(this), Console.Out));
        }

        private delegate void WriteTextBox();

        private void UpdateProperty(object state)
        {
            if (!stopTimer)
            {
                if (textLog.InvokeRequired)
                {
                    textLog.Invoke((MethodInvoker)delegate { UpdateProperty(state); });
                }
                else
                {
                    lock (thisLock)
                    {
                        textLog.Text = logStringBuilder.ToString();
                    }
                }
            }
        }

        public void UpdateLog()
        {
            if (textLog.InvokeRequired)
            {
                textLog.Invoke((MethodInvoker)delegate { UpdateLog(); });
            }
            else
            {
                lock (thisLock)
                {
                    textLog.Text = logStringBuilder.ToString();
                }
            }
        }

        public void ClearPage()
        {
            logStringBuilder.Clear();
        }

        private void formLog_FormClosing(object sender, FormClosingEventArgs e)
        {
            stopTimer = true;
        }

        private void formLog_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void textLog_TextChanged(object sender, EventArgs e)
        {
            textLog.SelectionStart = textLog.Text.Length;
            textLog.ScrollToCaret();
        }
    }
}
