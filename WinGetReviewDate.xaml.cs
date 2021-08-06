using System;
using System.Collections.Generic;
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
    /// Interaction logic for WinGetReviewDate.xaml
    /// </summary>
    public partial class WinGetReviewDate : Window
    {
        public WinGetReviewDate()
        {
            InitializeComponent();
        }

        public DateTime SelectedDate { get; set; }
        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedDate = (DateTime)calDate.SelectedDate;
            Close();
        }
    }
}
