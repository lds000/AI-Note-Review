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


//Ctrl-KX, or KS (surround) snippet

namespace AI_Note_Review
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string strApikey = "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";
        DocInfo CurrentDoc = new DocInfo();
        List<SqlCheckpoint> CheckPointList = new List<SqlCheckpoint>();

        public MainWindow()
        {
            InitializeComponent();
            ProgramInit();
            //C:\Users\llostod\source\repos\AI Note Review\NoteReviewDB.db
            SqlLiteDataAccess.SQLiteDBLocation = Properties.Settings.Default.DataFolder;

            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                string sql = "Select * from NoteSections;";
                SqlLiteDataAccess.NoteSections =  cnn.Query<SqlNoteSection>(sql).ToList();
                sql = "Select * from CheckPoints;";
                CheckPointList = cnn.Query<SqlCheckpoint>(sql).ToList();
                sql = "Select * from NoteSections;";
                CF.NoteSections = cnn.Query<SqlNoteSection>(sql).ToList();
                sql = "Select * from TagRegExTypes;";
                CF.TagRegExTypes = cnn.Query<SqlTagRegExType>(sql).ToList();
                sql = "Select * from ICD10Segments;";
                CF.NoteICD10Segments = cnn.Query<SqlICD10Segment>(sql).ToList();
            }

            CurrentDoc.PtName = "Mark Smith";
            CurrentDoc.Provider = "Lloyd Stolworthy, MD";
            CurrentDoc.PtAgeYrs = 18;
            CurrentDoc.PtSex = "M";
            CurrentDoc.VisitDate = "7/12/2021";
            CurrentDoc.ICD10s.Add("R10.10");
            CurrentDoc.ICD10s.Add("I10");

            CurrentDoc.HPI = "Mark is a 30yo male who presents today complaining of right lower quadrant abdominal pain that began two days ago and acutely worse today. " +
                "He denies diarrhea or constipation.  He states he cannot tolerate a full meal due to the pain.  " +
                "Mark has tried OTC medications.  He denies chest pain.  He denies blood in the vomit. Thus far he has tried ibuprofen and tylenol";

            CurrentDoc.CurrentMeds = "ibuprofen, Tylenol, prednisone";

            //******************************* Test document ************************
            CurrentDoc.ICD10s.Add("R11");
            CurrentDoc.NoteSectionText[0] = $"Age:57 Sex:F Elderly"; //Demographics 
            CurrentDoc.NoteSectionText[1] = CurrentDoc.HPI + CurrentDoc.ROS; //HPI
            CurrentDoc.NoteSectionText[2] = CurrentDoc.CurrentMeds; //CurrentMeds
            CurrentDoc.NoteSectionText[3] = CurrentDoc.ProblemList; //Active Problem List
            CurrentDoc.NoteSectionText[4] = CurrentDoc.PMHx; //Past Medical History
            CurrentDoc.NoteSectionText[5] = CurrentDoc.SocHx; //Social History
            CurrentDoc.NoteSectionText[6] = CurrentDoc.Allergies; //Allergies
            CurrentDoc.NoteSectionText[7] = CurrentDoc.Vitals; //Vital Signs
            CurrentDoc.NoteSectionText[8] = ""; //Examination
            CurrentDoc.NoteSectionText[9] = CurrentDoc.Assessments; //Assessments
            CurrentDoc.NoteSectionText[10] = CurrentDoc.Treatment; //Treatment
            CurrentDoc.NoteSectionText[11] = CurrentDoc.LabsOrdered; //Labs
            CurrentDoc.NoteSectionText[12] = CurrentDoc.ImagesOrdered; //Imaging
            CurrentDoc.NoteSectionText[13] = CurrentDoc.ROS; //Review of Systems
            CurrentDoc.NoteSectionText[14] = CurrentDoc.Assessments; //Assessments
            CurrentDoc.NoteSectionText[15] = CurrentDoc.MedsStarted; //Prescribed Medications

            this.DataContext = CurrentDoc;

            //GetHashTags();


            winDbRelICD10CheckpointsEditor wdb = new winDbRelICD10CheckpointsEditor();
            wdb.ShowDialog();
            //Close();

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


        private string[] noteSection = {"Chief Complaint(s):", "HPI:", "Current Medication:", "Medical History:", "Allergies/Intolerance:", "Surgical History:", "Hospitalization:",
            "Family History:", "Social History:", "ROS:", "Vitals:", "Examination:", "Assessment:","Treatment:","Procedures:","Immunizations:","Therapeutic Injections:","Diagnostic Imaging:",
            "Lab Reports:","Next Appointment:","Visit Code:","Procedure Codes:","Images:", "Objective:","Procedure Orders:","Preventive Medicine:","Billing Information:","Plan:" };


        /*
         * 
Chief Complaint(s):,HPI:, Current Medication:, Medical History:, Allergies/Intolerance:, Surgical History:, Hospitalization:, Family History:, Social History:, ROS:, Vitals:, Examination:, Assessment:,Treatment:,Procedures:,Immunizations:,Therapeutic Injections:,Diagnostic Imaging:,Lab Reports:,Next Appointment:,Visit Code:,Procedure Codes:,Images:
         */

        public void processUnlocked(HtmlDocument HDoc)
        {
            Console.WriteLine("Processing document");
            this.DataContext = null;
            string strNote = HDoc.Body.InnerText;
            string strCommand = "";
            string strMedType = "";
            CurrentDoc.Clear();
            foreach (var myString in strNote.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (myString.StartsWith("Patient:") && myString.Contains("DOB:"))
                {
                    string strPtInfo = myString.Replace("Patient:", "");
                    strPtInfo = strPtInfo.Replace("DOB:", "|");
                    strPtInfo = strPtInfo.Replace("Age:", "|");
                    strPtInfo = strPtInfo.Replace("Sex:", "|");
                    CurrentDoc.PtName = strPtInfo.Split('|')[0].Trim();
                    CurrentDoc.PtSex = strPtInfo.Split('|')[3].Trim();
                    string strDOB = strPtInfo.Split('|')[1];
                    CurrentDoc.DOB = DateTime.Parse(strDOB);
                }


                if (myString.StartsWith("Encounter Date:") && myString.Contains("Provider:"))
                {
                    string strPtInfo = myString.Replace("Encounter Date:", "");
                    strPtInfo = strPtInfo.Replace("Provider:", "|");
                    CurrentDoc.VisitDate = strPtInfo.Split('|')[0].Trim();
                    CurrentDoc.Provider = strPtInfo.Split('|')[1].Trim();
                }

                if (myString.StartsWith("Appointment Facility:"))
                {
                    CurrentDoc.Facility = myString.Split(':')[1];
                }


                if (noteSection.Contains(myString.Trim()))
                {
                    strCommand = myString.Trim();
                }
                else
                {
                    switch (strCommand)
                    {
                        case "HPI:":
                            if (myString.Trim() == "Respiratory Clinic") break; 
                            if (myString.Trim() == "Note:") break;
                            CurrentDoc.HPI += myString + Environment.NewLine;
                            break;
                        case "Allergies/Intolerance:":
                            CurrentDoc.Allergies += myString + Environment.NewLine;
                            break;
                        case "Medical History:":
                            CurrentDoc.PMHx += myString + Environment.NewLine;
                            break;
                        case "Current Medication:":
                            if (myString.Trim() == "None") break;
                            if (myString.Trim() == "Medication List reviewed and reconciled with the patient.") break;
                            if (myString.Trim() == "Not-Taking/PRN")
                            {
                                strMedType = "prn";
                                break;
                            }
                            if (myString.Trim() == "Taking")
                            {
                                strMedType = "active";
                                break;
                            }
                            if (myString.Trim() == "Unknown")
                            {
                                strMedType = "unknown";
                                break;
                            }
                            if (myString.Trim() == "Discontinued")
                            {
                                strMedType = "Discontinued";
                                break;
                            }
                            

                            if (strMedType == "prn")
                            {
                                CurrentDoc.CurrentPrnMeds += myString + " (prn)" + Environment.NewLine;
                                break;
                            }

                            if (strMedType == "unknown")
                            {
                                CurrentDoc.CurrentMeds += myString + " (unknown)" + Environment.NewLine;
                                break;
                            }

                            if (strMedType == "active")
                            {
                                CurrentDoc.CurrentMeds += myString + " (active)" + Environment.NewLine;
                                break;
                            }

                            if (strMedType == "Discontinued")
                            {
                                //CurrentDoc.CurrentPrnMeds += myString + " (Discontinued)" + Environment.NewLine;
                                break;
                            }
                            CurrentDoc.CurrentMeds += myString + " (??????)" + Environment.NewLine;
                            break;
                        case "Surgical History:":
                            CurrentDoc.SurgHx += myString + Environment.NewLine;
                            break;
                        case "Family History:":
                            CurrentDoc.FamHx += myString + Environment.NewLine;
                            break;
                        case "Social History:":
                            CurrentDoc.SocHx += myString + Environment.NewLine;
                            break;
                        case "ROS:":
                            CurrentDoc.ROS += myString + Environment.NewLine;
                            break;
                        case "Vitals:":
                            CurrentDoc.Vitals += myString + Environment.NewLine;
                            break;
                        case "Examination:":
                            CurrentDoc.Exam += myString + Environment.NewLine;
                            break;
                        case "Assessment:":
                            CurrentDoc.Assessments += myString + Environment.NewLine;
                            break;
                        case "Treatment:":
                            CurrentDoc.Treatment += myString + Environment.NewLine;
                            break;
                        case "Immunizations:":
                            CurrentDoc.Treatment += myString + Environment.NewLine;
                            break;
                        case "Therapeutic Injections:":
                            CurrentDoc.Treatment += myString + Environment.NewLine;
                            break;
                        case "Procedures:":
                            CurrentDoc.Treatment += myString + Environment.NewLine;
                            break;
                        case "Diagnostic Imaging:":
                            CurrentDoc.ImagesOrdered += myString + Environment.NewLine;
                            break;
                        case "Lab Reports:":
                            CurrentDoc.LabsOrdered += myString + Environment.NewLine;
                            break;
                        case "Next Appointment:":
                            CurrentDoc.FollowUp += myString + Environment.NewLine;
                            break;
                        case "Visit Code:":
                            break;
                        default:
                            break;
                    }
                }

            }
                foreach (var tmpAssessment in CurrentDoc.Assessments.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (tmpAssessment.Contains(" - "))
                    {
                        string strClean = tmpAssessment.TrimEnd(" (Primary) ").Trim();
                        strClean = strClean.Replace(" - ", "|");
                        string strCode = strClean.Split('|')[1].Trim();
                        CurrentDoc.ICD10s.Add(strCode);
                    }
                }

                CurrentDoc.NoteSectionText[0] = $"{CurrentDoc.PtAgeYrs} yo Sex{CurrentDoc.PtSex}"; //Demographics 
                CurrentDoc.NoteSectionText[1] = CurrentDoc.HPI + CurrentDoc.ROS; //HPI
                CurrentDoc.NoteSectionText[2] = CurrentDoc.CurrentMeds; //CurrentMeds
                CurrentDoc.NoteSectionText[3] = CurrentDoc.ProblemList; //Active Problem List
                CurrentDoc.NoteSectionText[4] = CurrentDoc.PMHx; //Past Medical History
                CurrentDoc.NoteSectionText[5] = CurrentDoc.SocHx; //Social History
                CurrentDoc.NoteSectionText[6] = CurrentDoc.Allergies; //Allergies
                CurrentDoc.NoteSectionText[7] = CurrentDoc.Vitals; //Vital Signs
                CurrentDoc.NoteSectionText[8] = CurrentDoc.Exam; //Examination
                CurrentDoc.NoteSectionText[9] = CurrentDoc.Assessments; //Assessments
                CurrentDoc.NoteSectionText[10] = CurrentDoc.Treatment; //Treatment
                CurrentDoc.NoteSectionText[11] = CurrentDoc.LabsOrdered; //Labs
                CurrentDoc.NoteSectionText[12] = CurrentDoc.ImagesOrdered; //Imaging
                CurrentDoc.NoteSectionText[13] = CurrentDoc.ROS; //Review of Systems
                CurrentDoc.NoteSectionText[14] = CurrentDoc.Assessments; //Assessments
                CurrentDoc.NoteSectionText[15] = CurrentDoc.MedsStarted; //Prescribed Medications

                GetHashTags();

                this.DataContext = null;
                this.DataContext = CurrentDoc;

            
            /*
             * 
            //mshtml.HTMLDocument hd = HDoc.DomDocument as mshtml.HTMLDocument;
            HtmlElementCollection AllTRItems = HDoc.Body.GetElementsByTagName("DIV"); //5 ms

            foreach (HtmlElement el in AllTRItems)
            {
                if (el.Id != null)
                if (el.Id =="pnData")
                {
                        foreach (HtmlElement childel in el.Children)
                        {
                            //Console.WriteLine(elspan.GetAttribute("cattabitemname"));
                            Console.WriteLine(el.InnerText);
                        }
                    }
            }


            //HtmlElementCollection AllTRItems = HDoc..Body.GetElementsByTagName("TR"); //5 ms
            */
        }

        public void processDocument(HtmlDocument HDoc)
        {
        

            if (HDoc.Body.InnerHtml.StartsWith("<LINK"))
            {
                processUnlocked(HDoc);
                return;
            }
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
                           // if (strInnerText.Contains("DOB:")) CurrentDoc.PtAge = strInnerText.Split(':')[0].TrimEnd("DOB");
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

            CurrentDoc.NoteSectionText[0] = $"{CurrentDoc.PtAgeYrs} Sex{CurrentDoc.PtSex}"; //Demographics 
            CurrentDoc.NoteSectionText[1] = CurrentDoc.HPI + CurrentDoc.ROS; //HPI
            CurrentDoc.NoteSectionText[2] = CurrentDoc.CurrentMeds; //CurrentMeds
            CurrentDoc.NoteSectionText[3] = CurrentDoc.ProblemList; //Active Problem List
            CurrentDoc.NoteSectionText[4] = CurrentDoc.PMHx; //Past Medical History
            CurrentDoc.NoteSectionText[5] = CurrentDoc.SocHx; //Social History
            CurrentDoc.NoteSectionText[6] = CurrentDoc.Allergies; //Allergies
            CurrentDoc.NoteSectionText[7] = CurrentDoc.Vitals; //Vital Signs
            CurrentDoc.NoteSectionText[8] = CurrentDoc.Exam; //Examination
            CurrentDoc.NoteSectionText[9] = CurrentDoc.Assessments; //Assessments
            CurrentDoc.NoteSectionText[10] = CurrentDoc.Treatment; //Treatment
            CurrentDoc.NoteSectionText[11] = CurrentDoc.LabsOrdered; //Labs
            CurrentDoc.NoteSectionText[12] = CurrentDoc.ImagesOrdered; //Imaging
            CurrentDoc.NoteSectionText[13] = CurrentDoc.ROS; //Review of Systems
            CurrentDoc.NoteSectionText[14] = CurrentDoc.Assessments; //Assessments
            CurrentDoc.NoteSectionText[15] = CurrentDoc.MedsStarted; //Prescribed Medications

            GetHashTags();

            this.DataContext = null;
            this.DataContext = CurrentDoc;
            Console.WriteLine($"Run time: {watch.ElapsedMilliseconds}");

        }


        private bool CheckTag(SqlTag tag)
        {
            if (CurrentDoc.DocumentTags.Contains(tag.TagText)) return true; //do not double check tags

            bool includeTag = false;
            List<SqlTagRegEx> tmpTagRegExs = tag.GetTagRegExs();

            foreach (SqlTagRegEx TagRegEx in tmpTagRegExs)
            {
                if (TagRegEx.TagRegExType == 1) //Any, if one match then include tag
                {
                    foreach (string strRegEx in TagRegEx.RegExText.Split(','))
                    {
                        
                        Console.WriteLine($"\tChecking Tag {tag.TagText} {strRegEx.Trim()}");
                        if (CurrentDoc.NoteSectionText[TagRegEx.TargetSection] != null)
                            if (Regex.IsMatch(CurrentDoc.NoteSectionText[TagRegEx.TargetSection], strRegEx.Trim()))
                            {
                                Console.WriteLine($"\t\tCondition met for the 'Any' RegEx Type due to Tag with text '{tag.TagText}'.");
                                includeTag = true;
                                break; //condition met, go to next.
                            }
                            else
                            {
                                Console.WriteLine($"\t\tCondition not met for the 'Any' RegEx Type due to Tag with text '{tag.TagText}'.");

                            }
                    }
                }
            }

            foreach (SqlTagRegEx TagRegEx in tmpTagRegExs)
            {
                if (TagRegEx.TagRegExType == 2) //ALL, if one not match then include tag
                {
                    if (CurrentDoc.NoteSectionText[TagRegEx.TargetSection] != null)
                        if (!Regex.IsMatch(CurrentDoc.NoteSectionText[TagRegEx.TargetSection], TagRegEx.RegExText))
                    {
                            Console.WriteLine($"Condition not met for the 'All' RegEx Type due to Tag with text '{tag.TagText}'.");
                            return false; //done! all must be included.
                    }
                }
            }

            foreach (SqlTagRegEx TagRegEx in tmpTagRegExs)
            {
                if (CurrentDoc.NoteSectionText[TagRegEx.TargetSection] != null)
                    if (TagRegEx.TagRegExType == 3) //none
                    {
                        if (CurrentDoc.NoteSectionText[TagRegEx.TargetSection] != null)
                            if (Regex.IsMatch(CurrentDoc.NoteSectionText[TagRegEx.TargetSection], TagRegEx.RegExText))
                            {
                                Console.WriteLine($"Condition met for the 'None' RegEx Type Tag with text '{tag.TagText}'.");
                                return true;
                            }
                    }
            }

            if (includeTag) CurrentDoc.DocumentTags.Add(tag.TagText);
            return includeTag;
        }

        private void GetHashTags()
        {
            CurrentDoc.DocumentTags.Clear();
            //get icd10 segments
            CF.RelevantICD10Segments.Clear();
            CF.RelevantICD10Segments = CurrentDoc.ICD10Segments;
            CF.PassedCP.Clear();
            CF.FailedCP.Clear();


            if (CF.RelevantICD10Segments.Count == 0)
            {
                //System.Windows.MessageBox.Show("No icd10 segments found!");
                return;
            }

            int[] relType = { 5, 6, 9, 10, 12 };

            foreach (SqlICD10Segment ns in CF.RelevantICD10Segments)
            {
                Console.WriteLine($"Now checking segment: {ns.SegmentTitle}");

                foreach (SqlCheckpoint cp in ns.GetCheckPoints())
                {
                    Console.WriteLine($"Now analyzing '{cp.CheckPointTitle}' checkpoint.");
                    bool pass = true;
                    foreach (SqlTag tag in cp.GetTags())
                    {
                       if (!CheckTag(tag)) pass = false;
                    }
                    if (pass)
                    {
                        if (relType.Contains(cp.CheckPointType))
                        {
                            CF.RelevantCP.Add(cp);
                        }
                        else
                        {
                            CF.PassedCP.Add(cp);
                        }

                    }
                    else
                    {
                        if (relType.Contains(cp.CheckPointType))
                        {
                            CF.IrrelaventCP.Add(cp);
                        }
                        else
                        {
                            CF.FailedCP.Add(cp);
                        }
                    }
                }
            }

            //Generate Report

            Console.WriteLine($"Document report:");
            foreach (string strTag in CurrentDoc.DocumentTags)
            {
                Console.WriteLine($"\tTag: {strTag}");
            }

            Console.WriteLine($"Passed Checkpoints");
            foreach (SqlCheckpoint cp in CF.PassedCP)
            {
                Console.WriteLine($"\t{cp.CheckPointTitle}");
            }

            Console.WriteLine($"Failed Checkpoints");
            foreach (SqlCheckpoint cp in CF.FailedCP)
            {
                Console.WriteLine($"\t{cp.CheckPointTitle}");
            }

            Console.WriteLine($"Relevant Checkpoints");
            foreach (SqlCheckpoint cp in CF.RelevantCP)
            {
                Console.WriteLine($"\t{cp.CheckPointTitle}");
            }

            Console.WriteLine($"Irrelavant Checkpoints");
            foreach (SqlCheckpoint cp in CF.IrrelaventCP)
            {
                Console.WriteLine($"\t{cp.CheckPointTitle}");
            }


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
            GetHashTags();
            foreach (SqlICD10Segment icd10Segment in CF.RelevantICD10Segments)
            {
                Console.WriteLine($"Report for {icd10Segment.SegmentTitle}");
                foreach (SqlCheckpoint cp in icd10Segment.GetCheckPoints())
                {
                }
            }
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
    }
}
