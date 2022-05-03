using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Data.SQLite;
using Dapper;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows.Interop;


//Ctrl-KX, or KS (surround) snippet
//Ctrl alt down or up for multi curso
namespace AI_Note_Review
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //string strApikey = "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"; //for AI

        private MasterReviewSummaryVM mrs;

        public MainWindow()
        {
            ProgramInit();
            InitializeComponent();
            mrs = new MasterReviewSummaryVM();
            DataContext = mrs;

            /*
            //Test checkpointeditor
            CheckPointEditorV w = new CheckPointEditorV();
            w.DataContext = mrs;
            w.DataContext = new CheckPointEditorVM();
            w.ShowDialog();
            return;

            /*Test VisitReport
            
            mrs.VisitReport.NewEcWDocument(); //reset document
            mrs.VisitReport.PopulateCPStatuses();
            VisitReportV wp = new VisitReportV();
            wp.DataContext = mrs.VisitReport;
            wp.ShowDialog();
            return;
            */



            Properties.Settings.Default.PropertyChanged += Default_PropertyChanged;

            //Note hunter test
            /*
            NoteHunterM nh = new NoteHunterM();
            for (int iDay = 7; iDay <= 12; iDay++)
            {
                nh.SetDay(new DateTime(2021, 10, iDay));
            }
            Console.WriteLine("done");
            */

            /*
            Console.WriteLine("sending keys");
            int i = AutoIt.AutoItX.WinActivate("Encounters");
            AutoIt.AutoItX.Send("{F2}");
            Console.WriteLine($"done: {i}");
            this.Close();
            
            /*
            

            string sqlCheck = $"Select * from MasterReviewSummary;";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                MasterReviewSummaryVM mrs = cnn.Query<MasterReviewSummaryVM>(sqlCheck).FirstOrDefault();
                ReportToHtmlVM r = new ReportToHtmlVM();
                WinPreviewHTML wp = new WinPreviewHTML();
                wp.MyWB.NavigateToString(r.ExecutiveSummary(mrs));
                wp.ShowDialog();
                this.Close();
                
            }
            */

            //return; // disable hook
            //hook up window changed event


            _winEventProc = new WinEventDelegate(WinEventProc);
            m_hhook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND,
                EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, _winEventProc,
                0, 0, WINEVENT_OUTOFCONTEXT);
            ActiveWindowChanged += MainWindow_ActiveWindowChanged;

        }

        private void Default_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "MonitorActive")
            {
                Properties.Settings.Default.Save();
            }
        }


        #region Monitor active Window

        //to Monitor active window
        public delegate void ActiveWindowChangedHandler(object sender, String windowHeader, IntPtr hwnd);
        public event ActiveWindowChangedHandler ActiveWindowChanged;

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType,
            IntPtr hwnd, int idObject, int idChild, uint dwEventThread,
            uint dwmsEventTime);

        const uint WINEVENT_OUTOFCONTEXT = 0;
        const uint EVENT_SYSTEM_FOREGROUND = 3;
        const uint EVENT_OBJECT_DESTROY = 0x8001;

        [DllImport("user32.dll")]
        static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax,
            IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc,
            uint idProcess, uint idThread, uint dwFlags);

        IntPtr m_hhook;
        private WinEventDelegate _winEventProc;

        void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd,
            int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (eventType == EVENT_SYSTEM_FOREGROUND)
            {
                if (ActiveWindowChanged != null)
                    ActiveWindowChanged(this, GetActiveWindowTitle(hwnd), hwnd);
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(HandleRef hWnd, out RECT lpRect);

        [DllImport("USER32.DLL")]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);


        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }


        private string GetActiveWindowTitle(IntPtr hwnd)
        {
            StringBuilder Buff = new StringBuilder(500);
            GetWindowText(hwnd, Buff, Buff.Capacity);
            return Buff.ToString();
        }



        #endregion


        /// <summary>
        /// Hook up active window changed event and set database location.
        /// </summary>
        private void ProgramInit()
        {

            string strUserName = Environment.GetEnvironmentVariable("USERNAME");

            SqlLiteDataAccess.SQLiteDBLocation = Properties.Settings.Default.DataFolder;

            //C:\Users\lds00\source\repos\lds000\AI-Note-Review

            if (File.Exists(@"C:\Users\lds00\source\repos\lds000\AI-Note-Review\NoteReviewDB.db"))
            {
                SqlLiteDataAccess.SQLiteDBLocation = @"C:\Users\lds00\source\repos\lds000\AI-Note-Review\NoteReviewDB.db";
            }
            else
            {
                SqlLiteDataAccess.SQLiteDBLocation = @"C:\Users\llostod\source\repos\AI Note Review\NoteReviewDB.db";
            }

            if (File.Exists(@"C:/Users/lds00/source/repos/data/NoteData.db"))
            {
                SqlLiteDataAccess.SQLiteNotesLocation = @"C:/Users/lds00/source/repos/data/NoteData.db";
            }
            else
            {
                SqlLiteDataAccess.SQLiteNotesLocation = @"C:\Users\llostod\source\repos\data\NoteData.db";
            }
        }

        string strLastDoc = "";

        #region window functions
        /// <summary>
        /// Monitors windows as the focus is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="windowHeader"></param>
        /// <param name="hwnd"></param>
        private void MainWindow_ActiveWindowChanged(object sender, string windowHeader, IntPtr hwnd)
        {
            if (!Properties.Settings.Default.MonitorActive)
                return;
            //mrs.AddLog($"Window ({windowHeader}) focused.");
            //Console.WriteLine($"Header: {windowHeader}");
            //Get window data (top, left, size)
            RECT rct;
            if (!GetWindowRect(new HandleRef(this, hwnd), out rct))
            {
                Console.WriteLine($"Window titled '{windowHeader}' produced an error obtaining window dimensions.");
                return;
            }
            System.Drawing.Rectangle WinPosInfo = new System.Drawing.Rectangle();
            WinPosInfo.X = rct.Left;
            WinPosInfo.Y = rct.Top;
            WinPosInfo.Width = rct.Right - rct.Left;
            WinPosInfo.Height = rct.Bottom - rct.Top;
            PresentationSource source = PresentationSource.FromVisual(this);

            double dpiX = 1;
            double dpiY = 1;

            if (source != null)
            {
                dpiX = source.CompositionTarget.TransformToDevice.M11;
                dpiY = source.CompositionTarget.TransformToDevice.M22;
            }

            if (windowHeader == "Encounters")
            {

            }

            //load patient.
            if (windowHeader.Contains("Patient Encounter Summary")) //PatientM Encounter Summary
            {
                int i = 0;
                if (!CF.IsReviewWindowOpen) //do not load new patient if in a current review.
                    while (i < 20)
                    {
                        HookIE h = new HookIE(hwnd, 0);
                        if (h.EcwHTMLDocument != null)
                        {
                            if (h.IHTMLDocument != null)
                                if (h.EcwHTMLDocument.Body != null)
                                    if (h.EcwHTMLDocument.Body.InnerHtml != null)
                                    {
                                        try
                                        {
                                            //do not reset the document if it has already been loaded and analyzed, check length for performance
                                            if (h.EcwHTMLDocument.Body.InnerText.Length == strLastDoc.Length)
                                            {
                                                return;
                                            }
                                            strLastDoc = h.EcwHTMLDocument.Body.InnerText;
                                            mrs.AddLog("Document Found....");
                                            mrs.VisitReport.Document.NoteHTML = h.EcwHTMLDocument;
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e.Message);
                                            break;
                                        }
                                        if (mrs.VisitReport.Patient.PtName != "") break;
                                    }
                        }
                        Thread.Sleep(200);
                        i++;
                    }
            }
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CF.SetWindowPosition(this);
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CF.SaveWindowPosition(this);
        }
        #endregion

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            WinSettings ws = new WinSettings();
            ws.Owner = this;
            ws.ShowDialog();
        }

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            //AutoIt.AutoItX.WinActivate("Select Provider(s)");
            AutoIt.AutoItX.MouseClick("LEFT", 330, 165);
            AutoIt.AutoItX.Send("{TAB}");
            AutoIt.AutoItX.Send("{DOWN}");
            string nextName = AutoIt.AutoItX.WinGetText("Select Provider(s)");
        }

        private void Lookup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string strNum = AutoIt.AutoItX.ClipGet().Split(' ')[0];
                AutoIt.AutoItX.ClipPut(strNum);
                AutoIt.AutoItX.MouseClick("LEFT", 300, 75);
                AutoIt.AutoItX.WinWaitActive("Patient Lookup");
                AutoIt.AutoItX.MouseClick("LEFT", 500, 65);
                AutoIt.AutoItX.Send("^V{ENTER}");
                Thread.Sleep(3000);
                AutoIt.AutoItX.WinWaitActive("Patient Hub");
                AutoIt.AutoItX.Send("!p");

            }
            catch
            {
            
            }

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
