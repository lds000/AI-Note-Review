using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AI_Note_Review
{
    public class NoteDataVM : INotifyPropertyChanged
    {

        #region inotify
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        private NoteDataM noteData
        {
            get;
            set;
        }

        public NoteDataVM()
        {
            noteData = new NoteDataM();
        }

        public MasterReviewSummaryVM ParentMasterReviewSummary
        {
            get;
            set;
        }

        public NoteDataVM(DocumentVM d)
        {
            noteData = new NoteDataM();

            VisitDate = d.VisitDate;
            ProviderID = d.Provider.ProviderID;


            string strAnonymousNote = d.NoteHTML.Body.InnerHtml;
            int tmpIndex = strAnonymousNote.IndexOf(@"id=""pnDetails");
            if (tmpIndex > 10)
            {
                strAnonymousNote = strAnonymousNote.Substring(tmpIndex - 8);
            }
            //make name anonymousf
            string tmpFirst = "John";
            if (d.Patient.isFemale)
                tmpFirst = "Jane";
                    
            strAnonymousNote = strAnonymousNote.Replace(d.Patient.PtFirstName, tmpFirst);
            strAnonymousNote = strAnonymousNote.Replace(d.Patient.PtLastName, "Noname");
            strAnonymousNote = strAnonymousNote.Replace(d.Patient.PtLastName.ToUpper(), "Noname");

            strAnonymousNote = strAnonymousNote.Replace(d.Patient.PtAddress, "Address-hidden");
            strAnonymousNote = strAnonymousNote.Replace(d.Patient.PtPhone, "Phone-number-hidden");
            //make DOB anonymous
            strAnonymousNote = strAnonymousNote.Replace($"{d.Patient.DOB.Month}/{d.Patient.DOB.Day}/{d.Patient.DOB.Year}", $"{d.Patient.DOB.Month}/01/{d.Patient.DOB.Year}");

            if (d.Patient.DOB.Month <=9)
                strAnonymousNote = strAnonymousNote.Replace($"0{d.Patient.DOB.Month}/{d.Patient.DOB.Day}/{d.Patient.DOB.Year}", $"{d.Patient.DOB.Month}/01/{d.Patient.DOB.Year}");

            if (d.Patient.DOB.Day <= 9)
                strAnonymousNote = strAnonymousNote.Replace($"{d.Patient.DOB.Month}/0{d.Patient.DOB.Day}/{d.Patient.DOB.Year}", $"{d.Patient.DOB.Month}/01/{d.Patient.DOB.Year}");

            if ((d.Patient.DOB.Month <= 9) && (d.Patient.DOB.Month <= 9))
                strAnonymousNote = strAnonymousNote.Replace($"0{d.Patient.DOB.Month}/0{d.Patient.DOB.Day}/{d.Patient.DOB.Year}", $"0{d.Patient.DOB.Month}/01/{d.Patient.DOB.Year}");

            NoteString = Encryption.Encrypt(strAnonymousNote);


            string sql = "";
            sql = $"INSERT INTO Data (VisitDate,ProviderID,NoteString) VALUES (@VisitDate,@ProviderID,@NoteString);SELECT last_insert_rowid()";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteNotesLocation))
            {
                noteData.NoteID = cnn.ExecuteScalar<int>(sql, this);
            }
        }

        public string HTML
        {
            get
            {
                DocumentVM d = new DocumentVM(this);
                string strNote = "";
                strNote += "<b>Sex:</b>" + Environment.NewLine;
                strNote += d.Patient.PtSex + Environment.NewLine;
                strNote += "<br><b>DOB:</b>" + Environment.NewLine;
                strNote += d.Patient.DOB.ToShortDateString() + Environment.NewLine;
                strNote += "<br><b>PtID:</b>" + Environment.NewLine;
                strNote += d.Patient.PtID + Environment.NewLine;
                strNote += "<br><b>Date of Service:</b>" + Environment.NewLine;
                strNote += d.VisitDate.ToShortDateString() + Environment.NewLine;
                strNote += "<br><b>Reason for Appointment:</b><br>" + Environment.NewLine;
                strNote += StringToHTML(d.CC) + Environment.NewLine;
                strNote += "<br><b>History of Present Illness:</b><br>" + Environment.NewLine;
                strNote += StringToHTML(d.HPI) + Environment.NewLine;
                strNote += "<br>Review of Systems</b><br>" + Environment.NewLine;
                strNote += StringToHTML(d.ROS) + Environment.NewLine;
                strNote += "<br><b>Current Medications:</b><br>" + Environment.NewLine;
                strNote += StringToHTML(d.CurrentMeds) + Environment.NewLine;
                strNote += "<br><b>PRN Medications</b><br>" + Environment.NewLine;
                strNote += StringToHTML(d.CurrentPrnMeds) + Environment.NewLine;
                strNote += "<br><b>Active Problem List</b><br>" + Environment.NewLine;
                strNote += StringToHTML(d.ProblemList) + Environment.NewLine;
                strNote += "<br><b>Past Medical History</b><br>" + Environment.NewLine;
                strNote += StringToHTML(d.PMHx) + Environment.NewLine;
                strNote += "<br><b>Allergies</b><br>" + Environment.NewLine;
                strNote += StringToHTML(d.Allergies) + Environment.NewLine;
                strNote += "<br><b>Surgical History</b><br>" + Environment.NewLine;
                strNote += StringToHTML(d.SurgHx) + Environment.NewLine;
                strNote += "<br><b>Family History</b><br>" + Environment.NewLine;
                strNote += StringToHTML(d.FamHx) + Environment.NewLine;
                strNote += "<br><b>Social History</b><br>" + Environment.NewLine;
                strNote += StringToHTML(d.SocHx) + Environment.NewLine;
                strNote += "<br><b>Hospitalizations</b><br>" + Environment.NewLine;
                strNote += StringToHTML(d.Hospitalizations) + Environment.NewLine;
                strNote += "<br>Vital Signs<b>Review of Systems</b><br>" + Environment.NewLine;
                strNote += StringToHTML(d.Vitals) + Environment.NewLine;
                strNote += "<br><b>Examination</b><br>" + Environment.NewLine;
                strNote += StringToHTML(d.Exam) + Environment.NewLine;
                strNote += "<br><b>Procedure Note</b><br>" + Environment.NewLine;
                strNote += StringToHTML(d.ProcedureNote) + Environment.NewLine;
                strNote += "<br><b>Assessments</b><br>" + Environment.NewLine;
                strNote += StringToHTML(d.Assessments) + Environment.NewLine;
                strNote += "<br><b>Started Medications</b><br>" + Environment.NewLine;
                strNote += StringToHTML(d.MedsStarted) + Environment.NewLine;
                strNote += "<br><b>Labs Ordered</b><br>" + Environment.NewLine;
                strNote += StringToHTML(d.LabsOrdered) + Environment.NewLine;
                strNote += "<br><b>Treatment</b><br>" + Environment.NewLine;
                strNote += StringToHTML(d.Treatment) + Environment.NewLine;
                strNote += "<br><b>Follow Up</b><br>" + Environment.NewLine;
                strNote += StringToHTML(d.FollowUp) + Environment.NewLine;



                return strNote;
            }
        }

        string StringToHTML(string str)
        {
            return str.Replace(Environment.NewLine, "<br>");
        }

        public int NoteID
        {
            get
            {
                return noteData.NoteID;
            }
            set
            {
                noteData.NoteID = value;
            }
        }

        public DateTime VisitDate
        {
            get { return noteData.VisitDate; }
            set { noteData.VisitDate = value; }
        }

        public int ProviderID
        {
            get { return noteData.ProviderID; }
            set { noteData.ProviderID = value; }
        }

        public string NoteString
        {
            get { return noteData.NoteString; }
            set { noteData.NoteString = value; }
        }


        public bool Reviewed
        {
            get { return noteData.Reviewed; }
            set { noteData.Reviewed = value; }
        }

        public void DeleteNote()
        {
            string sql = "Delete from Data WHERE NoteID=@NoteID;";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteNotesLocation))
            {
                cnn.Execute(sql, this);
            }
        }

        string[] NoteSegmentTitles = { "Sex","PtID","DOB","Reason for Appointment", "History of Present Illness", "Current Medications", "Taking", "Started Medications", "Not-Taking/PRN", "Discontinued",
            "Unknown", "Active Problem List", "Past Medical History", "Surgical History","Surgical History","Family History","Social History",
            "Hospitalizations","Review of Systems","Vital Signs","Examination","Assessments","Treatment","Follow Up",
            "Allergies","Medication Summary","Electronically signed*","Procedure","Procedure Codes","PRN Medications","Ordered Labs"
        };

        public string GetSegment(string strSegment)
        {
            if (NoteString == null)
                return "";
            string strResult = "";

            bool SegmentActive = false;

            foreach (string strLine in NoteStrings)
            {
                foreach (string s in NoteSegmentTitles)
                {
                    if (strLine.Trim() == s)
                        SegmentActive = false;
                }
                if (SegmentActive && !string.IsNullOrEmpty(strLine.Trim()))
                    strResult += strLine + Environment.NewLine;
                if (strLine == strSegment)
                    SegmentActive = true;
            }
            return strResult;
        }

        private List<string> noteStrings;
        public List<string> NoteStrings
        {
            get
            {
                if (NoteString == null)
                    return null;
                if (noteStrings == null)
                {
                    foreach (string str in NoteSegmentTitles)
                    {
                       noteData.NoteString.Replace(str, Environment.NewLine + str + Environment.NewLine);
                    }
                    noteStrings = Regex.Split(noteData.NoteString, "\r\n|\r|\n").Select(x => x.Trim()).ToList();
                }
                return noteStrings;
            }
        }


        public void SaveNote()
        {
            string sql = "UPDATE Data SET " +
"NoteID=@NoteID, " +
"VisitDate=@VisitDate, " +
"ProviderID=@ProviderID, " +
"NoteString=@NoteString" +
"Reviewed=@Reviewed " +
"WHERE NoteID=@NoteID;";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteNotesLocation))
            {
                cnn.Execute(sql, this);
            }
        }
    }
}
