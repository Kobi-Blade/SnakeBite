using System.IO;
using System.Text;
using System.Windows.Forms;

namespace SnakeBite
{
    public class ControBoxWriter : TextWriter
    {
        private delegate void WriteCallbackChar(char text);
        private delegate void WriteCallbackString(string text);

        private readonly TextBox textbox;
        public ControBoxWriter(TextBox textbox)
        {
            this.textbox = textbox;
        }

        public override void Write(char value)
        {
            if (textbox.InvokeRequired)
            {
                WriteCallbackChar d = new WriteCallbackChar(Write);
                textbox.Invoke(d, new object[] { value });
            }
            else
            {
                textbox.AppendText(value.ToString());
            }
        }

        public override void Write(string value)
        {
            if (textbox.InvokeRequired)
            {
                WriteCallbackString d = new WriteCallbackString(Write);
                textbox.Invoke(d, new object[] { value });
            }
            else
            {
                textbox.AppendText(value);
            }
        }

        public override Encoding Encoding => Encoding.ASCII;
    }
}
