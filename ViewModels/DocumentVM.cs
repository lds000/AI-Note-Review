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
        private DocumentM document;

        /// <summary>
        /// Constructor used in MasterReviewSummary, get masterreview, provider, patient, and create a document based on SampleDocument
        /// </summary>
        /// <param name="mrs"></param>
        public DocumentVM(MasterReviewSummaryVM mrs)
        {
            masterReviewVM = mrs;
            patientVM = mrs.Patient;
            document = SampleDocument; //New DocumentM() called under this.
        }

        /// <summary>
        /// Constructor used in notehunter.
        /// </summary>
        /// <param name="doc"></param>
        public DocumentVM(HtmlDocument doc) //used by document hunter
        {
            document = new DocumentM();
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
                        tmps.InitiateSection(document, patientVM);
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

        public void SaveNote()
        {
            if (NoteHTML == null)
                return;
            if (NoteHTML.Body == null)
                return;
            //save encrypted note
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteNotesLocation))
            {
                NoteDataVM nvm = new NoteDataVM(this);
            }

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
                document = new DocumentM();
                SetProvider("Devin Hansen"); //Provider ID comes from this.
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
                SetProvider("Andrea Stevens, NP");
                return document;
            }
        }

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

        #region Process EcwContent

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

        /// <summary>
        /// Sift through the HTML of the eCW document and exctract the details of a locked eCw chart note
        /// </summary>
        /// <param name="HDoc"></param>
        public void processUnlocked(HtmlDocument HDoc)
        {
            string strNote = HDoc.Body.InnerText;
            if (strNote == null)
                return;
            #region Process unlocked magic

            string strCommand = "";
            string strMedType = "";

            //process the html one line at a time
            foreach (var myString in strNote.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                //The patient identity html, process patient name, sex, and DOB
                if (myString.StartsWith("Patient:") && myString.Contains("DOB:"))
                {
                    string strPtInfo = myString.Replace("Patient:", "");
                    strPtInfo = strPtInfo.Replace("DOB:", "|");
                    strPtInfo = strPtInfo.Replace("Age:", "|");
                    strPtInfo = strPtInfo.Replace("Sex:", "|");
                    patientVM.PtName = strPtInfo.Split('|')[0].Trim();
                    patientVM.PtSex = strPtInfo.Split('|')[3].Trim();
                    string strDOB = strPtInfo.Split('|')[1];
                    patientVM.DOB = DateTime.Parse(strDOB);
                }

                //The encounter information html code, get visitdate and provider
                if (myString.StartsWith("Encounter Date:") && myString.Contains("Provider:"))
                {
                    string strPtInfo = myString.Replace("Encounter Date:", "");
                    strPtInfo = strPtInfo.Replace("Provider:", "|");
                    VisitDate = DateTime.Parse(strPtInfo.Split('|')[0].Trim());
                    SetProvider(strPtInfo.Split('|')[1].Trim());
                }

                //Facility HTML, get location.
                if (myString.StartsWith("Appointment Facility:"))
                {
                    document.Facility = myString.Split(':')[1];
                }

                //Account number line, get patientID
                if (myString.StartsWith("Account Number:"))
                {
                    patientVM.PtID = myString.Split(':')[1];
                }

                //If the html line contains a notesection identifier process that notesection
                if (noteSection.Contains(myString.Trim())) //first, let's see if we are in a section, if so then set the strCommand to the current section
                {
                    strCommand = myString.Trim();
                }
                else //Now process any text under that command
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
                                //CurrentPrnMeds += myString + " (Discontinued)" + Environment.NewLine;
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
                            document.Vitals += myString.Trim().TrimEnd('.') + Environment.NewLine;
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
                            if (myString.Trim().StartsWith("Start")) //may not always work, keep an eye on this.
                            {
                                document.MedsStarted += myString.Trim() + Environment.NewLine;
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
            if (Vitals != "")
            {
                strVitals = Vitals.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)[0];
            }
            Vitals = strVitals.Trim().TrimEnd('.');

            #endregion
        }

        public void processLocked(HtmlDocument HDoc)
        {
            #region Process locked document magic
            var watch = System.Diagnostics.Stopwatch.StartNew();

            List<string> medsStarted = new List<string>();
            List<string> lLabsOrdered = new List<string>();
            string strMedsSarted = "";
            string strLabsOrdered = "";
            string strImagesOrdered = "";

            HtmlElementCollection AllTRItems = HDoc.Body.GetElementsByTagName("TR"); //5 ms
            HtmlElementCollection AllTHEADItems = HDoc.Body.GetElementsByTagName("THEAD"); //5 ms

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
            int tmpIDX = 0;
            foreach (HtmlElement TempEl in AllTRItems) //99 items, 1.9 seconds!
            {
                string strClass = TempEl.GetAttribute("className");
                if (strClass == "")
                    continue;
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
                                SetProvider(strDocname.Split('|')[0]);
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
                                SetProvider(strDocname.Split('|')[0]);

                            }
                            continue;
                        }
                        if (strClass == "PtHeading")
                        {
                            patientVM.PtName = TempEl.InnerText.Replace("\n", "").Replace("\r", "");  //set first name
                        }
                        if (strClass == "PtData") //field has note informaition
                        {
                            tmpIDX++;
                            string strInnerText = TempEl.InnerText;
                            if (tmpIDX == 3)
                            {
                                patientVM.PtAddress = strInnerText;
                            }
                            if (tmpIDX == 4)
                            {
                                patientVM.PtPhone = strInnerText.Trim();
                            }
                            if (strInnerText == null)
                            {
                                continue;
                            }
                            if (strInnerText.Contains("Account Number:"))
                                patientVM.PtID = strInnerText.Split(':')[1].Trim();
                            if (strInnerText.Contains("Appointment Facility:"))
                                document.Facility = strInnerText.Split(':')[1].Trim();
                            if (strInnerText.Contains("DOB:"))
                            {
                                //   PtAge = strInnerText.Split(':')[0].TrimEnd("DOB");
                                patientVM.DOB = DateTime.Parse(strInnerText.Split(':')[1].Trim());
                                if (strInnerText.Contains("Female"))
                                {
                                    patientVM.PtSex = "F";
                                }
                                else
                                {
                                    patientVM.PtSex = "M";
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
                                var result = strInnerText.Split(new[] { '\r', '\n' });
                                string tmpStr = "";
                                foreach (string str in result)
                                {
                                    if (str.Length >=3)
                                    tmpStr += str.Substring(3) + "\n";
                                }
                                CC = tmpStr;
                            }
                            if (strCurrentHeading == "History of Present Illness")
                            {
                                var result = strInnerText.Split(new[] { '\r', '\n' });
                                string tmpStr = "";
                                foreach (string str in result)
                                {
                                    if (str.Trim() != "Note::" && str.Trim() != "")
                                        tmpStr += str.Trim() + "\n";
                                }
                                HPI = tmpStr;
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
                                    if (str == "Taking:")
                                        continue;
                                    if (str.StartsWith("Medication List reviewed"))
                                        continue;
                                    if (str.StartsWith("None"))
                                        continue;
                                    if (str.Trim() == "")
                                        continue;
                                    if (str == "Not-Taking:")
                                    {
                                        prn = true;
                                        continue;
                                    }
                                    if (str.StartsWith("/PRN"))
                                        continue;
                                    if (str == "Discontinued:")
                                        break;

                                    string strList = str;
                                    if (prn)
                                        strList = strList + "(NOT TAKING/PRN STATUS)";
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

                                /*
                                string strDx;
                                foreach (HtmlElement el in TempEl.Children)
                                {
                                    if (el.TagName == "TD")
                                    {
                                        strDx = el.InnerText;
                                    }
                                } 
                                */

                                if (strInnerText == null)
                                    continue;
                                var result = strTextToParse.Split(new[] { '\r', '\n' });
                                List<string> lProblemList = new List<string>();
                                foreach (string str in result)
                                {
                                    if (str.Trim() == "")
                                        continue;
                                    if (str.StartsWith("Onset"))
                                        continue;
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
                                    if (str.Trim() == "")
                                        continue;
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
                                if (strInnerText == null)
                                    continue;
                                var result = strTextToParse.Split(new[] { '\r', '\n' });
                                foreach (string str in result)
                                {
                                    if (str.Trim() == "")
                                        continue;
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
                                foreach (string str in result)
                                {
                                    if (str.Trim().StartsWith("LAB:"))
                                    {
                                        lLabsOrdered.Add(str);
                                        strLabsOrdered += str + "\n";
                                    }
                                    if (str.Trim().StartsWith("Start "))
                                    {
                                        medsStarted.Add(str);
                                        strMedsSarted += str + "\n";
                                    }
                                    if (str.Trim().StartsWith("IMAGING:"))
                                    {
                                        strImagesOrdered += str.Trim() + "\n";
                                    }
                                }
                                ImagesOrdered = strImagesOrdered;
                                Treatment += strInnerText;
                                MedsStarted = strMedsSarted;
                                LabsOrdered = strLabsOrdered;
                            }

                            if (strCurrentHeading.StartsWith("Diagnostic Imaging") && strInnerText != null)
                            {
                                var result = strInnerText.Split(new[] { '\r', '\n' });
                                List<string> ImagessOrdered = new List<string>();
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
        }
        #endregion


        #region mirror properties of DocumentM and notify on changed
        public string Facility
        {
            get
            {
                return document.Facility;
            }
            set
            {
                document.Facility = value;
                OnPropertyChanged();
            }
        }

        public void SetProvider(string strFullName)
        {
            if (strFullName == "")
            {
                Provider = new ProviderVM();
            }
            else
            Provider = ProviderVM.SqlGetProviderByFullName(strFullName);
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
                return document.VisitDate;
            }
            set
            {
                document.VisitDate = value;
                OnPropertyChanged();
                OnPropertyChanged("DocumentMRS");
            }
        }

        private MasterReviewSummaryVM documentMRS;
        public MasterReviewSummaryVM DocumentMRS
        {
            get
            {
                string sql = $"Select * from MasterReviewSummary where '{VisitDate.ToString("yyyy-MM-dd")}' Between StartDate and EndDate";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    documentMRS = cnn.Query<MasterReviewSummaryVM>(sql).FirstOrDefault();
                }
                return documentMRS;
            }
        }

        public string ReasonForAppt
        {
            get
            {
                return document.ReasonForAppt;
            }
            set
            {
                document.ReasonForAppt = value;
                OnPropertyChanged();
            }
        }
        public string CC
        {
            get
            {
                return document.CC;
            }
            set
            {
                document.CC = value;
                OnPropertyChanged();
            }
        }
        public string HPI
        {
            get
            {
                return document.HPI;
            }
            set
            {
                document.HPI = value;
                OnPropertyChanged();
            }
        }
        public string Allergies
        {
            get
            {
                return document.Allergies;
            }
            set
            {
                document.Allergies = value;
                OnPropertyChanged();
            }
        }
        public string SurgHx
        {
            get
            {
                return document.SurgHx;
            }
            set
            {
                document.SurgHx = value;
                OnPropertyChanged();
            }
        }
        public string FamHx
        {
            get
            {
                return document.FamHx;
            }
            set
            {
                document.FamHx = value;
                OnPropertyChanged();
            }
        }
        public string CurrentMeds
        {
            get
            {
                return document.CurrentMeds;
            }
            set
            {
                document.CurrentMeds = value;
                OnPropertyChanged();
            }
        }
        public string CurrentPrnMeds
        {
            get
            {
                return document.CurrentPrnMeds;
            }
            set
            {
                document.CurrentPrnMeds = value;
                OnPropertyChanged();
            }
        }
        public string ProblemList
        {
            get
            {
                return document.ProblemList;
            }
            set
            {
                document.ProblemList = value;
                OnPropertyChanged();
            }
        }
        public string ROS
        {
            get
            {
                return document.ROS;
            }
            set
            {
                document.ROS = value;
                OnPropertyChanged();
            }
        }
        public string PMHx
        {
            get
            {
                return document.PMHx;
            }
            set
            {
                document.PMHx = value;
                OnPropertyChanged();
            }
        }
        public string SocHx
        {
            get
            {
                return document.SocHx;
            }
            set
            {
                document.SocHx = value;
                OnPropertyChanged();
            }
        }
        public string GeneralHx
        {
            get
            {
                return document.GeneralHx;
            }
            set
            {
                document.GeneralHx = value;
                OnPropertyChanged();
            }
        }
        public string Vitals
        {
            get
            {
                return document.Vitals;
            }
            set
            {
                document.Vitals = value;
                OnPropertyChanged();
                parseVitalsString(value); //Convert the Vitals to valeus

            }
        }
        public string Exam
        {
            get
            {
                return document.Exam;
            }
            set
            {
                document.Exam = value;
                OnPropertyChanged();
            }
        }
        public string Treatment
        {
            get
            {
                return document.Treatment;
            }
            set
            {
                document.Treatment = value;
                OnPropertyChanged();
            }
        }
        public string PreventiveMed
        {
            get
            {
                return document.PreventiveMed;
            }
            set
            {
                document.PreventiveMed = value;
                OnPropertyChanged();
            }
        }
        public string MedsStarted
        {
            get
            {
                return document.MedsStarted;
            }
            set
            {
                document.MedsStarted = value;
                OnPropertyChanged();
            }
        }
        public string ImagesOrdered
        {
            get
            {
                return document.ImagesOrdered;
            }
            set
            {
                document.ImagesOrdered = value;
                OnPropertyChanged();
            }
        }
        public string VisitCodes
        {
            get
            {
                return document.VisitCodes;
            }
            set
            {
                document.VisitCodes = value;
                OnPropertyChanged();
            }
        }
        public string LabsOrdered
        {
            get
            {
                return document.LabsOrdered;
            }
            set
            {
                document.LabsOrdered = value;
                OnPropertyChanged();
            }
        }
        public string Assessments
        {
            get
            {
                return document.Assessments;
            }
            set
            {
                document.Assessments = value;
                OnPropertyChanged();
            }
        }
        public string FollowUp
        {
            get
            {
                return document.FollowUp;
            }
            set
            {
                document.FollowUp = value;
                OnPropertyChanged();
            }
        }
        public string ProcedureNote
        {
            get
            {
                return document.ProcedureNote;
            }
            set
            {
                document.ProcedureNote = value;
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
            document.HashTags += strHashTag + ", ";
        }

        /// <summary>
        /// HashTags for the more complex match functions
        /// </summary>
        public string HashTags
        {
            get
            {
                if (document.HashTags == null)
                {
                    HashTags = "";
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
                return document.HashTags;
            }
            set
            {
                document.HashTags = value;
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
                if (document.ICD10s == null)
                {
                    document.ICD10s = new ObservableCollection<string>();
                    foreach (var tmpAssessment in Assessments.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (tmpAssessment.Contains(" - "))
                        {
                            string strClean = tmpAssessment.TrimEnd(" (Primary) ").Trim();
                            strClean = strClean.Replace(" - ", "|");
                            string strCode = strClean.Split('|')[1].Trim();
                            document.ICD10s.Add(strCode);
                        }
                    }
                }
                return document.ICD10s;
            }
            set
            {
                document.ICD10s = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Contains the raw HTML of the note, may be used to persist note at some point, If I can remove identifying information that is.
        /// </summary>
        public HtmlDocument NoteHTML
        {
            get
            {
                return document.NoteHTML;
            }
            set
            {
                if (document.NoteHTML == value)
                    return; //do nothing if nothing has changed.

                resetNoteData();

                document.NoteHTML = value;
                if (document.NoteHTML.Body.InnerHtml.Contains("pnDetails")) //unique text to identify unlocked chart
                {
                    processUnlocked(document.NoteHTML);
                    Console.WriteLine($"Processed unlocked chart for {patientVM.PtName}.");
                }
                else
                {
                    processLocked(document.NoteHTML);
                    masterReviewVM.AddLog("Processing locked chart");
                    Console.WriteLine($"Processed locked chart for {patientVM.PtName}.");
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
                    return document.NoteHTML.Body.OuterHtml;
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
            //When NoteHTML is set, reset everything.
            //demographics
            //I'm trying to decide if using the parent or childe Property here.
            document.Facility = "";
            document.VisitDate = new DateTime(2020, 1, 1);
            SetProvider("");
            document.ProviderID = 0;
            document.ReasonForAppt = "";
            document.Allergies = "";
            document.Vitals = "";
            document.CC = "";
            document.HPI = "";
            document.CurrentMeds = "";
            document.ProcedureNote = "";
            document.PreventiveMed = "";
            document.CurrentPrnMeds = "";
            document.ProblemList = "";
            document.ROS = "";
            document.PMHx = "";
            document.SocHx = "";
            document.GeneralHx = "";
            document.Exam = "";
            document.Treatment = "";
            document.MedsStarted = "";
            document.ImagesOrdered = "";
            document.LabsOrdered = "";
            document.Assessments = "";
            document.FollowUp = "";
            document.SurgHx = "";
            document.FamHx = "";
            document.VisitCodes = "";
            document.Vitals = "";
            document.ICD10s = null;
            document.HashTags = null;
            document.ICD10Segments = null;
            noteSections = null;
        }

        /// <summary>
        /// Holds the matched ICD10Segments of the eCwDocument assigned, and discovered through the document (ie pregnantcapable), ICD10s is a subset of this.
        /// </summary>
        public ObservableCollection<SqlICD10SegmentVM> ICD10Segments
        {
            get
            {
                if (document.ICD10Segments == null)
                    UpdateICD10Segments();
                foreach (var tmpSeg in document.ICD10Segments)
                {
                    tmpSeg.ParentDocument = this;
                    tmpSeg.ParentReport = this.pare //intentional error
                }
                return document.ICD10Segments; ////see code below...
            }
            set
            {
                document.ICD10Segments = value;
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
            #endregion
            document.ICD10Segments = tmpICD10Segments;
        }


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
