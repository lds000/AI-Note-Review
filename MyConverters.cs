using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

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
}
