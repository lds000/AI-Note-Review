using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace AI_Note_Review
{

    static class GenericMethods
    {
        public static void CreateNewOrUpdateExisting<TKey, TValue>(
this IDictionary<TKey, TValue> map, TKey key, TValue value)
        {
            map[key] = value;
        }

    }

    class CF
    {

        public static string strRegexPrefix = @"[ \-,.;\n\r\s]";
        public static List<SqlNoteSection> NoteSections { get; set; }
        public static List<SqlTagRegExType> TagRegExTypes { get; set; }
        public static List<SqlICD10Segment> NoteICD10Segments = new List<SqlICD10Segment>();
        public static DocInfo CurrentDoc = new DocInfo();
        public static List<SqlCheckpoint> CheckPointList = new List<SqlCheckpoint>();

        public static void UpdateNoteICD10Segments()
        {
            using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
            {
                string sql = "Select * from ICD10Segments order by icd10Chapter, icd10CategoryStart;";
                NoteICD10Segments = cnn.Query<SqlICD10Segment>(sql).ToList();
            }
            char charChapter = 'A';
            double CodeStart = 0;
            double CodeEnd = 0;
            foreach (SqlICD10Segment ns in CF.NoteICD10Segments)
            {
                ns.icd10Margin = new Thickness(0);
                if (charChapter == char.Parse(ns.icd10Chapter))
                {
                    if ((ns.icd10CategoryStart >= CodeStart) && (ns.icd10CategoryEnd <= CodeEnd))
                    {
                        ns.icd10Margin = new Thickness(10, 0, 0, 0);
                    }
                    else
                    {
                        CodeStart = ns.icd10CategoryStart;
                        CodeEnd = ns.icd10CategoryEnd;
                        charChapter = char.Parse(ns.icd10Chapter);
                    }
                }
                else
                {
                    charChapter = char.Parse(ns.icd10Chapter);
                    CodeStart = 0;
                    CodeEnd = 0;
                }
                if (ns.ICD10SegmentID == 90)
                {
                    ns.IncludeSegment = false;
                }
            }
        }
        

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

            if (!IsOnScreen((int)dblLeft, (int) dblTop))
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
