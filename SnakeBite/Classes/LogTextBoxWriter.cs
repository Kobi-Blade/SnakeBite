using SnakeBite.ModPages;
using System.IO;
using System.Text;

namespace SnakeBite
{
    public class LogTextBoxWriter : TextWriter
    {
        private delegate void WriteCallbackChar(char text);
        private delegate void WriteCallbackString(string text);

        private readonly LogPage logPage;

        public LogTextBoxWriter(LogPage logPage)
        {
            this.logPage = logPage;
        }

        public override void Write(char value)
        {
            logPage.logStringBuilder.Append(value);
            logPage.UpdateLog();
        }

        public override void Write(string value)
        {
            logPage.logStringBuilder.Append(value);
            logPage.UpdateLog();
        }

        public override Encoding Encoding => Encoding.ASCII;
    }
}
