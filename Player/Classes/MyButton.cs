using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Player.Classes
{
    class MyButton :Button
    {
                
        public bool Checkview
        {
            get { return (bool)GetValue(CheckviewProperty); }
            set { SetValue(CheckviewProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Checkview.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CheckviewProperty =
            DependencyProperty.Register("Checkview", typeof(bool), typeof(MyButton), new PropertyMetadata(visibleView));

        private static void visibleView(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            MyButton bu = (MyButton)source;

            if (bu.Checkview==true)
            {
                bu.Visibility = Visibility.Visible;
            }
            else
            {
                bu.Visibility = Visibility.Hidden;
            }
        }
    }
}
