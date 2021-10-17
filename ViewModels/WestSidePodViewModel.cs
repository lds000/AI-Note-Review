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
    public class WestSidePodViewModel
    {
        private WestSidePod westSidePod;

        public WestSidePod WestSidePod
        {
            get { return westSidePod; }
        }

    }
}
