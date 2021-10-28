using System;
using System.Collections.Generic;
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
    /// Interaction logic for WinShowCheckPointRichText.xaml
    /// </summary>
    public partial class WinShowCheckPointRichText : Window
    {
        public event EventHandler ImChanged;

        public WinShowCheckPointRichText()
        {
            InitializeComponent();
        }

        private void Button_Click_Done(object sender, RoutedEventArgs e)
        {
            UpDownPressed = false;
            Close();
        }

        public bool UpDownPressed = true;
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                UpDownPressed = true;
                this.Close();
            }
            if (e.Key == Key.Up)
            {
                UpDownPressed = true;
                this.Close();
            }
            if (e.Key == Key.Return)
            {
                UpDownPressed = false;
                this.Close();

            }
            if (e.Key == Key.Escape)
            {
                UpDownPressed = false;
                this.Close();

            }
        }

        private void PressMe_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void PressMe_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
         //   UpDownPressed = false;
        }
    }


}
