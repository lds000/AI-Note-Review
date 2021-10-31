using Dapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;

namespace AI_Note_Review
{
    public class DocumentVM : INotifyPropertyChanged
    {
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public bool GeneralCheckPointsOnly { get; set; }

        private PatientVM patient;
        private DocumentM document;
        private SqlProvider sqlProvider;

        public DocumentVM(SqlProvider prov, PatientVM pvm)
        {
            sqlProvider = prov;
            patient = pvm;
            document = SampleDocument; //New DocumentM() called under this.
            SetUpNote(); //todo: might be better way of implementing this.
        }

        public DocumentVM()
        {
            document = SampleDocument; //New DocumentM() called under this.
            SetUpNote(); //todo: might be better way of implementing this.
        }

        #region mirror properties of DocumentM
        public string Facility { get { return document.Facility; } set { document.Facility = value; OnPropertyChanged(); } }
        public string Provider { get { return document.Provider; } set { document.Provider = value; OnPropertyChanged(); } }
        public SqlProvider ProviderSql { get { return document.ProviderSql; } set { document.ProviderSql = value; OnPropertyChanged(); } }
        public int ProviderID { get { return document.ProviderID; } set { document.ProviderID = value; OnPropertyChanged(); } }
        public DateTime VisitDate { get { return document.VisitDate; } set { document.VisitDate = value; OnPropertyChanged(); } }
        public string HashTags { get { return document.HashTags; } set { document.HashTags = value; OnPropertyChanged(); } }
        public ObservableCollection<string> ICD10s { get { return document.ICD10s; } set { document.ICD10s = value; OnPropertyChanged(); } }
        //try to get rid of these!!!
        public string[] NoteSectionText { get { return document.NoteSectionText; } }
        public string NoteHTML { get { return document.NoteHTML; } set { document.NoteHTML = value; OnPropertyChanged(); } }
        public string ReasonForAppt { get { return document.ReasonForAppt; } set { document.ReasonForAppt = value; OnPropertyChanged(); } }
        public string CC { get { return document.CC; } set { document.CC = value; OnPropertyChanged();             }        }
        public string HPI { get { return document.HPI; } set { document.HPI = value; OnPropertyChanged(); } }
        public string Allergies { get { return document.Allergies; } set { document.Allergies = value; OnPropertyChanged(); } }
        public string SurgHx { get { return document.SurgHx; } set { document.SurgHx = value; OnPropertyChanged(); } }
        public string FamHx { get { return document.FamHx; } set { document.FamHx = value; OnPropertyChanged(); } }
        public string CurrentMeds { get { return document.CurrentMeds; } set { document.CurrentMeds = value; OnPropertyChanged(); } }
        public string CurrentPrnMeds { get { return document.CurrentPrnMeds; } set { document.CurrentPrnMeds = value; OnPropertyChanged(); } }
        public string ProblemList { get { return document.ProblemList; } set { document.ProblemList = value; OnPropertyChanged(); } }
        public string ROS { get { return document.ROS; } set { document.ROS = value; OnPropertyChanged(); } }
        public string PMHx { get { return document.PMHx; } set { document.PMHx = value; OnPropertyChanged(); } }
        public string SocHx { get { return document.SocHx; } set { document.SocHx = value; OnPropertyChanged(); } }
        public string GeneralHx { get { return document.GeneralHx; } set { document.GeneralHx = value; OnPropertyChanged(); } }
        public string Vitals { get { return document.Vitals; } set { document.Vitals = value; OnPropertyChanged(); } }
        public string Exam { get { return document.Exam; } set { document.Exam = value; OnPropertyChanged(); } }
        public string Treatment { get { return document.Treatment; } set { document.Treatment = value; OnPropertyChanged(); } }
        public string PreventiveMed { get { return document.PreventiveMed; } set { document.PreventiveMed = value; OnPropertyChanged(); } }
        public string MedsStarted { get { return document.MedsStarted; } set { document.MedsStarted = value; OnPropertyChanged(); } }
        public string ImagesOrdered { get { return document.ImagesOrdered; } set { document.ImagesOrdered = value; OnPropertyChanged(); } }
        public string VisitCodes { get { return document.VisitCodes; } set { document.VisitCodes = value; OnPropertyChanged(); } }
        public string LabsOrdered { get { return document.LabsOrdered; } set { document.LabsOrdered = value; OnPropertyChanged(); } }
        public string Assessments { get { return document.Assessments; } set { document.Assessments = value; OnPropertyChanged(); } }
        public string FollowUp { get { return document.FollowUp; } set { document.FollowUp = value; OnPropertyChanged(); } }

        //public string { get {return document.;} set{document. = value;} }
        public string ProcedureNote { get { return document.ProcedureNote; } set { document.ProcedureNote = value; OnPropertyChanged(); } }
        #endregion  

        //{ get {return document.;} set{document. = value;} }

        public PatientVM Patient
        {
            get
            {
                return patient;
            }
        }

        public SqlProvider SqlProvider
        {
            get
            {
                return sqlProvider;
            }
        }

        /// <summary>
        /// return a sample document for testing
        /// </summary>
        public DocumentM SampleDocument
        {
            get
            {
                document = new DocumentM();
                Provider = "Devin Hansen"; //Provider ID comes from this.
                VisitDate = new DateTime(2021, 10, 14);
                CC = "Abdominal pain for 10 days";
                Facility = "Meridian UC";
                ICD10s.Add("R10.10");
                ICD10s.Add("I10");
                //DateTime.Now;

                HPI = "Mark is a 30yo male who presents today complaining of right lower quadrant abdominal pain that began two days ago and acutely worse today. " +
                    "He denies diarrhea or constipation.  He states he cannot tolerate a full meal due to the pain.  " +
                    "Mark has tried OTC medications.  He denies chest pain.  He denies blood in the vomit. Thus far he has tried ibuprofen and tylenol";

                HPI = "Dalton Anthony Ware is a 26 y.o. male who presents with a chief complaint of rash, blisters. Patient presents to the ER today with rash, lesions in the mouth, nose and eye on the left.He indicates that about 2 - 3 weeks ago he felt like he got a cold / sinus infection.He was taking multiple medications for this including NSAID, mucinex, spray in the nose to help with this.Didn't have a cough, didn't have a fever.  Had some chest discomfort that he felt was due to some chest congestion that seems to have resolved.Never had any n / v / d.He has not had any pain with urination, but indicates that his urine smells funny like he has had asparagus, but he has not. On Tuesday felt like he was getting a canker sore on the left side of the lip and by the next day was getting larger.He now has very large sores on the left, bilateral cheeks and under the tongue, also feels like something in the throat as well.He has some pain and irritation up in the nose on the left side, feels some crusting there.He has had purulent drainage from the left eye as well over the last couple of days and some generalized irritation.He has not been able to eat / drink much over the last couple of days due to the oral discomfort.He did use a new toothpaste once prior to this all starting, no longer using, this was thought to be part of the cause.  He denies any current n / v / d.He was told that might be SJS and he then looked at the scrotum today and feels like it might be more red than normal, but again, no pain with urination.No fever or chills.  He was never tested for COVID during the URI type illness that he had prior.He does currently complain of headache and pressure behind the eyes as well. No oral sex, patient states ever.Neither have ever had STI otherwise. Patients partner is 7 months pregnant at this point as well.He has never had acold sore, but does get canker sores occasionally.";

                CurrentMeds = "ibuprofen, Tylenol, prednisone";
                Exam = "AO, NAD PERRL\nNormal OP\nCTA bilat\nRRR no murmurs\nS NTND NABS, no guarding, no rebound\nNo edema";
                return document;
            }
        }

        /// <summary>
        /// Helper method for SetUpNote
        /// </summary>
        /// <param name="strHashTag"></param>
        public void AddHashTag(string strHashTag)
        {
            HashTags += strHashTag + ", ";
        }

        /// <summary>
        /// After the document has been copied from ECW to AINoteReview, process the information
        /// </summary>
        public void SetUpNote()
        {
            //add hashtags here. #Hash
            HashTags = "";
            if (patient.PtAgeYrs > 65) AddHashTag("@Elderly");             //75	X	5	5	Elderly
            if (patient.PtSex.StartsWith("F")) AddHashTag("@Female");
            if (patient.PtAgeYrs < 4) AddHashTag("@Child");             //80	X	7	7	Children
            if (patient.PtAgeYrs < 2) AddHashTag("@Infant");             //76	X	6	6	Infant
            if (patient.IsHTNUrgency) AddHashTag("!HTNUrgency");             //40	X	1	1	Hypertensive Urgency
            if (patient.isO2Abnormal) AddHashTag("!Hypoxic");
            if (patient.IsPregCapable) AddHashTag("@pregnantcapable");            //82	X	9	9	Possible Pregnant State
            if (patient.PtAgeYrs >= 13) AddHashTag("@sexuallyActiveAge");
            if (patient.PtAgeYrs >= 16) AddHashTag("@DrinkingAge");
            if (patient.PtAgeYrs >= 2) AddHashTag("@SpeakingAge");
            if (patient.PtAgeYrs < 1) AddHashTag("@Age<1");
            if (patient.PtAgeYrs < 2) AddHashTag("@Age<2");
            if (patient.PtAgeYrs < 4) AddHashTag("@Age<4");
            if (patient.GetAgeInDays < 183) AddHashTag("@Age<6mo");
            if (patient.isRRHigh) AddHashTag("!RRHigh");             //72	X	2	2	Rapid Respiratory Rate
            if (patient.isTempHigh) AddHashTag("!HighFever");             //73	X	3	3	High Fever
            if (patient.isHRHigh) AddHashTag("!Tachycardia");             //74	X	4	4	Tachycardia
            if (patient.GetAgeInDays <= 90 && patient.VitalsTemp > 100.4)
            {
                //MessageBoxResult mr = MessageBox.Show($"This patient is {patient.GetAgeInDays()} days old and has a fever of {patient.VitalsTemp}.  Was the patient sent to an ED or appropriate workup performed?", "Infant Fever", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                //if (mr == MessageBoxResult.No) DocumentM.HashTags += "#NeonteNotSentToED";
            }
            HashTags = HashTags.TrimEnd().TrimEnd(',');

            //not happy with this, set notesection texts.
            NoteSectionText[0] = $"{patient.PtAgeYrs} Sex{patient.PtSex}"; //Demographics 
            NoteSectionText[1] = HPI + ROS; //HPI
            NoteSectionText[2] = CurrentMeds + CurrentPrnMeds; //CurrentMeds
            NoteSectionText[3] = ProblemList; //Active Problem List
            NoteSectionText[4] = PMHx; //Past Medical History
            NoteSectionText[5] = SocHx; //Social History
            NoteSectionText[6] = Allergies; //Allergies
            NoteSectionText[7] = Vitals; //Vital Signs
            NoteSectionText[8] = Exam; //Examination
            NoteSectionText[9] = Assessments; //Assessments
            NoteSectionText[10] = Treatment; //Treatment
            NoteSectionText[11] = LabsOrdered; //Labs
            NoteSectionText[12] = ImagesOrdered; //Imaging
            NoteSectionText[13] = ROS; //Review of Systems
            NoteSectionText[14] = Assessments; //Assessments
            NoteSectionText[15] = MedsStarted; //Prescribed Medications
            NoteSectionText[16] = FamHx;
            NoteSectionText[17] = SurgHx;
            NoteSectionText[18] = HashTags;
            NoteSectionText[19] = CC;
            NoteSectionText[20] = ProcedureNote;
            NoteSectionText[21] = PreventiveMed;
            OnPropertyChanged("Patient");
        }

        /// <summary>
        /// Clear Note Values
        /// </summary>
        public void Clear()
        {
            //demographics
            Facility = "";
            VisitDate = new DateTime(2020, 1, 1);
            Provider = "";
            ProviderID = 0;
            ReasonForAppt = "";
            Allergies = "";
            NoteHTML = "";
            Vitals = "";
            CC = "";
            HPI = "";
            CurrentMeds = "";
            ProcedureNote = "";
            PreventiveMed = "";
            CurrentPrnMeds = "";
            ProblemList = "";
            ROS = "";
            PMHx = "";
            SocHx = "";
            GeneralHx = "";
            Exam = "";
            Treatment = "";
            MedsStarted = "";
            ImagesOrdered = "";
            LabsOrdered = "";
            Assessments = "";
            FollowUp = "";
            SurgHx = "";
            FamHx = "";
            VisitCodes = "";
            ICD10s.Clear();
        }

        public ObservableCollection<SqlICD10SegmentVM> ICD10Segments
        {
            get
            {
                UpdateICD10Segments();
                return document.ICD10Segments; ////see code above...
            }
            set
            {
                document.ICD10Segments = value;
            }
        }

        private void UpdateICD10Segments()
        {
            ObservableCollection<SqlICD10SegmentVM> tmpICD10Segments = new ObservableCollection<SqlICD10SegmentVM>();
            #region Yikes! ugly, only open if you have to
            //get icd10 segments
            if (!GeneralCheckPointsOnly)
            foreach (string strICD10 in ICD10s)
            {
                string strAlphaCode = strICD10.Substring(0, 1);
                string str = "";
                foreach (char ch in strICD10)
                {
                    if (Char.IsDigit(ch)) str += ch;
                    if (ch == '.') str += ch; //preserve decimal
                    if (Char.ToLower(ch) == 'x') break; //if placeholder character, then stop.
                }
                double icd10numeric = double.Parse(str);
                foreach (SqlICD10SegmentVM ns in SqlICD10SegmentVM.NoteICD10Segments)
                {
                    if (strAlphaCode == ns.SqlICD10Segment.icd10Chapter)
                    {
                        if (icd10numeric >= ns.SqlICD10Segment.icd10CategoryStart && icd10numeric <= ns.SqlICD10Segment.icd10CategoryEnd)
                        {
                            ns.IncludeSegment = true;
                            tmpICD10Segments.Add(ns);
                        }
                    }
                }
            }

            //add all general sections
            foreach (SqlICD10SegmentVM ns in SqlICD10SegmentVM.NoteICD10Segments)
            {
                if (ns.SqlICD10Segment.icd10Chapter == "X")
                {
                    ns.IncludeSegment = true;
                    #region Assign Include segments
                    if (!HashTags.Contains("!HTNUrgency") && ns.SqlICD10Segment.ICD10SegmentID == 40) //if htnurgency is not present
                    {
                        ns.IncludeSegment = false;
                    }
                    if (ns.SqlICD10Segment.ICD10SegmentID == 72) //Adult rapid RR
                    {
                        if (patient.PtAgeYrs <= 17) ns.IncludeSegment = false; //do not include children in 72
                        if (!HashTags.Contains("!RRHigh")) ns.IncludeSegment = false; //do not include children in 72
                    }
                    if (ns.SqlICD10Segment.ICD10SegmentID == 91) //Peds rapid RR
                    {
                        if (patient.PtAgeYrs >= 18) ns.IncludeSegment = false;
                        if (!HashTags.Contains("!RRHigh")) ns.IncludeSegment = false; //do not include children in 72
                    }
                    if (!HashTags.Contains("@Elderly") && ns.SqlICD10Segment.ICD10SegmentID == 75) //if htnurgency is not present
                    {
                        ns.IncludeSegment = false;
                    }
                    if (!HashTags.Contains("@Child") && ns.SqlICD10Segment.ICD10SegmentID == 80) //if htnurgency is not present
                    {
                        ns.IncludeSegment = false;
                    }
                    if (!HashTags.Contains("@Infant") && ns.SqlICD10Segment.ICD10SegmentID == 76) //if htnurgency is not present
                    {
                        ns.IncludeSegment = false;
                    }
                    if (!HashTags.Contains("@pregnantcapable") && ns.SqlICD10Segment.ICD10SegmentID == 82) //if htnurgency is not present
                    {
                        ns.IncludeSegment = false;
                    }
                    if (!HashTags.Contains("!HighFever") && ns.SqlICD10Segment.ICD10SegmentID == 73) //if htnurgency is not present
                    {
                        ns.IncludeSegment = false;
                    }
                    if (!HashTags.Contains("!Tachycardia") && ns.SqlICD10Segment.ICD10SegmentID == 74) //if htnurgency is not present
                    {
                        ns.IncludeSegment = false;
                    }
                    if (ns.SqlICD10Segment.ICD10SegmentID == 92) //todo: find better way to see if procedure note included.
                    {
                        if (ProcedureNote == null)
                        {
                            ns.IncludeSegment = false;
                        }
                        else
                        {
                            if (ProcedureNote.Length < 100) ns.IncludeSegment = false;
                        }
                    }
                    if (ns.SqlICD10Segment.ICD10SegmentID == 90) //never check ED Transfer
                    {
                        ns.IncludeSegment = false;
                    }

                    #endregion

                    tmpICD10Segments.Add(ns);
                }
            }
            #endregion
            ICD10Segments = tmpICD10Segments;
            //OnPropertyChanged("ICD10Segments");

        }

        private string[] noteSection = {"Chief Complaint(s):", "HPI:", "Current Medication:", "Medical History:", "Allergies/Intolerance:", "Surgical History:", "Hospitalization:",
            "Family History:", "Social History:", "ROS:", "Vitals:", "Examination:", "Assessment:","Treatment:","Procedures:","Immunizations:","Therapeutic Injections:","Diagnostic Imaging:",
            "Lab Reports:","Next Appointment:","Visit Code:","Procedure Codes:","Images:", "Objective:","Procedure Orders:","Preventive Medicine:","Billing Information:","Plan:"};

        #region Process EcwContent
        public void processLockedt(HtmlDocument HDoc)
        {

            Clear();
            if (HDoc.Body.InnerHtml.StartsWith("<LINK"))
            {
                processUnlocked(HDoc);
                return;
            }
            #region Process locked document magic
            var watch = System.Diagnostics.Stopwatch.StartNew();

            HtmlElementCollection AllTRItems = HDoc.Body.GetElementsByTagName("TR"); //5 ms
            HtmlElementCollection AllTHEADItems = HDoc.Body.GetElementsByTagName("THEAD"); //5 ms
            NoteHTML = HDoc.Body.InnerHtml; //3 ms

            string strCurrentHeading = "";
            foreach (HtmlElement TempEl in AllTHEADItems) //1 item, 1.9 seconds!
            {
                if (TempEl.TagName == "THEAD")
                {
                    string strInnerText = TempEl.InnerText;
                    if (strInnerText.Contains("DOS:"))
                    {
                        strInnerText = strInnerText.Replace("DOS:", "|");
                        VisitDate = DateTime.Parse(strInnerText.Split('|')[1]);
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
                        //<TD class=PageHeader align=right>Progress Note:&nbsp; Patrick F Castellano, PA-C</TD>
                        if (strClass == "PageHeader")
                        {
                            string strInnerText = TempEl.InnerText;
                            if (strInnerText.StartsWith("Progress Note:"))
                            {
                                string strDocname = strInnerText.Split(':')[1].Trim();
                                strDocname = strDocname.Replace("    ", "|");
                                Provider = strDocname.Split('|')[0];
                            }
                            continue;

                        }
                        if (strClass == "TableFooter")
                        {
                            string strInnerText = TempEl.InnerText;
                            if (strInnerText.Contains("Progress Note:"))
                            {
                                string strDocname = strInnerText.Split(':')[1].Trim();
                                strDocname = strDocname.Replace("    ", "|");
                                Provider = strDocname.Split('|')[0];

                            }
                            continue;
                        }
                        if (strClass == "PtHeading")
                        {
                            patient.PtName = TempEl.InnerText.Replace("\n", "").Replace("\r", "");  //set first name
                        }
                        if (strClass == "PtData") //field has note informaition
                        {
                            string strInnerText = TempEl.InnerText;
                            if (strInnerText == null)
                            {
                                continue;
                            }
                            if (strInnerText.Contains("Account Number:")) patient.PtID = strInnerText.Split(':')[1].Trim();
                            if (strInnerText.Contains("Appointment Facility:")) document.Facility = strInnerText.Split(':')[1].Trim();
                            if (strInnerText.Contains("DOB:"))
                            {
                                //   PtAge = strInnerText.Split(':')[0].TrimEnd("DOB");
                                patient.DOB = DateTime.Parse(strInnerText.Split(':')[1].Trim());
                                if (strInnerText.Contains("Female"))
                                {
                                    patient.PtSex = "F";
                                }
                                else
                                {
                                    patient.PtSex = "M";
                                }
                            }
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
                                CC = strInnerText.Substring(3);
                            }
                            if (strCurrentHeading == "History of Present Illness")
                            {
                                HPI = strInnerText;
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

                                CurrentMeds = strOut;
                            }
                            if (strCurrentHeading == "Active Problem List")
                            {
                                string strTextToParse = strInnerText;
                                if (strInnerText == null) continue;
                                var result = strTextToParse.Split(new[] { '\r', '\n' });
                                List<string> lProblemList = new List<string>();
                                foreach (string str in result)
                                {
                                    if (str.Trim() == "") continue;
                                    if (str.StartsWith("Onset")) continue;
                                    lProblemList.Add(str);
                                }
                                string strOut = "";
                                foreach (string strProblem in lProblemList)
                                {
                                    strOut += strProblem + "\n";
                                }
                                ProblemList = strOut;
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
                                PMHx = strOut;
                            }
                            if (strCurrentHeading == "Social History")
                            {
                                SocHx = strInnerText;
                            }
                            if (strCurrentHeading == "Allergies")
                            {
                                Allergies = strInnerText;
                            }
                            if (strCurrentHeading == "Review of Systems")
                            {
                                ROS = strInnerText;
                            }

                            if (strCurrentHeading == "Vital Signs")
                            {
                                var result = strInnerText.Split(new[] { '\r', '\n' });
                                Vitals = result[0];
                            }
                            if (strCurrentHeading == "Examination")
                            {
                                Exam = strInnerText;
                            }
                            if (strCurrentHeading == "Visit Code")
                            {
                                VisitCodes = strInnerText;
                            }
                            if (strCurrentHeading == "Preventive Medicine")
                            {
                                PreventiveMed = strInnerText;
                            }

                            if (strCurrentHeading == "Follow Up")
                            {
                                FollowUp = strInnerText;
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
                                    strOut += strProblem + Environment.NewLine;
                                }
                                Assessments = strOut;
                            }

                            if (strCurrentHeading.Trim() == "Procedures")
                            {
                                ProcedureNote += strInnerText;
                            }

                            if (strCurrentHeading == "Treatment")
                            {
                                var result = strInnerText.Split(new[] { '\r', '\n' });
                                List<string> medsStarted = new List<string>();
                                List<string> lLabsOrdered = new List<string>();
                                string strMedsSarted = "";
                                string strLabsOrdered = "";
                                foreach (string str in result)
                                {
                                    if (str.Trim().StartsWith("LAB:"))
                                    {
                                        lLabsOrdered.Add(str);
                                        strLabsOrdered += str + "\n";
                                    }
                                    if (str.StartsWith("Start "))
                                    {
                                        medsStarted.Add(str);
                                        strMedsSarted += str + "\n";
                                    }
                                }
                                Treatment = strInnerText;
                                MedsStarted = strMedsSarted;
                                LabsOrdered = strLabsOrdered;
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
                                ImagesOrdered = strImagesOrdered;
                            }
                            if (strCurrentHeading.Trim() == "Labs")
                            {
                                LabsOrdered += strInnerText;
                            }

                        }


                        //Console.WriteLine($"{strClass} - {TempEl.InnerText}");
                    }

                }





                //Console.WriteLine($"Run time: {watch.ElapsedMilliseconds}");

            }
            ICD10s = new ObservableCollection<string>();
            try
            {
                if (Assessments != null)
                    foreach (var tmpAssessment in Assessments.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (tmpAssessment.Contains(" - "))
                        {
                            string strClean = tmpAssessment.TrimEnd(" (Primary)").Trim();
                            strClean = strClean.Replace(" - ", "|");
                            string strCode = strClean.Split('|')[1].Trim();
                            ICD10s.Add(strCode);
                        }
                    }
            }
            catch (Exception)
            {
                throw;
            }
            #endregion
            SetUpNote();
            parseVitalsString(Vitals);
        }
        private void parseVitalsString(string strVitals)
        {
            patient.VitalsRR = 0;
            patient.VitalsHR = 0;
            patient.VitalsSystolic = 0;
            patient.VitalsDiastolic = 0;
            patient.VitalsTemp = 0;
            patient.VitalsO2 = 0;
            patient.VitalsWt = 0;
            patient.VitalsBMI = 0;

            if (strVitals == null) return;

            foreach (string strPartVital in strVitals.Split(','))
            {
                if (strPartVital.Contains("BP"))
                {
                    try
                    {
                        patient.VitalsSystolic = int.Parse(strPartVital.Trim().Split(' ')[1].Split('/')[0]);
                        patient.VitalsDiastolic = int.Parse(strPartVital.Trim().Split(' ')[1].Split('/')[1]);
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
                        patient.VitalsHR = int.Parse(strPartVital.Trim().Split(' ')[1].Trim());
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
                        patient.VitalsRR = int.Parse(strPartVital.Trim().Split(' ')[1].Trim());
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
                        patient.VitalsWt = double.Parse(strPartVital.Trim().Split(' ')[1].Trim());
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
                        patient.VitalsBMI = double.Parse(strPartVital.Trim().Split(' ')[1].Trim());
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
                        patient.VitalsO2 = int.Parse(strPartVital.Trim().Split('%')[1].Trim());
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
                        patient.VitalsTemp = double.Parse(strPartVital.Trim().Split(' ')[1].Trim());
                    }
                    catch
                    {
                        Console.WriteLine("Error obtainin temp.");
                    }

                }

            }


            //parse icd10
            ICD10s = new ObservableCollection<string>();
            foreach (var tmpAssessment in Assessments.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (tmpAssessment.Contains(" - "))
                {
                    string strClean = tmpAssessment.TrimEnd(" (Primary) ").Trim();
                    strClean = strClean.Replace(" - ", "|");
                    string strCode = strClean.Split('|')[1].Trim();
                    ICD10s.Add(strCode);
                }
            }

            SetUpNote();


        }

        public void processUnlocked(HtmlDocument HDoc)
        {
            string strNote = HDoc.Body.InnerText;
            #region Process unlocked magic
            if (strNote == null) return;
            string strCommand = "";
            string strMedType = "";
            Clear();
            foreach (var myString in strNote.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (myString.StartsWith("Patient:") && myString.Contains("DOB:"))
                {
                    string strPtInfo = myString.Replace("Patient:", "");
                    strPtInfo = strPtInfo.Replace("DOB:", "|");
                    strPtInfo = strPtInfo.Replace("Age:", "|");
                    strPtInfo = strPtInfo.Replace("Sex:", "|");
                    patient.PtName = strPtInfo.Split('|')[0].Trim();
                    patient.PtSex = strPtInfo.Split('|')[3].Trim();
                    string strDOB = strPtInfo.Split('|')[1];
                    patient.DOB = DateTime.Parse(strDOB);
                }


                if (myString.StartsWith("Encounter Date:") && myString.Contains("Provider:"))
                {
                    string strPtInfo = myString.Replace("Encounter Date:", "");
                    strPtInfo = strPtInfo.Replace("Provider:", "|");
                    VisitDate = DateTime.Parse(strPtInfo.Split('|')[0].Trim());
                    Provider = strPtInfo.Split('|')[1].Trim();
                }

                if (myString.StartsWith("Appointment Facility:"))
                {
                    Facility = myString.Split(':')[1];
                }

                if (myString.StartsWith("Account Number:"))
                {
                    patient.PtID = myString.Split(':')[1];
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
                            CC += myString;
                            break;
                        case "HPI:":
                            if (myString.Trim() == "Respiratory Clinic") break;
                            if (myString.Trim() == "Note:") break;
                            HPI += myString + Environment.NewLine;
                            break;
                        case "Allergies/Intolerance:":
                            Allergies += myString + Environment.NewLine;
                            break;
                        case "Medical History:":
                            PMHx += myString + Environment.NewLine;
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
                                CurrentPrnMeds += myString + " (prn)" + Environment.NewLine;
                                break;
                            }

                            if (strMedType == "unknown")
                            {
                                CurrentMeds += myString + " (unknown)" + Environment.NewLine;
                                break;
                            }

                            if (strMedType == "active")
                            {
                                CurrentMeds += myString + " (active)" + Environment.NewLine;
                                break;
                            }

                            if (strMedType == "Discontinued")
                            {
                                //CurrentPrnMeds += myString + " (Discontinued)" + Environment.NewLine;
                                break;
                            }
                            CurrentMeds += myString + " (??????)" + Environment.NewLine;
                            break;
                        case "Surgical History:":
                            SurgHx += myString + Environment.NewLine;
                            break;
                        case "Family History:":
                            FamHx += myString + Environment.NewLine;
                            break;
                        case "Social History:":
                            SocHx += myString + Environment.NewLine;
                            break;
                        case "ROS:":
                            ROS += myString + Environment.NewLine;
                            break;
                        case "Vitals:":
                            Vitals += myString + Environment.NewLine;
                            break;
                        case "Examination:":
                            Exam += myString + Environment.NewLine;
                            break;
                        case "Assessment:":
                            Assessments += myString + Environment.NewLine;
                            break;
                        case "Preventive Medicine:":
                            PreventiveMed += myString + Environment.NewLine;
                            break;
                        case "Treatment:":
                            if (myString.StartsWith("         Start")) //may not always work, keep an eye on this.
                            {
                                MedsStarted += myString + Environment.NewLine;
                            }
                            else
                            {
                                Treatment += myString + Environment.NewLine;
                            }
                            break;
                        case "Immunizations:":
                            Treatment += myString + Environment.NewLine;
                            break;
                        case "Therapeutic Injections:":
                            Treatment += myString + Environment.NewLine;
                            break;
                        case "Procedures:":
                            ProcedureNote += myString + Environment.NewLine;
                            break;
                        case "Diagnostic Imaging:":
                            ImagesOrdered += myString + Environment.NewLine;
                            break;
                        case "Lab Reports:":
                            LabsOrdered += myString + Environment.NewLine;
                            break;
                        case "Next Appointment:":
                            FollowUp += myString + Environment.NewLine;
                            break;
                        case "Visit Code:":
                            VisitCodes += myString + Environment.NewLine;
                            break;
                        default:
                            break;
                    }
                }

            }

            //parse Vitals
            //       BP 138/74, HR 144, Temp 97.9, O2 sat % 99.
            string strVitals = "";
            if (Vitals != "")
            {
                strVitals = Vitals.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)[0];
            }
            strVitals = strVitals.Trim().TrimEnd('.');

            parseVitalsString(strVitals);
            #endregion
            SetUpNote();
        }
        #endregion

        private ICommand mShowReport;
        public ICommand ShowReport
        {
            #region Command Def
            get
            {
                if (mShowReport == null)
                    mShowReport = new ShowReport();
                return mShowReport;
            }
            set
            {
                mShowReport = value;
            }
            #endregion
        }


        private ICommand mShowReportGen;
        public ICommand ShowReportGen
        {
            #region Command Def
            get
            {
                if (mShowReportGen == null)
                    mShowReportGen = new ShowReportGen();
                return mShowReportGen;
            }
            set
            {
                mShowReportGen = value;
            }
            #endregion
        }
    }

    class ShowReport : ICommand
    {
        #region ICommand Members  

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        #endregion

        public void Execute(object parameter)
        {
            VisitReportVM rvm = parameter as VisitReportVM;
            rvm.GeneralCheckPointsOnly = false;
            rvm.NewEcWDocument(); //reset document
            rvm.UpdateCPs(); //I don't think I need to update CPs if there are none.
            VisitReportV wp = new VisitReportV(rvm);
            wp.ShowDialog();
        }
    }

    class ShowReportGen : ICommand
    {
        #region ICommand Members  

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        #endregion

        public void Execute(object parameter)
        {
            VisitReportVM rvm = parameter as VisitReportVM;
            rvm.GeneralCheckPointsOnly = true;
            rvm.NewEcWDocument(); //reset document
            rvm.UpdateCPs(); //I don't think I need to update CPs if there are none.
            VisitReportV wp = new VisitReportV(rvm);
            wp.ShowDialog();
        }
    }

}
