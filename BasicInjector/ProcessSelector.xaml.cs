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
using System.Windows.Shapes;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;

namespace BasicInjector
{
    /// <summary>
    /// Interaction logic for ProcessSelector.xaml
    /// </summary>
    public partial class ProcessSelector : UserControl
    {
        public ProcessSelector()
        {
            InitializeComponent();
            this.QueRefreshProcesses();
        }

        public Process SelectedProcess;

        private Thread _processSearchThread;

        public Brush MiddleColor = new SolidColorBrush(Colors.Silver);

        public void QueRefreshProcesses()
        {
            string query = processSearchBox.Text;
            bool hideWindowless = hideWindowLessProcesses.IsChecked == true;
            if (_processSearchThread != null)
            {
                if (_processSearchThread.IsAlive) _processSearchThread.Abort();
            }
            processBox.Items.Clear();
            _processSearchThread = new Thread(() => refreshProcesses(query, hideWindowless));
            _processSearchThread.Start();
        }

        private void refreshProcesses(string query, bool hideWindowless)
        {
            query = query.ToLower();
            foreach (Process process in Process.GetProcesses())
            {
                try
                {
                    int pid = process.Id;
                    string processName = process.ProcessName;
                    string windowName = process.MainWindowTitle;
                    if (hideWindowless && (process.MainWindowHandle == IntPtr.Zero || process.MainWindowTitle == string.Empty)) continue;
                    if (!processName.ToLower().Contains(query) && !windowName.ToLower().Contains(query)) continue;
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                    {

                        DockPanel dockPanel = new DockPanel();

                        Label pidLabel = new Label { Content = pid.ToString(), Width = 50 };
                        DockPanel.SetDock(pidLabel, Dock.Left);
                        dockPanel.Children.Add(pidLabel);

                        Label processNameLabel = new Label { Foreground = this.MiddleColor, Content = processName };
                        dockPanel.Children.Add(processNameLabel);

                        Label windowNameLabel = new Label { Content = windowName };
                        DockPanel.SetDock(windowNameLabel, Dock.Right);
                        dockPanel.Children.Add(windowNameLabel);

                        ListBoxItem listBoxItem = new ListBoxItem();
                        listBoxItem.MouseDoubleClick += ListBoxItem_MouseDoubleClick;
                        listBoxItem.Content = dockPanel;
                        processBox.Items.Add(listBoxItem);
                    }));
                }
                catch (Exception e) { }
            }
        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ProcessSelectedFunc();
        }

        private void ProcessSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            QueRefreshProcesses();
        }

        private void ProcessBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            processSelectButton.IsEnabled = (processBox.SelectedItem != null);
        }

        private void HideWindowLessProcesses_Checked(object sender, RoutedEventArgs e)
        {
            QueRefreshProcesses();
        }

        private void ProcessSelectedFunc()
        {
            if (!processSelectButton.IsEnabled) return;
            if (processBox.SelectedItem == null) return;
            int pid = int.Parse(((Label)((DockPanel)((ListBoxItem)processBox.SelectedItem).Content).Children[0]).Content.ToString());
            this.SelectedProcess = Process.GetProcessById(pid);
            processSelected?.Invoke(this.SelectedProcess);
        }

        private void ProcessSelectButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessSelectedFunc();
        }

        public void Search(string name)
        {
            this.processSearchBox.Text = name;
        }

        public delegate void ProcessSelected(Process process);
        public event ProcessSelected processSelected;
    }
}
