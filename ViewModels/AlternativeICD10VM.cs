using Dapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
namespace AI_Note_Review
{
    public class AlternativeICD10VM : INotifyPropertyChanged
    {
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChangedSave([CallerMemberName] string name = null)
        {
            if (PropertyChanged != null)
            {
                SaveToDB();
                Console.WriteLine($"Property {name} was saved from SqlCheckpointVM!");
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }



        public int RelAlternativeICD10ID { get; set; }
        public string AlternativeICD10Title { get; set; }
        public string AlternativeICD10 { get; set; }
        public int TargetICD10Segment { get; set; }

        public SqlICD10SegmentVM ParentSegment { get; set; }

        private SqlRelAlternativeICD10M alternativeICD10M;

        public AlternativeICD10VM()
        {
            alternativeICD10M = new SqlRelAlternativeICD10M();
        }

        public AlternativeICD10VM(string _AlternativeICD10Title, string _AlternativeICD10, int _TargetICD10Segment)
        {
            string sql = "";
            sql = $"INSERT INTO RelAlternativeICD10 (AlternativeICD10Title,AlternativeICD10,TargetICD10Segment) VALUES ('{_AlternativeICD10Title}','{_AlternativeICD10}',{_TargetICD10Segment});SELECT last_insert_rowid()";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                var tmpResult = cnn.Query<AlternativeICD10VM>($"Select * from RelAlternativeICD10 where AlternativeICD10 like '{_AlternativeICD10}';").FirstOrDefault();
                if (tmpResult != null)
                {
                    MessageBox.Show($"ICD10 Code {_AlternativeICD10} already exists in the database under the name {tmpResult.AlternativeICD10Title};");
                    return;
                }
                int lastID = cnn.ExecuteScalar<int>(sql, this);
                alternativeICD10M = new SqlRelAlternativeICD10M();
                RelAlternativeICD10ID = lastID;
                AlternativeICD10 = _AlternativeICD10;
                TargetICD10Segment = _TargetICD10Segment;
            }
        }

        public void SaveToDB()
        {
                string sql = "UPDATE RelAlternativeICD10 SET " +
            "AlternativeICD10Title=@AlternativeICD10Title, " +
            "AlternativeICD10=@AlternativeICD10, " +
            "TargetICD10Segment=@TargetICD10Segment " +
            "WHERE RelAlternativeICD10ID=@RelAlternativeICD10ID;";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    cnn.Execute(sql, this);
                }
        }

        public bool DeleteFromDB()
        {
            MessageBoxResult mr = MessageBox.Show("Are you sure you want to remove this alternative ICD10? This is permenant and will delete all content.", "Confirm Delete", MessageBoxButton.YesNo);
            if (mr != MessageBoxResult.Yes)
            {
                return false;
            }

            string sql = "Delete from RelAlternativeICD10 where RelAlternativeICD10ID=@RelAlternativeICD10ID;";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cnn.Execute(sql, this);
            }
            return true;

        }

        private List<AlternativeICD10VM> alternativeICD10List;
        public List<AlternativeICD10VM> AlternativeICD10List
        {
            get
            {
                if (alternativeICD10List == null)
                {
                    string sql = $"Select * from  RelAlternativeICD10;";
                    using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                    {
                        var tmpList = cnn.Query<AlternativeICD10VM>(sql).ToList();
                        foreach (var item in tmpList)
                        {
                            //item.ParentSegment = ;
                        }
                        alternativeICD10List = tmpList;
                    }
                }
                return alternativeICD10List;
            }
        }
    }
}
