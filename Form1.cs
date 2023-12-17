using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Diagnostics;
using Microsoft.VisualStudio.Threading;

namespace PS5CodeReader
{
    public partial class Form1 : Form
    {
        private readonly string FileNameCache = @"cache.json";
        private readonly string StrAuto = @"Auto";
        private PS5ErrorCodeList? errorCodeList;
        private CancellationTokenSource? cancellationTokenSource;

        /*
         * Possible Commands
         * version
         * bringup
         * shutdown
         * firmud
         * bsn
         * halt
         * cp ready
         * cp busy
         * cp reset
         * bestat
         * powersw
         * resetsw
         * bootbeep stat
         * bootbeep on
         * bootbeep off
         * reset syscon
         * xdrdiag info
         * xdrdiag start
         * xdrdiag result
         * xiodiag
         * fandiag
         * errlog 
         * readline
         * tmpforcp <zone id>
         * cp beepreote
         * cp beep2kn1n3
         * cp beep2kn2n3
         * csum
         * osbo
         * scopen
         * scclose
         * ejectsw
         */


        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            LoadDatabaseTypes();
            LoadOperationTypes();
            LoadPorts();
            ComboBoxOperationType.SelectedValueChanged += ComboBoxOperationType_SelectedValueChanged;
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

        private void ComboBoxOperationType_SelectedValueChanged(object? sender, EventArgs e)
        {
            PanelRawCommand.Visible = ComboBoxOperationType.SelectedValue is OperationType type && type == OperationType.RunRawCommand;
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
                    await CacheErrorListLocalAsync();
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
            if (errorCodeList != default && errorCodeList.PlayStation5 != null && errorCodeList.PlayStation5.ErrorCodes.Any())
            {
                LogBox.AppendLine($"[+] Loaded {errorCodeList.PlayStation5.ErrorCodes.Count} Errors Succesfully.", ReadOnlyRichTextBox.ColorSuccess);
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


        /// <summary>
        /// Gets a list of Error Codes from Cached File on System
        /// </summary>
        /// <returns></returns>
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
            if (cached == default || cached.PlayStation5 != default && !cached.PlayStation5.ErrorCodes.Any())
            {
                LogBox.Fail();
                return default;
            }
            LogBox.Okay();
            return cached;
        }


        /// <summary>
        /// Save error codes to a cached file on system
        /// </summary>
        /// <param name="fileName">cached.json</param>
        /// <returns></returns>
        private async Task SaveCacheFileAsync(string fileName)
        {
            using var stream = File.Create(fileName);
            var options = new JsonSerializerOptions { WriteIndented = true };
            await JsonSerializer.SerializeAsync(stream, errorCodeList, options: options);
            await stream.DisposeAsync();
            return; // only need to store it as a cahce first time creating it
        }


        /// <summary>
        /// Cachce error list on local disk
        /// </summary>
        /// <returns></returns>
        private async Task CacheErrorListLocalAsync()
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
                ButtonRunOperation.Text = value ? @"Run Operation" : @"Cancel";
                ButtonRunOperation.Tag = !value;
                ComboBoxDevices.Enabled = value;
                ComboBoxDeviceType.Enabled = value;
                ComboBoxOperationType.Enabled = value;
                TextBoxRawCommand.Enabled = !value;
            }
        }

        private async Task<Device?> AutoDetectDeviceAsync()
        {
            Device? device = ComboBoxDevices.SelectedItem as Device;
            if (device == default || errorCodeList == null) return default;
            var autoDetect = device.Port.StartsWith(StrAuto, StringComparison.InvariantCultureIgnoreCase);
            if (!autoDetect) return device;
            var devices = ComboBoxDevices.Items.OfType<Device>().ToList();
            devices.Remove(device); // remove the auto detect device. 
            if (devices.Count == 1) return devices.FirstOrDefault(); //if only 1 device is detected we can just skipp detecting it.
            cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            foreach (var autoDevice in devices)
            {
                LogBox.AppendLine($"[*] Auto Detecting Playstation 5 on {autoDevice}", ReadOnlyRichTextBox.ColorInformation);
                LogBox.AppendLine("\t- Disconnect power cord from PS5\r\n\t- Wait 5 seconds.\r\n\t- Connect Power to PS5 due not power on!", ReadOnlyRichTextBox.ColorError);
                using var serial = new SerialPort(autoDevice.Port);
                LogBox.Append($"Opening Device on {autoDevice.FriendlyName}...");
                serial.Open();
                LogBox.Okay();
                LogBox.AppendLine("[*] Listening for Playstation 5.", ReadOnlyRichTextBox.ColorInformation);
                List<string> Lines = new();
                do
                {
                    try
                    {
                        var line = await serial.ReadLineAsync(cancellationTokenSource.Token);
                        Lines.Add(line);
                    }
                    catch (OperationCanceledException)
                    {
                        cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                        await serial.SendBreakAsync(cancellationToken: cancellationTokenSource.Token);
                        var line = await serial.ReadLineAsync(cancellationTokenSource.Token);
                        Lines.Add(line);
                    }
                } while (serial.BytesToRead != 0);

                var flag = Lines.Any(x => x.StartsWith(@"$$ [MANU] UART CMD READY:36") || x.StartsWith(@"NG E0000003:4D") || x.StartsWith("OK 00000000:3A"));
                if (flag)
                {
                    LogBox.AppendLine($@"[+] Detected a Playstation 5 on {autoDevice.FriendlyName}", ReadOnlyRichTextBox.ColorSuccess);
                    ComboBoxDevices.SelectedItem = autoDevice;
                    return autoDevice;
                }
            }
            return default;
        }

        private async void ButtonRunOperations_Click(object sender, EventArgs e)
        {
            if (ComboBoxOperationType.SelectedValue is not OperationType type) return;
            try
            {
                if (ButtonRunOperation.Tag is not null && ButtonRunOperation.Tag is bool cancel && cancel
                    && cancellationTokenSource != null)
                {
                    cancellationTokenSource.Cancel(false);
                    return;
                }

                LogBox.Clear();
                InterfaceState = false;
                await RunOperationsAsync(type);
            }
            catch (OperationCanceledException)
            {
                LogBox.AppendLine("[!] Operation Cancelled");
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
                case OperationType.RunRawCommand:
                    LogBox.AppendLine("[*] Operation: Run Raw Command.", ReadOnlyRichTextBox.ColorError);
                    await RunRawCommandAsync();
                    break;

            }
        }

        #region Run Operation Types

        /// <summary>
        /// Read past 10 error logs
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        private async Task ReadCodesAsync(int count = 10)
        {
            var device = await AutoDetectDeviceAsync();
            if (device == default)
            {
                LogBox.AppendLine("[-] No Playstation 5 Detected!", ReadOnlyRichTextBox.ColorError);
                return;
            }
            cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            using var serial = new SerialPort(device.Port);
            serial.Open();
            List<string> Lines = new();
            for (var i = 0; i <= count; i++)
            {
                var command = $"errlog {i}";
                var checksum = SerialPort.CalculateChecksum(command);
                await serial.WriteLineAsync(command);
                do
                {
                    var line = await serial.ReadLineAsync(cancellationTokenSource.Token);
                    if (!string.Equals($"{command}:{checksum:X2}", line, StringComparison.InvariantCultureIgnoreCase))
                    {
                        //ignore the echo'd command capture everything else. 
                        Lines.Add(line);
                    }
                } while (serial.BytesToRead != 0);
            }

            foreach (var l in Lines)
            {
                var split = l.Split(' ');
                if (!split.Any()) continue;
                switch (split[0])
                {
                    case "NG":
                        LogBox.AppendLine("Failed to read data");
                        break;
                    case "OK":
                        var errorCode = split[2];
                        var errorLookup = errorCodeList.PlayStation5.ErrorCodes.First(x => x.ID == errorCode);
                        LogBox.AppendLine($"{errorLookup.ID}: {errorLookup.Message}");
                        break;
                }
            }
        }

        /// <summary>
        /// Clears Error Logs
        /// </summary>
        /// <returns></returns>
        private async Task ClearLogsAsync()
        {
            var device = await AutoDetectDeviceAsync();
            if (device == default)
            {
                LogBox.AppendLine("[-] No Playstation 5 Detected!", ReadOnlyRichTextBox.ColorError);
                return;
            }
            cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            using var serial = new SerialPort(device.Port);
            serial.Open();
            LogBox.Append("[+]\tClearing Logs...", ReadOnlyRichTextBox.ColorInformation);
            var command = "errlog clear";
            var checksum = SerialPort.CalculateChecksum(command);
            await serial.WriteLineAsync("errlog clear", cancellationTokenSource.Token);
            string? response = default;
            do
            {
                var line = await serial.ReadLineAsync(cancellationTokenSource.Token);
                if (!string.Equals($"{command}:{checksum:X2}", line, StringComparison.InvariantCultureIgnoreCase))
                {
                    //ignore the echo'd command capture everything else. 
                    response = line;
                }
            } while (serial.BytesToRead != 0);
            var split = response?.Split(' ');
            if (split == default || split.Any())
            {
                LogBox.Okay();
                return;
            }
            switch (split[0])
            {
                case "NG":
                    LogBox.Fail();
                    break;
                case "OK":
                    LogBox.Okay();
                    break;
            }
        }


        /// <summary>
        /// Run in monitor mode. This will listen to anything the console might be saying. 
        /// </summary>
        /// <returns></returns>
        private async Task RunMonitorModeAsync()
        {
            var device = await AutoDetectDeviceAsync();
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
        }

        /// <summary>
        /// Run a list of commands saved in a text file. 
        /// </summary>
        /// <returns></returns>
        private async Task RunCommmandListAsync()
        {
            var device = await AutoDetectDeviceAsync();
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
            FileInfo file = new(ofd.FileName);
            using var stream = new StreamReader(file.FullName);
            string? command = default;
            cancellationTokenSource = new CancellationTokenSource();
            using var serial = new SerialPort(device.Port);
            serial.Open();
            do
            {
                command = await stream.ReadLineAsync();
                if (string.IsNullOrEmpty(command)) continue;
                await serial.WriteLineAsync(command, cancellationTokenSource.Token);
                do
                {
                    var response = await serial.ReadLineAsync(cancellationTokenSource.Token);
                    LogBox.AppendLine(response);

                } while (serial.BytesToRead != 0);
            } while (!stream.EndOfStream);
        }


        private readonly AsyncAutoResetEvent AutoResetEventRawCommand = new AsyncAutoResetEvent(false);
        /// <summary>
        /// Run raw command from user. Keeps port open.
        /// </summary>
        /// <returns></returns>
        private async Task RunRawCommandAsync()
        {
            var device = await AutoDetectDeviceAsync();
            if (device == default)
            {
                LogBox.AppendLine("[-] No Playstation 5 Detected!", ReadOnlyRichTextBox.ColorError);
                return;
            }
            using var serial = new SerialPort(device.Port);
            serial.Open();
            do
            {
                cancellationTokenSource = new CancellationTokenSource();
                await AutoResetEventRawCommand.WaitAsync(cancellationTokenSource.Token);
                var command = TextBoxRawCommand.Text.Trim();
                TextBoxRawCommand.Clear();
                cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                await serial.WriteLineAsync(command);
                do
                {
                    var line = await serial.ReadLineAsync(cancellationTokenSource.Token);
                    LogBox.AppendLine(line);
                } while (serial.BytesToRead != 0);
            } while (!cancellationTokenSource.IsCancellationRequested);
        }
        #endregion

        private void TextBoxRawCommand_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!TextBoxRawCommand.Text.Any()) return; // dont send empty commands
            if (e.KeyChar == (char)Keys.Enter)
            {
                AutoResetEventRawCommand.Set();
            }
        }
    }
}