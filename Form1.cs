using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;

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

        private async Task<Device?> AutoDetectDeviceAsync()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var devices = ComboBoxDevices.Items.OfType<Device>();
            foreach (var device in devices)
            {
                if (!string.IsNullOrEmpty(device.FriendlyName) && device.Port.StartsWith(StrAuto, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }
                LogBox.AppendLine($"[*] Auto Detecting Playstation 5 on {device}", ReadOnlyRichTextBox.ColorInformation);
                LogBox.AppendLine("\t- Disconnect power cord from PS5\r\n\t- Wait 5 seconds.\r\n\t- Connect Power to PS5 due not power on!", ReadOnlyRichTextBox.ColorError);
                using var serial = new SerialPort(device.Port);
                LogBox.Append($"Opening Device on {device.FriendlyName}...");
                serial.Open();
                LogBox.Okay();
                LogBox.AppendLine("[*] Listening for Playstation 5.", ReadOnlyRichTextBox.ColorInformation);
                List<string> Lines = new();
                do
                {
                    try
                    {
                        var line = await serial.ReadLineAsync(cts.Token);
                        Lines.Add(line);
                    }
                    catch
                    {
                        try
                        {
                            serial.SendBreak();
                            if (cts.IsCancellationRequested)
                                cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                            var line = await serial.ReadLineAsync(cts.Token);
                            Lines.Add(line);
                            break;
                        }
                        catch
                        {

                        }
                    }
                } while (serial.BytesToRead != 0);

                var flag = Lines.Any(x => x.StartsWith(@"$$ [MANU] UART CMD READY:36") || x.StartsWith(@"NG E0000003:4D") || x.StartsWith("OK 00000000:3A"));
                if (flag)
                {
                    LogBox.AppendLine($@"[+] Detected a Playstation 5 on {device.FriendlyName}", ReadOnlyRichTextBox.ColorSuccess);
                    ComboBoxDevices.SelectedItem = device;
                    return device;
                }
            }
            return default;
        }

        private async void ButtonReadCodes_Click(object sender, EventArgs e)
        {
            await RunOperationsAsync(ButtonReadCodes);      
        }

        private void TestRead(SerialPort serial, int count = 10)
        {
            for (var i = 0; i <= count; i++)
            {
                serial.Write($"errlog {i}");
                List<string> Lines = new();
                do
                {
                    try
                    {
                        var line = serial.ReadLine();
                        Lines.Add(line);
                        //LogBox.AppendLine(line);
                    }
                    catch
                    {
                        serial.SendBreak();
                        var line = serial.ReadLine();
                        Lines.Add(line);
                        //LogBox.AppendLine(line);
                        break;
                    }
                } while (serial.BytesToRead != 0);
                LogBox.AppendLine($@"Error {i}");
                foreach (var l in Lines.Where(x => x.StartsWith("OK")))
                {
                    var r = MyRegex().Matches(l)[1];
                    var rd = r.Groups[0].Value;
                    var test = errorCodeList.ErrorCodes.First(x => x.ID == rd);
                    LogBox.AppendLine($"{test.ID}: {test.Message}");
                }
            }
        }


        private async void ButtonClearLogs_Click(object sender, EventArgs e)
        {
            await RunOperationsAsync(ButtonClearLogs);
        }

        private bool _interfaceState;
        private bool InterfaceState
        {
            get => _interfaceState; 
            set
            {

                ButtonReadCodes.Enabled = value;
                ButtonClearLogs.Enabled = value;
                ComboBoxDevices.Enabled = value;
            }
        }
        private async Task RunOperationsAsync(object caller, [CallerMemberName] string membername = "")
        {
            LogBox.Clear();
            InterfaceState = false;
            try
            {
                if (caller == ButtonReadCodes)
                {
                    LogBox.AppendLine("[*] Operation: Read UART Codes.", ReadOnlyRichTextBox.ColorError);
                    await ReadCodesAsync();
                }
                else if (caller == ButtonClearLogs)
                {
                    LogBox.AppendLine("[*] Operation: Clear UART Codes.", ReadOnlyRichTextBox.ColorError);
                    await ClearLogsAsync();
                }
            }
            catch
            {
                //todo: Error Handling;
            }
            finally
            {
                InterfaceState = true;
            }
        }

        private async Task ReadCodesAsync()
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
                LogBox.AppendLine("[-] No Playstation 5 Detected!", ReadOnlyRichTextBox.ColorError);
                return;
            }
            using var serial = new SerialPort(device.Port);
            serial.Open();
            TestRead(serial);
        }

        private async Task ClearLogsAsync()
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
                LogBox.AppendLine("[-] No Playstation 5 Detected!", ReadOnlyRichTextBox.ColorError);
                return;
            }
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            using var serial = new SerialPort(device.Port);
            serial.Open();
            LogBox.Append("[+]\tClearing Logs...", ReadOnlyRichTextBox.ColorInformation);
            var l = await serial.WriteLineAsync("errlog clear", cts.Token);
            LogBox.Okay();
            LogBox.AppendLine(l);
            l = await serial.ReadLineAsync(cts.Token);
            LogBox.AppendLine(l);
        }
    }
}