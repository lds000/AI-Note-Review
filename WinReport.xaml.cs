using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for WinReport.xaml
    /// </summary>
    public partial class WinReport : Window
    {
        public WinReport()
        {
            InitializeComponent();
            CF.CurrentDoc.ICD10Segments = CF.CurrentDoc.GetSegments();

            CF.CurrentDoc.GenerateReport();
            DataContext = CF.CurrentDoc;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }




        private void lbFail_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBox lb = sender as ListBox;
            SqlCheckpoint cp = (SqlCheckpoint) lb.SelectedItem;
            WinCheckPointEditor wce = new WinCheckPointEditor(cp);
            wce.Owner = this;
            wce.ShowDialog();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CF.SetWindowPosition(this);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CF.SaveWindowPosition(this);
        }

        private void Button_Click_Recheck(object sender, RoutedEventArgs e)
        {

            CF.CurrentDoc.GenerateReport();


        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CF.CurrentDoc.GenerateReport();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            //DataContext = null;

            CF.CurrentDoc.GenerateReport();
            //DataContext = CF.CurrentDoc;
            
        }

        private void Button_CopyReportClick(object sender, RoutedEventArgs e)
        {
            string strReport = $"Checkpoint report for patient:{CF.CurrentDoc.PtID} seen by {CF.CurrentDoc.Provider} on {CF.CurrentDoc.VisitDate.ToShortDateString()}:"; // "This report is using a programmed algorythm that searches for terms in your documentation.  I personally programmed these terms so they may not apply to this clinical scenario.  I'm working on version 1.0 and I know this report is not perfect, but by version infinity.0 it will be. Please let me know how well my program worked (or failed). Your feedback is so much more important than any feedback I may provide you. Most important is that you let me know if this information is in any way incorrect. I will edit or re-write code to make it correct. Thanks for all you do! ";
            strReport += Environment.NewLine;
            strReport += "See reference at end of review for details of each check point." + Environment.NewLine + Environment.NewLine;

            strReport += "Passed check points: " + Environment.NewLine;


            foreach (SqlCheckpoint cp in (from c in CF.CurrentDoc.PassedCheckPoints orderby c.ErrorSeverity descending select c))
            {
                strReport += $"\t'{cp.CheckPointTitle}'" + Environment.NewLine;
            }

            strReport += Environment.NewLine;

            strReport += "Missed check points: " + Environment.NewLine;
            foreach (SqlCheckpoint cp in (from c in CF.CurrentDoc.MissedCheckPoints where c.ErrorSeverity > 0 orderby c.ErrorSeverity descending select c))
            {
                if (cp.IncludeCheckpoint)
                    strReport += cp.GetReport();        
            }

            strReport += Environment.NewLine;
            strReport += "Missed, but very minor check points: " + Environment.NewLine;
            foreach (SqlCheckpoint cp in (from c in CF.CurrentDoc.MissedCheckPoints where c.ErrorSeverity == 0 orderby c.ErrorSeverity descending select c))
            {
                if (cp.IncludeCheckpoint)
                    strReport += cp.GetReport();
            }

            foreach (SqlCheckpoint cp in (from c in CF.CurrentDoc.RelevantCheckPoints orderby c.ErrorSeverity descending select c))
            {
                if (cp.IncludeCheckpoint)
                strReport += cp.GetReport();
            }

            string strTempOut = "";
            foreach (SqlCheckpoint cp in (from c in CF.CurrentDoc.PassedCheckPoints orderby c.ErrorSeverity descending select c))
            {
                if (cp.IncludeCheckpoint)
                    strTempOut += cp.GetReport();
            }
            foreach (SqlCheckpoint cp in (from c in CF.CurrentDoc.IrrelaventCP orderby c.ErrorSeverity descending select c))
            {
                if (cp.IncludeCheckpoint)
                    strTempOut += cp.GetReport();
            }

            if (strTempOut != "")
            {
                strReport += Environment.NewLine;
                strReport += "Interesting check points to consider: " + Environment.NewLine;
                strReport += strTempOut;
            }

            Clipboard.SetText(strReport);
            //MessageBox.Show(strReport);
        }

        private void Button_CommittReportClick(object sender, RoutedEventArgs e)
        {
            string sqlCheck = $"Select Count() from RelCPPRovider where PtID={CF.CurrentDoc.PtID} AND VisitDate='{CF.CurrentDoc.VisitDate.ToString("yyyy-MM-dd")}';";
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                int iCount = cnn.ExecuteScalar<int>(sqlCheck);
                if (iCount > 0)
                {
                    MessageBoxResult mr = MessageBox.Show($"The patient ID and visit date already exist with {iCount} checkpoints. Press 'ok' to continue and replace previous report.", "Review Already Exists!", MessageBoxButton.OKCancel);
                    if (mr == MessageBoxResult.Cancel)
                    {
                        return;
                    }
                    string strDelete = $"Delete from RelCPPRovider where PtID={CF.CurrentDoc.PtID} AND VisitDate='{CF.CurrentDoc.VisitDate.ToString("yyyy-MM-dd")}';";
                    using (IDbConnection cnn1 = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                    {
                        cnn1.Execute(strDelete);
                    }
                }
            }

            foreach (SqlCheckpoint cp in (from c in CF.CurrentDoc.MissedCheckPoints orderby c.ErrorSeverity descending select c))
            {
                 cp.Commit(CF.CurrentDoc, SqlRelCPProvider.MyCheckPointStates.Fail);
            }

            foreach (SqlCheckpoint cp in (from c in CF.CurrentDoc.RelevantCheckPoints orderby c.ErrorSeverity descending select c))
            {
                 cp.Commit(CF.CurrentDoc, SqlRelCPProvider.MyCheckPointStates.Relevant);
            }

            foreach (SqlCheckpoint cp in (from c in CF.CurrentDoc.PassedCheckPoints orderby c.ErrorSeverity descending select c))
            {
                 cp.Commit(CF.CurrentDoc, SqlRelCPProvider.MyCheckPointStates.Pass);
            }
            foreach (SqlCheckpoint cp in (from c in CF.CurrentDoc.IrrelaventCP orderby c.ErrorSeverity descending select c))
            {
                cp.Commit(CF.CurrentDoc, SqlRelCPProvider.MyCheckPointStates.Irrelevant);
            }

            MessageBox.Show("Report committed.");

        }

        private void Button_ReportsReviewClick(object sender, RoutedEventArgs e)
        {
        }

        private void Button_CopyIndexClick(object sender, RoutedEventArgs e)
        {
            string sqlCheck = $"Select * from(Select distinct CheckPointID rel from RelCPPRovider where PtID={ CF.CurrentDoc.PtID} " +
            $"AND ReviewDate = '{CF.CurrentDoc.ReviewDate.ToString("yyyy-MM-dd")}') " + 
            $"inner join CheckPoints cp on rel = cp.CheckPointID " + 
            $"order by cp.TargetSection, cp.ErrorSeverity desc;";

            List<SqlCheckpoint> cplist = new List<SqlCheckpoint>();
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cplist = cnn.Query<SqlCheckpoint>(sqlCheck).ToList();
            }

            string strOut = "Index for relevant checkpoints." + Environment.NewLine;
            foreach (SqlCheckpoint cp in cplist)
            {
                strOut += cp.GetIndex();
            }
            Clipboard.SetText(strOut);

        }

        private void Button_CheckpointAuditClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
