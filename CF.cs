using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace AI_Note_Review
{

    public static class GenericMethods
    {
        public static void CreateNewOrUpdateExisting<TKey, TValue>(this IDictionary<TKey, TValue> map, TKey key, TValue value)
        {
            map[key] = value;
        }
    }

    public class CF : INotifyPropertyChanged
    {
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        

        /// <summary>
        /// This prefix is used to match whole words and not parts of words
        /// </summary>
        //public static string strRegexPrefix = @"[ \-,.;\n\r\s^]";
        public static string strRegexPrefix = @"\b";
        public static PatientM CurrentPatient;
        public static DocumentM ClinicNote;
        public static ReportM NoteReview;

        public static List<SqlCheckpoint> CheckPointList
        {
            get
            {
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    string sql = "Select * from CheckPoints;";
                    return cnn.Query<SqlCheckpoint>(sql).ToList();
                }
            }
        }
        public static bool IsReviewWindowOpen = false;
        public static void SetWindowPosition(Window _Window)
        {
            WindowPosition wp = SqlLiteDataAccess.GetWindowPosition(_Window.Title);
            if (wp == null)
            {
                wp = new WindowPosition(_Window.Title, (int)_Window.Top, (int)_Window.Left);
                wp.SaveToDB();
                return;
            }

            double dblTop = wp.WindowPositionTop;
            double dblLeft = wp.WindowPositionLeft;

            //if (dblTop < 0) dblTop = 0; //do this if you don't have two screens.
            //if (dblLeft < 0) dblLeft = 0;

            if (!IsOnScreen((int)dblLeft, (int)dblTop))
            {
                dblTop = 0;
                dblLeft = 0;
            }
            _Window.Top = dblTop;
            _Window.Left = dblLeft;
        }

        public static bool IsOnScreen(int iLeft, int iTop)
        {
            Screen[] screens = Screen.AllScreens;
            foreach (Screen screen in screens)
            {
                System.Drawing.Point formTopLeft = new System.Drawing.Point(iLeft, iTop);

                if (screen.WorkingArea.Contains(formTopLeft))
                {
                    return true;
                }
            }

            return false;
        }
        public static void SaveWindowPosition(Window _Window)
        {
            WindowPosition wp = SqlLiteDataAccess.GetWindowPosition(_Window.Title);
            if (wp == null)
            {
                return;
            }

            double dblTop = wp.WindowPositionTop;
            double dblLeft = wp.WindowPositionLeft;

            if ((_Window.Top != dblTop) || (_Window.Left != dblLeft))
            {
                wp.WindowPositionTop = (int)_Window.Top;
                wp.WindowPositionLeft = (int)_Window.Left;
                wp.SaveToDB();
            }
        }
    }
}
