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
using System.Net;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Windows.Forms;
using System.Timers;
using System.Windows.Threading;
using System.IO;
using System.Media;

namespace EnvatoSalesNotifier
{
    public partial class MainWindow : Window
    {
        private NotifyIcon notifyIcon;
        static string SaleCounterOld { get; set; }
        static string SaleCounter { get; set; }
        private DispatcherTimer dispTimer = new DispatcherTimer();
        private int TimeSelected { get; set; }
        private bool UpdateData { get; set; }
        ContextMenuStrip cms = new ContextMenuStrip();      //    Tray=>>
        ToolStripMenuItem mI1 = new ToolStripMenuItem();    // =>>Menu Strip
        public MainWindow()
        {
            InitializeComponent();
            tbxUname.Focus();
            cbTimer.SelectionChanged -= cbTimer_SelectionChanged;
            cbTimer.SelectedIndex = 0;
            cbTimer.SelectionChanged += cbTimer_SelectionChanged;
            dispTimer.Tick += new EventHandler(dispTimer_Tick);
            UpdateData = false;
        }
        public void CheckForSales() // Update Allower
        {
            if (SaleCounterOld != SaleCounter)
            {
                UpdateData = true;
            }
        }

        #region TimerInitialize
        private void TimerInitialize()
        {
            switch (cbTimer.SelectedIndex)
            {
                case 0:
                    TimeSelected = 5;
                    break;
                case 1:
                    TimeSelected = 10;
                    break;
                case 2:
                    TimeSelected = 30;
                    break;
                case 3:
                    TimeSelected = 60;
                    break;
            }

            dispTimer.Interval = new TimeSpan(0, 0, TimeSelected);
        }
        private void StartTimer()
        {
            dispTimer.Start();
        }
        private void ResetTimer()
        {
            dispTimer.Stop();
            TimerInitialize();
        }
        #endregion

        #region SalesCounter
        private void SalesChecker()
        {
            WebClient wc = new WebClient();
            if (String.IsNullOrEmpty(tbxUname.Text) || String.IsNullOrWhiteSpace(tbxUname.Text))
            {
                System.Windows.MessageBox.Show("Empty username!", "Warning!");
            }
            else if (cBoxMarket.Text == null)
            {
                System.Windows.MessageBox.Show("Choose the market!", "Warning!");
            }
            else
            {
                try //user not found
                {
                    string Response = wc.DownloadString("https://" + cBoxMarket.Text + ".net/user/" + tbxUname.Text);
                    string strStart = "AuthorSales:";
                    string strEnd = @""" /";
                    string counter = SimpleParser(Response, strStart, strEnd);
                    tbsaleCounter.Text = counter;
                    SaleCounter = counter;
                }
                catch (Exception)
                {
                    SaleCounter = "No Sales";
                    timerChb.IsChecked = false;
                    System.Windows.MessageBox.Show("User not found", "Warning");
                }

            }
        }
        private string SimpleParser(string strSource, string strStart, string strEnd)
        {
            int Start, End;
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }
            else
            {
                return "No Sales";
            }
        }
        #endregion

        #region events
        private void Tray_Exit(object sender, EventArgs e) //CloseAppViaTray
        {
            notifyIcon.Dispose();
            System.Windows.Application.Current.Shutdown();
        }
        private void username_Change(object sender, EventArgs e)
        {
            SaleCounterOld = null;
            SaleCounter = null;
        }
        private void tbxUname_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Button_Click(this, null);
            }
        }
        private void Window_StateChanged(object sender, EventArgs e)
        {
            switch (WindowState)
            {
                case WindowState.Maximized:
                    break;
                case WindowState.Minimized:
                    if (trayAllow.IsChecked == true && SaleCounter != "No Sales")
                    {
                        Hide();
                        notifyIcon.Visible = true;
                    }
                    break;
                case WindowState.Normal:
                    trayAllow.IsChecked = false;
                    timerChb.IsChecked = false;
                    break;
            }
        }
        private void notifyIcon_DoubleClick(object Sender, EventArgs e) //MaximizeFromTray
        {
            notifyIcon.Dispose();
            if (WindowState == WindowState.Minimized)
            {
                Show();
                WindowState = WindowState.Normal;
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SalesChecker();
            if (String.IsNullOrEmpty(tbxUname.Text) || String.IsNullOrWhiteSpace(tbxUname.Text))
            {

            }
            else if (SaleCounter != "No Sales" && timerChb.IsChecked == true)
            {
                StartTimer();
            }
            SaleCounterOld = SaleCounter;
            tbxUname.TextChanged += username_Change;
        }
        private void dispTimer_Tick(object sender, EventArgs e) //OnTimeEnd
        {
            SalesChecker();
            CheckForSales();
            if (UpdateData == true)
            {
                SaleCounterOld = SaleCounter;
                if (trayAllow.IsChecked == true)
                {
                    SoundPlayer sp = new SoundPlayer(Properties.Resources.notSound);
                    sp.Play();
                    ShowNot();
                    UpdateData = false;
                }
            }
        }
        private void timerChb_Checked(object sender, RoutedEventArgs e) //TimerOn
        {
            TimerInitialize();
        }
        private void timerChb_Unchecked(object sender, RoutedEventArgs e) //TimerOff
        {
            ResetTimer();
        }
        private void cbTimer_SelectionChanged(object sender, SelectionChangedEventArgs e) //OnTimeChange
        {
            ResetTimer();
            if (String.IsNullOrEmpty(tbxUname.Text) || String.IsNullOrWhiteSpace(tbxUname.Text))
            {

            }
            else
            {
                if (timerChb.IsChecked == true)
                {
                    StartTimer();
                }
            }
        }
        private void trayAllow_Checked(object sender, RoutedEventArgs e)
        {
            TrayInit();
        }
        private void trayAllow_Unchecked(object sender, RoutedEventArgs e)
        {
            notifyIcon.Dispose();
        }
        #endregion
        
        #region Tray
        public void TrayInit()
        {
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = Properties.Resources.TrayIcon;
            notifyIcon.DoubleClick += new EventHandler(notifyIcon_DoubleClick);
            notifyIcon.BalloonTipTitle = "New Sale!";
            ContextMenuTray();
            if (SaleCounter != "No Sales")
            {
                if (String.IsNullOrEmpty(SaleCounter) || String.IsNullOrWhiteSpace(SaleCounter))
                {
                }
                else
                {
                    if (SaleCounterOld != SaleCounter)
                    {

                        ShowNot();
                    }
                }
            }
        }
        public void ContextMenuTray()
        {
            mI1.Text = "Exit";
            mI1.Click += new EventHandler(Tray_Exit);
            cms.Items.Add(mI1);
            notifyIcon.ContextMenuStrip = cms;
        }
        public void ShowNot()
        {
            notifyIcon.Visible = true;
            notifyIcon.BalloonTipText = "Sales: " + SaleCounter;
            notifyIcon.ShowBalloonTip(10000);
        }
        #endregion        
    }
}