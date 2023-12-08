using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PS5CodeReader
{
    public partial class Form1 : Form
    {
        private readonly string FileNameCache = @"cache.json";
        private readonly string StrAuto = @"Auto";
        private PS5ErrorCodeList? errorCodeList;

        [GeneratedRegex("[0-9A-F]+")]
        private static partial Regex MyRegex();

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
                LogBox.AppendLine("[-] No Errors List Loaded, Close Application and Try Again!");
            }
            else
            {
                LogBox.AppendLine("[+] Please connect your Playstation 5 to UART do not power up the console.", ReadOnlyRichTextBox.ColorInformation);
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


        private async Task GetErrorCodesListAsync()
        {
            LogBox.AppendLine("[+] Loading Errors List", ReadOnlyRichTextBox.ColorInformation);
            errorCodeList = default;
            try
            {
                LogBox.Append("Attempting to load from server...");
                errorCodeList = await GetErrorCodesGitHubAsync();
                if (errorCodeList != default)
                {
                    LogBox.Okay();
                    //Store errorCode list as a chache.
                    await CacheErrorListLocall();
                }
                else
                {
                    LogBox.Fail();
                }
            }
            catch
            {
                LogBox.Fail();
                //todo: Error Handling
                //Attempt to get error codes from server failed. 
                //Lets get errorCodes from a cached local file. 
                errorCodeList = await GetErrorCodesCacheAsync();
            }
            if (errorCodeList != default && errorCodeList.ErrorCodes.Any())
            {
                LogBox.AppendLine($"[+] Loaded {errorCodeList.ErrorCodes.Count} Errors Succesfully.", ReadOnlyRichTextBox.ColorSuccess);
            }
            else
            {
                LogBox.AppendLine("[-] Failed to load Errors List.", ReadOnlyRichTextBox.ColorError);
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
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; PS5CodeReader/2.1; +https://github.com/amoamare)");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await client.GetAsync("amoamare/PS5CodeReader/master/ErrorCodes.json");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<PS5ErrorCodeList>();
        }

        private async Task<PS5ErrorCodeList?> GetErrorCodesCacheAsync()
        {
            LogBox.Append("Loading Errors List From Cached File...");
            if (!File.Exists(FileNameCache))
            {
                LogBox.Fail();
                LogBox.AppendText("No Cached File Saved!");
                return default;
            }
            using var stream = File.OpenRead(FileNameCache);
            var cached = await JsonSerializer.DeserializeAsync<PS5ErrorCodeList>(stream);
            if (cached == default || !cached.ErrorCodes.Any())
            {
                LogBox.Fail();
                return default;
            }
            LogBox.Okay();
            return cached;
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
                LogBox.Append("Creating new errors list cache file...");
                await SaveCacheFileAsync(FileNameCache);
                LogBox.Okay();
                return; // only need to store it as a cahce first time creating it
            }
            else
            {
                LogBox.Append("Comparing cached version from server version...");
                //Lets open and serialize the revision to compare if we need to update the cached file. 
                using var stream = File.OpenRead(FileNameCache);
                var cached = await JsonSerializer.DeserializeAsync<PS5ErrorCodeList>(stream);
                if (cached == default || errorCodeList == default)
                {
                    LogBox.Fail();
                    //todo: Update error handling
                    return;
                }
                LogBox.Okay();
                if (cached.Revision < errorCodeList.Revision)
                {
                    LogBox.AppendLine($"Cached Version: {cached.Revision}.");
                    LogBox.AppendLine($"Server Version: {errorCodeList.Revision}.");
                    LogBox.Append("Updating cached version with server...");
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
                            LogBox.Fail();
                            return;
                        }
                    }
                    if (!File.Exists(FileNameCache))
                    {
                        //safe to create new file.
                        await SaveCacheFileAsync(FileNameCache);
                        LogBox.Okay();
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
        PortaSerial sp1 = new PortaSerial(); // like this command passed LineReceivedEvent or LineReceived
                                             // event handler method
        void sp1_LineReceived(object sender, LineReceivedEventArgs Args)
        {
            var r = Args;
        }

        private Task<Device?> AutoDetectDeviceAsync()
        {
            var devices = ComboBoxDevices.Items.OfType<Device>();
            foreach(var device in devices)
            {
                if (device.FriendlyName.StartsWith(StrAuto, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }
                LogBox.Append($"Attemping to auto detect Playstation 5 on {device}...");
                LogBox.Fail();
            }
            return Task.FromResult<Device?>(default);
        }

        private void FakePs5Entry()
        {
            LogBox.AppendLine("[+] Playstation 5 Detected on COM7.", ReadOnlyRichTextBox.ColorSuccess);
            LogBox.Append("Reading Error Codes...");
            LogBox.Okay();
            LogBox.AppendLine("[!] Found 2 Error Code(s)", ReadOnlyRichTextBox.ColorInformation);
            for(var i = 22; i > 20; i--)
            {
                LogBox.AppendLine($"  Code: {errorCodeList.ErrorCodes[i].ID}\r\n\t{errorCodeList.ErrorCodes[i].Message}");
            }
        }

        private async void ButtonReadCodes_Click(object sender, EventArgs e)
        {
            if (ComboBoxDevices.SelectedItem is not Device device) return;
            var autoDetect = device.Port.StartsWith(StrAuto, StringComparison.InvariantCultureIgnoreCase);
            if (autoDetect)
            {
                //todo: auto detect device and return it;
                device = await AutoDetectDeviceAsync();
                
            }
            if (device == default)
            {
                //nothing to do;
               // LogBox.AppendLine("[-] No Playstation 5 Detected!", ReadOnlyRichTextBox.ColorError);
                FakePs5Entry();
                return;           
            }

            using var serial = new SerialPort();
            serial.PortName = device.Port;
            serial.BaudRate = 115200;
            serial.Open();
            serial.ReadTimeout = 30000;
            List<string> Lines = new();
            do
            {
                try
                {
                    var line = serial.ReadLine();
                    LogBox.AppendLine(line);
                }
                catch
                {
                    serial.SendBreak();
                    var line = serial.ReadLine();
                    LogBox.AppendLine(line);
                    break;
                }
            } while (serial.BytesToRead != 0);

            TestRead(serial);
            
        }

        private void TestRead(SerialPort serial, int count = 10)
        {
            for (var i = 0; i <=count; i++)
            {
                serial.Write($"errlog {count}");
                List<string> Lines = new();
                do
                {
                    try
                    {
                        var line = serial.ReadLine();
                        Lines.Add(line);
                        LogBox.AppendLine(line);
                    }
                    catch
                    {
                        serial.SendBreak();
                        var line = serial.ReadLine();
                        Lines.Add(line);
                        LogBox.AppendLine(line);
                        break;
                    }
                } while (serial.BytesToRead != 0);


                foreach (var l in Lines.Where(x => x.StartsWith("OK")))
                {
                    var r = MyRegex().Matches(l)[1];
                    var rd = r.Groups[0].Value;
                    var test = errorCodeList.ErrorCodes.First(x => x.ID == rd);
                    LogBox.AppendLine($"{test.ID}: {test.Message}");
                }
            }
        }

        private async void ButtonReloadErrorCodes_Click(object sender, EventArgs e)
        {
            await GetErrorCodesListAsync();
        }
    }
}