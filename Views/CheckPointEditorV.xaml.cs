using Dapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using System.IO;
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
    /// Interaction logic for CheckPointEditorV.xaml
    /// </summary>
    public partial class CheckPointEditorV : Window
    {

        SqlCheckpointM CurrentCheckpoint;
        public CheckPointEditorV()
        {
            InitializeComponent();
            #region SetDictionary
            //spellcheck setup
            IList dictionaries = SpellCheck.GetCustomDictionaries(tbComment);
            dictionaries.Add(new Uri(@"pack://application:,,,/MedTerms.lex"));
            //I'm getting an error below
            try
            {
                tbComment.SpellCheck.IsEnabled = true;
            }
            catch (Exception)
            {
            }
            #endregion  
            DataContext = new SqlICD10SegmentVM();
        }

        private void closeclick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CF.SetWindowPosition(this);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CF.SaveWindowPosition(this);
        }
        private void btnLinkClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(CurrentCheckpoint.Link);
        }


        /// <summary>
        /// Not sure why I'm holding onto this
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CPSummaryClick(object sender, RoutedEventArgs e)
        {
            SqlICD10SegmentM seg = lbICD10.SelectedItem as SqlICD10SegmentM;
            if (seg != null)
            {
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    string sql = $"Select * from CheckPoints where TargetICD10Segment == {seg.ICD10SegmentID}";
                    List<SqlCheckpointM> lcp = cnn.Query<SqlCheckpointM>(sql).ToList();
                    sql = "Select * from CheckPointTypes order by ItemOrder;";
                    List<SqlCheckPointType> lcpt = cnn.Query<SqlCheckPointType>(sql).ToList();

                    string strSummary = "";
                    foreach (SqlCheckPointType cpt in lcpt)
                    {
                        string strTempOut = "";
                        foreach (SqlCheckpointM cp in lcp)
                        {
                            if (cp.CheckPointType == cpt.CheckPointTypeID)
                            {
                                strTempOut += $"\t- {cp.CheckPointTitle}" + Environment.NewLine;
                                strTempOut += $"\t\t {cp.Comment}" + Environment.NewLine;
                                strTempOut += $"\t\t Severity: {cp.ErrorSeverity}/10" + Environment.NewLine;
                                strTempOut += $"\t\t Action: {cp.Action}" + Environment.NewLine;
                            }
                        }
                        if (strTempOut != "")
                        {
                            strSummary += $"Section: {cpt.Title} ({cpt.Comment})" + Environment.NewLine;
                            strSummary += strTempOut;
                            strSummary += Environment.NewLine;
                        }
                    }
                    Clipboard.SetText(strSummary);
                }

            }
        }
    }


}
