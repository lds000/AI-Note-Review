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

        VisitReportVM reportVM;
        BiMonthlyReviewVM biMonthlyReviewVM;

        List<DocumentVM> htmlDocs = new List<DocumentVM>();
        public MainWindow()
        {
            ProgramInit();
            InitializeComponent();

            reportVM = new VisitReportVM();
            biMonthlyReviewVM = new BiMonthlyReviewVM();
            this.DataContext = reportVM;
            biMonthReviewMI.DataContext = biMonthlyReviewVM;
            for (int iDay = 7; iDay <= 12; iDay++)
            {
                SetDay(new DateTime(2021, 10, iDay));
            }
            Console.WriteLine("done");

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

        public void SetDay(DateTime dt)
        {
            /*
            AutoIt.AutoItX.MouseMove(600, 230);
            Thread.Sleep(2000);
            AutoIt.AutoItX.MouseMove(600, 855);
            Thread.Sleep(2000);
            this.Close();
            */
            Console.WriteLine("sending keys");

            int i = AutoIt.AutoItX.WinActivate("eClinicalWorks (");
            AutoIt.AutoItX.MouseClick("LEFT", 600, 190,2); //click month

            //send date
            AutoIt.AutoItX.Send(dt.Month.ToString()); 
            Thread.Sleep(200);
            AutoIt.AutoItX.MouseClick("LEFT", 617, 190, 2); //click day
            AutoIt.AutoItX.Send(dt.Day.ToString());
            Thread.Sleep(200);
            AutoIt.AutoItX.MouseClick("LEFT", 640, 190, 2); //click year
            AutoIt.AutoItX.Send(dt.Year.ToString());
            Thread.Sleep(2000); //wait for encounters to load

            //get encounters information from Copy (F2) function
            AutoIt.AutoItX.Send("{F2}");
            Thread.Sleep(200);
            AutoIt.AutoItX.WinActivate("CwReport");
            Thread.Sleep(200);
            AutoIt.AutoItX.Send("^a");
            Thread.Sleep(200);
            AutoIt.AutoItX.Send("^c");
            Thread.Sleep(200);
            string strVisitReport = AutoIt.AutoItX.ClipGet();
            AutoIt.AutoItX.Send("^w");

            //process visits
            using (StringReader reader = new StringReader(strVisitReport))
            {
                string line;
                int VisitCount = 0;
                int offset = 217; //Where the first encounter starts (y-offset)
                double iHeight = (double)(860-217)/(double)40; //Height of 40 encounters
                int y = 0; //counter for lines
                while (null != (line = reader.ReadLine())) //read line by line to end
                {
                    int linecount = line.Split('\t').Count(); //tab delimited file
                    {
                        if (y <= 47) //only do top 47, rest is off screen.. later consider clicking on lower 1/2
                        {
                            string[] strArray = line.Split('\t');
                            string strVisitType = strArray[2];
                            if (strVisitType == "UC" || strVisitType == "Resp") //only respiratory or UC visits
                            if (strArray[7].ToLower().Contains("sore"))
                            {
                                VisitCount++;
                                AutoIt.AutoItX.MouseClick("LEFT",1900, 235); //reset the horizontal slide bar
                                AutoIt.AutoItX.MouseClick("LEFT", 580, offset + (int)(iHeight * (double)y),2); //double click encounter to open, this needs long delay
                                Console.WriteLine($"Clicked on visit. CC: {strArray[7]}");
                                    int counter = 1;
                                    while (counter <= 5)
                                    {
                                        AutoIt.AutoItX.MouseClick("LEFT", 1200, 300); //blank space on encounter note, to load note
                                        IntPtr hwnd = AutoIt.AutoItX.WinGetHandle();
                                        Thread.Sleep(1000);
                                        HookIE h = new HookIE(hwnd, 0);
                                        if (h.EcwHTMLDocument.Body.InnerHtml != null)
                                        {
                                            DocumentVM d = new DocumentVM(h.EcwHTMLDocument);
                                            if (MasterReviewSummaryVM.CurrentMasterReview.ContainsDocument(d))
                                            {
                                                htmlDocs.Add(d);
                                            }
                                            counter = 6;
                                        }
                                        else
                                        {
                                            counter++;
                                            Console.WriteLine($"{counter}/5 seconds no document loaded.");
                                            Thread.Sleep(1000);
                                        }
                                    }
                                AutoIt.AutoItX.MouseClick("LEFT", 1620, 70); //Encounters bean
                                Thread.Sleep(5000);
                                //Thread.Sleep(100);
                            }
                        }
                    }
                    y++;
                }
                Console.WriteLine($"Visit count is: {VisitCount}");
            }

            Console.WriteLine($"done: {i}");
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

            if (File.Exists(@"C:\Users\Lloyd\Source\Repos\lds000\AI-Note-Review\NoteReviewDB.db"))
            {
                SqlLiteDataAccess.SQLiteDBLocation = @"C:\Users\Lloyd\Source\Repos\lds000\AI-Note-Review\NoteReviewDB.db";
            }
            else
            {
                SqlLiteDataAccess.SQLiteDBLocation = @"C:\Users\llostod\source\repos\AI Note Review\NoteReviewDB.db";
            }


        }

        #region window functions
        /// <summary>
        /// Monitors windows as the focus is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="windowHeader"></param>
        /// <param name="hwnd"></param>
        private void MainWindow_ActiveWindowChanged(object sender, string windowHeader, IntPtr hwnd)
        {

            Console.WriteLine($"Window ({windowHeader}) focused.");
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
                                            reportVM.Document.ProcessDocument(h.EcwHTMLDocument);
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e.Message);
                                            break;
                                        }
                                        if (reportVM.Patient.PtName != "") break;
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
    }
}
