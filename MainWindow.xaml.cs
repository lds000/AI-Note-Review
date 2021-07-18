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

namespace AI_Note_Review
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string strApikey = "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";
        DocInfo CurrentDoc = new DocInfo();
        public MainWindow()
        {
            InitializeComponent();
            ProgramInit();
            SqlLightDataAccess.SQLiteDBLocation = Properties.Settings.Default.DataFolder;

            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLightDataAccess.SQLiteDBLocation))
            {
                string sql = "Select * from NoteSections;";
                try
                {
                    SqlLightDataAccess.NoteSections =  cnn.Query<SqlNoteSection>(sql).ToList();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error on saving variation data: {e.Message}");
                }
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

        private void ProgramInit()
        {
            //hook up window changed event
            _winEventProc = new WinEventDelegate(WinEventProc);
            m_hhook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND,
                EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, _winEventProc,
                0, 0, WINEVENT_OUTOFCONTEXT);
            ActiveWindowChanged += MainWindow_ActiveWindowChanged;
            string strUserName = Environment.GetEnvironmentVariable("USERNAME");
        }

        /// <summary>
        /// Monitors windows as the focus is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="windowHeader"></param>
        /// <param name="hwnd"></param>
        private void MainWindow_ActiveWindowChanged(object sender, string windowHeader, IntPtr hwnd)
        {

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


            //load patient.
            if (windowHeader.Contains("Patient Encounter Summary")) //Patient Encounter Summary
            {
                int i = 0;

                while (i < 20)
                {
                    HookIE h = new HookIE(hwnd, 0);
                    if (h.EcwHTMLDocument != null)
                    {
                        if (h.IHTMLDocument != null)
                            if (h.EcwHTMLDocument.Body != null)
                                if (h.EcwHTMLDocument.Body.InnerHtml != null)
                                {
                                    processDocument(h.EcwHTMLDocument);
                                    if (CurrentDoc.PtName != "") break;
                                }
                    }
                    Thread.Sleep(200);
                    i++;
                }
            }
        }

        private HtmlElement GetNextElement(HtmlElement el)
        {
            if (el.FirstChild != null) return el.FirstChild;
            if (el.NextSibling != null) return el.NextSibling;
            HtmlElement nextParentSibling = el.Parent.NextSibling;
            if (nextParentSibling != null) return nextParentSibling;
            while (nextParentSibling == null)
            {
                nextParentSibling = el.Parent.NextSibling;
                if (nextParentSibling != null) return nextParentSibling;
            }
            return null;
        }

        public void processDocument(HtmlDocument HDoc)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            HtmlElementCollection AllTRItems = HDoc.Body.GetElementsByTagName("TR"); //5 ms
            HtmlElementCollection AllTHEADItems = HDoc.Body.GetElementsByTagName("THEAD"); //5 ms


            CurrentDoc.NoteHTML = HDoc.Body.InnerHtml; //3 ms
            

            string strCurrentHeading = "";
            foreach (HtmlElement TempEl in AllTHEADItems) //1 item, 1.9 seconds!
            {
                if (TempEl.TagName == "THEAD")
                {
                    string strInnerText = TempEl.InnerText;
                    if (strInnerText.Contains("DOS:"))
                    {
                        strInnerText = strInnerText.Replace("DOS:", "|");
                        CurrentDoc.VisitDate = strInnerText.Split('|')[1];
                        continue;
                    }
                }
            }
            List<string> AssessmentList = new List<string>();

            foreach (HtmlElement TempEl in AllTRItems) //99 items, 1.9 seconds!
            {
                string strClass = TempEl.GetAttribute("className");
                if (strClass == "") continue;
                if (TempEl.TagName == "TR")
                {
                    if (strClass != "")
                    {
                        if (strClass == "TableFooter")
                        {
                            string strInnerText = TempEl.InnerText;
                            if (strInnerText.StartsWith("Progress Note:"))
                            {
                                string strDocname = strInnerText.Split(':')[1].Trim();
                                strDocname = strDocname.Replace("    ", "|");
                                CurrentDoc.Provider = strDocname.Split('|')[0];
                            }
                            continue;
                        }
                        if (strClass == "PtHeading") CurrentDoc.PtName = TempEl.InnerText; //set first name
                        if (strClass == "PtData") //field has note informaition
                        {
                            string strInnerText = TempEl.InnerText;
                            if (strInnerText.Contains("Account Number:")) CurrentDoc.PtID = strInnerText.Split(':')[1].Trim();
                            if (strInnerText.Contains("Appointment Facility:")) CurrentDoc.Facility = strInnerText.Split(':')[1].Trim();
                            if (strInnerText.Contains("DOB:")) CurrentDoc.PtAge = strInnerText.Split(':')[0].TrimEnd("DOB");
                            continue;
                        }
                        if (strClass == "rightPaneHeading" || strClass == "leftPaneHeading")
                        {
                            string strInnerText = TempEl.InnerText;
                            strCurrentHeading = strInnerText;
                        }
                        if (strClass == "rightPaneData" || strClass == "leftPaneData")
                        {
                            string strInnerText = TempEl.InnerText;

                            if (strCurrentHeading == "Reason for Appointment")
                                {
                                    CurrentDoc.CC = strInnerText.Substring(3);
                                }
                            if (strCurrentHeading == "History of Present Illness")
                            {
                                CurrentDoc.HPI = strInnerText;
                            }
                            if (strCurrentHeading == "Current Medications")
                            {
                                string strTextToParse = strInnerText;
                                strTextToParse = strTextToParse.Replace("Taking", "Taking:\n");
                                strTextToParse = strTextToParse.Replace("Discontinued", "Discontinued:\n");
                                var result = strTextToParse.Split(new[] { '\r', '\n' });
                                List<string> medsTaking = new List<string>();
                                bool prn = false;
                                foreach (string str in result)
                                {
                                    if (str == "Taking:") continue;
                                    if (str.StartsWith("Medication List reviewed")) continue;
                                    if (str.Trim() == "") continue;
                                    if (str == "Not-Taking:")
                                    {
                                        prn = true;
                                        continue;
                                    }
                                    if (str == "Discontinued:") break;

                                    string strList = str;
                                    if (prn) strList = strList + "(NOT TAKING/PRN STATUS)";
                                    medsTaking.Add(strList);
                                }

                                string strOut = "";
                                foreach (string strMed in medsTaking)
                                {
                                    strOut += strMed + "\n";
                                }

                                CurrentDoc.CurrentMeds = strOut;
                            }
                            if (strCurrentHeading == "Active Problem List")
                            {
                                string strTextToParse = strInnerText;
                                if (strInnerText == null) continue;
                                var result = strTextToParse.Split(new[] { '\r', '\n' });
                                List<string> ProblemList = new List<string>();
                                foreach (string str in result)
                                {
                                    if (str.Trim() == "") continue;
                                    if (str.StartsWith("Onset")) continue;
                                    ProblemList.Add(str);
                                }
                                string strOut = "";
                                foreach (string strProblem in ProblemList)
                                {
                                    strOut += strProblem + "\n";
                                }
                                CurrentDoc.ProblemList = strOut;
                            }
                            if (strCurrentHeading == "Past Medical History")
                            {
                                string strTextToParse = strInnerText;
                                var result = strTextToParse.Split(new[] { '\r', '\n' });
                                List<string> PMHxList = new List<string>();
                                foreach (string str in result)
                                {
                                    if (str.Trim() == "") continue;
                                    PMHxList.Add(str);
                                }
                                string strOut = "";
                                foreach (string strMHx in PMHxList)
                                {
                                    strOut += strMHx + "\n";
                                }
                                CurrentDoc.PMHx = strOut;
                            }
                            if (strCurrentHeading == "Social History")
                            {
                                CurrentDoc.SocHx = strInnerText;
                            }
                            if (strCurrentHeading == "Allergies")
                            {
                                CurrentDoc.Allergies = strInnerText;
                            }
                            if (strCurrentHeading == "Review of Systems")
                            {
                                CurrentDoc.ROS = strInnerText;
                            }
                            
                            if (strCurrentHeading == "Vital Signs")
                            {
                                CurrentDoc.Vitals = strInnerText;
                            }
                            if (strCurrentHeading == "Examination")
                            {
                                CurrentDoc.Exam = strInnerText;
                            }
                            if (strCurrentHeading == "Follow Up")
                            {
                                CurrentDoc.FollowUp = strInnerText;
                            }
                            if (strCurrentHeading == "Assessments")
                            {
                                string strTextToParse = strInnerText;
                                if (strInnerText == null) continue;
                                var result = strTextToParse.Split(new[] { '\r', '\n' });
                                foreach (string str in result)
                                {
                                    if (str.Trim() == "") continue;
                                    AssessmentList.Add(str);
                                }
                                string strOut = "";
                                foreach (string strProblem in AssessmentList)
                                {
                                    strOut += strProblem + "\n";
                                }
                                CurrentDoc.Assessments = strOut;
                            }

                            if (strCurrentHeading == "Treatment")
                            {
                                var result = strInnerText.Split(new[] { '\r', '\n' });
                                List<string> medsStarted = new List<string>();
                                List<string> LabsOrdered = new List<string>();
                                string strMedsSarted = "";
                                string strLabsOrdered = "";
                                foreach (string str in result)
                                {
                                    if (str.Trim().StartsWith("LAB:"))
                                    {
                                        LabsOrdered.Add(str);
                                        strLabsOrdered += str + "\n";
                                    }
                                    if (str.StartsWith("Start "))
                                    {
                                        medsStarted.Add(str);
                                        strMedsSarted += str + "\n";
                                    }
                                }
                                CurrentDoc.Treatment = strInnerText;
                                CurrentDoc.MedsStarted = strMedsSarted;
                                CurrentDoc.LabsOrdered = strLabsOrdered;
                            }

                            if (strCurrentHeading.StartsWith("Diagnostic Imaging") && strInnerText != null)
                            {
                                var result = strInnerText.Split(new[] { '\r', '\n' });
                                List<string> ImagessOrdered = new List<string>();
                                string strImagesOrdered = "";
                                foreach (string str in result)
                                {
                                    if (str.Trim().StartsWith("Imaging:"))
                                    {
                                        ImagessOrdered.Add(str.Trim());
                                        strImagesOrdered += str.Trim() + "\n";
                                    }
                                }
                                CurrentDoc.ImagesOrdered = strImagesOrdered;
                            }
                        }


                        //Console.WriteLine($"{strClass} - {TempEl.InnerText}");
                    }

                }
            }

            this.DataContext = null;
            this.DataContext = CurrentDoc;
            Console.WriteLine($"Run time: {watch.ElapsedMilliseconds}");

        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.Label l = sender as System.Windows.Controls.Label;
            System.Windows.Clipboard.SetText(l.Content.ToString());
        }

        private void EditorClick(object sender, RoutedEventArgs e)
        {
            winDbRelICD10CheckpointsEditor wdb = new winDbRelICD10CheckpointsEditor();
            wdb.ShowDialog();
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
    }
}
