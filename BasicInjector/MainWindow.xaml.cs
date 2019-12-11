using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace BasicInjector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.defaultHeight = this.Height;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            loadSettings();
            updateStatus();
        }

        private Process selectedProcess = null;
        private string selectedFilePath = "NULL";

        private void processSelector_processSelected(System.Diagnostics.Process process)
        {
            this.selectedProcess = process;
            this.processNameLabel.Content = process.ProcessName + " : " + process.Id.ToString();
            this.mainTabControl.SelectedIndex = 0;
            updateStatus();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.mainTabControl.SelectedIndex = 1;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog { Filter = "dll files |*.dll", RestoreDirectory = true };
            if (openFileDialog.ShowDialog() == true)
            {
                this.selectedFilePath = openFileDialog.FileName;
                this.fileNameLabel.Content = Path.GetFileName(this.selectedFilePath);
                updateStatus();
            }
            else
            {
                this.selectedFilePath = string.Empty;
            }
        }

        private bool checkFile()
        {
            return File.Exists(this.selectedFilePath);
        }

        private bool checkProcess()
        {
            return (selectedProcess == null || this.selectedProcess.Id != 0);
        }

        public static string NormalizePath(string path)
        {
            return Path.GetFullPath(new Uri(path).LocalPath)
                       .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                       .ToUpperInvariant();
        }

        private bool isLoaded()
        {
            try
            {
                foreach (ProcessModule module in selectedProcess.Modules)
                {
                    if (NormalizePath(module.FileName) == NormalizePath(this.selectedFilePath)) return true;
                }
            } catch (Exception e) { }
            return false;
        }

        private void updateStatus()
        {
            if (!checkProcess())
            {
                this.statusLabel.Content = "No Process Selected";
                this.injectButton.IsEnabled = false;
                return;
            }
            if (!checkFile())
            {
                this.statusLabel.Content = "No File Selected";
                this.injectButton.IsEnabled = false;
                return;
            }
            if (isLoaded())
            {
                this.statusLabel.Content = "Module Loaded into Process";
                this.injectButton.IsEnabled = false;
                return;
            }

            saveSettings();
            this.statusLabel.Content = "Ready to Inject";
            this.injectButton.IsEnabled = true;
        }

        void loadSettings()
        {
            var lastProcs = Process.GetProcessesByName(Properties.Settings.Default.lastProcessName);
            if (lastProcs.Length != 0)
            {
                processSelector_processSelected(lastProcs[0]);
            }
            if (Properties.Settings.Default.lastDll != null && File.Exists(Properties.Settings.Default.lastDll))
            {
                this.selectedFilePath = Properties.Settings.Default.lastDll;
                this.fileNameLabel.Content = Path.GetFileName(this.selectedFilePath);
                updateStatus();
            }
        }

        void saveSettings()
        {
            if (checkProcess())
            {
                Properties.Settings.Default.lastProcessName = selectedProcess.ProcessName;
            }
            if (checkFile())
            {
                Properties.Settings.Default.lastDll = selectedFilePath;
            }
            Properties.Settings.Default.Save();
        }


        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Injector injector = new Injector();
            injector.inject(this.selectedProcess, this.selectedFilePath);
            updateStatus();
        }

        private double defaultHeight;

        private void mainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            const double heightChange = 220;
                if (e.Source is TabControl)
                {
                    if (this.mainTabControl.SelectedIndex == 1)
                    {
                        this.Height += heightChange;
                    }
                    else
                    {
                        this.Height = this.defaultHeight;
                    }
                }
        }

    }
}
