using System.Runtime.InteropServices;

namespace PS5CodeReader
{
    public class ReadOnlyRichTextBox : RichTextBox
    {

        [DllImport("user32.dll")]
        private static extern int HideCaret(IntPtr hwnd);

        internal static Color ColorError = Color.IndianRed;
        internal static Color ColorSuccess = Color.MediumSeaGreen;
        internal static Color ColorInformation = Color.DarkOrange;

        public ReadOnlyRichTextBox()
        {
            MouseDown += ReadOnlyRichTextBox_Mouse;
            MouseUp += ReadOnlyRichTextBox_Mouse;
            Resize += ReadOnlyRichTextBox_Resize;
            ReadOnly = true;
            TabStop = false;
            _ = HideCaret(Handle);
        }

        public override Color BackColor => Color.White;


        protected override void OnGotFocus(EventArgs e)
        {
            _ = HideCaret(Handle);
        }

        protected override void OnEnter(EventArgs e)
        {
            _ = HideCaret(Handle);
        }

        private void ReadOnlyRichTextBox_Mouse(object sender, MouseEventArgs e)
        {
            _ = HideCaret(Handle);
        }

        private void InitializeComponent()
        {
            //
            // ReadOnlyRichTextBox
            //
            Resize += ReadOnlyRichTextBox_Resize;
            BorderStyle = BorderStyle.None;
        }

        private void ReadOnlyRichTextBox_Resize(object sender, EventArgs e)
        {
            _ = HideCaret(Handle);
        }

        internal void AppendText(string text, Color color)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => AppendText(text, color)));
                return;
            }

            SelectionStart = TextLength;
            SelectionLength = 0;
            SelectionColor = color;
            AppendText(text);
            SelectionColor = ForeColor;

            Select(Text.Length, 0);
            SelectionStart = Text.Length;

            SelectionLength = 0; ;
            ScrollToCaret();
        }

        internal void AppendLine(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => AppendLine(text)));
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
                Invoke(new MethodInvoker(() => Append(text)));
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
