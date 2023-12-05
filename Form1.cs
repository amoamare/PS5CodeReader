using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Net.Http.Headers;
using System.Text.Json;

namespace PS5CodeReader
{
    public partial class Form1 : Form
    {
        private readonly string FileNameCache = @"cache.json";
        private PS5ErrorCodeList? errorCodeList;
        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            LoadPorts();
            await GetErrorCodesListAsync();
            if (errorCodeList == default)
            {
                textBox1.AppendText("Unable to load error codes from either internet or cachce.");
                textBox1.AppendText("Place error codes list in same directory as cache.json,");
                textBox1.AppendText("Or Reload PS5 Errorcodes");
            }

        }
        private void ComboBoxDevices_DropDown(object sender, EventArgs e)
        {
            ComboBoxDevices.DataSource = SerialPort.SelectSerial().ToList();
        }

        private void LoadPorts()
        {
            ComboBoxDevices.DataSource = SerialPort.SelectSerial().ToList();
        }

        PortaSerial sp1 = new PortaSerial(); // like this command passed LineReceivedEvent or LineReceived
                                             // event handler method
        void sp1_LineReceived(object sender, LineReceivedEventArgs Args)
        {
            //do things with line
            //  MessageBox.ShowDialog(Args.LineData);
        }
        private async Task GetErrorCodesListAsync()
        {
            errorCodeList = default;
            try
            {
                errorCodeList = await GetErrorCodesGitHubAsync();
                if (errorCodeList != default)
                {
                    //Store errorCode list as a chache.
                    await CacheErrorListLocall();
                }
            }
            catch
            {
                //todo: Error Handling
                //Attempt to get error codes from server failed. 
                //Lets get errorCodes from a cached local file. 
                errorCodeList = GetErrorCodesCache();
            }
        }

        /// <summary>
        /// Get List of Error Codes for the PS5 from Git hub server.
        /// </summary>
        /// <returns>Error Code List</returns>
        private static async Task<PS5ErrorCodeList?> GetErrorCodesGitHubAsync()
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri("https://raw.githubusercontent.com/");
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await client.GetAsync("amoamare/ps5codereader/main/errorcodes.json");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<PS5ErrorCodeList>();
        }

        private static PS5ErrorCodeList? GetErrorCodesCache()
        {
            return default;
        }

        private async Task SaveCacheFileAsync(string fileName)
        {
            using var stream = File.Create(fileName);
            var options = new JsonSerializerOptions { WriteIndented = true };
            await JsonSerializer.SerializeAsync(stream, errorCodeList, options: options);
            await stream.DisposeAsync();
            return; // only need to store it as a cahce first time creating it
        }

        private async Task CacheErrorListLocall()
        {
            if (errorCodeList == default)
            {
                //Can't save what we don't have right?
                return;
            }
            if (!File.Exists(FileNameCache) && errorCodeList != default)
            {
                await SaveCacheFileAsync(FileNameCache);
                return; // only need to store it as a cahce first time creating it
            }
            else
            {
                //Lets open and serialize the revision to compare if we need to update the cached file. 
                using var stream = File.OpenRead(FileNameCache);
                var cached = await JsonSerializer.DeserializeAsync<PS5ErrorCodeList>(stream);
                if (cached == default || errorCodeList == default)
                {
                    //todo: Update error handling
                    return;
                }
                if (cached.Revision < errorCodeList.Revision)
                {
                    //Our downloaded error codes have updated. Lets update the cached version.
                    await stream.DisposeAsync();
                    try
                    {
                        File.Delete(FileNameCache);
                    }
                    catch
                    {
                        try
                        {
                            File.Move(FileNameCache, $"{FileNameCache}.old");
                        }
                        catch
                        {
                            //todo: update error handling if we can not delete or move the file.
                        }
                    }
                    if (!File.Exists(FileNameCache))
                    {
                        //safe to create new file.
                        await SaveCacheFileAsync(FileNameCache);
                    }
                }
            }
        }

        private void TryNext()
        {
            //add handler to event
            sp1.LineReceived += new LineReceivedEventHandler(sp1_LineReceived);
            //   cmbPortas.Items.AddRange(sp1.GetPortas());
            //   cmbVelocidade.Items.AddRange(sp1.GetVelocidades());

        }

        private void ButtonReadCodes_Click(object sender, EventArgs e)
        {
            var devices = SerialPort.SelectSerial();
            var port = devices.First().Port;
            using var serial = new SerialPort();
            serial.PortName = port;
            serial.BaudRate = 115200;
            serial.Open();
            serial.ReadTimeout = 1000;
            string line = string.Empty;
            textBox1.AppendText("Waiting for ps5");
            while (string.IsNullOrEmpty(line))
            {
                try
                {
                    line = serial.ReadLine();
                }
                catch { }

                textBox1.AppendText(".");
            }


            textBox1.AppendText(line);
            //      Thread.Sleep(1000);
            serial.Write("errlog 0");
            serial.ReadTimeout = 1000;
            textBox1.AppendText(serial.ReadLine());
            textBox1.AppendText(serial.ReadLine());
        }

        private async void ButtonReloadErrorCodes_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBox1.AppendText("Updating error codes database.\r\n");
            await GetErrorCodesListAsync();
            if (errorCodeList == default)
            {
                textBox1.AppendText("Unable to load error codes from either internet or cachce.");
                textBox1.AppendText("Place error codes list in same directory as cache.json,");
                textBox1.AppendText("Or Reload PS5 Errorcodes");
            }
            else
            {
                textBox1.AppendText($"Database Revision: {errorCodeList.Revision}\r\n{errorCodeList.Description}\r\n");
            }
        }

 
    }
}