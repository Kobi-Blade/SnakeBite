using SnakeBite.ModPages;
using System.IO;
using System.Text;

namespace SnakeBite
{
    public class LogTextBoxWriter : TextWriter
    {
        private delegate void WriteCallbackChar(char text);
        private delegate void WriteCallbackString(string text);

        //private DateTime lastUpdate = new DateTime();
        //private int displayRate = 100;//ms

        private readonly LogPage logPage;

        public LogTextBoxWriter(LogPage logPage)
        {
            this.logPage = logPage;
        }

        public override void Write(char value)
        {
            _ = logPage.logStringBuilder.Append(value);
            logPage.UpdateLog();

            /*
            var current = DateTime.Now;
            var delta = (current - lastUpdate).TotalMilliseconds;
            if (delta > displayRate)
            {
                lastUpdate = current;
                logPage.UpdateLog();
            }
            */
        }

        public override void Write(string value)
        {
            _ = logPage.logStringBuilder.Append(value);
            logPage.UpdateLog();

            /*
            var current = DateTime.Now;
            var delta = (current - lastUpdate).TotalMilliseconds;
            if (delta > displayRate)
            {
                lastUpdate = current;
                logPage.UpdateLog();
            }
            */
        }

        public override Encoding Encoding => Encoding.ASCII;
    }
}
