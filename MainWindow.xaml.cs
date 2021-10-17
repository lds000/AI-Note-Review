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
        string strApikey = "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";

        public MainWindow()
        {
            ProgramInit();
            //This is just for compiler purposes
            //CF.CurrentPatient = new PatientViewModel().SamplePatient;
            //CF.ClinicNote = (new DocumentViewModel(CF.CurrentPatient)).SampleDocument;
            InitializeComponent();
            DocumentViewModel dvm = new DocumentViewModel();
            this.DataContext = dvm;
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
            //hook up window changed event
            _winEventProc = new WinEventDelegate(WinEventProc);
            m_hhook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND,
                EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, _winEventProc,
                0, 0, WINEVENT_OUTOFCONTEXT);
            ActiveWindowChanged += MainWindow_ActiveWindowChanged;

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

        /// <summary>
        /// Monitors windows as the focus is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="windowHeader"></param>
        /// <param name="hwnd"></param>
        private void MainWindow_ActiveWindowChanged(object sender, string windowHeader, IntPtr hwnd)
        {

            //Console.WriteLine($"Window ({windowHeader}) focused.");
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
            if (windowHeader.Contains("Patient Encounter Summary")) //Patient Encounter Summary
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
                                    //processLockedt(h.EcwHTMLDocument);
                                    //if (document.PtName != "") break;
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

        private void CheckNote(object sender, RoutedEventArgs e)
        {
        }

        private void CheckNoteX(object sender, RoutedEventArgs e)
        {
            WinReport wp = new WinReport(true);
            wp.ShowDialog();
        }

        private void EditCP(object sender, RoutedEventArgs e)
        {
            winDbRelICD10CheckpointsEditor w = new winDbRelICD10CheckpointsEditor();
            w.Owner = this;
            w.Show();
        }


        private void Button_ClickReviewDate(object sender, RoutedEventArgs e)
        {
            WinGetReviewDate wr = new WinGetReviewDate();
            wr.Owner = this;
            wr.ShowDialog();
            //if (wr.SelectedDate > DateTime.MinValue)
            //document.ReviewDate = wr.SelectedDate;
        }

        private void Button_ReportsReviewClick(object sender, RoutedEventArgs e)
        {
            WinCommittReport wcr = new WinCommittReport();
            wcr.Owner = this;
            wcr.ShowDialog();
        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            WinSettings ws = new WinSettings();
            ws.Owner = this;
            ws.ShowDialog();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Reviews_Click(object sender, RoutedEventArgs e)
        {
            WinProviderReviews wp = new WinProviderReviews();
            wp.Owner = this;
            wp.ShowDialog();
        }

        private void Label_MouseDown_1(object sender, MouseButtonEventArgs e)
        {

        }

        private void ProviderLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //SqlProvider sp = SqlProvider.SqlGetProviderByID(document.ProviderID);
            //WinEnterText wet = new WinEnterText($"Private notes for {sp.FullName}", sp.PersonalNotes);
            //wet.ShowDialog();
            //sp.PersonalNotes = wet.ReturnValue;
            //sp.SaveToDatabase();
            
        }
    }

    public static class ExtensionMethods
    {
        public static string TrimEnd(this string input, string suffixToRemove, StringComparison comparisonType = StringComparison.CurrentCulture)
        {
            if (suffixToRemove != null && input.EndsWith(suffixToRemove, comparisonType))
            {
                return input.Substring(0, input.Length - suffixToRemove.Length);
            }

            return input;
        }

        public static bool RegexContains(this string input, string strRegEx)
        {
            RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace;
            return Regex.IsMatch(input, strRegEx, options);
        }



        public static string ParseHistory(this string strInput)
        {
            char[] delimiterChars = { ' ', ',', '.', ':', '\t' };

            string strHPI = strInput;
            strHPI = strHPI.Replace("has not had", "hasnothad");
            strHPI = strHPI.Replace("has had", "hashad");
            strHPI = strHPI.Replace("not have", "nothave");

            string[] strNegMarker = { "denies", "no history of", "no hx of", "does not have", "didn't have","nothave", "hasnothad", "reports", "admits" };
            string[] strPosMarker = { "complain", "c/o", "endorses","indicates","hashad", "presents", "presented", "presenting", "has been", "states", "does have" };
            string[] words = strHPI.Split(delimiterChars);
            System.Console.WriteLine($"{words.Length} words in text:");
            string strCompare = "";
            Queue<string> strStack = new Queue<string>();
            string strPosNeg = "Intro";
            string strLastCompareResult = "";
            string strResult = "";
            foreach (var word in words)
            {
                if (word == "") continue;
                strCompare += " " + word.ToLower();
                strStack.Enqueue(word);

                bool strContainsPos = false;
                foreach (string str in strPosMarker)
                {
                    if (strCompare.Contains(str))
                    {
                        strContainsPos = true;
                        strResult = str;
                        break;
                    }
                }
                if (strContainsPos)
                {
                    string strOutput = "";
                    while (strStack.Count > 0)
                    {
                        strOutput += strStack.Dequeue() + " ";
                    }
                    strOutput = strOutput.Trim();
                    strOutput = strOutput.Replace(strResult, "");
                    System.Console.WriteLine($"{strPosNeg}- <{strLastCompareResult}>  {strOutput}");
                    strLastCompareResult = strResult;
                    strPosNeg = "Pos";
                    strCompare = "";
                }

                bool strContainsNeg = false;
                foreach (string str in strNegMarker)
                {
                    if (strCompare.Contains(str))
                    {
                        strContainsNeg = true;
                        strResult = str;
                        break;
                    }
                }
                if (strContainsNeg)
                {
                    string strOutput = "";
                    while (strStack.Count > 0)
                    {
                        strOutput += strStack.Dequeue() + " ";
                    }
                    strOutput = strOutput.Trim();
                    strOutput = strOutput.Replace(strResult, "");
                    System.Console.WriteLine($"{strPosNeg}- <{strLastCompareResult}>  {strOutput}");
                    strLastCompareResult = strResult;
                    strPosNeg = "Neg";
                    strCompare = "";
                }
            }
            string strOutputFinal = "";
            while (strStack.Count > 0)
            {
                strOutputFinal += strStack.Dequeue() + " ";
            }
            strOutputFinal = strOutputFinal.Trim();
            if (strResult!="")
            strOutputFinal = strOutputFinal.Replace(strResult, "");
            System.Console.WriteLine($"{strPosNeg}- <{strLastCompareResult}>  {strOutputFinal}");

            return strHPI;
        }
    }

    public class DateTimeHandler : SqlMapper.TypeHandler<DateTimeOffset>
    {
        private readonly TimeZoneInfo databaseTimeZone = TimeZoneInfo.Local;
        public static readonly DateTimeHandler Default = new DateTimeHandler();

        public DateTimeHandler()
        {

        }

        public override DateTimeOffset Parse(object value)
        {
            DateTime storedDateTime;
            if (value == null)
                storedDateTime = DateTime.MinValue;
            else
                storedDateTime = (DateTime)value;

            if (storedDateTime.ToUniversalTime() <= DateTimeOffset.MinValue.UtcDateTime)
                return DateTimeOffset.MinValue;
            else
                return new DateTimeOffset(storedDateTime, databaseTimeZone.BaseUtcOffset);
        }

        public override void SetValue(IDbDataParameter parameter, DateTimeOffset value)
        {
            DateTime paramVal = value.ToOffset(this.databaseTimeZone.BaseUtcOffset).DateTime;
            parameter.Value = paramVal;
        }
    }
}
