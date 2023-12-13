using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace PS5CodeReader
{
    public partial class Form1 : Form
    {
        private readonly string FileNameCache = @"cache.json";
        private readonly string StrAuto = @"Auto";
        private PS5ErrorCodeList? errorCodeList;
        private CancellationTokenSource? cancellationTokenSource;

        [GeneratedRegex("[0-9A-F]+")]
        private static partial Regex MyRegex();

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            var text = @"NG E0000003:4D";

 //NG E0000004:4E"
            var error = @"OK 00000000 0000000 0000000 00000000 000000000 0000 0000 00000 0000:FC";
            var split = error.Split(" ", StringSplitOptions.TrimEntries);
            var r = split;
            LoadDatabaseTypes();
            LoadOperationTypes();
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
            LoadPorts();
        }


        #region Data Source Information

        private void LoadPorts()
        {
            ComboBoxDevices.DataSource = SerialPort.SelectSerial();
        }

        private void LoadDatabaseTypes()
        {
            ComboBoxDeviceType.EnumForComboBox<DataBaseType>();
            ComboBoxDeviceType.DisplayMember = "Description";
            ComboBoxDeviceType.ValueMember = "Value";
        }

        private void LoadOperationTypes()
        {
            ComboBoxOperationType.EnumForComboBox<OperationType>();
            ComboBoxOperationType.DisplayMember = "Description";
            ComboBoxOperationType.ValueMember = "Value";
        }

        #endregion

        #region Error Codes List From Server

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

        #endregion

        private bool InterfaceState
        {
            set
            {

                //ButtonRunOperation.Enabled = value;
                ButtonRunOperation.Text = value ? @"Run Operation" : @"Cancel";
                ButtonRunOperation.Tag = !value;
                ComboBoxDevices.Enabled = value;
                ComboBoxDeviceType.Enabled = value;
                ComboBoxOperationType.Enabled = value;
            }
        }

        private async Task<Device?> AutoDetectDeviceAsync()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
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

        private async void ButtonRunOperations_Click(object sender, EventArgs e)
        {
            if (ComboBoxOperationType.SelectedValue is not OperationType type) return;
            try
            {
                LogBox.Clear();

                if (ButtonRunOperation.Tag is not null && ButtonRunOperation.Tag is bool cancel && cancel
                    && cancellationTokenSource != null)
                {
                    cancellationTokenSource.Cancel(false);
                    return;
                }
                InterfaceState = false;
                await RunOperationsAsync(type);
            }
            catch (Exception ex)
            {
                //todo: add error handling
                Debug.WriteLine(ex);
            }
            finally
            {
                InterfaceState = true;
            }
        }

        private async Task RunOperationsAsync(OperationType type)
        {
            switch (type)
            {
                default: return;
                case OperationType.ReadErrorCodes:
                    LogBox.AppendLine("[*] Operation: Read UART Codes.", ReadOnlyRichTextBox.ColorError);
                    await ReadCodesAsync();
                    break;
                case OperationType.ClearErrorCodes:
                    LogBox.AppendLine("[*] Operation: Clear UART Codes.", ReadOnlyRichTextBox.ColorError);
                    await ClearLogsAsync();
                    break;
                case OperationType.MonitorMode:
                    LogBox.AppendLine("[*] Operation: Running Monitor Mode.", ReadOnlyRichTextBox.ColorError);
                    await RunMonitorModeAsync();
                    break;
                case OperationType.RunCommandList:
                    LogBox.AppendLine("[*] Operation: Run Command List.", ReadOnlyRichTextBox.ColorError);
                    await RunCommmandListAsync();
                    break;

            }
        }

        #region Run Operation Types

        private async Task ReadData(SerialPort serial, string command, CancellationTokenSource cts)
        {
            await serial.WriteLineAsync(command);
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
                    await serial.SendBreakAsync(cancellationToken: cts.Token);
                    var line = await serial.ReadLineAsync(cts.Token);
                    Lines.Add(line);
                    break;
                }
            } while (serial.BytesToRead != 0);
            foreach (var l in Lines)
            {
                LogBox.AppendLine(l);
            }
        }

        private async Task ReadCodesAsync(int count = 10)
        {
            Device? device = ComboBoxDevices.SelectedItem as Device;
            if (device == default || errorCodeList == null) return;
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
            for (var i = 0; i <= count; i++)
            {
                var command = $"errlog {i}";
                await serial.WriteLineAsync(command);
                List<string> Lines = new();
                do
                {
                    try
                    {
                        var line = await serial.ReadLineAsync(cts.Token);
                        if (!string.Equals(command, line, StringComparison.InvariantCultureIgnoreCase))
                        {
                            //ignore the echo'd command capture everything else. 
                            Lines.Add(line);
                        }
                    }
                    catch
                    {
                        await serial.SendBreakAsync(cancellationToken: cts.Token);
                        var line = await serial.ReadLineAsync(cts.Token);
                        Lines.Add(line);
                        break;
                    }
                } while (serial.BytesToRead != 0);
                LogBox.AppendLine($@"Error {i}");

                //OK 00000000 0000000 0000000 00000000 000000000 0000 0000 00000 0000:FC
                foreach (var l in Lines.Where(x => x.StartsWith("OK")))
                {
                    var r = MyRegex().Matches(l)[1];
                    var rd = r.Groups[0].Value;
                    var test = errorCodeList.ErrorCodes.First(x => x.ID == rd);
                    LogBox.AppendLine($"{test.ID}: {test.Message}");
                }
            }
        }

        private async Task ClearLogsAsync()
        {
            Device? device = ComboBoxDevices.SelectedItem as Device;
            if (device == default || errorCodeList == null) return;
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
            cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            using var serial = new SerialPort(device.Port);
            serial.Open();
            LogBox.Append("[+]\tClearing Logs...", ReadOnlyRichTextBox.ColorInformation);
            await serial.WriteLineAsync("errlog clear", cancellationTokenSource.Token);
            LogBox.Okay();         
        }

        private async Task RunMonitorModeAsync()
        {
            Device? device = ComboBoxDevices.SelectedItem as Device;
            if (device == default || errorCodeList == null) return;
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
            cancellationTokenSource = new CancellationTokenSource();
            using var serial = new SerialPort(device.Port);
            serial.Open();
            do
            {
                var line = await serial.ReadLineAsync(cancellationTokenSource.Token);
                LogBox.AppendLine(line);

            } while (!cancellationTokenSource.IsCancellationRequested);
            //todo:
        }

        private async Task RunCommmandListAsync()
        {
            Device? device = ComboBoxDevices.SelectedItem as Device;
            if (device == default || errorCodeList == null) return;
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

            using var ofd = new OpenFileDialog();
            ofd.InitialDirectory = Directory.GetCurrentDirectory();
            ofd.RestoreDirectory = true;
            ofd.Title = @"Select Command List";
            ofd.DefaultExt = @"txt";
            ofd.Filter = @"txt files (*.txt)|*.txt";
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            if (ofd.ShowDialog() != DialogResult.OK) return;
            FileInfo file = new (ofd.FileName);
            using var stream = new StreamReader(file.FullName);
            string? line = default;
            cancellationTokenSource = new CancellationTokenSource();
            using var serial = new SerialPort(device.Port);
            serial.Open(); 
            do
            {
                line = await stream.ReadLineAsync();
                if (string.IsNullOrEmpty(line)) continue;
                await serial.WriteLineAsync(line, cancellationTokenSource.Token);
                do
                {
                    try
                    {
                        line = await serial.ReadLineAsync(cancellationTokenSource.Token);
                        LogBox.AppendLine(line);
                    }
                    catch
                    {
                       
                    }
                } while (serial.BytesToRead != 0);
            } while (!stream.EndOfStream);
        }

        #endregion

        
    }
}