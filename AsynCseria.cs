using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Amosoft.IO.Port
{
    internal class AsyncSerial : SerialPort
    {

        private const int DcbFlagAbortOnError = 14;
        private const int CommStateRetries = 10;
        private const int DefaultTimeout = 5500;
        private byte[] _buffer;
        private CommProp _commProp;
        private Dcb _dcb;

        #region Nested type: COMMPROP
        [StructLayout(LayoutKind.Sequential)]
        private struct CommProp
        {
            internal short wPacketLength;
            internal short wPacketVersion;
            internal int dwServiceMask;
            internal int dwReserved1;
            internal int dwMaxTxQueue;
            internal int dwMaxRxQueue;
            internal int dwMaxBaud;
            internal int dwProvSubType;
            internal int dwProvCapabilities;
            internal int dwSettableParams;
            internal int dwSettableBaud;
            internal short wSettableData;
            internal short wSettableStopParity;
            internal int dwCurrentTxQueue;
            internal int dwCurrentRxQueue;
            internal int dwProvSpec1;
            internal int dwProvSpec2;
            internal string wcProvChar;
        }

        #endregion

        #region Nested type: COMSTAT

        [StructLayout(LayoutKind.Sequential)]
        private struct Comstat
        {
            internal readonly uint Flags;
            internal readonly uint cbInQue;
            internal readonly uint cbOutQue;
        }

        #endregion

        #region Nested type: DCB

        [StructLayout(LayoutKind.Sequential)]
        private struct Dcb
        {
            internal readonly uint DCBlength;
            internal readonly uint BaudRate;
            internal uint Flags;
            internal readonly ushort wReserved;
            internal readonly ushort XonLim;
            internal readonly ushort XoffLim;
            internal readonly byte ByteSize;
            internal readonly byte Parity;
            internal readonly byte StopBits;
            internal readonly byte XonChar;
            internal readonly byte XoffChar;
            internal readonly byte ErrorChar;
            internal readonly byte EofChar;
            internal readonly byte EvtChar;
            internal readonly ushort wReserved1;
        }

        #endregion

        [DllImport("kernel32.dll")]
        private static extern bool GetCommProperties(SafeFileHandle hFile, ref CommProp lpCommProp);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern SafeFileHandle CreateFile(string lpFileName, int dwDesiredAccess,
            int dwShareMode, IntPtr securityAttrs, int dwCreationDisposition,
            int dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern SafeFileHandle CreateFile(
            [MarshalAs(UnmanagedType.LPTStr)] string lpFileName,
            [MarshalAs(UnmanagedType.U4)] FileAccess fileAccess,
            [MarshalAs(UnmanagedType.U4)] FileShare fileShare,
            IntPtr securityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode fileMode,
            [MarshalAs(UnmanagedType.U4)] FileAttributes fileAttributes,
            IntPtr hTemplateFile);

        private static SafeFileHandle CreateFile(string portName)
        {
            return CreateFile($@"\\.\{portName}", FileAccess.Read, FileShare.None, default, FileMode.Open, FileAttributes.Normal, default);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int FormatMessage(int dwFlags, HandleRef lpSource, int dwMessageId, int dwLanguageId,
            StringBuilder lpBuffer, int nSize, IntPtr arguments);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GetCommState(SafeFileHandle hFile, ref Dcb lpDcb);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetCommState(SafeFileHandle hFile, ref Dcb lpDcb);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool ClearCommError(SafeFileHandle hFile, ref int lpErrors, ref Comstat lpStat);

        private readonly CancellationToken _token;
        private readonly CancellationTokenRegistration _tokenRegistration;
        internal AsyncSerial(int readTimeout, int writeTimeout, CancellationToken token)
        {

            //_tokenRegistration = token.Register(Close);
            _tokenRegistration = token.Register(x =>
            {
                var port = (SerialPort)x;
                if (port.IsOpen)
                {
                    port.DiscardInBuffer();
                    port.DiscardOutBuffer();
                }
                port.Close();
            }, this);
            _token = token;
            ReadTimeout = readTimeout;
            WriteTimeout = writeTimeout;
            Encoding = Encoding.UTF8;
        }

        internal AsyncSerial(CancellationToken token) : this(DefaultTimeout, DefaultTimeout, token)
        {
        }

        internal AsyncSerial(string port, CancellationToken token) : this(DefaultTimeout, DefaultTimeout, token)
        {
            PortName = port;
        }

        internal AsyncSerial(string port, int timeout, CancellationToken token) : this(timeout, timeout, token)
        {
            PortName = port;
        }

        internal AsyncSerial(string port, TimeSpan timeout, CancellationToken token) : this(port,
            (int)timeout.TotalMilliseconds, token)
        {

        }

        internal new bool Open()
        {
            if (!SetMaxBaudRate()) return false;
            bool flag;
            try
            {
                base.Open();
                flag = true;
            }
            catch
            {
                flag = false;
            }
            return flag;
        }

        /// <summary>
        /// Sets the maximum baud rate
        /// </summary>
        /// <returns></returns>
        private bool SetMaxBaudRate()
        {
            _commProp = new CommProp();
            SafeFileHandle hFile = null;
            for (var i = 0; i < 3; i++)
            {
                try
                {
                    hFile = CreateFile(PortName);
                    if (hFile.IsInvalid)
                    {
                        if (i >= 3) return false;
                        Thread.Sleep(500);
                        continue;
                    }

                    if (hFile.IsClosed)
                    {
                        return false;
                    }
                    GetCommProperties(hFile, ref _commProp);
                    _buffer = new byte[_commProp.dwCurrentRxQueue];
                    _dcb = new Dcb();
                    GetCommStateNative(ref _dcb, hFile);
                    _dcb.Flags &= ~(1u << DcbFlagAbortOnError);
                    SetCommStateNative(ref _dcb, hFile);
                    break;
                }
                catch
                {
                    Thread.Sleep(500);
                }
                finally
                {
                    hFile?.Close();
                }
            }

            if (_commProp.dwMaxBaud <= 0)
                return false;
            BaudRate = _commProp.dwMaxBaud;
            return true;
        }

        private static void GetCommStateNative(ref Dcb lpDcb, SafeFileHandle handle)
        {
            var commErrors = 0;
            var comStat = new Comstat();

            for (var i = 0; i < CommStateRetries; i++)
            {
                if (!ClearCommError(handle, ref commErrors, ref comStat))
                {
                    WinIoError();
                }
                if (GetCommState(handle, ref lpDcb))
                {
                    break;
                }
                if (i == CommStateRetries - 1)
                {
                    WinIoError();
                }
            }
        }

        private static void SetCommStateNative(ref Dcb lpDcb, SafeFileHandle handle)
        {
            var commErrors = 0;
            var comStat = new Comstat();

            for (var i = 0; i < CommStateRetries; i++)
            {
                if (!ClearCommError(handle, ref commErrors, ref comStat))
                {
                    WinIoError();
                }
                if (SetCommState(handle, ref lpDcb))
                {
                    break;
                }
                if (i == CommStateRetries - 1)
                {
                    WinIoError();
                }
            }
        }


        internal void Write(byte[] bytes)
        {
            try
            {
                _token.ThrowIfCancellationRequested();
                Write(bytes, 0, bytes.Length);
            }
            catch (IOException)
            {
                _token.ThrowIfCancellationRequested();
                throw;
            }
        }

        internal bool Send(byte[] bytes)
        {
            Write(bytes);
            return BytesToWrite == 0;
        }

        internal byte[] Read()
        {
            try
            {
                _token.ThrowIfCancellationRequested();
                var read = Read(_buffer, 0, _buffer.Length);
                if (read == 0) throw new InvalidDataException(@"No data received");
                var buffer = new byte[read];
                Array.Copy(_buffer, buffer, read);
                return buffer;
            }
            catch (IOException)
            {
                _token.ThrowIfCancellationRequested();
                throw;
            }
        }



        //public async Task<int> ReadAsync(byte[] buffer, int offset, int count, int delay, CancellationToken cancellationToken = default)
        //{
        //    if (delay > 0)  await Task.Delay(delay, _token);
        //    return await BaseStream.ReadAsync(buffer, offset, count, cancellationToken);
        //}

        //public async Task<int> ReadAsync(byte[] f_Buffer, int f_Offset, int f_Count, int f_Delay)
        //{
        //    var register = _token.Register((param) =>
        //    {
        //        SerialPort l_Serial = (SerialPort)param;

        //        bool l_Was_Open = l_Serial.IsOpen;

        //        l_Serial.BaseStream.Close();

        //        if (l_Was_Open) l_Serial.Open();

        //    }, this);

        //    if (f_Delay > 0)
        //        await Task.Delay(f_Delay, f_Token);

        //    try
        //    {
        //        int l_Result = await f_Serial.BaseStream.ReadAsync(f_Buffer, f_Offset, f_Count, f_Token);

        //        return l_Result;
        //    }
        //    catch (System.IO.IOException Ex)
        //    {
        //        throw new OperationCanceledException("ReadSerialAsync operation Cancelled.", Ex);
        //    }
        //    finally
        //    {
        //        l_Token_Registration.Dispose();
        //    }
        //}

        //public static async Task WriteSerialAsync(this SerialPort f_Serial, byte[] f_Buffer, int f_Offset, int f_Count)
        //{
        //    await f_Serial.BaseStream.WriteAsync(f_Buffer, f_Offset, f_Count);
        //}

        //public static async Task WriteSerialAsync(this SerialPort f_Serial, byte[] f_Buffer, int f_Offset, int f_Count, CancellationToken f_Token)
        //{
        //    CancellationTokenRegistration l_Token_Registration = f_Token.Register((param) =>
        //    {
        //        SerialPort l_Serial = (SerialPort)param;

        //        bool l_Was_Open = l_Serial.IsOpen;

        //        l_Serial.BaseStream.Close();

        //        if (l_Was_Open) l_Serial.Open();

        //    }, f_Serial);

        //    try
        //    {
        //        await f_Serial.BaseStream.WriteAsync(f_Buffer, f_Offset, f_Count, f_Token);
        //    }
        //    catch (System.IO.IOException Ex)
        //    {
        //        throw new OperationCanceledException("WriteSerialAsync operation Cancelled.", Ex);
        //    }
        //    finally
        //    {
        //        l_Token_Registration.Dispose();
        //    }
        //}
        private bool _isTimeout;
        internal async Task<byte[]> ReadAsync()
        {
            try
            {
                _isTimeout = false;
                using var cts = new CancellationTokenSource(ReadTimeout);
                cts.Token.Register(x =>
                {
                    var port = (AsyncSerial)x;
                    if (!port.IsOpen) return;
                    port.DiscardInBuffer();
                    port.DiscardOutBuffer();
                    _isTimeout = true;
                }, this);
                var read = await BaseStream.ReadAsync(_buffer, 0, _buffer.Length, cts.Token);
                if (read == 0) throw new InvalidDataException(@"No data received");
                var buffer = new byte[read];
                Array.Copy(_buffer, buffer, read);
                return buffer;
            }
            catch (IOException)
            {
                ThrowTimeoutException();
                _token.ThrowIfCancellationRequested();
                throw;
            }

        }

        internal void ThrowTimeoutException()
        {
            if (_isTimeout) throw new TimeoutException();
        }


        internal async Task<bool> WriteAsync(byte[] bytes)
        {
            try
            {
                _isTimeout = false;
                using var cts = new CancellationTokenSource(WriteTimeout);
                cts.Token.Register(x =>
                {
                    var port = (SerialPort)x;
                    if (!port.IsOpen) return;
                    port.DiscardInBuffer();
                    port.DiscardOutBuffer();
                    _isTimeout = true;
                }, this);
                await BaseStream.WriteAsync(bytes, 0, bytes.Length, cts.Token);
                return BytesToWrite == 0;
            }
            catch (IOException)
            {
                ThrowTimeoutException();
                _token.ThrowIfCancellationRequested();
                throw;
            }
        }

        internal async Task<byte[]> WriteReadAsync(byte[] bytes)
        {
            await WriteAsync(bytes);
            return await ReadAsync();
        }

        internal byte[] WriteRead(byte[] bytes)
        {
            Write(bytes);
            return Read();
        }



        private static void WinIoError()
        {
            var errorCode = Marshal.GetLastWin32Error();
            throw new IOException(GetMessage(errorCode), MakeHrFromErrorCode(errorCode));
        }

        private static int MakeHrFromErrorCode(int errorCode)
        {
            return (int)(0x80070000 | (uint)errorCode);
        }

        private static string GetMessage(int errorCode)
        {
            var lpBuffer = new StringBuilder(0x200);
            return FormatMessage(0x3200, new HandleRef(null, IntPtr.Zero), errorCode, 0, lpBuffer, lpBuffer.Capacity,
                       IntPtr.Zero) != 0 ? lpBuffer.ToString() : "Unknown Error";
        }

        protected override void Dispose(bool disposing)
        {
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            _tokenRegistration.Dispose();
            base.Dispose(disposing);
        }


    }
}
