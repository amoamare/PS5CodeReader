using System.Text;

namespace PS5CodeReader
{
    internal static class SerialPortExtensions
    {
        internal static async Task<string> ReadLineAsync(this SerialPort serialPort)
        {
            var sb = new StringBuilder();
            var buffer = new byte[1];
            string? response;
            while (true)
            {
                await serialPort.BaseStream.ReadAsync(buffer.AsMemory()).ConfigureAwait(false);
                sb.Append(serialPort.Encoding.GetString(buffer));
                var newLine = StringBuilderEndsWith(sb, serialPort.NewLine);
                if (newLine)
                {
                    response = sb.ToString()[..^serialPort.NewLine.Length];
                    break;
                }
            }
            return response;
        }

        internal static async Task<string> ReadLineAsync(this SerialPort serialPort, CancellationToken cancellationToken)
        {
            var sb = new StringBuilder();
            var buffer = new byte[1];
            string? response;
            _ = cancellationToken.Register(x =>
            {
                if (x is not SerialPort port || !port.IsOpen) return;
                port.DiscardInBuffer();
                port.DiscardOutBuffer();
            }, serialPort);
            while (true)
            {
                await serialPort.BaseStream.ReadAsync(buffer.AsMemory(), cancellationToken).ConfigureAwait(false);
                sb.Append(serialPort.Encoding.GetString(buffer));
                var newLine = StringBuilderEndsWith(sb, serialPort.NewLine);
                if (newLine)
                {
                    response = sb.ToString()[..^serialPort.NewLine.Length];
                    break;
                }
            }
            return response;
        }

        internal static async Task WriteLineAsync(this SerialPort serialPort, string str, CancellationToken cancellationToken)
        {
            var data = serialPort.Encoding.GetBytes($"{str}{serialPort.NewLine}");
            await serialPort.BaseStream.WriteAsync(data, cancellationToken).ConfigureAwait(false);
            await serialPort.BaseStream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        internal static async Task WriteLineAsync(this SerialPort serialPort, string str)
        {
            var data = serialPort.Encoding.GetBytes($"{str}{serialPort.NewLine}");
            await serialPort.BaseStream.WriteAsync(data).ConfigureAwait(false);
            await serialPort.BaseStream.FlushAsync().ConfigureAwait(false);
        }

        internal static async Task<string> RequestResponseAsync(this SerialPort serialPort, string str)
        {
            await WriteLineAsync(serialPort, str).ConfigureAwait(false);
            var response = await ReadLineAsync(serialPort).ConfigureAwait(false);
            return response;
        }

        internal static async Task<string> RequestResponseAsync(this SerialPort serialPort, string str, CancellationToken cancellationToken)
        {
            await WriteLineAsync(serialPort, str, cancellationToken).ConfigureAwait(false);
            var response = await ReadLineAsync(serialPort, cancellationToken).ConfigureAwait(false);
            return response;
        }

        private static bool StringBuilderEndsWith(StringBuilder sb, string str)
        {
            if (sb.Length < str.Length) return false;
            var end = sb.ToString(sb.Length - str.Length, str.Length);
            return end.Equals(str);
        }
    }
}
