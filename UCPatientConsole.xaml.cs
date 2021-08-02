using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AI_Note_Review
{
    /// <summary>
    /// Interaction logic for UCPatientConsole.xaml
    /// </summary>
    public partial class UC_PatientConsole : UserControl
    {
        public UC_PatientConsole()
        {
            InitializeComponent();
        }
    }

    public class HashTagConverter : MarkupExtension
    {
        public HashTagConverter()
        {
        }

        public HashTagConverter(string textToTranslate)
        {
            TextToTranslate = textToTranslate;
        }

        public string TextToTranslate { get; set; }

        public IValueConverter Converter { get; set; }

        public object ConverterParameter { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {

            WrapPanel wp = new WrapPanel();
            if (TextToTranslate == null) return wp;
            wp.Orientation = Orientation.Horizontal;
            string strVal = TextToTranslate.ToString().Trim().TrimEnd(',').Trim();
            foreach (string str in strVal.Split(','))
            {
                Label lb = new Label();
                lb.Content = str;
                lb.Foreground = Brushes.White;
                if (str.Contains('!')) lb.Foreground = Brushes.Red;
                wp.Children.Add(lb);
            }
            return wp;
        }
        

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
