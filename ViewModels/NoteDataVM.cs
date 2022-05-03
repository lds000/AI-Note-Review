using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AI_Note_Review
{
    class NoteDataVM : INotifyPropertyChanged
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

        public NoteDataVM(DocumentVM d, PatientVM p)
        {
            noteData = new NoteDataM();

            VisitDate = d.VisitDate;
            ProviderID = d.ProviderID;
            string strAnonymousNote = d.NoteHTML.Body.InnerHtml;
            //make name anonymous
            strAnonymousNote = strAnonymousNote.Replace(p.PtName, "Smith,Bob");
            //make DOB anonymous
            strAnonymousNote = strAnonymousNote.Replace($"{p.DOB.Month}/{p.DOB.Day}/{p.DOB.Year}", $"{p.DOB.Month}/1/{p.DOB.Year}");
            NoteString = Encryption.Encrypt(strAnonymousNote);


            string sql = "";
            sql = $"INSERT INTO Data (VisitDate,ProviderID,NoteString) VALUES (@VisitDate,@ProviderID,@NoteString);SELECT last_insert_rowid()";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteNotesLocation))
            {
                noteData.NoteID = cnn.ExecuteScalar<int>(sql, this);
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
