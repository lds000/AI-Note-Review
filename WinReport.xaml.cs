using System;
using System.Collections.Generic;
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
            
            GetHashTags();

            lbSegmentsCheck.ItemsSource = CF.RelevantICD10Segments;

            GenerateReport();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        enum TagResult { Pass, Fail, FailNoCount };

        private TagResult CheckTagGroup(List<SqlTagRegEx> tmpTagRegExs)
        {
            foreach (SqlTagRegEx TagRegEx in tmpTagRegExs)
            {
                if (TagRegEx.TagRegExType == 1 || TagRegEx.TagRegExType == 4) //Any, if one match then include tag
                {
                    bool isMatch = false;
                    foreach (string strRegEx in TagRegEx.RegExText.Split(','))
                    {
                        if (CF.CurrentDoc.NoteSectionText[TagRegEx.TargetSection] != null)
                            if (Regex.IsMatch(CF.CurrentDoc.NoteSectionText[TagRegEx.TargetSection].ToLower(), strRegEx.Trim().ToLower())) // /i is lower case directive for regex
                            {
                                isMatch = true;
                            }
                        if (isMatch == false && TagRegEx.TagRegExType == 4) return TagResult.FailNoCount; //don't continue if type is "ANY NF" this is a stopper.
                    }
                    if (!isMatch) return TagResult.Fail; //no conditions met for this one so all fail.
                }

                //todo: check the logic for the rest!

                if (TagRegEx.TagRegExType == 2) //ALL, if one not match then include tag
                {
                    foreach (string strRegEx in TagRegEx.RegExText.Split(','))
                    {
                        if (CF.CurrentDoc.NoteSectionText[TagRegEx.TargetSection] != null)
                            if (!Regex.IsMatch(CF.CurrentDoc.NoteSectionText[TagRegEx.TargetSection], strRegEx.Trim() + "/i"))
                            {
                                return TagResult.Fail; //any mismatch makes it false.
                            }
                    }
                }

                if (TagRegEx.TagRegExType == 3) //none
                {
                    foreach (string strRegEx in TagRegEx.RegExText.Split(','))
                    {
                        if (CF.CurrentDoc.NoteSectionText[TagRegEx.TargetSection] != null)
                            if (Regex.IsMatch(CF.CurrentDoc.NoteSectionText[TagRegEx.TargetSection], strRegEx.Trim() + "/i")) ;
                        {
                            return TagResult.Fail; //any match makes it false
                        }
                    }
                }
            }

            return TagResult.Pass;
        }

        private void GetHashTags()
        {
            CF.CurrentDoc.DocumentTags.Clear();
            //get icd10 segments
            CF.RelevantICD10Segments.Clear();

            CF.RelevantICD10Segments = CF.CurrentDoc.ICD10Segments;
            if (CF.CurrentDoc.IsHTNUrgency)
            {
                CF.RelevantICD10Segments.Add(SqlLiteDataAccess.GetSegment(40)); //pull in HTNUrgencySegment
            }

            CF.RelevantICD10Segments.Add(SqlLiteDataAccess.GetSegment(36)); //add general segment that applies to all visits.
        }

        private void GenerateReport()
        {

            CF.PassedCP.Clear();
            CF.FailedCP.Clear();
            CF.IrrelaventCP.Clear();
            CF.RelevantCP.Clear();


            if (CF.RelevantICD10Segments.Count == 0)
            {
                System.Windows.MessageBox.Show("No icd10 segments found!");
                return;
            }

            //todo put into database as relevant/irrelavent vs pass/fail
            int[] relType = { 5, 6, 9, 10, 12 };

            List<int> AlreadyAddedPoints = new List<int>();


            foreach (SqlICD10Segment ns in CF.RelevantICD10Segments)
            {
                Console.WriteLine($"Now checking segment: {ns.SegmentTitle}");


                foreach (SqlCheckpoint cp in ns.GetCheckPoints())
                {
                    if (AlreadyAddedPoints.Contains(cp.CheckPointID))
                    {
                        continue;
                    }
                    AlreadyAddedPoints.Add(cp.CheckPointID);
                    Console.WriteLine($"Now analyzing '{cp.CheckPointTitle}' checkpoint.");
                    TagResult pass = TagResult.Pass;
                    foreach (SqlTag tag in cp.GetTags())
                    {
                        if (tag.TagText.Contains("Diarrhea"))
                        {

                        }

                        TagResult includeTag;
                        List<SqlTagRegEx> tmpTagRegExs = tag.GetTagRegExs();
                        includeTag = CheckTagGroup(tmpTagRegExs);


                        if (includeTag == TagResult.Fail || includeTag == TagResult.FailNoCount)
                        {
                            //tag fails, no match.
                            pass = includeTag;
                            break; //if the first tag does not qualify, then do not proceed to the next tag.
                        }
                        CF.CurrentDoc.DocumentTags.Add(tag.TagText);
                    }

                    if (pass == TagResult.Pass)
                    {
                        if (relType.Contains(cp.CheckPointType))
                        {
                                CF.RelevantCP.Add(cp);
                        }
                        else
                        {
                            if (ns.ICD10SegmentID != 36) CF.PassedCP.Add(cp); //do not include passed for All diagnosis.
                        }

                    }
                    else
                    {
                        if (relType.Contains(cp.CheckPointType) || pass == TagResult.FailNoCount)
                        {
                            if (ns.ICD10SegmentID != 36) CF.IrrelaventCP.Add(cp); //do not include irrelevant for All diagnosis.
                        }
                        else
                        {
                            CF.FailedCP.Add(cp);
                        }
                    }
                }
                lbFail.ItemsSource = null;
                lbPassed.ItemsSource = null;
                lbIrrelavant.ItemsSource = null;
                lbRelavant.ItemsSource = null;

                lbFail.ItemsSource = CF.FailedCP;
                lbPassed.ItemsSource = CF.PassedCP;
                lbIrrelavant.ItemsSource = CF.IrrelaventCP;
                lbRelavant.ItemsSource = CF.RelevantCP;

            }



            //Generate Report

            Console.WriteLine($"Document report:");
            foreach (string strTag in CF.CurrentDoc.DocumentTags)
            {
                Console.WriteLine($"\tTag: {strTag}");
            }

            Console.WriteLine($"Passed Checkpoints");
            foreach (SqlCheckpoint cp in CF.PassedCP)
            {
                Console.WriteLine($"\t{cp.CheckPointTitle}");
            }

            Console.WriteLine($"Failed Checkpoints");
            foreach (SqlCheckpoint cp in CF.FailedCP)
            {
                Console.WriteLine($"\t{cp.CheckPointTitle}");
            }

            Console.WriteLine($"Relevant Checkpoints");
            foreach (SqlCheckpoint cp in CF.RelevantCP)
            {
                Console.WriteLine($"\t{cp.CheckPointTitle}");
            }

            Console.WriteLine($"Irrelavant Checkpoints");
            foreach (SqlCheckpoint cp in CF.IrrelaventCP)
            {
                Console.WriteLine($"\t{cp.CheckPointTitle}");
            }

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
            lbFail.ItemsSource = null;
            lbPassed.ItemsSource = null;
            lbIrrelavant.ItemsSource = null;
            lbRelavant.ItemsSource = null;

            GetHashTags();

            lbFail.ItemsSource = CF.FailedCP;
            lbPassed.ItemsSource = CF.PassedCP;
            lbIrrelavant.ItemsSource = CF.IrrelaventCP;
            lbRelavant.ItemsSource = CF.RelevantCP;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (lbSegmentsCheck != null)
            {
                CheckBox cb = sender as CheckBox;
                SqlICD10Segment seg = cb.DataContext as SqlICD10Segment;
                   if (!CF.RelevantICD10Segments.Contains(seg)) CF.RelevantICD10Segments.Add(seg);
                GenerateReport();

            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (lbSegmentsCheck != null)
            {
                CheckBox cb = sender as CheckBox;
                SqlICD10Segment seg = cb.DataContext as SqlICD10Segment;
                if (CF.RelevantICD10Segments.Contains(seg)) CF.RelevantICD10Segments.Remove(seg);
                GenerateReport();

            }

        }
    }
}
