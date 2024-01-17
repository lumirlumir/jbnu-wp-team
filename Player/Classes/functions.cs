using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Player.Classes
{
    public static class functions
    {

        public static Button Findbutton(string name) //button 검색
        {
            object findname = (Application.Current.MainWindow as MainWindow).MainScreen.FindName(name);
            Button returnbutton = (Button)findname;
            return returnbutton;
        }
        public static MediaElement Findmedi(int i) //mediaelement 검색
        {
            object findmu = (Application.Current.MainWindow as MainWindow).MainScreen.FindName("Music" + i.ToString());
            MediaElement mu = (MediaElement)findmu;
            return mu;
        }
        public static Button Findsubbutton(Button parent) //노말버튼 ->h버튼
        {
            object findname = (Application.Current.MainWindow as MainWindow).MainScreen.FindName(parent.Name + "h");
            Button returnbutton = (Button)findname;
            return returnbutton;
        }
        public static Button FindParent(Button son) // h버튼 -> 노말버튼
        {
            object findname = (Application.Current.MainWindow as MainWindow).MainScreen.FindName(son.Name.Substring(0, son.Name.Length - 1));
            Button returnbutton = (Button)findname;
            return returnbutton;
        }

        public static Color ConvertStringToColor(String hex) // ARGB 컬러값을 Color형으로 변환해줌
        {
            //remove the # at the front
            hex = hex.Replace("#", "");

            byte a = 255;
            byte r = 255;
            byte g = 255;
            byte b = 255;

            int start = 0;

            //handle ARGB strings (8 characters long)
            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                start = 2;
            }

            //convert RGB characters to bytes
            r = byte.Parse(hex.Substring(start, 2), System.Globalization.NumberStyles.HexNumber);
            g = byte.Parse(hex.Substring(start + 2, 2), System.Globalization.NumberStyles.HexNumber);
            b = byte.Parse(hex.Substring(start + 4, 2), System.Globalization.NumberStyles.HexNumber);

            return Color.FromArgb(a, r, g, b);
        } // argb to Color
        public static string Bankchange(string inputBank, string nowBank) // 뱅크가 바뀌면
        {
            object nowb = (Application.Current.MainWindow as MainWindow).MainScreen.FindName("F" + nowBank.Substring(4, nowBank.Length - 4));
            Button oldb = (Button)nowb;
            object presskey = (Application.Current.MainWindow as MainWindow).MainScreen.FindName(inputBank);
            Button newb = functions.Findbutton(inputBank);
            oldb.Content = oldb.Name;
            nowBank = (String)newb.Tag;
            newb.Content = "Now";
            oldb.Background = newb.Background;
            oldb.BorderBrush = newb.BorderBrush;
            oldb.Foreground = newb.Foreground;
            newb.Background = new SolidColorBrush(Colors.White);
            newb.BorderBrush = new SolidColorBrush(Colors.LightGray);
            newb.Foreground = new SolidColorBrush(Colors.LightGray);        
            return newb.Tag.ToString();
        }
        public static void allstop() // 소리 전체 멈추기
        {
            for (int i = 0; i < 52; i++)
            {
                MediaElement b = Findmedi(i);
                b.Tag = "Not";
                b.LoadedBehavior = MediaState.Manual;
                b.UnloadedBehavior = MediaState.Manual;
                b.Stop();
                Button Finder = (Button)b.Parent;
                string temp = Finder.Name.ToString();
                temp = temp.Substring(0, temp.Length - 1);
                object tempbutton = (Application.Current.MainWindow as MainWindow).MainScreen.FindName(temp);
                Button MotherButton = tempbutton as Button;
                MotherButton.Background = (Brush)(Application.Current.MainWindow as MainWindow).FindResource("BackCol");
                MotherButton.BorderBrush = (Brush)(Application.Current.MainWindow as MainWindow).FindResource("BorderCol");
            }

        }


    }
}
