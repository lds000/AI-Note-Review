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


//Ctrl-KX, or KS (surround) snippet

namespace AI_Note_Review
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string strApikey = "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";

        public void SetDataContext()
        {

            //add hashtags here.
            CF.CurrentDoc.HashTags = "";
            if (CF.CurrentDoc.PtAgeYrs > 65) CF.CurrentDoc.HashTags += "!Elderly, ";
            if (CF.CurrentDoc.PtSex.StartsWith("M")) CF.CurrentDoc.HashTags += "#Male, ";
            if (CF.CurrentDoc.PtSex.StartsWith("F")) CF.CurrentDoc.HashTags += "#Female, ";
            if (CF.CurrentDoc.PtAgeYrs < 4) CF.CurrentDoc.HashTags += "#Child, ";
            if (CF.CurrentDoc.IsHTNUrgency) CF.CurrentDoc.HashTags += "!HTNUrgency, ";
            if (CF.CurrentDoc.isO2Abnormal) CF.CurrentDoc.HashTags += "!Hypoxic, ";
            if (CF.CurrentDoc.IsPregCapable) CF.CurrentDoc.HashTags += "!pregnantcapable, ";
            if (CF.CurrentDoc.PtAgeYrs >= 13) CF.CurrentDoc.HashTags += "!sexuallyActiveAge, ";



            CF.CurrentDoc.NoteSectionText[0] = $"{CF.CurrentDoc.PtAgeYrs} Sex{CF.CurrentDoc.PtSex}"; //Demographics 
            CF.CurrentDoc.NoteSectionText[1] = CF.CurrentDoc.HPI + CF.CurrentDoc.ROS; //HPI
            CF.CurrentDoc.NoteSectionText[2] = CF.CurrentDoc.CurrentMeds + CF.CurrentDoc.CurrentPrnMeds; //CurrentMeds
            CF.CurrentDoc.NoteSectionText[3] = CF.CurrentDoc.ProblemList; //Active Problem List
            CF.CurrentDoc.NoteSectionText[4] = CF.CurrentDoc.PMHx; //Past Medical History
            CF.CurrentDoc.NoteSectionText[5] = CF.CurrentDoc.SocHx; //Social History
            CF.CurrentDoc.NoteSectionText[6] = CF.CurrentDoc.Allergies; //Allergies
            CF.CurrentDoc.NoteSectionText[7] = CF.CurrentDoc.Vitals; //Vital Signs
            CF.CurrentDoc.NoteSectionText[8] = CF.CurrentDoc.Exam; //Examination
            CF.CurrentDoc.NoteSectionText[9] = CF.CurrentDoc.Assessments; //Assessments
            CF.CurrentDoc.NoteSectionText[10] = CF.CurrentDoc.Treatment; //Treatment
            CF.CurrentDoc.NoteSectionText[11] = CF.CurrentDoc.LabsOrdered; //Labs
            CF.CurrentDoc.NoteSectionText[12] = CF.CurrentDoc.ImagesOrdered; //Imaging
            CF.CurrentDoc.NoteSectionText[13] = CF.CurrentDoc.ROS; //Review of Systems
            CF.CurrentDoc.NoteSectionText[14] = CF.CurrentDoc.Assessments; //Assessments
            CF.CurrentDoc.NoteSectionText[15] = CF.CurrentDoc.MedsStarted; //Prescribed Medications
            CF.CurrentDoc.NoteSectionText[16] = CF.CurrentDoc.FamHx;
            CF.CurrentDoc.NoteSectionText[17] = CF.CurrentDoc.SurgHx;
            CF.CurrentDoc.NoteSectionText[18] = CF.CurrentDoc.HashTags;
            CF.CurrentDoc.NoteSectionText[19] = CF.CurrentDoc.CC;

            //CF.CurrentDoc.NoteSectionText[1].ParseHistory();

            //this.DataContext = null;
            //this.DataContext = CF.CurrentDoc;
        }


        public MainWindow()
        {
            InitializeComponent();
            ProgramInit();
            //C:\Users\Lloyd\Source\Repos\lds000\NoteReviewDB.db
            //C:\Users\llostod\source\repos\AI Note Review\NoteReviewDB.db
            SqlLiteDataAccess.SQLiteDBLocation = Properties.Settings.Default.DataFolder;

            if (File.Exists(@"C:\Users\Lloyd\Source\Repos\lds000\AI-Note-Review\NoteReviewDB.db"))
            {
                SqlLiteDataAccess.SQLiteDBLocation = @"C:\Users\Lloyd\Source\Repos\lds000\AI-Note-Review\NoteReviewDB.db";
            }
            else
            {
                SqlLiteDataAccess.SQLiteDBLocation = @"C:\Users\llostod\source\repos\AI Note Review\NoteReviewDB.db";
            }

            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                string sql = "Select * from CheckPoints;";
                CF.CheckPointList = cnn.Query<SqlCheckpoint>(sql).ToList();
                sql = "Select * from NoteSections order by SectionOrder;";
                CF.NoteSections = cnn.Query<SqlNoteSection>(sql).ToList();
                sql = "Select * from TagRegExTypes;";
                CF.TagRegExTypes = cnn.Query<SqlTagRegExType>(sql).ToList();
                sql = "Select * from ICD10Segments;";
                CF.NoteICD10Segments = cnn.Query<SqlICD10Segment>(sql).ToList();
            }

            CF.CurrentDoc.PtName = "Mark Smith";
            CF.CurrentDoc.Provider = "Lloyd Stolworthy, MD";
            CF.CurrentDoc.PtAgeYrs = 18;
            CF.CurrentDoc.PtSex = "M";
            CF.CurrentDoc.VisitDate = "7/12/2021";
            CF.CurrentDoc.CC = "Abdominal pain for 10 days";
            CF.CurrentDoc.DOB = new DateTime(1969, 10, 23);
            CF.CurrentDoc.Facility = "Meridian UC";
            CF.CurrentDoc.VitalsBMI = 41;
            CF.CurrentDoc.VitalsDiastolic = 115;
            CF.CurrentDoc.VitalsHR = 88;
            CF.CurrentDoc.VitalsO2 = 92;
            CF.CurrentDoc.VitalsRR = 16;
            CF.CurrentDoc.VitalsSystolic = 182;
            CF.CurrentDoc.VitalsTemp = 101.2;
            CF.CurrentDoc.VitalsWt = 194;
            CF.CurrentDoc.ICD10s.Add("R10.10");
            CF.CurrentDoc.ICD10s.Add("I10");

            CF.CurrentDoc.HPI = "Mark is a 30yo male who presents today complaining of right lower quadrant abdominal pain that began two days ago and acutely worse today. " +
                "He denies diarrhea or constipation.  He states he cannot tolerate a full meal due to the pain.  " +
                "Mark has tried OTC medications.  He denies chest pain.  He denies blood in the vomit. Thus far he has tried ibuprofen and tylenol";

            CF.CurrentDoc.HPI = "Dalton Anthony Ware is a 26 y.o. male who presents with a chief complaint of rash, blisters. Patient presents to the ER today with rash, lesions in the mouth, nose and eye on the left.He indicates that about 2 - 3 weeks ago he felt like he got a cold / sinus infection.He was taking multiple medications for this including NSAID, mucinex, spray in the nose to help with this.Didn't have a cough, didn't have a fever.  Had some chest discomfort that he felt was due to some chest congestion that seems to have resolved.Never had any n / v / d.He has not had any pain with urination, but indicates that his urine smells funny like he has had asparagus, but he has not. On Tuesday felt like he was getting a canker sore on the left side of the lip and by the next day was getting larger.He now has very large sores on the left, bilateral cheeks and under the tongue, also feels like something in the throat as well.He has some pain and irritation up in the nose on the left side, feels some crusting there.He has had purulent drainage from the left eye as well over the last couple of days and some generalized irritation.He has not been able to eat / drink much over the last couple of days due to the oral discomfort.He did use a new toothpaste once prior to this all starting, no longer using, this was thought to be part of the cause.  He denies any current n / v / d.He was told that might be SJS and he then looked at the scrotum today and feels like it might be more red than normal, but again, no pain with urination.No fever or chills.  He was never tested for COVID during the URI type illness that he had prior.He does currently complain of headache and pressure behind the eyes as well. No oral sex, patient states ever.Neither have ever had STI otherwise. Patients partner is 7 months pregnant at this point as well.He has never had acold sore, but does get canker sores occasionally.";

            CF.CurrentDoc.CurrentMeds = "ibuprofen, Tylenol, prednisone";


            //CF.CurrentDoc.HPI.ParseHistory();
            //Close();

            SetDataContext();
            this.DataContext = CF.CurrentDoc;

            //winDbRelICD10CheckpointsEditor wdb = new winDbRelICD10CheckpointsEditor();
            //wdb.ShowDialog();
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
                                    if (CF.CurrentDoc.PtName != "") break;
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
            //this.DataContext = null;
            string strNote = HDoc.Body.InnerText;
            if (strNote == null) return;
            string strCommand = "";
            string strMedType = "";
            CF.CurrentDoc.Clear();
            foreach (var myString in strNote.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (myString.StartsWith("Patient:") && myString.Contains("DOB:"))
                {
                    string strPtInfo = myString.Replace("Patient:", "");
                    strPtInfo = strPtInfo.Replace("DOB:", "|");
                    strPtInfo = strPtInfo.Replace("Age:", "|");
                    strPtInfo = strPtInfo.Replace("Sex:", "|");
                    CF.CurrentDoc.PtName = strPtInfo.Split('|')[0].Trim();
                    CF.CurrentDoc.PtSex = strPtInfo.Split('|')[3].Trim();
                    string strDOB = strPtInfo.Split('|')[1];
                    CF.CurrentDoc.DOB = DateTime.Parse(strDOB);
                }


                if (myString.StartsWith("Encounter Date:") && myString.Contains("Provider:"))
                {
                    string strPtInfo = myString.Replace("Encounter Date:", "");
                    strPtInfo = strPtInfo.Replace("Provider:", "|");
                    CF.CurrentDoc.VisitDate = strPtInfo.Split('|')[0].Trim();
                    CF.CurrentDoc.Provider = strPtInfo.Split('|')[1].Trim();
                }

                if (myString.StartsWith("Appointment Facility:"))
                {
                    CF.CurrentDoc.Facility = myString.Split(':')[1];
                }


                if (noteSection.Contains(myString.Trim()))
                {
                    strCommand = myString.Trim();
                }
                else
                {
                    switch (strCommand)
                    {
                        case "Chief Complaint(s):":
                            CF.CurrentDoc.CC += myString;
                            break;
                        case "HPI:":
                            if (myString.Trim() == "Respiratory Clinic") break;
                            if (myString.Trim() == "Note:") break;
                            CF.CurrentDoc.HPI += myString + Environment.NewLine;
                            break;
                        case "Allergies/Intolerance:":
                            CF.CurrentDoc.Allergies += myString + Environment.NewLine;
                            break;
                        case "Medical History:":
                            CF.CurrentDoc.PMHx += myString + Environment.NewLine;
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
                                CF.CurrentDoc.CurrentPrnMeds += myString + " (prn)" + Environment.NewLine;
                                break;
                            }

                            if (strMedType == "unknown")
                            {
                                CF.CurrentDoc.CurrentMeds += myString + " (unknown)" + Environment.NewLine;
                                break;
                            }

                            if (strMedType == "active")
                            {
                                CF.CurrentDoc.CurrentMeds += myString + " (active)" + Environment.NewLine;
                                break;
                            }

                            if (strMedType == "Discontinued")
                            {
                                //CF.CurrentDoc.CurrentPrnMeds += myString + " (Discontinued)" + Environment.NewLine;
                                break;
                            }
                            CF.CurrentDoc.CurrentMeds += myString + " (??????)" + Environment.NewLine;
                            break;
                        case "Surgical History:":
                            CF.CurrentDoc.SurgHx += myString + Environment.NewLine;
                            break;
                        case "Family History:":
                            CF.CurrentDoc.FamHx += myString + Environment.NewLine;
                            break;
                        case "Social History:":
                            CF.CurrentDoc.SocHx += myString + Environment.NewLine;
                            break;
                        case "ROS:":
                            CF.CurrentDoc.ROS += myString + Environment.NewLine;
                            break;
                        case "Vitals:":
                            CF.CurrentDoc.Vitals += myString + Environment.NewLine;
                            break;
                        case "Examination:":
                            CF.CurrentDoc.Exam += myString + Environment.NewLine;
                            break;
                        case "Assessment:":
                            CF.CurrentDoc.Assessments += myString + Environment.NewLine;
                            break;
                        case "Treatment:":
                            CF.CurrentDoc.Treatment += myString + Environment.NewLine;
                            break;
                        case "Immunizations:":
                            CF.CurrentDoc.Treatment += myString + Environment.NewLine;
                            break;
                        case "Therapeutic Injections:":
                            CF.CurrentDoc.Treatment += myString + Environment.NewLine;
                            break;
                        case "Procedures:":
                            CF.CurrentDoc.Treatment += myString + Environment.NewLine;
                            break;
                        case "Diagnostic Imaging:":
                            CF.CurrentDoc.ImagesOrdered += myString + Environment.NewLine;
                            break;
                        case "Lab Reports:":
                            CF.CurrentDoc.LabsOrdered += myString + Environment.NewLine;
                            break;
                        case "Next Appointment:":
                            CF.CurrentDoc.FollowUp += myString + Environment.NewLine;
                            break;
                        case "Visit Code:":
                            break;
                        default:
                            break;
                    }
                }

            }

            //parse Vitals
            //       BP 138/74, HR 144, Temp 97.9, O2 sat % 99.
            string strVitals = "";
            if (CF.CurrentDoc.Vitals != "")
            {
                strVitals = CF.CurrentDoc.Vitals.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)[0];
            }
            strVitals = strVitals.Trim().TrimEnd('.');

            CF.CurrentDoc.VitalsRR = 0;
            CF.CurrentDoc.VitalsHR = 0;
            CF.CurrentDoc.VitalsSystolic = 0;
            CF.CurrentDoc.VitalsDiastolic = 0;
            CF.CurrentDoc.VitalsTemp = 0;
            CF.CurrentDoc.VitalsO2 = 0;
            CF.CurrentDoc.VitalsWt = 0;
            CF.CurrentDoc.VitalsBMI = 0;

            foreach (string strPartVital in strVitals.Split(','))
            {
                if (strPartVital.Contains("BP"))
                {
                    try
                    {
                        CF.CurrentDoc.VitalsSystolic = int.Parse(strPartVital.Trim().Split(' ')[1].Split('/')[0]);
                        CF.CurrentDoc.VitalsDiastolic = int.Parse(strPartVital.Trim().Split(' ')[1].Split('/')[1]);
                    }
                    catch
                    {
                        Console.WriteLine("Error obtainin BP.");
                    }

                }


                if (strPartVital.Contains("HR"))
                {
                    try
                    {
                        CF.CurrentDoc.VitalsHR = int.Parse(strPartVital.Trim().Split(' ')[1].Trim());
                    }
                    catch
                    {
                        Console.WriteLine("Error obtainin HR.");
                    }

                }

                if (strPartVital.Contains("RR"))
                {
                    try
                    {
                        CF.CurrentDoc.VitalsRR = int.Parse(strPartVital.Trim().Split(' ')[1].Trim());
                    }
                    catch
                    {
                        Console.WriteLine("Error obtainin RR.");
                    }

                }

                if (strPartVital.Contains("Wt"))
                {
                    try
                    {
                        CF.CurrentDoc.VitalsWt = double.Parse(strPartVital.Trim().Split(' ')[1].Trim());
                    }
                    catch
                    {
                        Console.WriteLine("Error obtainin Wt.");
                    }

                }
                if (strPartVital.Contains("BMI"))
                {
                    try
                    {
                        CF.CurrentDoc.VitalsBMI = double.Parse(strPartVital.Trim().Split(' ')[1].Trim());
                    }
                    catch
                    {
                        Console.WriteLine("Error obtainin Wt.");
                    }

                }


                if (strPartVital.Contains("O2"))
                {
                    try
                    {
                        CF.CurrentDoc.VitalsO2 = int.Parse(strPartVital.Trim().Split('%')[1].Trim());
                    }
                    catch
                    {
                        Console.WriteLine("Error obtaining O2.");
                    }

                }

                if (strPartVital.Contains("Temp"))
                {
                    try
                    {
                        CF.CurrentDoc.VitalsTemp = double.Parse(strPartVital.Trim().Split(' ')[1].Trim());
                    }
                    catch
                    {
                        Console.WriteLine("Error obtainin temp.");
                    }

                }

            }



            //parse icd10
            CF.CurrentDoc.ICD10s = new List<string>();
            foreach (var tmpAssessment in CF.CurrentDoc.Assessments.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (tmpAssessment.Contains(" - "))
                    {
                        string strClean = tmpAssessment.TrimEnd(" (Primary) ").Trim();
                        strClean = strClean.Replace(" - ", "|");
                        string strCode = strClean.Split('|')[1].Trim();
                        CF.CurrentDoc.ICD10s.Add(strCode);
                    }
                }

                SetDataContext();

            
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


                CF.CurrentDoc.NoteHTML = HDoc.Body.InnerHtml; //3 ms


                string strCurrentHeading = "";
                foreach (HtmlElement TempEl in AllTHEADItems) //1 item, 1.9 seconds!
                {
                    if (TempEl.TagName == "THEAD")
                    {
                        string strInnerText = TempEl.InnerText;
                        if (strInnerText.Contains("DOS:"))
                        {
                            strInnerText = strInnerText.Replace("DOS:", "|");
                            CF.CurrentDoc.VisitDate = strInnerText.Split('|')[1];
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
                                    CF.CurrentDoc.Provider = strDocname.Split('|')[0];
                                }
                                continue;
                            }
                            if (strClass == "PtHeading") CF.CurrentDoc.PtName = TempEl.InnerText; //set first name
                            if (strClass == "PtData") //field has note informaition
                            {
                                string strInnerText = TempEl.InnerText;
                                if (strInnerText.Contains("Account Number:")) CF.CurrentDoc.PtID = strInnerText.Split(':')[1].Trim();
                                if (strInnerText.Contains("Appointment Facility:")) CF.CurrentDoc.Facility = strInnerText.Split(':')[1].Trim();
                                // if (strInnerText.Contains("DOB:")) CF.CurrentDoc.PtAge = strInnerText.Split(':')[0].TrimEnd("DOB");
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
                                    CF.CurrentDoc.CC = strInnerText.Substring(3);
                                }
                                if (strCurrentHeading == "History of Present Illness")
                                {
                                    CF.CurrentDoc.HPI = strInnerText;
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

                                    CF.CurrentDoc.CurrentMeds = strOut;
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
                                    CF.CurrentDoc.ProblemList = strOut;
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
                                    CF.CurrentDoc.PMHx = strOut;
                                }
                                if (strCurrentHeading == "Social History")
                                {
                                    CF.CurrentDoc.SocHx = strInnerText;
                                }
                                if (strCurrentHeading == "Allergies")
                                {
                                    CF.CurrentDoc.Allergies = strInnerText;
                                }
                                if (strCurrentHeading == "Review of Systems")
                                {
                                    CF.CurrentDoc.ROS = strInnerText;
                                }

                                if (strCurrentHeading == "Vital Signs")
                                {
                                    CF.CurrentDoc.Vitals = strInnerText;
                                }
                                if (strCurrentHeading == "Examination")
                                {
                                    CF.CurrentDoc.Exam = strInnerText;
                                }
                                if (strCurrentHeading == "Follow Up")
                                {
                                    CF.CurrentDoc.FollowUp = strInnerText;
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
                                    CF.CurrentDoc.Assessments = strOut;
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
                                    CF.CurrentDoc.Treatment = strInnerText;
                                    CF.CurrentDoc.MedsStarted = strMedsSarted;
                                    CF.CurrentDoc.LabsOrdered = strLabsOrdered;
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
                                    CF.CurrentDoc.ImagesOrdered = strImagesOrdered;
                                }
                            }


                            //Console.WriteLine($"{strClass} - {TempEl.InnerText}");
                        }

                    }
                

                SetDataContext();

                Console.WriteLine($"Run time: {watch.ElapsedMilliseconds}");

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
            WinReport wp = new WinReport();
            wp.ShowDialog();
        }

        private void EditCP(object sender, RoutedEventArgs e)
        {
            winDbRelICD10CheckpointsEditor w = new winDbRelICD10CheckpointsEditor();
            w.Owner = this;
            w.ShowDialog();
        }

        /*
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbSegments == null) return;
            if (lbSegments.SelectedValue == null) return;
            SqlICD10Segment seg = lbSegments.SelectedValue as SqlICD10Segment;
            if (seg != null)
            {
                winDbRelICD10CheckpointsEditor w = new winDbRelICD10CheckpointsEditor();
                
                foreach (SqlICD10Segment tmpSeg in w.lbICD10.Items)
                {
                    if (tmpSeg.ICD10SegmentID == seg.ICD10SegmentID)
                    {
                        w.lbICD10.SelectedValue = tmpSeg;
                    }
                }
                w.ShowDialog();
            }
            }
            */

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void spReviewDate_MouseDown(object sender, MouseButtonEventArgs e)
        {
            WinGetReviewDate wr = new WinGetReviewDate();
            wr.Owner = this;
            wr.ShowDialog();
            CF.CurrentDoc.ReviewDate = wr.SelectedDate;
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
}
