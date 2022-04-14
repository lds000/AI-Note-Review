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
    /// Interaction logic for WinChooseSegment.xaml
    /// </summary>
    public partial class WinChooseSegment : Window
    {
        public WinChooseSegment()
        {
            InitializeComponent();
        }
        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public SqlICD10SegmentVM SelectedICD10Segment;
        private void lbICD10_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbICD10.SelectedValue != null)
            {
                SelectedICD10Segment = lbICD10.SelectedValue as SqlICD10SegmentVM;
                this.Close();
            }
        }
    }
}
