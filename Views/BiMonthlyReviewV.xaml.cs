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
    /// Interaction logic for WinProviderReviews.xaml
    /// </summary>
    public partial class BiMonthlyReviewV : Window
    {
        public BiMonthlyReviewV(BiMonthlyReviewVM bmrvm)
        {
            InitializeComponent();
            lbProviders.DataContext = bmrvm;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        #region window position save/recall functions
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CF.SetWindowPosition(this);
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CF.SaveWindowPosition(this);
        }
        #endregion
    }
}
