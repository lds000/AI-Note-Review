using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AI_Note_Review
{
    /// <summary>
    /// Interaction logic for winDbRelICD10CheckpointsEditor.xaml
    /// </summary>
    public partial class winDbRelICD10CheckpointsEditor : Window
    {
        public winDbRelICD10CheckpointsEditor()
        {
            InitializeComponent();
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLightDataAccess.SQLiteDBLocation))
            {
                string sql = "Select * from ICD10Segments order by icd10Chapter, icd10CategoryStart;";
                SqlLightDataAccess.ICD10Segments = cnn.Query<SqlICD10Segment>(sql).ToList();
                sql = "Select * from CheckPointTypes;";
                cbTypes.ItemsSource = cnn.Query(sql).ToList();
                sql = "Select * from NoteSections;";
                cbTargetSection.ItemsSource = cnn.Query(sql).ToList();
            }
            lbICD10.ItemsSource = SqlLightDataAccess.ICD10Segments;
        }

        private void closeclick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void lbICD10_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SqlICD10Segment seg = lbICD10.SelectedItem as SqlICD10Segment;
            if (seg != null)
            {
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLightDataAccess.SQLiteDBLocation))
                {
                    string sql = $"Select * from CheckPointSummary where ICD10SegmentID == {seg.ICD10SegmentID};";
                    try
                    {
                        lbCheckpoints.ItemsSource = cnn.Query(sql).ToList();
                        lbCheckpoints.SelectedValuePath = "CheckPointID";
                    }
                    catch (Exception e2)
                    {
                        Console.WriteLine($"Error on saving variation data: {e2.Message}");
                    }
                }
                
            }
        }

        private void lbCheckpoints_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SqlICD10Segment seg = lbICD10.SelectedItem as SqlICD10Segment;
            if (seg != null)
            {
                lbCheckpoints.ItemsSource = seg.GetCheckPoints();
            }
        }

        private void lbCheckpoints_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           int selectedCheckPointID = int.Parse(lbCheckpoints.SelectedValue.ToString());
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLightDataAccess.SQLiteDBLocation))
            {
                string sql = $"Select * from CheckPoints where CheckPointID == {selectedCheckPointID};";
                try
                {
                    SqlCheckpoint cp = cnn.QuerySingle<SqlCheckpoint>(sql);
                    //cbTypes.SelectedIndex = cp.CheckPointType;
                    dpCheckpoint.DataContext = cp;
                }
                catch (Exception e2)
                {
                    Console.WriteLine($"Error on saving variation data: {e2.Message}");
                }
            }
        }
    }
}
