using Dapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AI_Note_Review
{
    class MyConverters
    {
    }

    public class ArrayMultiValueConverter : IMultiValueConverter
    {
        #region interface implementations

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Clone();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
    public class EnumDescriptionConverter : IValueConverter
    {
        private string GetEnumDescription(Enum enumObj)
        {
            FieldInfo fieldInfo = enumObj.GetType().GetField(enumObj.ToString());

            object[] attribArray = fieldInfo.GetCustomAttributes(false);

            if (attribArray.Length == 0)
            {
                return enumObj.ToString();
            }
            else
            {
                DescriptionAttribute attrib = attribArray[0] as DescriptionAttribute;
                return attrib.Description;
            }
        }

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Enum myEnum = (Enum)value;
            string description = GetEnumDescription(myEnum);
            return description;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }

        public class SqlTagRegExToXamlConverter : IMultiValueConverter
        {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
            if (values == null) return null;
            string s = (string)values[0];
            SqlTagRegExM.EnumMatch rType = (SqlTagRegExM.EnumMatch)values[1];
            string strRegex = (string)values[2];
            
            string input = s;
            Brush HighlightColor = Brushes.Yellow;
            switch (rType)
            {
                case SqlTagRegExM.EnumMatch.Any:
                    HighlightColor = Brushes.Yellow;
                    break;
                case SqlTagRegExM.EnumMatch.All:
                    HighlightColor = Brushes.Green;
                    break;
                case SqlTagRegExM.EnumMatch.None:
                    HighlightColor = Brushes.Red;
                    break;
                case SqlTagRegExM.EnumMatch.Ask:
                    HighlightColor = Brushes.Yellow;
                    break;
                default:
                    break;
            }
            if (input != null)
            {
                var textBlock = new TextBlock();
                textBlock.TextWrapping = TextWrapping.Wrap;
                string strSearchTerms = strRegex;
                if (rType == SqlTagRegExM.EnumMatch.Ask)
                {
                    strSearchTerms = strSearchTerms.Split('|')[0];
                }
                string escapedXml = input; // SecurityElement.Escape(input);
                foreach (string strSearch in strSearchTerms.Split(','))
                {
                    List<string> replacedStr = new List<string>();
                    // I took this out
                    //                    foreach (Match m in Regex.Matches(input, CF.strRegexPrefix + strSearch.Trim(), RegexOptions.IgnoreCase))
                    if (strSearch.Trim() != "")
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
                    { FontWeight = FontWeights.Bold, Background = HighlightColor, Foreground = Brushes.Black });
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

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CheckpointsFromType : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values == null)
                return null;
            if (values[0] == DependencyProperty.UnsetValue)
                return null;
            try
            {
                var tmpList = (IEnumerable<SqlCheckpointVM>)values[0];
                int rType = (int)values[1];
                return (from c in tmpList where c.CheckPointType == rType select c).ToList();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    [ValueConversion(typeof(SqlTagRegExVM), typeof(string))]
    public class SqlTagRegExToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SqlTagRegExVM s = value as SqlTagRegExVM;
            string str = s.TargetSectionText;
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
            var ns = (from c in SqlNoteSection.NoteSections where c.SectionID == (int)value select c).FirstOrDefault();
            return $" ({ns.NoteSectionShortTitle})";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    [ValueConversion(typeof(string), typeof(Visibility))]
    public class CPCommentToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;

            if ((string)value == "")
                return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }


    [ValueConversion(typeof(SqlCheckPointImage), typeof(BitmapImage))]
    public class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SqlCheckPointImageVM si = value as SqlCheckPointImageVM;
            if (si == null) return null;
            using (var memoryStream = new System.IO.MemoryStream(si.ImageData))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = memoryStream;
                image.EndInit();
                return image;
            }
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
            foreach (SqlICD10SegmentVM ns in SqlICD10SegmentVM.NoteICD10Segments)
            {
                if (charChapter == ns.SqlICD10Segment.icd10Chapter)
                {
                    if ((ns.SqlICD10Segment.icd10CategoryStart >= CodeStart) && (ns.SqlICD10Segment.icd10CategoryEnd <= CodeEnd))
                    {
                        if (ns.SqlICD10Segment.ICD10SegmentID == (int)value)
                        {
                            return new Thickness(5, 0, 0, 0);
                        }
                    }
                    CodeStart = ns.SqlICD10Segment.icd10CategoryStart;
                    CodeEnd = ns.SqlICD10Segment.icd10CategoryEnd;
                    charChapter = ns.SqlICD10Segment.icd10Chapter;
                }
                else
                {
                    charChapter = ns.SqlICD10Segment.icd10Chapter;
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

    [ValueConversion(typeof(int), typeof(Brush))]
    public class ChartCountToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value == null)
                return Brushes.White;
            if ((int)value == 0) return Brushes.Red;
            if ((int)value >= 10) return Brushes.Green;
            if ((int)value >= 1) return Brushes.Yellow;
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
            if (strTest.Contains("(Primary)"))
            {
                strTest = strTest.Split('(')[0].Trim();
            }
            List<string> _ICD10Segments = new List<string>();
            if (strTest.StartsWith("Z"))
                return _ICD10Segments;
            char strAlphaCode = char.Parse(strTest.Substring(0, 1));
            string str = "";
            foreach (char ch in strTest)
            {
                if (Char.IsDigit(ch)) str += ch;
                if (ch == '.') str += ch; //preserve decimal
                if (Char.ToLower(ch) == 'x') break; //if placeholder character, then stop.
            }
            double icd10numeric = double.Parse(str);
            foreach (SqlICD10SegmentVM ns in SqlICD10SegmentVM.NoteICD10Segments)
            {
                if (strAlphaCode == ns.SqlICD10Segment.icd10Chapter)
                {
                    if (icd10numeric >= ns.SqlICD10Segment.icd10CategoryStart && icd10numeric <= ns.SqlICD10Segment.icd10CategoryEnd) _ICD10Segments.Add(ns.SqlICD10Segment.SegmentTitle);
                }
            }
            if (_ICD10Segments.Count == 0)
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
