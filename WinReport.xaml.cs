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
    /// Interaction logic for WinReport.xaml
    /// </summary>
    public partial class WinReport : Window
    {
        public WinReport()
        {
            InitializeComponent();
            lbFail.ItemsSource = CF.FailedCP;
            lbPassed.ItemsSource = CF.PassedCP;
            lbIrrelavant.ItemsSource = CF.IrrelaventCP;
            lbRelavant.ItemsSource = CF.RelevantCP;
        }
    }
}
