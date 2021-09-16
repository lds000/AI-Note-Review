using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace AI_Note_Review
{
    class MyConverters
    {
    }

    class SqlTagRegExToXamlConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SqlTagRegEx s = value as SqlTagRegEx;
            string input = CF.CurrentDoc.NoteSectionText[s.TargetSection];
            if (input != null)
            {
                var textBlock = new TextBlock();
                textBlock.TextWrapping = TextWrapping.Wrap;
                string escapedXml = SecurityElement.Escape(input);
                foreach (string strSearch in s.RegExText.Split(','))
                {
                    List<string> replacedStr = new List<string>();

                    if (input != null)
                        foreach (Match m in Regex.Matches(input, CF.strRegexPrefix + strSearch.Trim(), RegexOptions.IgnoreCase))
                        {
                            if (!replacedStr.Contains(m.Value)) //do not replace same value more than once!
                            escapedXml = escapedXml.Replace(m.Value, $"|~s~|{m.Value}|~e~|");
                            replacedStr.Add(m.Value);
                        }

                    //todo: replace without case sensitive search
                }
                while (escapedXml.IndexOf("|~s~|") != -1)
                {
                    //up to |~S~| is normal
                    textBlock.Inlines.Add(new Run(escapedXml.Substring(0, escapedXml.IndexOf("|~s~|"))));
                    //between |~S~| and |~E~| is highlighted
                    textBlock.Inlines.Add(new Run(escapedXml.Substring(escapedXml.IndexOf("|~s~|") + 5,
                                              escapedXml.IndexOf("|~e~|") - (escapedXml.IndexOf("|~s~|") + 5)))
                    { FontWeight = FontWeights.Bold, Background = Brushes.Yellow, Foreground = Brushes.Black });
                    //the rest of the string (after the |~E~|)
                    escapedXml = escapedXml.Substring(escapedXml.IndexOf("|~e~|") + 5);
                }

                if (escapedXml.Length > 0)
                {
                    textBlock.Inlines.Add(new Run(escapedXml));
                }
                textBlock.Background = Brushes.Black;
                textBlock.Foreground = Brushes.White;
                return textBlock;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("This converter cannot be used in two-way binding.");
        }

    }

    [ValueConversion(typeof(SqlTagRegEx), typeof(string))]
    public class SqlTagRegExToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SqlTagRegEx s = value as SqlTagRegEx;
            string str = CF.CurrentDoc.NoteSectionText[s.TargetSection];
            return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }


    public class BooleanToVisibilityConverter : IValueConverter
    {

        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            if (targetType == typeof(Visibility))
            {
                var visible = System.Convert.ToBoolean(value, culture);
                if (InvertVisibility)
                    visible = !visible;
                return visible ? Visibility.Visible : Visibility.Collapsed;
            }
            throw new InvalidOperationException("Converter can only convert to value of type Visibility.");
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("Converter cannot convert back.");
        }

        public Boolean InvertVisibility { get; set; }

    }

    [ValueConversion(typeof(int), typeof(String))]
    public class MultiplyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "N/A";
            var ns = (from c in CF.NoteSections where c.SectionID == (int)value select c).FirstOrDefault();
            return $" ({ns.NoteSectionShortTitle})";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    [ValueConversion(typeof(int), typeof(Thickness))]
    public class ICD10Margin : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            char charChapter = 'A';
            double CodeStart = 0;
            double CodeEnd = 0;
            foreach (SqlICD10Segment ns in CF.NoteICD10Segments)
            {
                if (charChapter == char.Parse(ns.icd10Chapter))
                {
                    if ((ns.icd10CategoryStart >= CodeStart) && (ns.icd10CategoryEnd <= CodeEnd))
                    {
                        if (ns.ICD10SegmentID == (int)value)
                        {
                            return new Thickness(5, 0, 0, 0);
                        }
                    }
                    CodeStart = ns.icd10CategoryStart;
                    CodeEnd = ns.icd10CategoryEnd;
                    charChapter = char.Parse(ns.icd10Chapter);
                }
                else
                {
                    charChapter = char.Parse(ns.icd10Chapter);
                    CodeStart = 0;
                    CodeEnd = 0;
                }
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    [ValueConversion(typeof(int), typeof(Brush))]
    public class SeverityToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            
            if (value == null)
                return Brushes.White;
            if ((int)value >= 7) return Brushes.Red;
            if ((int)value >= 4) return Brushes.Yellow;
            if ((int)value >= 0) return Brushes.Green;
            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    [ValueConversion(typeof(string), typeof(List<String>))]
    public class ICD10Segments : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string strTest = value as string;
            List<string> _ICD10Segments = new List<string>();
            string strAlphaCode = strTest.Substring(0, 1);
            string str = "";
            foreach (char ch in strTest)
            {
                if (Char.IsDigit(ch)) str += ch;
                if (ch == '.') str += ch; //preserve decimal
                if (Char.ToLower(ch) == 'x') break; //if placeholder character, then stop.
            }
            double icd10numeric = double.Parse(str);
            foreach (SqlICD10Segment ns in CF.NoteICD10Segments)
            {
                if (strAlphaCode == ns.icd10Chapter)
                {
                    if (icd10numeric >= ns.icd10CategoryStart && icd10numeric <= ns.icd10CategoryEnd) _ICD10Segments.Add(ns.SegmentTitle);
                }
            }
            if (_ICD10Segments.Count == 0 )
            {
                string sql = "";
                sql = $"INSERT INTO MissingICD10Codes (StrCode) VALUES ('{strTest}');";
                using (IDbConnection cnn = new SQLiteConnection("Data Source=" + SqlLiteDataAccess.SQLiteDBLocation))
                {
                    cnn.Execute(sql);
                }
                Console.WriteLine($"ICD10 Code with no found segment: {strTest}");

            }
            return _ICD10Segments;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

}
