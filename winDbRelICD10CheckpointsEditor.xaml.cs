using Dapper;
using System;
using System.Collections;
using System.Collections.Generic;
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
    /// Interaction logic for winDbRelICD10CheckpointsEditor.xaml
    /// </summary>
    public partial class winDbRelICD10CheckpointsEditor : Window
    {

        SqlCheckpoint CurrentCheckpoint;
        public winDbRelICD10CheckpointsEditor()
        {
            InitializeComponent();
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                string sql = "Select * from ICD10Segments order by icd10Chapter, icd10CategoryStart;";
                SqlLiteDataAccess.ICD10Segments = cnn.Query<SqlICD10Segment>(sql).ToList();
                sql = "Select * from CheckPointTypes;";
                cbTypes.ItemsSource = cnn.Query(sql).ToList();
                sql = "Select * from NoteSections;";
                cbTargetSection.ItemsSource = cnn.Query(sql).ToList();
                sql = "Select * from ICD10Segments;";
                cbTargetICD10.ItemsSource = cnn.Query(sql).ToList();
            }
            lbICD10.ItemsSource = SqlLiteDataAccess.ICD10Segments;

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
        }

        private void closeclick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void lbICD10_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshICD10();
        }

        private void RefreshICD10()
        {
            SqlICD10Segment seg = lbICD10.SelectedItem as SqlICD10Segment;
            if (seg != null)
            {
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    string sql = $"Select * from CheckPointSummary where ICD10SegmentID == {seg.ICD10SegmentID}";
                    try
                    {
                        lbCheckpoints.ItemsSource = cnn.Query(sql).ToList();
                        lbCheckpoints.SelectedValuePath = "CheckPointID";
                        CurrentCheckpoint = null;
                        UpdateCurrentCheckPoint();
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
            if (lbCheckpoints.SelectedValue == null) return;
           int selectedCheckPointID = int.Parse(lbCheckpoints.SelectedValue.ToString());
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                string sql = $"Select * from CheckPoints where CheckPointID == {selectedCheckPointID};";
                try
                {
                    CurrentCheckpoint = cnn.QuerySingle<SqlCheckpoint>(sql);
                    UpdateCurrentCheckPoint();
                }
                catch (Exception e2)
                {
                    Console.WriteLine($"Error on saving variation data: {e2.Message}");
                }
            }
        }

        private void UpdateCurrentCheckPoint()
        {
            dpCheckpoint.DataContext = CurrentCheckpoint;
            string strRtext = @"{\rtf1\ansi\ansicpg1252\uc1\htmautsp\deff2{\fonttbl{\f0\fcharset0 Times New Roman;}{\f2\fcharset0 Segoe UI;}}{\colortbl\red0\green0\blue0;\red255\green255\blue255;}\loch\hich\dbch\pard\plain\ltrpar\itap0{\lang1033\fs18\f2\cf0 \cf0\ql}}";
            if (CurrentCheckpoint != null)
            {
                if (CurrentCheckpoint.RichText != null)
                {
                    strRtext = CurrentCheckpoint.RichText;
                }
            }
            //this is not working. dang.
            StrToRTB(strRtext, myRTB);
            string str2 = RTBtoStr(myRTB);
            StrToRTB(str2, myRTB);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CF.SetWindowPosition(this);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CF.SaveWindowPosition(this);
        }


        private void cbTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrentCheckpoint == null) return;
            if (cbTypes.SelectedValue == null) return;
            CurrentCheckpoint.CheckPointType = int.Parse(cbTypes.SelectedValue.ToString());
            CurrentCheckpoint.SaveToDB();
        }

        private void cbTargetSection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrentCheckpoint == null) return;
            if (cbTargetSection.SelectedValue == null) return;
            CurrentCheckpoint.TargetSection = int.Parse(cbTargetSection.SelectedValue.ToString());
            CurrentCheckpoint.SaveToDB();
        }

        private void cbTargetICD10_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrentCheckpoint == null) return;
            if (cbTargetICD10.SelectedValue == null) return;
            CurrentCheckpoint.TargetICD10Segment = int.Parse(cbTargetICD10.SelectedValue.ToString());
            CurrentCheckpoint.SaveToDB();
        }

        private void tbComment_LostFocus(object sender, RoutedEventArgs e)
        {
            if (CurrentCheckpoint == null) return;
            CurrentCheckpoint.Comment = tbComment.Text;
            CurrentCheckpoint.SaveToDB();
        }

        private void tbAction_LostFocus(object sender, RoutedEventArgs e)
        {
            if (CurrentCheckpoint == null) return;
            CurrentCheckpoint.Action = tbAction.Text;
            CurrentCheckpoint.SaveToDB();
        }

        private void tbLink_LostFocus(object sender, RoutedEventArgs e)
        {
            if (CurrentCheckpoint == null) return;
            CurrentCheckpoint.Link = tbLink.Text;
            CurrentCheckpoint.SaveToDB();
        }

        private void tbTitle_LostFocus(object sender, RoutedEventArgs e)
        {
            if (CurrentCheckpoint == null) return;
            CurrentCheckpoint.CheckPointTitle = tbTitle.Text;
            CurrentCheckpoint.SaveToDB();

        }

        private void AddCP(object sender, RoutedEventArgs e)
        {
            SqlICD10Segment seg = lbICD10.SelectedItem as SqlICD10Segment;
            if (seg != null)
            {

                WinEnterText wet = new WinEnterText("Please input new title.");
                wet.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                wet.Owner = this;
                wet.ShowDialog();
                if (wet.ReturnValue == null) return;
                if (wet.ReturnValue.Trim() != "")
                {
                    CurrentCheckpoint = new SqlCheckpoint(wet.ReturnValue, seg.ICD10SegmentID);
                    UpdateCurrentCheckPoint();
                    int tmpID = CurrentCheckpoint.CheckPointID;
                    RefreshICD10();
                    lbCheckpoints.SelectedValue = tmpID;
                    //CurrentCheckpoint.SaveToDB();
                }
            }
        }

        private void deleteCP(object sender, RoutedEventArgs e)
        {
            if (CurrentCheckpoint.DeleteFromDB())
            {
                dpCheckpoint.DataContext = null;
                
                RefreshICD10();
            }
        }

        private void myRTB_MouseLeave(object sender, MouseEventArgs e)
        {
            if (CurrentCheckpoint == null) return;
        }

        private void myRTB_LostFocus(object sender, RoutedEventArgs e)
        {
            if (CurrentCheckpoint == null) return;
            CurrentCheckpoint.RichText = RTBtoStr(myRTB);
            CurrentCheckpoint.SaveToDB();
        }

        private void StrToRTB(string strIn, RichTextBox rtb)
         {
            byte[] byteArray = Encoding.ASCII.GetBytes(strIn);
            using (MemoryStream ms = new MemoryStream(byteArray))
            {
                TextRange tr = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
                tr.Load(ms, DataFormats.Rtf);
            }
        }

        private string RTBtoStr(RichTextBox rtb)
        {
            string rtfText; //string to save to db
            TextRange tr = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            using (MemoryStream ms = new MemoryStream())
            {
                tr.Save(ms, DataFormats.Rtf);
                rtfText = Encoding.ASCII.GetString(ms.ToArray());
            }
            return rtfText;

        }
    }
}
