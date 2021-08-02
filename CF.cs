using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AI_Note_Review
{
    class CF
    {
        public static List<SqlNoteSection> NoteSections { get; set; }
        public static List<SqlTagRegExType> TagRegExTypes { get; set; }
        public static List<SqlICD10Segment> NoteICD10Segments = new List<SqlICD10Segment>();
        public static List<SqlICD10Segment> RelevantICD10Segments = new List<SqlICD10Segment>();
        public static List<SqlCheckpoint> PassedCP = new List<SqlCheckpoint>();
        public static List<SqlCheckpoint> FailedCP = new List<SqlCheckpoint>();
        public static List<SqlCheckpoint> RelevantCP = new List<SqlCheckpoint>();
        public static List<SqlCheckpoint> IrrelaventCP = new List<SqlCheckpoint>();
        public static DocInfo CurrentDoc = new DocInfo();
        public static List<SqlCheckpoint> CheckPointList = new List<SqlCheckpoint>();


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

                _Window.Top = dblTop;
                _Window.Left = dblLeft;
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
