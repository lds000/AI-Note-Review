using Dapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace AI_Note_Review
{

    public static class MyEnumerable
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
        {
            var result = new ObservableCollection<T>();
            foreach (var item in source)
                result.Add(item);
            return result;
        }
    }
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
        public static VisitReportM NoteReview;

        public static List<SqlCheckpointM> CheckPointList
        {
            get
            {
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    string sql = "Select * from CheckPoints;";
                    return cnn.Query<SqlCheckpointM>(sql).ToList();
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

    public static class ExtensionMethods
    {
        public static string TrimEnd(this string input, string suffixToRemove, StringComparison comparisonType = StringComparison.CurrentCulture)
        {
            if (suffixToRemove != null && input.EndsWith(suffixToRemove, comparisonType))
            {
                return input.Substring(0, input.Length - suffixToRemove.Length);
            }

            return input;
        }

        public static bool RegexContains(this string input, string strRegEx)
        {
            RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace;
            return Regex.IsMatch(input, strRegEx, options);
        }



        public static string ParseHistory(this string strInput)
        {
            char[] delimiterChars = { ' ', ',', '.', ':', '\t' };

            string strHPI = strInput;
            strHPI = strHPI.Replace("has not had", "hasnothad");
            strHPI = strHPI.Replace("has had", "hashad");
            strHPI = strHPI.Replace("not have", "nothave");

            string[] strNegMarker = { "denies", "no history of", "no hx of", "does not have", "didn't have", "nothave", "hasnothad", "reports", "admits" };
            string[] strPosMarker = { "complain", "c/o", "endorses", "indicates", "hashad", "presents", "presented", "presenting", "has been", "states", "does have" };
            string[] words = strHPI.Split(delimiterChars);
            System.Console.WriteLine($"{words.Length} words in text:");
            string strCompare = "";
            Queue<string> strStack = new Queue<string>();
            string strPosNeg = "Intro";
            string strLastCompareResult = "";
            string strResult = "";
            foreach (var word in words)
            {
                if (word == "") continue;
                strCompare += " " + word.ToLower();
                strStack.Enqueue(word);

                bool strContainsPos = false;
                foreach (string str in strPosMarker)
                {
                    if (strCompare.Contains(str))
                    {
                        strContainsPos = true;
                        strResult = str;
                        break;
                    }
                }
                if (strContainsPos)
                {
                    string strOutput = "";
                    while (strStack.Count > 0)
                    {
                        strOutput += strStack.Dequeue() + " ";
                    }
                    strOutput = strOutput.Trim();
                    strOutput = strOutput.Replace(strResult, "");
                    System.Console.WriteLine($"{strPosNeg}- <{strLastCompareResult}>  {strOutput}");
                    strLastCompareResult = strResult;
                    strPosNeg = "Pos";
                    strCompare = "";
                }

                bool strContainsNeg = false;
                foreach (string str in strNegMarker)
                {
                    if (strCompare.Contains(str))
                    {
                        strContainsNeg = true;
                        strResult = str;
                        break;
                    }
                }
                if (strContainsNeg)
                {
                    string strOutput = "";
                    while (strStack.Count > 0)
                    {
                        strOutput += strStack.Dequeue() + " ";
                    }
                    strOutput = strOutput.Trim();
                    strOutput = strOutput.Replace(strResult, "");
                    System.Console.WriteLine($"{strPosNeg}- <{strLastCompareResult}>  {strOutput}");
                    strLastCompareResult = strResult;
                    strPosNeg = "Neg";
                    strCompare = "";
                }
            }
            string strOutputFinal = "";
            while (strStack.Count > 0)
            {
                strOutputFinal += strStack.Dequeue() + " ";
            }
            strOutputFinal = strOutputFinal.Trim();
            if (strResult != "")
                strOutputFinal = strOutputFinal.Replace(strResult, "");
            System.Console.WriteLine($"{strPosNeg}- <{strLastCompareResult}>  {strOutputFinal}");

            return strHPI;
        }
    }

    public class DateTimeHandler : SqlMapper.TypeHandler<DateTimeOffset>
    {
        private readonly TimeZoneInfo databaseTimeZone = TimeZoneInfo.Local;
        public static readonly DateTimeHandler Default = new DateTimeHandler();

        public DateTimeHandler()
        {

        }

        public override DateTimeOffset Parse(object value)
        {
            DateTime storedDateTime;
            if (value == null)
                storedDateTime = DateTime.MinValue;
            else
                storedDateTime = (DateTime)value;

            if (storedDateTime.ToUniversalTime() <= DateTimeOffset.MinValue.UtcDateTime)
                return DateTimeOffset.MinValue;
            else
                return new DateTimeOffset(storedDateTime, databaseTimeZone.BaseUtcOffset);
        }

        public override void SetValue(IDbDataParameter parameter, DateTimeOffset value)
        {
            DateTime paramVal = value.ToOffset(this.databaseTimeZone.BaseUtcOffset).DateTime;
            parameter.Value = paramVal;
        }
    }
}
