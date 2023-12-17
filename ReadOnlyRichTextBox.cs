using static Vanara.PInvoke.User32;

namespace PS5CodeReader
{
    public class ReadOnlyRichTextBox : RichTextBox
    {
        internal static Color ColorError = Color.IndianRed;
        internal static Color ColorSuccess = Color.MediumSeaGreen;
        internal static Color ColorInformation = Color.DarkOrange;

        public ReadOnlyRichTextBox()
        {
            ReadOnly = true;
            TabStop = false;
            _ = HideCaret(Handle);
        }

        public override Color BackColor => Color.White;

        internal void AppendText(string text, Color color)
        {
            if (InvokeRequired)
            {
                _ = Invoke(new MethodInvoker(() => AppendText(text, color)));
                return;
            }

            SelectionStart = TextLength;
            SelectionLength = 0;
            SelectionColor = color;
            base.AppendText(text);
            SelectionColor = ForeColor;
            Select(Text.Length, 0);
            SelectionStart = Text.Length;
            SelectionLength = 0;
            ScrollToCaret();
            
        }


        internal new void AppendText(string text)
        {
            AppendText(text, ForeColor);
        }

        internal void AppendLine(string text)
        {
            if (InvokeRequired)
            {
                _ = Invoke(new MethodInvoker(() => AppendLine(text)));
                return;
            }
            AppendText($"{text}{Environment.NewLine}");
        }

        internal void Okay()
        {
            AppendLine("OK", ColorSuccess);
        }

        internal void Fail()
        {
            AppendLine("Fail", ColorError);
        }
        internal void Fail(string reason)
        {
            AppendLine("Fail", ColorError);
            AppendLine(reason, ColorError);
        }

        internal void AppendLine(string text, Color color)
        {
            AppendText($"{text}{Environment.NewLine}", color);
        }

        internal void Append(string text)
        {
            if (InvokeRequired)
            {
                _ = Invoke(new MethodInvoker(() => Append(text)));
                return;
            }
            AppendText(text);
        }

        internal void Append(string text, Color color)
        {
            AppendText(text, color);
        }
    }
}
