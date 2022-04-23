using Dapper;
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
    public class ProviderM
    {

        public int ProviderID
        {
            get; set;
        }
        public string FirstName
        {
            get;
            set;
        }
        public string
LastName
        {
            get; set;
        }

        public string Cert
        {
            get; set;
        }
        public string
HomeClinic
        {
            get; set;
        }
        public int
ReviewInterval
        {
            get;
            set;
        }
        public string
EMail
        {
            get;
            set;
        }
        public string
FullName
        {
            get;
            set;
        }
        public bool
IsWestSidePod
        {
            get;
            set;
        }
        public string

PersonalNotes
        {
            get;
            set;
        }

        public string Bio
        {
            get;
            set;
        }

    }
}