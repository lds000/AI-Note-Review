﻿using Dapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace AI_Note_Review
{
    public class BiMonthlyReviewVM : INotifyPropertyChanged
    {
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        private BiMonthlyReviewM biMonthlyReviewM;
        private MasterReviewSummaryVM biMonthlyReviews;

        public BiMonthlyReviewVM()
        {
            biMonthlyReviewM = new BiMonthlyReviewM();
            biMonthlyReviews = new MasterReviewSummaryVM();
            masterReviewSummaryVM = biMonthlyReviews.MasterReviewSummaryList;
            //todo: set selected reviewmodel to one with today's date.
            SelectedMasterReviewSummary = masterReviewSummaryVM.First();
        }


        public BiMonthlyReviewM BiMonthlyReviewM
        {
            get
            {
                return biMonthlyReviewM;
            }
        }

        public MasterReviewSummaryVM BiMonthlyReviews
        {
            get
            {
                return biMonthlyReviews;
            }
        }

        private ObservableCollection<MasterReviewSummaryVM> masterReviewSummaryVM;
        public ObservableCollection<MasterReviewSummaryVM> MasterReviewSummaryList
        {
            get
            {
                return masterReviewSummaryVM;
            }
        }

        private MasterReviewSummaryVM selectedMasterReviewSummary;
        public MasterReviewSummaryVM SelectedMasterReviewSummary
        {
            get
            {
                //if (selectedMasterReviewSummary == null) return new MasterReviewSummaryVM();
                return selectedMasterReviewSummary;
            }
            set
            {
                selectedMasterReviewSummary = value;
                OnPropertyChanged("MyPeeps");
                SelectedProviderForBiMonthlyReview = MyPeeps.First();
            }
        }



        private SqlProvider selectedProviderForBiMonthlyReview;
        public SqlProvider SelectedProviderForBiMonthlyReview
        {
            get
            {
                return selectedProviderForBiMonthlyReview;
            }
            set
            {
                selectedProviderForBiMonthlyReview = value;
                OnPropertyChanged("ListOfDocumentReviews");
                //when the provider is changed, set the selected document for review to the first item in the list.
                SelectedDocumentReview = ListOfDocumentReviews.FirstOrDefault();
                OnPropertyChanged("SelectedDocumentReview");
            }
        }

        /// <summary>
        /// Populates a class containing PtID and Dates for a time period (usually two months)
        /// </summary>
        public ObservableCollection<SqlDocumentReviewSummaryVM> ListOfDocumentReviews
        {
            get
            {
                if (selectedProviderForBiMonthlyReview == null) return null;
                if (SelectedMasterReviewSummary == null) return null;
                string sql = "";
                sql += $"Select distinct VisitDate, PtID from RelCPPRovider where ProviderID={selectedProviderForBiMonthlyReview.ProviderID} and VisitDate Between '{SelectedMasterReviewSummary.StartDate.ToString("yyyy-MM-dd")}' and '{SelectedMasterReviewSummary.EndDate.ToString("yyyy-MM-dd")}';";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    ObservableCollection<SqlDocumentReviewSummaryVM> tmpL = new ObservableCollection<SqlDocumentReviewSummaryVM>(cnn.Query<SqlDocumentReviewSummaryVM>(sql).ToList().OrderBy(c=>c.VisitDate));
                    foreach (var l in tmpL)
                    {
                        l.ParentProvider = selectedProviderForBiMonthlyReview;
                        ReportToTextVM r = new ReportToTextVM(selectedProviderForBiMonthlyReview, l.VisitDate, l.PtID);
                    }
                    return tmpL;
                }
            }
        }


        private SqlDocumentReviewSummaryVM selectedDocumentReview;
        public SqlDocumentReviewSummaryVM SelectedDocumentReview
        {
            get
            {
                return selectedDocumentReview;
            }
            set
            {
                selectedDocumentReview = value;
                OnPropertyChanged("ReviewHTML");
            }
        }

        public string ReviewHTML
        {
            get
            {
                if (selectedDocumentReview == null) return "Select a review";
                return selectedDocumentReview.ReviewHTML;
            }
        }



        public ObservableCollection<SqlProvider> MyPeeps
        {
            get
            {
                Console.WriteLine($"getting west side pod and assigning {SelectedMasterReviewSummary.MasterReviewSummaryTitle} to provider.");
                string sql = "";
                sql += $"Select * from Providers where IsWestSidePod == '1' order by FullName;"; //this part is to get the ID of the newly created phrase
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    var tmpPeeps = cnn.Query<SqlProvider>(sql).ToList();
                    foreach(var tmpPeep in tmpPeeps)
                    {
                        tmpPeep.ParentMasterReviewSummary = selectedMasterReviewSummary;
                    }
                    return tmpPeeps.ToObservableCollection();
                }
            }
        }

        public void SendReport()
        {
            var l = ListOfDocumentReviews;
            if (l.Count < 10)
            {
                MessageBox.Show("Not going to send until you get 10 reports...");
                return;
            }
            int sleeptime = 50;
            string strName = selectedProviderForBiMonthlyReview.FullName.Split(' ')[0];
            if (strName == "Crystalyn") strName = "Chrystalyn";
            if (strName == "Donald") strName = "Sanford";
            if (strName == "Joshua") strName = "Josh";
            if (strName == "Nickolas") strName = "Nick";
            if (selectedProviderForBiMonthlyReview.FullName.Contains("Denning")) strName = "Denning";
            if (selectedProviderForBiMonthlyReview.FullName.Contains("Shinkle")) strName = "Shinkle";
            if (selectedProviderForBiMonthlyReview.FullName.Contains("Barnum")) strName = "Barnum";
            AutoIt.AutoItX.WinActivate("Monthly Clinic Review");
            Thread.Sleep(sleeptime);
            AutoIt.AutoItX.Send(strName);
            AutoIt.AutoItX.Send("{Tab 2}");
            Thread.Sleep(sleeptime);
            AutoIt.AutoItX.Send("stolw");
            AutoIt.AutoItX.Send("{Tab 2}");
            Thread.Sleep(sleeptime);
            AutoIt.AutoItX.Send(DateTime.Now.ToString("MM-dd-yyyy"));
            AutoIt.AutoItX.Send("{Tab 2}");
            Thread.Sleep(sleeptime);
            for (int i = 0; i <= 9; i++)
            {
                ReportToTextVM r = new ReportToTextVM(l[i].ParentProvider, l[i].VisitDate, l[i].PtID);
                AutoIt.AutoItX.Send(r.AccountNumber);
                AutoIt.AutoItX.Send("{Tab}");
                Thread.Sleep(sleeptime);
                AutoIt.AutoItX.Send(r.DOS);
                AutoIt.AutoItX.Send("{Tab 2}");
                Thread.Sleep(sleeptime);
                AutoIt.AutoItX.Send(r.HPIScore);
                AutoIt.AutoItX.Send("{Tab}");
                Thread.Sleep(sleeptime);
                AutoIt.AutoItX.Send(r.ExamScore);
                AutoIt.AutoItX.Send("{Tab}");
                Thread.Sleep(sleeptime);
                AutoIt.AutoItX.Send(r.DxScore);
                AutoIt.AutoItX.Send("{Tab}");
                Thread.Sleep(sleeptime);
                AutoIt.AutoItX.Send(r.RxScore);
                AutoIt.AutoItX.Send("{Tab}");
                Thread.Sleep(sleeptime);
                AutoIt.AutoItX.ClipPut(r.Notes);
                Thread.Sleep(sleeptime);
                AutoIt.AutoItX.Send("^a");
                Thread.Sleep(sleeptime);
                AutoIt.AutoItX.Send("^v");
                Thread.Sleep(sleeptime);
                AutoIt.AutoItX.Send("{Tab}");
                Thread.Sleep(sleeptime);
            }
            AutoIt.AutoItX.Send("{Tab 6}");
            AutoIt.AutoItX.Send("{Down 3}");
            AutoIt.AutoItX.Send("{Enter}");
            AutoIt.AutoItX.Send("{Tab 2}");
        }

        public void SendOutlook()
        {
            var l = ListOfDocumentReviews;
            if (l.Count < 10)
            {
                MessageBox.Show("Not going to send until you get 10 reports...");
                return;
            }

            string strReport = ""; // @"<!DOCTYPE html><html><head></head><body>";
            double hpitot = 0;
            double dxtot = 0;
            double examtot = 0;
            double rxtot = 0;
            double totaltot = 0;

            string strTmp = "";
            for (int i = 0; i <= 9; i++)
            {
                strTmp += l[i].HtmlReport.CheckPointsSummaryHTML;
                //l[i].HtmlReport.calculateScores();
                hpitot += (double)l[i].HtmlReport.hPIScore;
                dxtot += (double)l[i].HtmlReport.dxScore;
                examtot += (double)l[i].HtmlReport.examScore;
                rxtot += (double)l[i].HtmlReport.rxScore;
                totaltot += (double)l[i].HtmlReport.totalScore;
            }
            strReport += $"Hi {selectedProviderForBiMonthlyReview.FirstName},<br> I hope things are going well for you.  You are receiving this mail from me because you have been assigned to the 'Westside' pod.  I call it that because likely your home clinic is west of Eagle road in Meridian. Dr. Hamblin asked me to review 10 (or more) charts for you every two months. I'm hoping to make this process as painless, straight forward, and helpful as possible. I have put a lot of effort writing a computer program that helped me analyze your notes.  It basically looks for specific ‘checkpoints’ that I have put together that are relevant to the ICD-10 code you assigned to your clinic note.  Every two months we will change the focus to specific ICD-10 codes.  For September and October we were asked to focus on general note concepts as outlined by Dr. Hamblin in his email introducing the new note review system. I also included general concepts such as BEERS criteria, ED transfer protocol, procedure note content, and documenting non-pregnant state when clinically relevant. I will send out a mailer in a few weeks regarding the topics for November and December.";
            strReport += $"<br>Generally this last review went very well for everyone. I have a few general comments regarding the notes I saw.  I reviewed over 300 notes, including many of my own. It seems that the providers who dictated notes produced better quality notes that those who relied heavily on templates or the tab presets.  I found quite a few inappropriate applications of templates to the nonspeaking infant/toddler age group and the elderly group. I also noticed quite a few Macrobid prescriptions being given to elderly when safer alternatives are available.  Finally, now that we are writing for more doxycycline for STIs I see notes not confirming absence of pregnant state by history or lab. We certainly don’t want permanent discoloration of teeth (yellow, gray, brown) following in utero exposure.";
            strReport += $"<br>The following is my note review, please don't hesitated giving feedback by clicking on the feedback link,";
            strReport += $"<br><br>Lloyd Stolworthy, M.D.<hr>";
            strReport += $"<font size='+2'>Total Score: HPI: {hpitot.ToString("0.##")}, Dx: {dxtot.ToString("0.##")}, Exam:  {examtot.ToString("0.##")}, Rx: {rxtot.ToString("0.##")}, Total Score: {totaltot.ToString("0.##")}</font><hr>";
            strReport += strTmp;
            strReport += "Footnotes:" + Environment.NewLine;
            strReport += "Total Score = (Total of Score Weights missed) / ((Total of Score Weights missed)+(Total of Score Weights passed)) * 2 + 8<br>" + Environment.NewLine;
            strReport += "Score Weight = An assigned weight of my estimated importance of the checkpoint." + Environment.NewLine;



            ClipboardHelper.CopyToClipboard(strReport, "");
            string mailto = string.Format("mailto:{0}?Subject={1}&Body={2}", selectedProviderForBiMonthlyReview.EMail, "Clinic Note Review For Sep-Oct 2021", "");
            mailto = Uri.EscapeUriString(mailto);
            System.Diagnostics.Process.Start(mailto);
        }

        public void SendExecutiveSummary()
        {
            ReportToHtmlVM r = new ReportToHtmlVM();
            ClipboardHelper.CopyToClipboard(r.ExecutiveSummary(SelectedMasterReviewSummary), "");
            string mailto = string.Format("mailto:{0}?Subject={1}&Body={2}", "lds00@yahoo.com", "Clinic Note Review For Sep-Oct 2021", "");
            mailto = Uri.EscapeUriString(mailto);
            System.Diagnostics.Process.Start(mailto);

        }

        private ICommand mShowBiMonthlyReport;
        public ICommand ShowBiMonthlyReport
        {
            #region Command Def
            get
            {
                if (mShowBiMonthlyReport == null)
                    mShowBiMonthlyReport = new ShowBiMonthlyReport();
                return mShowBiMonthlyReport;
            }
            set
            {
                mShowBiMonthlyReport = value;
            }
            #endregion
        }

        private ICommand mSendReport;
        public ICommand SendReportCommand
        {
            #region Command Def
            get
            {
                if (mSendReport == null)
                    mSendReport = new SendReport();
                return mSendReport;
            }
            set
            {
                mSendReport = value;
            }
            #endregion
        }

        private ICommand mSendOutlook;
        public ICommand SendOutlookCommand
        {
            #region Command Def
            get
            {
                if (mSendOutlook == null)
                    mSendOutlook = new SendOutlook();
                return mSendOutlook;
            }
            set
            {
                mSendOutlook = value;
            }
            #endregion
        }

        private ICommand mSendExecutiveSummary;
        public ICommand SendExecutiveSummaryCommand
        {
            #region Command Def
            get
            {
                if (mSendExecutiveSummary == null)
                    mSendExecutiveSummary = new SendExecutiveSummary();
                return mSendExecutiveSummary;
            }
            set
            {
                mSendExecutiveSummary = value;
            }
            #endregion
        }
    }

    class ShowBiMonthlyReport : ICommand
    {
        #region ICommand Members  

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        #endregion

        public void Execute(object parameter)
        {
            BiMonthlyReviewVM rvm = parameter as BiMonthlyReviewVM;
            BiMonthlyReviewV wp = new BiMonthlyReviewV(rvm);
            wp.ShowDialog();
        }
    }

    class SendReport : ICommand
    {
        #region ICommand Members  

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        #endregion

        public void Execute(object parameter)
        {
            BiMonthlyReviewVM rvm = parameter as BiMonthlyReviewVM;
            rvm.SendReport();
        }
    }

    class SendOutlook : ICommand
    {
        #region ICommand Members  

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        #endregion

        public void Execute(object parameter)
        {
            BiMonthlyReviewVM rvm = parameter as BiMonthlyReviewVM;
            rvm.SendOutlook();
        }
    }

        class SendExecutiveSummary : ICommand
        {
            #region ICommand Members  

            public bool CanExecute(object parameter)
            {
                return true;
            }
            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }
            #endregion

            public void Execute(object parameter)
            {
                BiMonthlyReviewVM rvm = parameter as BiMonthlyReviewVM;
                rvm.SendExecutiveSummary();
            }
        }

    }
