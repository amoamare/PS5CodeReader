using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS5CodeReader
{
    //delcare an event args clas
    public class LineReceivedEventArgs : EventArgs
    {
        //Data to pass to the event
        public string LineData { get; private set; }
        public LineReceivedEventArgs(string lineData)
        {
            LineData = lineData;
        }
    }
    //declare a delegate
    public delegate void LineReceivedEventHandler(object sender, LineReceivedEventArgs Args);
    public class PortaSerial  //: IDisposable
    {
        private readonly SerialPort serialPort;
        private readonly Queue recievedData = new ();
        //add event to class
        public event LineReceivedEventHandler? LineReceived;
        public PortaSerial()
        {
            serialPort = new SerialPort();
            serialPort.DataReceived += SerialPort_DataReceived;
        }
        public void Abrir(string porta, int velocidade)
        {
            serialPort.PortName = porta;
            serialPort.BaudRate = velocidade;
            serialPort.Open();
        }
        public string[] GetPortas()
        {
            return System.IO.Ports.SerialPort.GetPortNames();
        }
        public string[] GetVelocidades()
        {
            return new string[] { "1200", "2400", "4800", "9600", "19200", "38400", "57600", "115200" };
        }
        void SerialPort_DataReceived(object s, SerialDataReceivedEventArgs e)
        {
            byte[] data = new byte[serialPort.BytesToRead];
            serialPort.Read(data, 0, data.Length);
            data.ToList().ForEach(b => recievedData.Enqueue(b));
            ProcessData();
            //raise event here
            LineReceived?.Invoke(this, new LineReceivedEventArgs("some line data"));
        }
        private void ProcessData()
        {
            // Determine if we have a "packet" in the queue
            if (recievedData.Count > 50)
            {
                var packet = Enumerable.Range(0, 50).Select(i => recievedData.Dequeue());
            }
        }
        public void Dispose()
        {
            serialPort?.Dispose();
        }
    }
}
