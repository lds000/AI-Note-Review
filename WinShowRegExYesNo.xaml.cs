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
    /// Interaction logic for WinShowRegExYesNo.xaml
    /// </summary>
    public partial class WinShowRegExYesNo : Window
    {
        public bool YesNoResult = false;
        public WinShowRegExYesNo()
        {
            InitializeComponent();
        }

        private void clickYes(object sender, RoutedEventArgs e)
        {
            YesNoResult = true;
            this.Close();
        }
        private void clickNo(object sender, RoutedEventArgs e)
        {
            YesNoResult = true;
            this.Close();
        }
    }
}
