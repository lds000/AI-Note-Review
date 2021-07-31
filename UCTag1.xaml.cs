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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AI_Note_Review
{
    /// <summary>
    /// Interaction logic for UCTag1.xaml
    /// </summary>
    public partial class UCTag1 : UserControl
    {
        public event EventHandler AddMe;
        public UCTag1()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WinEnterText wet = new WinEnterText();

            Button b = sender as Button;
            SqlCheckpoint cp = DataContext as SqlCheckpoint;
            SqlTag st = b.DataContext as SqlTag;
            SqlTagRegEx srex = new SqlTagRegEx(st.TagID, "Search Text", cp.TargetSection, 1);
            AddMe(this, EventArgs.Empty);
        }

        private void UCTagRegEx_DeleteMe(object sender, EventArgs e)
        {
            AddMe(this, EventArgs.Empty);
        }

        private void btnRemoveTag_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            SqlCheckpoint cp = DataContext as SqlCheckpoint;
            SqlTag st = b.DataContext as SqlTag;
            cp.RemoveTag(st);
            AddMe(this, EventArgs.Empty);

        }
    }
}
