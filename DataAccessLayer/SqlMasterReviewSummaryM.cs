using Dapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace AI_Note_Review
{

    /*
 * Great example I found online.
 class PersonM {
    public string Name { get; set; }
  }

class PersonVM {
    private PersonM Person { get; set;}
    public string Name { get { return this.Person.Name; } }
    public bool IsSelected { get; set; } // example of state exposed by view model

    public PersonVM(PersonM person) {
        this.Person = person;
    }
}
*/
    public class SqlMasterReviewSummaryM : INotifyPropertyChanged
    {
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public int MasterReviewSummaryID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string MasterReviewSummaryTitle { get; set; }
        public string MasterReviewSummarySubject { get; set; }
        public string MasterReviewSummaryComment { get; set; }
        public string MasterReviewSummaryImpression { get; set; }

        public void SaveToDB()
        {
            string sql = "UPDATE MasterReviewSummary SET " +
                    "MasterReviewSummaryID=@MasterReviewSummaryID, " +
                    "StartDate=@StartDate, " +
                    "EndDate=@EndDate, " +
                    "MasterReviewSummaryTitle=@MasterReviewSummaryTitle, " +
                    "MasterReviewSummarySubject=@MasterReviewSummarySubject, " +
                    "MasterReviewSummaryComment=@MasterReviewSummaryComment, " +
                    "MasterReviewSummaryImpression=@MasterReviewSummaryImpression " +
                    "WHERE MasterReviewSummaryID=@MasterReviewSummaryID;";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql, this);
            }
        }
    }
}
