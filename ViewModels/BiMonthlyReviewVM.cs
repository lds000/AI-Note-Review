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
    public class BiMonthlyReviewVM : INotifyPropertyChanged
    {
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        private BiMonthlyReviewM biMonthlyReviewModel;
        private SqlProvider sqlProvider;

        public BiMonthlyReviewM BiMonthlyReviewModel
        {
            get {
                return biMonthlyReviewModel;
            }
        }

        public BiMonthlyReviewVM()
        {
            biMonthlyReviewModel = new BiMonthlyReviewM();
        }


        public SqlProvider SelectedProviderForBiMonthlyReview
        {
            get
            {
                return sqlProvider;
            }
            set
            {
                sqlProvider = value;
            }
        }


    }
}
