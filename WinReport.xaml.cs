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
        public WinReport(bool GeneralCheckPointsOnly = false)
        {
            InitializeComponent();
         //   document.ICD10Segments = document.GetSegments(GeneralCheckPointsOnly); //load all pertinent and 'X' segments
         //   document.GenerateReport(true);
         //   DataContext = document;
        }

        public WinReport(DocumentViewModel dvm, bool GeneralCheckPointsOnly = false)
        {
            InitializeComponent();
            ReportViewModel rvm = new ReportViewModel(dvm);
            DataContext = rvm;
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
            CF.IsReviewWindowOpen = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CF.SaveWindowPosition(this);
            CF.IsReviewWindowOpen = false;
        }

        private void Button_Click_Recheck(object sender, RoutedEventArgs e)
        {
            //document.GenerateReport();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //document.GenerateReport();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            //DataContext = null;

            //document.GenerateReport();
            //DataContext = document;
            
        }



        private void Button_ReportsReviewClick(object sender, RoutedEventArgs e)
        {
        }

        private void Button_CopyIndexClick(object sender, RoutedEventArgs e)
        {
            /*
            string sqlCheck = $"Select * from(Select distinct CheckPointID rel from RelCPPRovider where PtID={ document.PtID} " +
            $"AND ReviewDate = '{document.ReviewDate.ToString("yyyy-MM-dd")}') " + 
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
            */
        }


        private void Button_CheckpointAuditClick(object sender, RoutedEventArgs e)
        {

        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Image i = sender as Image;
            //SqlCheckpoint cp = i.DataContext as SqlCheckpoint;

            bool updown = true;            
            while (updown)
            {
                WinShowCheckPointRichText scp = new WinShowCheckPointRichText();
                ListBoxItem lbi = FocusManager.GetFocusedElement(this) as ListBoxItem;
                SqlCheckpoint cp = lbi.DataContext as SqlCheckpoint;
                scp.DataContext = cp;
                scp.Owner = this;
                //scp.ImChanged += Scp_AddMe;
                scp.ShowDialog();
                updown = scp.UpDownPressed;
            }
        }

        private void MovePassedCP(object sender, RoutedEventArgs e)
        {
            MenuItem i = sender as MenuItem;
            SqlCheckpoint cp = i.DataContext as SqlCheckpoint;
            //document.CPStatusOverrides.CreateNewOrUpdateExisting(cp, SqlRelCPProvider.MyCheckPointStates.Pass);
            //document.GenerateReport();
        }

        private void MoveMissedCP(object sender, RoutedEventArgs e)
        {
            MenuItem i = sender as MenuItem;
            SqlCheckpoint cp = i.DataContext as SqlCheckpoint;
            //document.CPStatusOverrides.CreateNewOrUpdateExisting(cp, SqlRelCPProvider.MyCheckPointStates.Fail);
            //document.GenerateReport();
        }

        private void DropCP(object sender, RoutedEventArgs e)
        {
            MenuItem i = sender as MenuItem;
            SqlCheckpoint cp = i.DataContext as SqlCheckpoint;
            //document.CPStatusOverrides.CreateNewOrUpdateExisting(cp, SqlRelCPProvider.MyCheckPointStates.Irrelevant);
            //document.GenerateReport();
        }

        private void AddCommentCP(object sender, RoutedEventArgs e)
        {
            MenuItem i = sender as MenuItem;
            SqlCheckpoint cp = i.DataContext as SqlCheckpoint;
            SqlCheckpointViewModel cpvm = new SqlCheckpointViewModel(cp);

            WinEnterText wet = new WinEnterText($"Add/Edit Comment: {cpvm.SqlCheckpoint.CheckPointTitle}", cpvm.CustomComment);
            wet.Owner = this;
            wet.ShowDialog();

            cpvm.CustomComment = wet.ReturnValue;
        }

        private void Button_ResetYesNo(object sender, RoutedEventArgs e)
        {
            //CF.YesNoSqlRegExIndex.Clear();
            //document.GenerateReport();
        }

        private void Button_CopyReportClick(object sender, RoutedEventArgs e)
        {

        }

        private void Button_CommittReportClick(object sender, RoutedEventArgs e)
        {
            
        }
    }

}
