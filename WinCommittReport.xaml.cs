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
    /// Interaction logic for WinCommittReport.xaml
    /// </summary>
    public partial class WinCommittReport : Window
    {
        public WinCommittReport()
        {
            InitializeComponent();
            DataContext = CF.CurrentDoc;


        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonCommmit_Click(object sender, RoutedEventArgs e)
        {
            foreach (SqlCheckpoint cp in (from c in CF.CurrentDoc.FailedCheckPoints orderby c.ErrorSeverity descending select c))
            {
                if (cp.IncludeCheckpoint) cp.Commit(CF.CurrentDoc);
            }

            foreach (SqlCheckpoint cp in (from c in CF.CurrentDoc.RelevantCheckPoints orderby c.ErrorSeverity descending select c))
            {
                if (cp.IncludeCheckpoint) cp.Commit(CF.CurrentDoc);
            }

            foreach (SqlCheckpoint cp in (from c in CF.CurrentDoc.PassedCheckPoints orderby c.ErrorSeverity descending select c))
            {
                if (cp.IncludeCheckpoint) cp.Commit(CF.CurrentDoc);
            }
            foreach (SqlCheckpoint cp in (from c in CF.CurrentDoc.IrrelaventCP orderby c.ErrorSeverity descending select c))
            {
                if (cp.IncludeCheckpoint) cp.Commit(CF.CurrentDoc);
            }
            this.Close();
        }
    }
}
