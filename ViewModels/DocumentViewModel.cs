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
    public class DocumentViewModel : INotifyPropertyChanged
    {
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private Document document;
        private Patient patient;
        private SqlProvider sqlProvider;
        private PatientViewModel patientViewModel;

        public DocumentViewModel(SqlProvider prov, PatientViewModel pvm)
        {
            sqlProvider = prov;
            patientViewModel = pvm;
            patient = pvm.SamplePatient;
            document = SampleDocument; //New Document() called under this.
            SetUpNote(); //todo: might be better way of implementing this.
        }

        public  PatientViewModel PatientViewModel
        {
            get
            {
                return patientViewModel;
            }
        }

        public Document Document
        {
            get {
                return document;
            }
        }

        public Patient Patient
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
            return  sqlProvider;
            }
            }

        /// <summary>
        /// return a sample document for testing
        /// </summary>
        public Document SampleDocument
        {
            get
            {
                document = new Document();
                document.Provider = "Devin Hansen";
                document.ProviderID = 1;
                document.VisitDate = new DateTime(2021, 7, 14);
                document.CC = "Abdominal pain for 10 days";
                document.Facility = "Meridian UC";
                document.ICD10s.Add("R10.10");
                document.ICD10s.Add("I10");
                //DateTime.Now;

                document.HPI = "Mark is a 30yo male who presents today complaining of right lower quadrant abdominal pain that began two days ago and acutely worse today. " +
                    "He denies diarrhea or constipation.  He states he cannot tolerate a full meal due to the pain.  " +
                    "Mark has tried OTC medications.  He denies chest pain.  He denies blood in the vomit. Thus far he has tried ibuprofen and tylenol";

                document.HPI = "Dalton Anthony Ware is a 26 y.o. male who presents with a chief complaint of rash, blisters. Patient presents to the ER today with rash, lesions in the mouth, nose and eye on the left.He indicates that about 2 - 3 weeks ago he felt like he got a cold / sinus infection.He was taking multiple medications for this including NSAID, mucinex, spray in the nose to help with this.Didn't have a cough, didn't have a fever.  Had some chest discomfort that he felt was due to some chest congestion that seems to have resolved.Never had any n / v / d.He has not had any pain with urination, but indicates that his urine smells funny like he has had asparagus, but he has not. On Tuesday felt like he was getting a canker sore on the left side of the lip and by the next day was getting larger.He now has very large sores on the left, bilateral cheeks and under the tongue, also feels like something in the throat as well.He has some pain and irritation up in the nose on the left side, feels some crusting there.He has had purulent drainage from the left eye as well over the last couple of days and some generalized irritation.He has not been able to eat / drink much over the last couple of days due to the oral discomfort.He did use a new toothpaste once prior to this all starting, no longer using, this was thought to be part of the cause.  He denies any current n / v / d.He was told that might be SJS and he then looked at the scrotum today and feels like it might be more red than normal, but again, no pain with urination.No fever or chills.  He was never tested for COVID during the URI type illness that he had prior.He does currently complain of headache and pressure behind the eyes as well. No oral sex, patient states ever.Neither have ever had STI otherwise. Patients partner is 7 months pregnant at this point as well.He has never had acold sore, but does get canker sores occasionally.";

                document.CurrentMeds = "ibuprofen, Tylenol, prednisone";
                document.Exam = "AO, NAD PERRL\nNormal OP\nCTA bilat\nRRR no murmurs\nS NTND NABS, no guarding, no rebound\nNo edema";
                return document;
            }
        }

        /// <summary>
        /// Helper method for SetUpNote
        /// </summary>
        /// <param name="strHashTag"></param>
        public void AddHashTag(string strHashTag)
        {
            Document.HashTags += strHashTag + ", ";
        }

        /// <summary>
        /// After the document has been copied from ECW to AINoteReview, process the information
        /// </summary>
        public void SetUpNote()
        {
            //add hashtags here. #Hash
            Document.HashTags = "";
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
            if (patient.GetAgeInDays() < 183) AddHashTag("@Age<6mo");
            if (patient.isRRHigh) AddHashTag("!RRHigh");             //72	X	2	2	Rapid Respiratory Rate
            if (patient.isTempHigh) AddHashTag("!HighFever");             //73	X	3	3	High Fever
            if (patient.isHRHigh) AddHashTag("!Tachycardia");             //74	X	4	4	Tachycardia
            if (patient.GetAgeInDays() <= 90 && patient.VitalsTemp > 100.4)
            {
                //MessageBoxResult mr = MessageBox.Show($"This patient is {patient.GetAgeInDays()} days old and has a fever of {patient.VitalsTemp}.  Was the patient sent to an ED or appropriate workup performed?", "Infant Fever", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                //if (mr == MessageBoxResult.No) Document.HashTags += "#NeonteNotSentToED";
            }
            Document.HashTags = Document.HashTags.TrimEnd().TrimEnd(',');
            Document.NoteSectionText[0] = $"{patient.PtAgeYrs} Sex{patient.PtSex}"; //Demographics 
            Document.NoteSectionText[1] = Document.HPI + Document.ROS; //HPI
            Document.NoteSectionText[2] = Document.CurrentMeds + Document.CurrentPrnMeds; //CurrentMeds
            Document.NoteSectionText[3] = Document.ProblemList; //Active Problem List
            Document.NoteSectionText[4] = Document.PMHx; //Past Medical History
            Document.NoteSectionText[5] = Document.SocHx; //Social History
            Document.NoteSectionText[6] = Document.Allergies; //Allergies
            Document.NoteSectionText[7] = Document.Vitals; //Vital Signs
            Document.NoteSectionText[8] = Document.Exam; //Examination
            Document.NoteSectionText[9] = Document.Assessments; //Assessments
            Document.NoteSectionText[10] = Document.Treatment; //Treatment
            Document.NoteSectionText[11] = Document.LabsOrdered; //Labs
            Document.NoteSectionText[12] = Document.ImagesOrdered; //Imaging
            Document.NoteSectionText[13] = Document.ROS; //Review of Systems
            Document.NoteSectionText[14] = Document.Assessments; //Assessments
            Document.NoteSectionText[15] = Document.MedsStarted; //Prescribed Medications
            Document.NoteSectionText[16] = Document.FamHx;
            Document.NoteSectionText[17] = Document.SurgHx;
            Document.NoteSectionText[18] = Document.HashTags;
            Document.NoteSectionText[19] = Document.CC;
            Document.NoteSectionText[20] = Document.ProcedureNote;
            Document.NoteSectionText[21] = Document.PreventiveMed;
        }

        /// <summary>
        /// Clear Note Values
        /// </summary>
        public void Clear()
        {
            //demographics
           Document.Facility = "";
           Document.VisitDate = new DateTime(2020, 1, 1);
           Document.Provider = "";
           Document.ReasonForAppt = "";
           Document.Allergies = "";
           Document.NoteHTML = "";
           Document.Vitals = "";
           Document.CC = "";
           Document.HPI = "";
           Document.CurrentMeds = "";
           Document.ProcedureNote = "";
           Document.PreventiveMed = "";
           Document.CurrentPrnMeds = "";
           Document.ProblemList = "";
           Document.ROS = "";
           Document.PMHx = "";
           Document.SocHx = "";
           Document.GeneralHx = "";
           Document.Exam = "";
           Document.Treatment = "";
           Document.MedsStarted = "";
           Document.ImagesOrdered = "";
           Document.LabsOrdered = "";
           Document.Assessments = "";
           Document.FollowUp = "";
           Document.SurgHx = "";
           Document.FamHx = "";
           Document.ICD10s.Clear();
        }

        private ObservableCollection<SqlICD10SegmentViewModel> iCD10Segments;
        public ObservableCollection<SqlICD10SegmentViewModel> ICD10Segments
        {
            get 
            {
                return iCD10Segments;
            }
            set
            {
                iCD10Segments = value;
            }
        }

        /// <summary>
        /// Load all pertinent and ICD10 related segments
        /// </summary>
        /// <param name="GeneralCheckPointsOnly"></param>
        /// <returns></returns>
        public ObservableCollection<SqlICD10SegmentViewModel> GetSegments(bool GeneralCheckPointsOnly = false)
        {
            //get icd10 segments
            ObservableCollection<SqlICD10SegmentViewModel> tmpICD10Segments = new ObservableCollection<SqlICD10SegmentViewModel>();

            if (!GeneralCheckPointsOnly) //do not check the ICD10s for general check
                foreach (string strICD10 in Document.ICD10s)
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

                    foreach (SqlICD10SegmentViewModel ns in SqlICD10SegmentViewModel.NoteICD10Segments)
                    {
                        if (strAlphaCode == ns.SqlICD10Segment.icd10Chapter)
                        {
                            if (icd10numeric >= ns.SqlICD10Segment.icd10CategoryStart && icd10numeric <= ns.SqlICD10Segment.icd10CategoryEnd)
                            {
                                if (!GeneralCheckPointsOnly) tmpICD10Segments.Add(ns);
                            }
                        }
                    }
                }

            //add all general sections
            foreach (SqlICD10SegmentViewModel ns in SqlICD10SegmentViewModel.NoteICD10Segments)
            {
                if (ns.SqlICD10Segment.icd10Chapter == "X")
                {
                    tmpICD10Segments.Add(ns);
                }
            }

            //if (IsHTNUrgency) tmpICD10Segments.Add(SqlLiteDataAccess.GetSegment(40)); //pull in HTNUrgencySegment
            //if (isRRHigh) tmpICD10Segments.Add(SqlLiteDataAccess.GetSegment(72)); //pull in HTNUrgencySegment
            //if (isTempHigh) tmpICD10Segments.Add(SqlLiteDataAccess.GetSegment(73)); //pull in HTNUrgencySegment
            //if (isHRHigh) tmpICD10Segments.Add(SqlLiteDataAccess.GetSegment(74)); //pull in HTNUrgencySegment

            //tmpICD10Segments.Add(SqlLiteDataAccess.GetSegment(36)); //add general segment that applies to all visits.
            ICD10Segments = tmpICD10Segments;
            return tmpICD10Segments;
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
            var watch = System.Diagnostics.Stopwatch.StartNew();

            HtmlElementCollection AllTRItems = HDoc.Body.GetElementsByTagName("TR"); //5 ms
            HtmlElementCollection AllTHEADItems = HDoc.Body.GetElementsByTagName("THEAD"); //5 ms
            document.NoteHTML = HDoc.Body.InnerHtml; //3 ms

            string strCurrentHeading = "";
            foreach (HtmlElement TempEl in AllTHEADItems) //1 item, 1.9 seconds!
            {
                if (TempEl.TagName == "THEAD")
                {
                    string strInnerText = TempEl.InnerText;
                    if (strInnerText.Contains("DOS:"))
                    {
                        strInnerText = strInnerText.Replace("DOS:", "|");
                        document.VisitDate = DateTime.Parse(strInnerText.Split('|')[1]);
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
                                document.Provider = strDocname.Split('|')[0];
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
                                document.Provider = strDocname.Split('|')[0];

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
                                //   document.PtAge = strInnerText.Split(':')[0].TrimEnd("DOB");
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
                                document.CC = strInnerText.Substring(3);
                            }
                            if (strCurrentHeading == "History of Present Illness")
                            {
                                document.HPI = strInnerText;
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

                                document.CurrentMeds = strOut;
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
                                document.ProblemList = strOut;
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
                                document.PMHx = strOut;
                            }
                            if (strCurrentHeading == "Social History")
                            {
                                document.SocHx = strInnerText;
                            }
                            if (strCurrentHeading == "Allergies")
                            {
                                document.Allergies = strInnerText;
                            }
                            if (strCurrentHeading == "Review of Systems")
                            {
                                document.ROS = strInnerText;
                            }

                            if (strCurrentHeading == "Vital Signs")
                            {
                                var result = strInnerText.Split(new[] { '\r', '\n' });
                                document.Vitals = result[0];
                            }
                            if (strCurrentHeading == "Examination")
                            {
                                document.Exam = strInnerText;
                            }
                            if (strCurrentHeading == "Visit Code")
                            {
                                document.VisitCodes = strInnerText;
                            }
                            if (strCurrentHeading == "Preventive Medicine")
                            {
                                document.PreventiveMed = strInnerText;
                            }

                            if (strCurrentHeading == "Follow Up")
                            {
                                document.FollowUp = strInnerText;
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
                                document.Assessments = strOut;
                            }

                            if (strCurrentHeading.Trim() == "Procedures")
                            {
                                document.ProcedureNote += strInnerText;
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
                                document.Treatment = strInnerText;
                                document.MedsStarted = strMedsSarted;
                                document.LabsOrdered = strLabsOrdered;
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
                                document.ImagesOrdered = strImagesOrdered;
                            }
                            if (strCurrentHeading.Trim() == "Labs")
                            {
                                document.LabsOrdered += strInnerText;
                            }

                        }


                        //Console.WriteLine($"{strClass} - {TempEl.InnerText}");
                    }

                }





                //Console.WriteLine($"Run time: {watch.ElapsedMilliseconds}");

            }
            document.ICD10s = new ObservableCollection<string>();
            try
            {
                if (document.Assessments != null)
                    foreach (var tmpAssessment in document.Assessments.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (tmpAssessment.Contains(" - "))
                        {
                            string strClean = tmpAssessment.TrimEnd(" (Primary)").Trim();
                            strClean = strClean.Replace(" - ", "|");
                            string strCode = strClean.Split('|')[1].Trim();
                            document.ICD10s.Add(strCode);
                        }
                    }
            }
            catch (Exception)
            {
                throw;
            }
            SetUpNote();
            parseVitalsString(document.Vitals);
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
            document.ICD10s = new ObservableCollection<string>();
            foreach (var tmpAssessment in document.Assessments.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (tmpAssessment.Contains(" - "))
                {
                    string strClean = tmpAssessment.TrimEnd(" (Primary) ").Trim();
                    strClean = strClean.Replace(" - ", "|");
                    string strCode = strClean.Split('|')[1].Trim();
                    document.ICD10s.Add(strCode);
                }
            }

            SetUpNote();


        }

        public void processUnlocked(HtmlDocument HDoc)
        {
            //Console.WriteLine("Processing document");
            //this.DataContext = null;
            string strNote = HDoc.Body.InnerText;
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
                    document.VisitDate = DateTime.Parse(strPtInfo.Split('|')[0].Trim());
                    document.Provider = strPtInfo.Split('|')[1].Trim();
                }

                if (myString.StartsWith("Appointment Facility:"))
                {
                    document.Facility = myString.Split(':')[1];
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
                            document.CC += myString;
                            break;
                        case "HPI:":
                            if (myString.Trim() == "Respiratory Clinic") break;
                            if (myString.Trim() == "Note:") break;
                            document.HPI += myString + Environment.NewLine;
                            break;
                        case "Allergies/Intolerance:":
                            document.Allergies += myString + Environment.NewLine;
                            break;
                        case "Medical History:":
                            document.PMHx += myString + Environment.NewLine;
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
                                document.CurrentPrnMeds += myString + " (prn)" + Environment.NewLine;
                                break;
                            }

                            if (strMedType == "unknown")
                            {
                                document.CurrentMeds += myString + " (unknown)" + Environment.NewLine;
                                break;
                            }

                            if (strMedType == "active")
                            {
                                document.CurrentMeds += myString + " (active)" + Environment.NewLine;
                                break;
                            }

                            if (strMedType == "Discontinued")
                            {
                                //document.CurrentPrnMeds += myString + " (Discontinued)" + Environment.NewLine;
                                break;
                            }
                            document.CurrentMeds += myString + " (??????)" + Environment.NewLine;
                            break;
                        case "Surgical History:":
                            document.SurgHx += myString + Environment.NewLine;
                            break;
                        case "Family History:":
                            document.FamHx += myString + Environment.NewLine;
                            break;
                        case "Social History:":
                            document.SocHx += myString + Environment.NewLine;
                            break;
                        case "ROS:":
                            document.ROS += myString + Environment.NewLine;
                            break;
                        case "Vitals:":
                            document.Vitals += myString + Environment.NewLine;
                            break;
                        case "Examination:":
                            document.Exam += myString + Environment.NewLine;
                            break;
                        case "Assessment:":
                            document.Assessments += myString + Environment.NewLine;
                            break;
                        case "Preventive Medicine:":
                            document.PreventiveMed += myString + Environment.NewLine;
                            break;
                        case "Treatment:":
                            if (myString.StartsWith("         Start")) //may not always work, keep an eye on this.
                            {
                                document.MedsStarted += myString + Environment.NewLine;
                            }
                            else
                            {
                                document.Treatment += myString + Environment.NewLine;
                            }
                            break;
                        case "Immunizations:":
                            document.Treatment += myString + Environment.NewLine;
                            break;
                        case "Therapeutic Injections:":
                            document.Treatment += myString + Environment.NewLine;
                            break;
                        case "Procedures:":
                            document.ProcedureNote += myString + Environment.NewLine;
                            break;
                        case "Diagnostic Imaging:":
                            document.ImagesOrdered += myString + Environment.NewLine;
                            break;
                        case "Lab Reports:":
                            document.LabsOrdered += myString + Environment.NewLine;
                            break;
                        case "Next Appointment:":
                            document.FollowUp += myString + Environment.NewLine;
                            break;
                        case "Visit Code:":
                            document.VisitCodes += myString + Environment.NewLine;
                            break;
                        default:
                            break;
                    }
                }

            }

            //parse Vitals
            //       BP 138/74, HR 144, Temp 97.9, O2 sat % 99.
            string strVitals = "";
            if (document.Vitals != "")
            {
                strVitals = document.Vitals.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)[0];
            }
            strVitals = strVitals.Trim().TrimEnd('.');

            parseVitalsString(strVitals);

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

        public void Execute(object parameter)
        {
            ReportViewModel rvm = parameter as ReportViewModel;
            WinReport wp = new WinReport(rvm);
            wp.ShowDialog();
        }
        #endregion
    }
        
}
