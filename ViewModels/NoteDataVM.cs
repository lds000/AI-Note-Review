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
                return NoteString;
            }
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

        string[] NoteSegmentTitles = { "Reason for Appointment", "History of Present Illness", "Current Medications", "Taking", "Not-Taking/PRN", "Discontinued",
            "Unknown", "Active Problem List", "Past Medical History", "Surgical History","Surgical History","Family History","Social History",
            "Hospitalization/Major Diagnostic Procedure","Review of Systems","Vital Signs","Examination","Assessments","Treatment","Follow Up",
            "Allergies","Medication Summary","Electronically signed*"
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
