using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
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
            DataContext = CF.ClinicNote;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonCommmit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnGetIndexClick(object sender, RoutedEventArgs e)
        {
            //tackle this later.... I hope you survived to this point
            /*
            string sqlCheck = $"Select * from(Select distinct CheckPointID rel from RelCPPRovider where " +
                $"ReviewDate = '{CF.R.ReviewDate.ToString("yyyy-MM-dd")}') " +
                $"inner join CheckPoints cp on rel = cp.CheckPointID " +
                $"order by cp.TargetSection, cp.ErrorSeverity desc;";

            List<SqlCheckpoint> cplist = new List<SqlCheckpoint>();
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                cplist = cnn.Query<SqlCheckpoint>(sqlCheck).ToList();
            }

            string strOut = "Index for relevant checkpoints." + Environment.NewLine;
            foreach (SqlCheckpointViewModel cp in cplist)
            {
                strOut += cp.GetIndex();
            }
            Clipboard.SetText(strOut);
            */
        }

    }
}
