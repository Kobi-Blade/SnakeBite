using SnakeBite.ModPages;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace SnakeBite
{
    public static class ProgressWindow
    {
        public static void Show(string WindowTitle, string ProgressText, Action WorkerFunction, LogPage logWindow)
        {
            formProgress progressWindow = new formProgress();
            progressWindow.StatusText.Text = ProgressText;

            logWindow.Text = ProgressText;

            BackgroundWorker progressWorker = new BackgroundWorker();
            progressWorker.DoWork += (obj, var) =>
            {
                WorkerFunction();
            };
            progressWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
                delegate (object sender, RunWorkerCompletedEventArgs e)
                {
                    if (e.Error != null)
                    {
                        Debug.LogLine(string.Format("[Error] Exception message '{0}'", e.Error.ToString()));
                        Debug.LogLine(string.Format("[Error] Exception StackTrace '{0}'", e.Error.StackTrace));
                        _ = logWindow.Invoke((MethodInvoker)delegate { logWindow.Text = string.Format("Error during process :'{0}'", ProgressText); });

                        _ = MessageBox.Show(string.Format("Exception :'{0}'\r\nCheck SnakeBites log for more info.", e.Error.ToString()), string.Format("Error during process :'{0}'", ProgressText), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _ = progressWindow.Invoke((MethodInvoker)delegate { progressWindow.Close(); });
                    }
                    else if (e.Cancelled)
                    {

                    }
                    else
                    {
                        _ = progressWindow.Invoke((MethodInvoker)delegate { progressWindow.Close(); });
                    }
                    progressWorker.Dispose();
                }
             );

            progressWorker.RunWorkerAsync();
            _ = progressWindow.ShowDialog();
        }
    }
}
