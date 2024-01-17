using Player.Classes;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace Player
{
    /// <summary>
    /// ColorPick.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ColorPick : Window
    {
        public delegate void returnEventHandler(Button a);
        public event returnEventHandler returnButton; // 대리자

        public ColorPick(Button argu)
        {
            InitializeComponent();
            StartButton.BorderBrush = argu.BorderBrush;
            StartButton.Background = argu.Background;
            Border.Background = StartButton.BorderBrush;
            Back.Background = StartButton.Background;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (sender == (Button)All) StartButton.Tag = "All"; 
            returnButton(StartButton);
            this.Close();
        }
        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            StartButton.Background =new SolidColorBrush(functions.ConvertStringToColor("#FFDCEDC1"));
            StartButton.BorderBrush = new SolidColorBrush(Colors.LightGray);
            StartButton.Tag = "Reset";
            returnButton(StartButton);
            string target = @"C:/Padsource/Colorset.db";
            File.Delete(target);
            this.Close();
        }


        private void BackColor_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (BackColor.SelectedColor.HasValue)
            {
                Color C = BackColor.SelectedColor.Value;
                SolidColorBrush brush = new SolidColorBrush(C);
                StartButton.Background = brush;
                Back.Background = brush;
            }
        }

        private void BorderColor_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (BorderColor.SelectedColor.HasValue)
            {
                Color C = BorderColor.SelectedColor.Value;
                SolidColorBrush brush = new SolidColorBrush(C);
                StartButton.BorderBrush = brush;
                Border.Background = brush;
            }
        }
    }
}
