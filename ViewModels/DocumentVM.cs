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
        #region INotify
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        //ViewModels declared locally for convenience
        private MasterReviewSummaryVM masterReviewVM;

        private PatientVM patientVM; //may be from mastereviewsummary, or created from scratch for notehunter

        //Models
        private DocumentM documentM;


        /// <summary>
        /// Constructor used in MasterReviewSummary, get masterreview, provider, patient, and create a document based on SampleDocument
        /// </summary>
        /// <param name="mrs"></param>
        public DocumentVM(MasterReviewSummaryVM mrs)
        {
            masterReviewVM = mrs;
            patientVM = mrs.Patient;
            documentM = SampleDocument; //New DocumentM() called under this.
        }

        /// <summary>
        /// Constructor used in notehunter.
        /// </summary>
        /// <param name="doc"></param>
        public DocumentVM(HtmlDocument doc) //used by document hunter
        {
            documentM = new DocumentM();
            patientVM = new PatientVM();
            NoteHTML = doc;
        }

        private List<NoteSectionM> noteSections;
        /// <summary>
        /// A list of notesections for use in combobox to select notesection, also contains NoteSectionContent (from the note) for tagregex searches
        /// </summary>
        public List<NoteSectionM> NoteSections
        {
            get
            {
                if (noteSections == null) //load only once
                {
                    using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                    {
                        string strSql = "Select * from NoteSections;";
                        noteSections = cnn.Query<NoteSectionM>(strSql).ToList();
                    }
                    foreach (var tmps in noteSections)
                    {
                        tmps.InitiateSection(documentM, patientVM);
                    }

                }
                return noteSections;
            }
            set
            {
                if (value == null)
                {
                    foreach (var tmpSection in NoteSections)
                    {
                        tmpSection.NoteSectionContent = null;
                    }
                }
            }
        }

        /// <summary>
        /// Used to save note content as a hippa compliant document (anonymized)
        /// </summary>
        public void SaveNote()
        {
            if (NoteHTML == null)
                return;
            if (NoteHTML.Body == null)
                return;
            Provider.AddNote(this);
            //save encrypted note
            //using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteNotesLocation))
            //{
            //    NoteDataVM nvm = new NoteDataVM(this);
            //}

            int numsaved = Provider.CurrentNoteDataCount;

            if (numsaved == 10)
            {
                System.Windows.MessageBox.Show($"Congrats! You have saved 10 documents.");
            }
            else
            {
                System.Windows.MessageBox.Show($"You have saved {numsaved} documents.");
            }
        }

        /// <summary>
        /// The patient that is extracted from the document
        /// </summary>
        public PatientVM Patient
        {
            get
            {
                return patientVM;
            }
        }



        /// <summary>
        /// Not used, perhaps at some point?
        /// </summary>
        public bool IsLocked
        {
            get
            {
                //unlocked charts html body start with the <link
                return !NoteHTML.Body.InnerHtml.StartsWith("<LINK");
            }
        }


        /// <summary>
        /// return a sample document for testing
        /// </summary>
        public DocumentM SampleDocument
        {
            get
            {
                documentM = new DocumentM();
                //important: may not need to retrieve provider...
                SetProvider(2); //Provider ID comes from this.
                VisitDate = new DateTime(2022, 3, 14);
                CC = "Abdominal pain for 10 days";
                Facility = "Meridian UC";
                ICD10s.Add("R10.10");
                ICD10s.Add("I10");

                HPI = "Mark is a 30yo male who presents today complaining of right lower quadrant abdominal pain that began two days ago and acutely worse today. " +
                    "He denies diarrhea or constipation.  He states he cannot tolerate a full meal due to the pain.  " +
                    "Mark has tried OTC medications.  He denies chest pain.  He denies blood in the vomit. Thus far he has tried ibuprofen and tylenol";

                HPI = "Dalton Anthony Ware is a 26 y.o. male who presents with a chief complaint of rash, blisters. Patient presents to the ER today with rash, lesions in the mouth, nose and eye on the left.He indicates that about 2 - 3 weeks ago he felt like he got a cold / sinus infection.He was taking multiple medications for this including NSAID, mucinex, spray in the nose to help with this.Didn't have a cough, didn't have a fever.  Had some chest discomfort that he felt was due to some chest congestion that seems to have resolved.Never had any n / v / d.He has not had any pain with urination, but indicates that his urine smells funny like he has had asparagus, but he has not. On Tuesday felt like he was getting a canker sore on the left side of the lip and by the next day was getting larger.He now has very large sores on the left, bilateral cheeks and under the tongue, also feels like something in the throat as well.He has some pain and irritation up in the nose on the left side, feels some crusting there.He has had purulent drainage from the left eye as well over the last couple of days and some generalized irritation.He has not been able to eat / drink much over the last couple of days due to the oral discomfort.He did use a new toothpaste once prior to this all starting, no longer using, this was thought to be part of the cause.  He denies any current n / v / d.He was told that might be SJS and he then looked at the scrotum today and feels like it might be more red than normal, but again, no pain with urination.No fever or chills.  He was never tested for COVID during the URI type illness that he had prior.He does currently complain of headache and pressure behind the eyes as well. No oral sex, patient states ever.Neither have ever had STI otherwise. Patients partner is 7 months pregnant at this point as well.He has never had acold sore, but does get canker sores occasionally.";

                CurrentMeds = "ibuprofen, Tylenol, prednisone";
                Exam = "AO, NAD PERRL\nNormal OP\nCTA bilat\nRRR no murmurs\nS NTND NABS, no guarding, no rebound\nNo edema";
                NoteHTML = GetHtmlDocument("This is a test");
                VisitDate = new DateTime(2022, 10, 20);
                SetProvider(2);
                return documentM;
            }
        }

        /// <summary>
        /// Returns an HTML document from a string.
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private System.Windows.Forms.HtmlDocument GetHtmlDocument(string html)
        {
            WebBrowser browser = new WebBrowser();
            browser.ScriptErrorsSuppressed = true;
            browser.DocumentText = html;
            browser.Document.OpenNew(true);
            browser.Document.Write(html);
            browser.Refresh();
            return browser.Document;
        }


        /// <summary>
        /// The string that defines the text of an existing notesection
        /// </summary>
        private string[] noteSection = {"Chief Complaint(s):", "HPI:", "Current Medication:", "Medical History:", "Allergies/Intolerance:", "Surgical History:", "Hospitalization:",
            "Family History:", "Social History:", "ROS:", "Vitals:", "Examination:", "Assessment:","Treatment:","Procedures:","Immunizations:","Therapeutic Injections:","Diagnostic Imaging:",
            "Lab Reports:","Next Appointment:","Visit Code:","Procedure Codes:","Images:", "Objective:","Procedure Orders:","Preventive Medicine:","Billing Information:","Plan:"};

        #region Process EcwContent, the magic

        /// <summary>
        /// Parse the vitals string to vitals, this is for both locked and unlocked html charts
        /// </summary>
        /// <param name="strVitals"></param>
        private void parseVitalsString(string strVitals)
        {
            patientVM.VitalsRR = 0;
            patientVM.VitalsHR = 0;
            patientVM.VitalsSystolic = 0;
            patientVM.VitalsDiastolic = 0;
            patientVM.VitalsTemp = 0;
            patientVM.VitalsO2 = 0;
            patientVM.VitalsWt = 0;
            patientVM.VitalsBMI = 0;

            if (strVitals == null) return;
            foreach (string strPartVital in strVitals.Split(','))
            {
                if (strPartVital.Contains("BP"))
                {
                    try
                    {
                        patientVM.VitalsSystolic = int.Parse(strPartVital.Trim().Split(' ')[1].Split('/')[0]);
                        patientVM.VitalsDiastolic = int.Parse(strPartVital.Trim().Split(' ')[1].Split('/')[1]);
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
                        patientVM.VitalsHR = int.Parse(strPartVital.Trim().Split(' ')[1].Trim());
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
                        patientVM.VitalsRR = int.Parse(strPartVital.Trim().Split(' ')[1].Trim());
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
                        patientVM.VitalsWt = double.Parse(strPartVital.Trim().Split(' ')[1].Trim());
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
                        patientVM.VitalsBMI = double.Parse(strPartVital.Trim().Split(' ')[1].Trim());
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
                        string tmpStr = strPartVital.Replace(".", String.Empty);
                        patientVM.VitalsO2 = int.Parse(tmpStr.Trim().Split('%')[1].Trim());
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
                        patientVM.VitalsTemp = double.Parse(strPartVital.Trim().Split(' ')[1].Trim());
                    }
                    catch
                    {
                        Console.WriteLine("Error obtainin temp.");
                    }

                }

            }
        }

        #endregion


        #region mirror properties of DocumentM and notify on changed
        public string Facility
        {
            get
            {
                return documentM.Facility;
            }
            set
            {
                documentM.Facility = value;
                OnPropertyChanged();
            }
        }

        public void SetProvider(int iID)
        {
            Provider = ProviderVM.SqlGetProviderByID(iID);
        }

        private ProviderVM provider;
        public ProviderVM Provider
        {
            get
            {
                return provider;
            }
            set
            {
                provider = value;
                OnPropertyChanged();
            }
        }


        public DateTime VisitDate
        {
            get
            {
                return documentM.VisitDate;
            }
            set
            {
                documentM.VisitDate = value;
                OnPropertyChanged();
                OnPropertyChanged("DocumentMRS");
            }
        }

        private MasterReviewSummaryVM documentMRS;
        public MasterReviewSummaryVM DocumentMRS
        {
            get
            {
                //string sql = $"Select * from MasterReviewSummary where '{VisitDate.ToString("yyyy-MM-dd")}' Between StartDate and EndDate";
                //not sure this is correct, but it doesn't seem to be pulling up the latest.
                string sql = $"Select * from MasterReviewSummary";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    documentMRS = cnn.Query<MasterReviewSummaryVM>(sql).LastOrDefault();
                }
                return documentMRS;
            }
        }

        public string ReasonForAppt
        {
            get
            {
                return documentM.ReasonForAppt;
            }
            set
            {
                documentM.ReasonForAppt = value;
                OnPropertyChanged();
            }
        }
        private string cC;
        public string CC
        {
            get
            {
                return documentM.CC;
            }
            set
            {
                documentM.CC = value;
                OnPropertyChanged();
            }
        }
        public string HPI
        {
            get
            {
                return documentM.HPI;
            }
            set
            {
                documentM.HPI = value;
                OnPropertyChanged();
            }
        }
        public string Allergies
        {
            get
            {
                return documentM.Allergies;
            }
            set
            {
                documentM.Allergies = value;
                OnPropertyChanged();
            }
        }
        public string SurgHx
        {
            get
            {
                return documentM.SurgHx;
            }
            set
            {
                documentM.SurgHx = value;
                OnPropertyChanged();
            }
        }
        public string FamHx
        {
            get
            {
                return documentM.FamHx;
            }
            set
            {
                documentM.FamHx = value;
                OnPropertyChanged();
            }
        }
        public string CurrentMeds
        {
            get
            {
                return documentM.CurrentMeds;
            }
            set
            {
                documentM.CurrentMeds = value;
                OnPropertyChanged();
            }
        }
        public string CurrentPrnMeds
        {
            get
            {
                return documentM.CurrentPrnMeds;
            }
            set
            {
                documentM.CurrentPrnMeds = value;
                OnPropertyChanged();
            }
        }
        public string ProblemList
        {
            get
            {
                return documentM.ProblemList;
            }
            set
            {
                documentM.ProblemList = value;
                OnPropertyChanged();
            }
        }
        public string ROS
        {
            get
            {
                return documentM.ROS;
            }
            set
            {
                documentM.ROS = value;
                OnPropertyChanged();
            }
        }
        public string PMHx
        {
            get
            {
                return documentM.PMHx;
            }
            set
            {
                documentM.PMHx = value;
                OnPropertyChanged();
            }
        }
        public string SocHx
        {
            get
            {
                return documentM.SocHx;
            }
            set
            {
                documentM.SocHx = value;
                OnPropertyChanged();
            }
        }
        public string GeneralHx
        {
            get
            {
                return documentM.GeneralHx;
            }
            set
            {
                documentM.GeneralHx = value;
                OnPropertyChanged();
            }
        }
        public string Vitals
        {
            get
            {
                return documentM.Vitals;
            }
            set
            {
                documentM.Vitals = value;
                OnPropertyChanged();
                parseVitalsString(value); //Convert the Vitals to valeus

            }
        }
        public string Exam
        {
            get
            {
                return documentM.Exam;
            }
            set
            {
                documentM.Exam = value;
                OnPropertyChanged();
            }
        }
        public string Treatment
        {
            get
            {
                return documentM.Treatment;
            }
            set
            {
                documentM.Treatment = value;
                OnPropertyChanged();
            }
        }
        public string PreventiveMed
        {
            get
            {
                return documentM.PreventiveMed;
            }
            set
            {
                documentM.PreventiveMed = value;
                OnPropertyChanged();
            }
        }
        public string MedsStarted
        {
            get
            {
                return documentM.MedsStarted;
            }
            set
            {
                documentM.MedsStarted = value;
                OnPropertyChanged();
            }
        }
        public string ImagesOrdered
        {
            get
            {
                return documentM.ImagesOrdered;
            }
            set
            {
                documentM.ImagesOrdered = value;
                OnPropertyChanged();
            }
        }
        public string VisitCodes
        {
            get
            {
                return documentM.VisitCodes;
            }
            set
            {
                documentM.VisitCodes = value;
                OnPropertyChanged();
            }
        }
        public string LabsOrdered
        {
            get
            {
                return documentM.LabsOrdered;
            }
            set
            {
                documentM.LabsOrdered = value;
                OnPropertyChanged();
            }
        }
        public string Assessments
        {
            get
            {
                return documentM.Assessments;
            }
            set
            {
                documentM.Assessments = value;
                OnPropertyChanged();
            }
        }
        public string FollowUp
        {
            get
            {
                return documentM.FollowUp;
            }
            set
            {
                documentM.FollowUp = value;
                OnPropertyChanged();
            }
        }
        public string ProcedureNote
        {
            get
            {
                return documentM.ProcedureNote;
            }
            set
            {
                documentM.ProcedureNote = value;
                OnPropertyChanged();
            }
        }
        #endregion


        /// <summary>
        /// Helper method for Hashtags
        /// </summary>
        /// <param name="strHashTag"></param>
        public void AddHashTag(string strHashTag)
        {
            documentM.HashTags += strHashTag + ", ";
        }

        /// <summary>
        /// HashTags for the more complex match functions
        /// </summary>
        public string HashTags
        {
            get
            {
                if (documentM.HashTags == null)
                {
                    HashTags = "";
                    if (documentM.ProcedureNote.ToLower().Contains("laceration"))
                    {
                        AddHashTag("!Laceration");
                    }
                    if (patientVM.PtAgeYrs > 65)
                        AddHashTag("@Elderly");             //75	X	5	5	Elderly
                    if (patientVM.PtSex.StartsWith("F"))
                        AddHashTag("@Female");
                    if (patientVM.PtAgeYrs < 4)
                        AddHashTag("@Child");             //80	X	7	7	Children
                    if (patientVM.PtAgeYrs < 2)
                        AddHashTag("@Infant");             //76	X	6	6	Infant
                    if (patientVM.IsHTNUrgency)
                        AddHashTag("!HTNUrgency");             //40	X	1	1	Hypertensive Urgency
                    if (patientVM.isO2Abnormal)
                        AddHashTag("!Hypoxic");
                    if (patientVM.IsPregCapable)
                        AddHashTag("@pregnantcapable");            //82	X	9	9	Possible Pregnant State
                    if (patientVM.PtAgeYrs >= 13)
                        AddHashTag("@sexuallyActiveAge");
                    if (patientVM.PtAgeYrs >= 16)
                        AddHashTag("@DrinkingAge");
                    if (patientVM.PtAgeYrs >= 2)
                        AddHashTag("@SpeakingAge");
                    if (patientVM.PtAgeYrs < 1)
                        AddHashTag("@Age<1");
                    if (patientVM.PtAgeYrs < 2)
                        AddHashTag("@Age<2");
                    if (patientVM.PtAgeYrs < 4)
                        AddHashTag("@Age<4");
                    if (patientVM.IsBPLow)
                        AddHashTag("!hypotensive");
                    if (patientVM.GetAgeInDays < 183)
                        AddHashTag("@Age<6mo");
                    //if (patientVM.isRRHigh) AddHashTag("!RRHigh");             //72	X	2	2	Rapid Respiratory Rate
                    if (patientVM.isTempHigh)
                        AddHashTag("!HighFever");             //73	X	3	3	High Fever
                    if (patientVM.isFebrile)
                        AddHashTag("!fever");
                    //if (patientVM.isHRHigh) AddHashTag("!Tachycardia");             //74	X	4	4	Tachycardia
                    if (patientVM.GetAgeInDays <= 90 && patientVM.VitalsTemp > 100.4)
                    {
                        //MessageBoxResult mr = MessageBox.Show($"This patient is {patient.GetAgeInDays()} days old and has a fever of {patient.VitalsTemp}.  Was the patient sent to an ED or appropriate workup performed?", "Infant Fever", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        //if (mr == MessageBoxResult.No) DocumentM.HashTags += "#NeonteNotSentToED";
                    }
                    HashTags = HashTags.TrimEnd().TrimEnd(',');

                }
                return documentM.HashTags;
            }
            set
            {
                documentM.HashTags = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// ICD10s assigned by provider in eCwDocument.
        /// </summary>
        public ObservableCollection<string> ICD10s
        {
            get
            {
                if (documentM.ICD10s == null)
                {
                    documentM.ICD10s = new ObservableCollection<string>();
                    foreach (var tmpAssessment in Assessments.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (tmpAssessment.Contains(" - "))
                        {
                            string strClean = tmpAssessment.TrimEnd(" (Primary) ").Trim();
                            strClean = strClean.Replace(" - ", "|");
                            string strCode = strClean.Split('|')[1].Trim();
                            documentM.ICD10s.Add(strCode);
                        }
                    }
                }
                return documentM.ICD10s;
            }
            set
            {
                documentM.ICD10s = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Contains the raw HTML of the note, may be used to persist note at some point, If I can remove identifying information that is.
        /// </summary>
        private NoteDataVM noteData;
        public NoteDataVM Notedata
        {
            get
            {
                return noteData;
            }
            set
            {
                resetNoteData();

                noteData = value;

                //important this will be removed and the processing farmed out to each property
                processTextNote(noteData.NoteString);

                updateNoteData();
                OnPropertyChanged();
            }
        }

        public void processTextNote(string strNote)
        {
            #region Process locked document magic
            CC = Notedata.GetSegment("Reason for Appointment");
            //VisitDate = Notedata.VisitDate;
            SetProvider(Notedata.ProviderID);
            List<string> AssessmentList = Notedata.GetSegment("Assessments").Split('|').ToList();
            patientVM.PtName = "John";  //set first name
            /*
             * "Reason for Appointment", "History of Present Illness", "Current Medications", "Taking", "Not-Taking/PRN", "Discontinued",
            "Unknown", "Active Problem List", "Past Medical History", "Surgical History","Surgical History","Family History","Social History",
            "Hospitalization/Major Diagnostic Procedure","Review of Systems","Vital Signs","Examination","Assessments","Treatment","Follow Up",
            "Allergies","Medication Summary","Electronically signed*"
        patientVM.PtName = TempEl.InnerText.Replace("\n", "").Replace("\r", "");  //set first name
        patientVM.PtID = strInnerText.Split(':')[1].Trim();
        patientVM.DOB = DateTime.Parse(strInnerText.Split(':')[1].Trim());
        patientVM.PtSex = "F";
        CC = tmpStr;
        HPI = tmpStr;
        List<string> medsTaking = new List<string>();
        CurrentMeds = strOut;
        List<string> lProblemList = new List<string>();
        List<string> PMHxList = new List<string>();
        PMHx = strOut;
        SocHx = strInnerText;
        Allergies = strInnerText;
        ROS = strInnerText;
        Vitals = result[0];
        Exam = strInnerText;
        VisitCodes = strInnerText;
        PreventiveMed = strInnerText;
        FollowUp = strInnerText;
        AssessmentList.Add(str);
        Assessments = strOut;
        ProcedureNote += strInnerText;
        lLabsOrdered.Add(str);
        medsStarted.Add(str);
        strImagesOrdered += str.Trim() + "\n";
        ImagesOrdered = strImagesOrdered;
        Treatment += strInnerText;
        MedsStarted = strMedsSarted;
        LabsOrdered = strLabsOrdered;
        LabsOrdered += strInnerText;
        ICD10s = new ObservableCollection<string>();
        */
            #endregion
        }




        /// <summary>
        /// Contains the raw HTML of the note, may be used to persist note at some point, If I can remove identifying information that is.
        /// </summary>
        public HtmlDocument NoteHTML
        {
            get
            {
                return documentM.NoteHTML;
            }
            set
            {
                if (documentM.NoteHTML == value)
                    return; //do nothing if nothing has changed.

                resetNoteData();

                documentM.NoteHTML = value;

                try
                {
                    if (documentM.NoteHTML.Body.InnerHtml.Contains("pnDetails")) //unique text to identify unlocked chart
                    {
                        //processUnlocked(documentM.NoteHTML);
                        Console.WriteLine($"Processed unlocked chart for {patientVM.PtName}.");
                    }
                    else
                    {
                        //processLocked(documentM.NoteHTML);
                        masterReviewVM.AddLog("Processing locked chart");
                        Console.WriteLine($"Processed locked chart for {patientVM.PtName}.");
                    }

                }
                catch (Exception)
                {
                    System.Windows.MessageBox.Show("Note contains no data");
                }

                updateNoteData();
                OnPropertyChanged();
            }
        }

        public string NoteHTMLBody
        {
            get
            {
                try
                {
                    return documentM.NoteHTML.Body.OuterHtml;
                }
                catch (Exception)
                {

                    return null;
                }
            }
        }

        private void updateNoteData()
        {
            OnPropertyChanged("Facility");
            OnPropertyChanged("VisitDate");
            OnPropertyChanged("Provider");
            OnPropertyChanged("ProviderID");
            OnPropertyChanged("ReasonForAppt");
            OnPropertyChanged("Allergies");
            OnPropertyChanged("Vitals");
            OnPropertyChanged("CC");
            OnPropertyChanged("HPI");
            OnPropertyChanged("CurrentMeds");
            OnPropertyChanged("ProcedureNote");
            OnPropertyChanged("PreventiveMed");
            OnPropertyChanged("CurrentPrnMeds");
            OnPropertyChanged("ProblemList");
            OnPropertyChanged("ROS");
            OnPropertyChanged("PMHx");
            OnPropertyChanged("SocHx");
            OnPropertyChanged("GeneralHx");
            OnPropertyChanged("Exam");
            OnPropertyChanged("Treatment");
            OnPropertyChanged("MedsStarted");
            OnPropertyChanged("ImagesOrdered");
            OnPropertyChanged("LabsOrdered");
            OnPropertyChanged("Assessments");
            OnPropertyChanged("FollowUp");
            OnPropertyChanged("SurgHx");
            OnPropertyChanged("FamHx");
            OnPropertyChanged("VisitCodes");
            OnPropertyChanged("ICD10s");
            OnPropertyChanged("Vitals");
            OnPropertyChanged("NoteSections");
            OnPropertyChanged("HashTags");
            OnPropertyChanged("ICD10Segments");
        }

        private void resetNoteData()
        {
            //important, maybe just create a new documentM

            //When NoteHTML is set, reset everything.
            //demographics
            //I'm trying to decide if using the parent or childe Property here.
            documentM.Facility = "";
            documentM.VisitDate = new DateTime(2020, 1, 1);
            //SetProvider(2);
            documentM.ProviderID = 0;
            documentM.ReasonForAppt = "";
            documentM.Allergies = "";
            documentM.Vitals = "";
            documentM.CC = "";
            documentM.HPI = "";
            documentM.CurrentMeds = "";
            documentM.ProcedureNote = "";
            documentM.PreventiveMed = "";
            documentM.CurrentPrnMeds = "";
            documentM.ProblemList = "";
            documentM.ROS = "";
            documentM.PMHx = "";
            documentM.SocHx = "";
            documentM.GeneralHx = "";
            documentM.Exam = "";
            documentM.Treatment = "";
            documentM.MedsStarted = "";
            documentM.ImagesOrdered = "";
            documentM.LabsOrdered = "";
            documentM.Assessments = "";
            documentM.FollowUp = "";
            documentM.SurgHx = "";
            documentM.FamHx = "";
            documentM.VisitCodes = "";
            documentM.Vitals = "";
            documentM.ICD10s = null;
            documentM.HashTags = null;
            documentM.ICD10Segments = null;
            noteSections = null;
        }

        /// <summary>
        /// Holds the matched ICD10Segments of the eCwDocument assigned, and discovered through the document (ie pregnantcapable), ICD10s is a subset of this.
        /// </summary>
        public ObservableCollection<SqlICD10SegmentVM> ICD10Segments
        {
            get
            {
                if (documentM.ICD10Segments == null)
                    UpdateICD10Segments();
                foreach (var tmpSeg in documentM.ICD10Segments)
                {
                    tmpSeg.ParentDocument = this;
                    tmpSeg.ParentReport = this.masterReviewVM.VisitReport; //intentional error
                }
                return documentM.ICD10Segments; ////see code below...
            }
            set
            {
                documentM.ICD10Segments = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// llostod
        /// </summary>
        private void UpdateICD10Segments()
        {
            //see also the converter that utilizes portions of this function.
            Console.WriteLine("Getting ICD10 Segments");
            ObservableCollection<SqlICD10SegmentVM> tmpICD10Segments = new ObservableCollection<SqlICD10SegmentVM>();
            #region Yikes! ugly, only open if you have to
            //get icd10 segments
            //iterate though each assigned ICD10
            foreach (string strICD10 in ICD10s)
            {
                char strAlphaCode = char.Parse(strICD10.Substring(0, 1));  //First Char is alpha code
                    if (Char.IsDigit(strAlphaCode))
                    {
                    System.Windows.MessageBox.Show($"Cannot convert first character of the ICD10 code to a character! Value:{strICD10}");
                }
                string str = "";
                foreach (char ch in strICD10)
                {
                    if (Char.IsDigit(ch))
                        str += ch; //only include digits
                    if (ch == '.')
                        str += ch; //preserve decimal
                    if (Char.ToLower(ch) == 'x')
                        break; //if placeholder character, then stop.
                }
                double icd10numeric = double.Parse(str); //convert to number so I can compare with my database ICD10segmnents
                foreach (SqlICD10SegmentVM ns in SqlICD10SegmentVM.NoteICD10Segments)
                {
                    if (strAlphaCode == ns.SqlICD10Segment.icd10Chapter)
                    {
                        if (icd10numeric >= ns.SqlICD10Segment.icd10CategoryStart && icd10numeric <= ns.SqlICD10Segment.icd10CategoryEnd)
                        {
                            ns.IncludeSegment = true;
                            bool duplicate = false;
                            foreach (var tmpSeg in tmpICD10Segments)
                            {
                                if (tmpSeg.ICD10SegmentID == ns.ICD10SegmentID)
                                    duplicate = true;
                            }
                            if (!duplicate) tmpICD10Segments.Add(ns);  //avoid duplicats and add to segment list, I know there is a much better way to do it. :) I'm tired.
                        }
                    }
                }
            }

            //add all general sections
            foreach (SqlICD10SegmentVM ns in SqlICD10SegmentVM.NoteICD10Segments)
            {
                if (ns.SqlICD10Segment.icd10Chapter == 'X') // X segments are custom segments based on chart finding such as htn or pregnant state
                {
                    ns.IncludeSegment = true;
                    #region Assign Include segments
                    if (!HashTags.Contains("!HTNUrgency") && ns.SqlICD10Segment.ICD10SegmentID == 40) //if htnurgency is not present
                    {
                        ns.IncludeSegment = false;
                    }
                    if (ns.SqlICD10Segment.ICD10SegmentID == 72) //Adult rapid RR
                    {
                        if (patientVM.PtAgeYrs <= 17)
                            ns.IncludeSegment = false; //do not include children in 72
                        if (!HashTags.Contains("!RRHigh"))
                            ns.IncludeSegment = false; //do not include children in 72
                    }
                    if (ns.SqlICD10Segment.ICD10SegmentID == 91) //Peds rapid RR
                    {
                        if (patientVM.PtAgeYrs >= 18)
                            ns.IncludeSegment = false;
                        if (!HashTags.Contains("!RRHigh"))
                            ns.IncludeSegment = false; //do not include children in 72
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
                    // This is getting obnoxious.
                    if (ns.SqlICD10Segment.ICD10SegmentID == 92) //todo: find better way to see if procedure note included.
                    {
                        if (ProcedureNote == null)
                        {
                            ns.IncludeSegment = false;
                        }
                        else
                        {
                            if (ProcedureNote.Length < 100)
                                ns.IncludeSegment = false;
                        }
                        //don't show
                        ns.IncludeSegment = false;
                    }

                    if (ns.SqlICD10Segment.ICD10SegmentID == 90) //never check ED Transfer
                    {
                        ns.IncludeSegment = false;
                    }

                    #endregion

                    if (!tmpICD10Segments.Contains(ns)) tmpICD10Segments.Add(ns);
                }
            }

            //Now check for synononimus ICD10s
            AlternativeICD10VM tmp = new AlternativeICD10VM();
            foreach (string strICD10 in ICD10s)
            {
                string tmpStr = strICD10;
                if (strICD10.Contains(" "))
                    tmpStr = strICD10.Split(' ')[0]; //most ICD10s have a number space then description
                foreach (AlternativeICD10VM ns in tmp.AlternativeICD10List)
                {
                    if (ns.AlternativeICD10 == tmpStr)
                    {
                        SqlICD10SegmentVM tmpSeg = (from c in SqlICD10SegmentVM.NoteICD10Segments where c.ICD10SegmentID == ns.TargetICD10Segment select c).FirstOrDefault();
                        tmpSeg.IncludeSegment = true;
                        tmpICD10Segments.Add(tmpSeg);
                    }
                }
            }

            foreach (SqlICD10SegmentVM ns in SqlICD10SegmentVM.NoteICD10Segments)
            {
                if (HashTags.Contains("!Laceration") && ns.SqlICD10Segment.ICD10SegmentID == 188) //if laceration is not present
                {
                    ns.IncludeSegment = true;
                    if (!tmpICD10Segments.Contains(ns))
                        tmpICD10Segments.Add(ns);
                }
            }
            #endregion

            documentM.ICD10Segments = tmpICD10Segments;
        }

        /// <summary>
        /// More complex note analysis features
        /// </summary>
        #region Note Analysis

        public bool SentToED()
        {
            return false;
        }

        public bool ProcedurePerformed()
        {
            return false;
        }

        #endregion

        /// <summary>
        /// Commands
        /// </summary>
        #region Commands
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
        #endregion
    }
}
