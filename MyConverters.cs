using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace AI_Note_Review
{
    class MyConverters
    {
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
            return _ICD10Segments;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

}
