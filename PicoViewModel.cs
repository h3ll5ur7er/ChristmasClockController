using System.IO.Ports;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Windows;
using System;

namespace ChristmasClockController {
    public class PicoViewModel : ViewModel {
        private string _comPort = string.Empty;
        private string _consoleInput = string.Empty;
        private string _firmwareDirectory = "~/pico/ChristmasClockSDK";
        private string _buffer = string.Empty;
        private string _selectedLine = string.Empty;
        public ObservableCollection<string> ComPorts { get; init; }
        public string ComPort {
            get => _comPort;
            set {
                Set(ref _comPort, value);
                OnPropertyChanged(nameof(ComPortSelected));
                OnPropertyChanged(nameof(Connected));
                OnPropertyChanged(nameof(Disconnected));
            }
        }
        public Visibility ComPortSelected  => string.IsNullOrWhiteSpace(_comPort) ? Visibility.Collapsed : Visibility.Visible;

        public Visibility Connected => ((ComPortSelected == Visibility.Visible) && (_serialPort?.IsOpen??false)) ? Visibility.Visible : Visibility.Collapsed;
        
        public Visibility Disconnected => ((ComPortSelected == Visibility.Visible) && !(_serialPort?.IsOpen??false)) ? Visibility.Visible : Visibility.Collapsed;
        
        public ObservableCollection<string> Console { get; init; }
        public string FirmwareDirectory {
            get => _firmwareDirectory;
            set => Set(ref _firmwareDirectory, value);
        }
        public string ConsoleInput {
            get => _consoleInput;
            set => Set(ref _consoleInput, value);
        }
        public string SelectedLine {
            get => _selectedLine;
            set {
                Set(ref _selectedLine, value);
                Trace.WriteLine(value);
                try{
                    Clipboard.SetText(value);
                }catch(Exception){}
            }
        }
        SerialPort? _serialPort;

        public RelayCommand GetAvailableComPorts => new RelayCommand(getAvailableComPorts);
        public RelayCommand Connect => new RelayCommand(connect);
        public RelayCommand Disconnect => new RelayCommand(disconnect);
        public RelayCommand BootMode => new RelayCommand(bootMode);
        public RelayCommand Build => new RelayCommand(build);
        public RelayCommand Deploy => new RelayCommand(deploy);
        public RelayCommand Send => new RelayCommand(send);
        public RelayCommand Clear => new RelayCommand(clear);

        public PicoViewModel(){
            Console = new ObservableCollection<string>();
            ComPorts = new ObservableCollection<string>();
        }

        private void getAvailableComPorts() {
            ComPorts.Clear();
            foreach(var p in SerialPort.GetPortNames()){
                ComPorts.Add(p);
            }
        }

        private void disconnect() {
            if(_serialPort?.IsOpen ?? false) _serialPort.Close();
            _serialPort = null;
            App.Current.MainWindow.Title = "Disconnected";

            OnPropertyChanged(nameof(Connected));
            OnPropertyChanged(nameof(Disconnected));
        }

        private void connect() {
            disconnect();
            _serialPort = new SerialPort(ComPort, 115200);
            _serialPort.Open();
            _serialPort.DtrEnable = true;
            _serialPort.RtsEnable = true;
            App.Current.MainWindow.Title = "Connected to " + ComPort;
            _serialPort.DataReceived += (sender, e) => {
                _buffer += _serialPort.ReadExisting();

                if(_buffer.Contains('\n'))
                {
                    var tmp = _buffer.Split('\n', 2, System.StringSplitOptions.RemoveEmptyEntries);
                    if(tmp.Length == 1){
                        _buffer = string.Empty;
                        App.Current.Dispatcher.Invoke(() => {
                            Console.Add(tmp[0].TrimEnd());
                        });
                    }else if(tmp.Length == 2){
                        _buffer = tmp[1];
                        App.Current.Dispatcher.Invoke(() => {
                            Console.Add(tmp[0].TrimEnd());
                        });
                    }else{
                        System.Console.WriteLine("Error during String split: more then 2 strings in spritarray!");
                    }
                }
            };
            OnPropertyChanged(nameof(Connected));
            OnPropertyChanged(nameof(Disconnected));
        }

        private void bootMode() {
            disconnect();
            _serialPort = new SerialPort(ComPort, 1200);
            try {
                _serialPort.Open();
            } catch {
                disconnect();
            }
            App.Current.MainWindow.Title = "Switched to boot mode";
            OnPropertyChanged(nameof(Connected));
            OnPropertyChanged(nameof(Disconnected));
        }

        private void build() {
            try {
                System.Console.WriteLine("build");
                using (var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = @"wsl.exe",
                        UseShellExecute = true,
                        RedirectStandardOutput = true,
                        RedirectStandardInput = true,
                        CreateNoWindow = false,
                    }
                }) {
                    proc.Start();
                    System.Console.WriteLine(proc.StandardOutput.ReadLine());
                    proc.StandardInput.WriteLine("cd {FirmwareDirectory}/build");
                    System.Console.WriteLine(proc.StandardOutput.ReadLine());
                    proc.StandardInput.WriteLine("cmake ..");
                    System.Console.WriteLine(proc.StandardOutput.ReadLine());
                    proc.StandardInput.WriteLine("make -j 8");
                    System.Console.WriteLine(proc.StandardOutput.ReadLine());
                    proc.StandardInput.WriteLine("cp app/*.uf3 /d/temp");
                    System.Console.WriteLine(proc.StandardOutput.ReadLine());
                    proc.StandardInput.WriteLine("exit");
                    System.Console.WriteLine(proc.StandardOutput.ReadLine());
                    proc.StandardInput.WriteLine("exit");
                    proc.StandardInput.Flush();
                    proc.StandardInput.Close();
                    proc.WaitForExit(5000);
                    System.Console.WriteLine(proc.StandardOutput.ReadToEnd());
                    System.Console.ReadLine();
                    // bootMode();
                    // new FileInfo("D:\\temp\\ChristmasClock.uf2").CopyTo("K:\\ChristmasClock.uf3", true);
                    
                }
            } catch (System.Exception e) {
                System.Console.WriteLine(e);
            }
        }

        private void deploy() {
            disconnect();
            _serialPort = new SerialPort(ComPort, 1200);
            try {
                _serialPort.Open();
            } catch {
                disconnect();
            }
            App.Current.MainWindow.Title = "Switched to boot mode";
        }

        private void send() {
            if (_serialPort == null) return;
            _serialPort.WriteLine(ConsoleInput);
            ConsoleInput = "";
        }

        private void clear() {
            Console.Clear();
        }
    }
}
