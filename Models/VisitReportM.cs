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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace AI_Note_Review
{
    /// 
    /// Code behind for report xaml
    /// </summary>
    public class VisitReportM : INotifyPropertyChanged
    {

        #region inotify

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.  
        // The CallerMemberName attribute that is applied to the optional propertyName  
        // parameter causes the property name of the caller to be substituted as an argument.  
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            //Console.WriteLine($"iNotify property {propertyName}");
        }

        #endregion
        private DocumentM document;
        public VisitReportM(DocumentM tmpDoc)
        {
            document = tmpDoc;
        }

        public VisitReportM()
        {

        }

        public DateTime ReviewDate
        {
            get
            {
                return reviewDate;
            }
            set
            {
                reviewDate = value;
            }
        }

        private DateTime reviewDate;

        #region DocumentReport

        /// <summary>
        /// int CheckpointID, int CPstatus
        /// </summary>
        public Dictionary<SqlCheckpointVM, SqlRelCPProvider.MyCheckPointStates> CPStatusOverrides { get { return cPStatusOverrides; } set { cPStatusOverrides = value; } }
        public ObservableCollection<string> DocumentTags { get { return documentTags; } set { documentTags = value; } }
        #endregion

        private ObservableCollection<string> documentTags = new ObservableCollection<string>();
        private Dictionary<SqlCheckpointVM, SqlRelCPProvider.MyCheckPointStates> cPStatusOverrides = new Dictionary<SqlCheckpointVM, SqlRelCPProvider.MyCheckPointStates>();

    }
}
