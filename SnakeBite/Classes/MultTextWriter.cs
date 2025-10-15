using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SnakeBite
{
    public class MultiTextWriter : TextWriter
    {
        private readonly IEnumerable<TextWriter> writers;
        public MultiTextWriter(IEnumerable<TextWriter> writers)
        {
            this.writers = writers.ToList();
        }
        public MultiTextWriter(params TextWriter[] writers)
        {
            this.writers = writers;
        }

        public override void Write(char value)
        {
            foreach (TextWriter writer in writers)
            {
                writer.Write(value);
            }
        }

        public override void Write(string value)
        {
            foreach (TextWriter writer in writers)
            {
                writer.Write(value);
            }
        }

        public override void Flush()
        {
            foreach (TextWriter writer in writers)
            {
                writer.Flush();
            }
        }

        public override void Close()
        {
            foreach (TextWriter writer in writers)
            {
                writer.Close();
            }
        }

        public override Encoding Encoding => Encoding.ASCII;
    }
}
