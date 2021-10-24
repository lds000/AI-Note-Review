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
    /// Interaction logic for WinEditSegment.xaml
    /// </summary>
    public partial class WinEditSegment : Window
    {
        public WinEditSegment()
        {
            InitializeComponent();
        }

        private SqlICD10SegmentVM parentSeg;
        public WinEditSegment(SqlICD10SegmentVM seg)
        {
            InitializeComponent();
            parentSeg = seg;
            DataContext = seg;
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                parentSeg.SegmentTitle = tbTitle.Text;
                parentSeg.icd10Chapter = tbChapter.Text;
                parentSeg.icd10CategoryStart = double.Parse(tbStart.Text);
                parentSeg.icd10CategoryEnd = double.Parse(tbEnd.Text);
                parentSeg.SegmentComment = tbComment.Text;
                parentSeg.SaveToDB();
                this.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("There was an error, fix the data.");
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
