using Microsoft.Win32;
using NAudio.Wave;
using Player.Classes;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Button = System.Windows.Controls.Button;

namespace Player
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        static string nowBank = "Bank1"; // 현재 선택된 뱅크
        Button searchbutton = null; // 선택된 버튼
        static bool recording = false; // 녹음중 유무
        WaveFileWriter RecordedAudioWriter = null; // 녹음변수1
        WasapiLoopbackCapture CaptureInstance = null; // 녹음변수2
        static bool repeat = false; // 반복여부판단
        string nowvolumeSet = "";//현재 불륨 조절
        List<ColorSet> Colorsets; //database
        ColorSet colorsave; // database담기


        public MainWindow()
        {
            InitializeComponent();
            for (int i = 1; i <= 12; i++) // 뱅크폴더 없으면 만들기
            {
                string folder = @"C:/Padsource/Bank" + Convert.ToString(i);
                DirectoryInfo Di = new DirectoryInfo(folder);
                if (Di.Exists == false)
                {
                    Di.Create();
                }
            }
            Colorsets = new List<ColorSet>();
            ReadData(); // 데이터 읽어오기
            for (int i = 0; i < 53; i++) // 버튼별로 소스 입력해줄것
            {
                try
                {
                    MediaElement mu = functions.Findmedi(i);
                    string realUri = @"C:/Padsource/" + nowBank + "/" + "s" + i.ToString() + ".wav";
                    FileInfo fi = new FileInfo(realUri);
                    if (fi.Exists) //파일 존재 안하면 소스 입력 안함
                    {
                        mu.Source = new Uri(realUri);
                        mu.UpdateLayout();
                    } //먼저 숫자 순서에 따라 소스 입력
                    int indexT = Colorsets.FindIndex(r => r.Id == i); // 아이디 읽어와서
                    //if (Colorsets[indexT].Name == "Default")
                    //{
                    //    var foundButton = MainScreen.Children.OfType<Button>().Where(x => x.Tag.ToString() == "2").FirstOrDefault();
                    //    foundButton.Background = new SolidColorBrush(functions.ConvertStringToColor(Colorsets[indexT].Back));
                    //    foundButton.BorderBrush = new SolidColorBrush(functions.ConvertStringToColor(Colorsets[indexT].Bor));

                    //}
                    Button getcolor = functions.Findbutton(Colorsets[indexT].Name + "h"); // h버튼에 색상 저장
                    Color backcol = functions.ConvertStringToColor(Colorsets[indexT].Back);
                    Color borcol = functions.ConvertStringToColor(Colorsets[indexT].Bor);
                    getcolor.Background = new SolidColorBrush(backcol);
                    getcolor.BorderBrush = new SolidColorBrush(borcol);

                }
                catch (Exception)
                {
                }

            }
            colorsave = new ColorSet();
            functions.allstop(); // 소스 열리면 소리나오는 문제 해결
            MainScreen.Visibility = Visibility.Visible; //로딩끝나면 한번 리셋
        }
        private void Key_Down(object sender, KeyEventArgs e) // 키눌리면
        {
            e.Handled = true;
            try
            {
                if ((e.Key.ToString().Length != 1) & (e.Key.ToString().Substring(0, 1) == "F")) /// 뱅크키면
                {
                    nowBank =  functions.Bankchange(e.Key.ToString(), nowBank);
                    functions.allstop();
                }
                else if ((e.Key.ToString() == "System")) // F10은 시스템키라 추가적 작업 필요
                {
                    if (e.SystemKey == Key.F10)
                    {
                        nowBank = functions.Bankchange(e.SystemKey.ToString(), nowBank);
                        functions.allstop();
                    }
                }
                else
                {
                    Button pressbutton = functions.Findbutton(e.Key.ToString()); // 눌린키에 따라서 버튼을 찾아줌
                    if (pressbutton.Name == "LeftCtrl") Recorded(pressbutton); // 녹음
                    else if (pressbutton.Name == "Space") // 반복
                    {
                        if (repeat == true)
                        {
                            repeat = false;
                            pressbutton.Background = (Brush)FindResource("BackCol");
                            pressbutton.BorderBrush = (Brush)FindResource("BorderCol");
                            pressbutton.Foreground = new SolidColorBrush(Colors.White);
                        }
                        else
                        {
                            repeat = true;
                            pressbutton.Background = new SolidColorBrush(Colors.Wheat);
                            pressbutton.BorderBrush = new SolidColorBrush(Colors.Black);
                            pressbutton.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }
                    else if (pressbutton.Name == "RightCtrl") // 올스탑
                    {
                        functions.allstop();
                    }
                    else // 플레이버튼
                    {
                        Button getcolor = functions.Findbutton(pressbutton.Name + "h");
                        string realUri = @"C:/Padsource/" + nowBank + "/" + "s" + pressbutton.Tag + ".wav";
                        MediaElement wa = functions.Findmedi(Convert.ToInt32(pressbutton.Tag));
                        FileInfo fi = new FileInfo(realUri);
                        if (fi.Exists)
                        {
                            wa.Source = new Uri(realUri);
                            wa.UpdateLayout();
                        }
                        else wa.Source = null;
                        pressbutton.BorderBrush = getcolor.BorderBrush;
                        pressbutton.Background = getcolor.Background;
                        if (repeat == true) wa.Tag = "Repeat";
                        else wa.Tag = "Not";
                        if (wa.Source != null)
                        {
                            wa.Play();
                        }
                        else
                        {
                            pressbutton.Background = (Brush)FindResource("BackCol");
                            pressbutton.BorderBrush = (Brush)FindResource("BorderCol");
                        }
;
                    }

                }
            }
            catch (Exception)
            {
            }
        }
        public void PlaySoundD(object sender, EventArgs e) // 버튼 눌릴때함수
        {
            try
            {

                Button tempbutton = (Button)sender;
                string realUri = @"C:/Padsource/" + nowBank + "/" + "s" + tempbutton.Tag + ".wav";
                Button getcolor = functions.Findbutton(tempbutton.Name + "h");
                MediaElement wa = functions.Findmedi(Convert.ToInt32(tempbutton.Tag));
                FileInfo fi = new FileInfo(realUri);
                if (fi.Exists)
                {
                    wa.Source = new Uri(realUri);
                    wa.UpdateLayout();
                }
                else wa.Source = null;
                tempbutton.BorderBrush = getcolor.BorderBrush;
                tempbutton.Background = getcolor.Background;
                if (repeat == true) wa.Tag = "Repeat";
                else wa.Tag = "Not";
                wa.UpdateLayout();
                if (wa.Source != null)
                {
                    wa.Play();
                }
                else
                {
                    tempbutton.Background = (Brush)FindResource("BackCol");
                    tempbutton.BorderBrush = (Brush)FindResource("BorderCol");
                }
            }
            catch (Exception)
            {
            }
        }

        public void Load_Click(object sender, RoutedEventArgs e) 
        {

            for (int i = 0; i < 53; i++) // 버튼별로 소스 입력해줄것
            {
                MediaElement mu = functions.Findmedi(i);
                mu.LoadedBehavior = MediaState.Close;
                mu.UnloadedBehavior = MediaState.Close;
            }
            OpenFileDialog open = new OpenFileDialog();
            open.Multiselect = false;
            open.Filter = "Audio File (*.wav)|*.wav;";
            string fileUri = "";
            MenuItem mnu = sender as MenuItem;
            ContextMenu MyContextMenu = (ContextMenu)mnu.Parent;
            Button MyButton = MyContextMenu.PlacementTarget as Button;
            if (open.ShowDialog() == true) // 파일이 선택됫으면 복사함
            {
                fileUri = open.FileName;
                string realUri = @"C:/Padsource/" + nowBank + "/" + "s" + MyButton.Tag.ToString() + ".wav";
                System.IO.File.Copy(fileUri, realUri, true);
                MediaElement input = functions.Findmedi(Convert.ToInt32(MyButton.Tag));
                input.Source = new Uri(realUri); // 해당 버튼에 음악파일 연결
            }
            functions.allstop();

        }

        private void Color_Click(object sender, RoutedEventArgs e) // 컬러버튼 누르면 새창을 띄우고 자식창에 버튼을 넘겨줌
        {
            MenuItem mnu = sender as MenuItem;
            Button MyButton = null;
            if (mnu != null)
            {
                ContextMenu MyContextMenu = (ContextMenu)mnu.Parent;
                MyButton = MyContextMenu.PlacementTarget as Button;
                searchbutton = functions.Findbutton(MyButton.Name + "h");
                ColorPick a = new ColorPick(searchbutton);
                a.returnButton += A_returnButton;
                a.ShowDialog();
            }
        }

        private void A_returnButton(Button a) // 대리자를 통해 자식창으로 부터 버튼 받아옴
        {

            if(a.Tag == null)
            {
                searchbutton.Background = a.Background;
                searchbutton.BorderBrush = a.BorderBrush;
                colorsave.Name = searchbutton.Name.Substring(0, searchbutton.Name.Length - 1);
                colorsave.Back = a.Background.ToString();
                colorsave.Bor = a.BorderBrush.ToString();

                using (SQLiteConnection conn = new SQLiteConnection(App.databasePath))
                {
                    conn.CreateTable<ColorSet>();
                    SQLiteCommand cmd = new SQLiteCommand(conn);
                    cmd.CommandText = "insert or replace into Colorset (ID, Name, Back, Bor) values ((select ID from Colorset where Name = @Name),@Name,@Back,@Bor)";
                    cmd.Bind("@Name", colorsave.Name);
                    cmd.Bind("@Back", colorsave.Back);
                    cmd.Bind("@Bor", colorsave.Bor);
                    cmd.ExecuteNonQuery();
                    //id 값은 53이내로 유지되도록하면서 버튼이름,눌릴때 배경색,눌릴때 테두리색을 저장함
                }
            }
            else
            {
                for (int i = 0; i < 53; i++) // 버튼별로 소스 입력해줄것
                {
                    MediaElement mu = functions.Findmedi(i);
                    Button Newb = (Button)mu.Parent;
                    Button name = functions.FindParent(Newb);
                    Newb.Background = a.Background;
                    Newb.BorderBrush = a.BorderBrush;
                    using (SQLiteConnection conn = new SQLiteConnection(App.databasePath))
                    {
                        conn.CreateTable<ColorSet>();
                        SQLiteCommand cmd = new SQLiteCommand(conn);
                        cmd.CommandText = "insert or replace into Colorset (ID, Name, Back, Bor) values ((select ID from Colorset where Name = @Name),@Name,@Back,@Bor)";
                        cmd.Bind("@Name", name.Name);
                        cmd.Bind("@Back", a.Background.ToString());
                        cmd.Bind("@Bor", a.BorderBrush.ToString());
                        cmd.ExecuteNonQuery();
                        //id 값은 53이내로 유지되도록하면서 버튼이름,눌릴때 배경색,눌릴때 테두리색을 저장함
                    }
                }
            }


        }

        private void Bank_Click(object sender, RoutedEventArgs e) // 뱅크버튼 클릭
        {
            Button tempname = (Button)sender;
            nowBank = functions.Bankchange(tempname.Name, nowBank);
            functions.allstop();

        }

        private void LeftCtrl_Click(object sender, RoutedEventArgs e) // LEFT버튼 클릭
        {
            Recorded((Button)sender);
        }


        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e) // 음악이 끝나면
        {

            MediaElement target = (MediaElement)sender;

            if (target.Tag.ToString() == "Repeat") //반복이면 반복시킴
            {
                target.LoadedBehavior = MediaState.Manual;
                target.UnloadedBehavior = MediaState.Manual;
                target.Position = TimeSpan.FromSeconds(0);
                target.Play();
            }
            else //아니면 종료시킴
            {
                Button Finder = (Button)target.Parent;
                string temp = Finder.Name.ToString();
                temp = temp.Substring(0, temp.Length - 1);
                object tempbutton = MainScreen.FindName(temp);
                Button MotherButton = tempbutton as Button;
                MotherButton.Background = (Brush)FindResource("BackCol");
                MotherButton.BorderBrush = (Brush)FindResource("BorderCol");
            }
        }


        private void RightCtrl_Click(object sender, RoutedEventArgs e) // 멈추기 버튼 클릭
        {
            functions.allstop();
        }
        void ReadData() // 데이터 읽어오기
        {

            using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(App.databasePath))
            {
                conn.CreateTable<ColorSet>();
                Colorsets = conn.Table<ColorSet>().ToList();
            }
        }




        private void Volume_Click(object sender, RoutedEventArgs e) // 불륨누르면 불륨조절키 나오게함
        {
            Speedbar.Visibility = Visibility.Hidden;
            Balancebar.Visibility = Visibility.Hidden;
            OK.Visibility = Visibility.Visible;
            MenuItem mnu = sender as MenuItem;
            Button MyButton = null;
            ContextMenu MyContextMenu = (ContextMenu)mnu.Parent;
            MyButton = MyContextMenu.PlacementTarget as Button;
            nowvolumeSet = "Music" + MyButton.Tag;
            object target = MainScreen.FindName(nowvolumeSet);
            MediaElement targetvol = (MediaElement)target;
            Volumebar.Visibility = Visibility.Visible;
            Volumebar.Value = targetvol.Volume;
            Volumebar.Maximum = 1;
            Volumebar.Minimum = 0;
        }
        private void Recorded(Button LeftCtrl) //녹음
        {
            string saverecord = null; //녹음파일 저장위치
            if (recording) // 녹음중
            {
                LeftCtrl.Background = (Brush)FindResource("BackCol");
                LeftCtrl.BorderBrush = (Brush)FindResource("BorderCol");
                LeftCtrl.Content = "Record(Ctrl)";
                recording = false;
                this.CaptureInstance.StopRecording();
            }
            else // 녹음 아니면
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Audio File (*.wav)|*.wav;";
                if (saveFileDialog.ShowDialog() == true) saverecord = saveFileDialog.FileName;
                LeftCtrl.Background = new SolidColorBrush(Colors.Red);
                LeftCtrl.BorderBrush = new SolidColorBrush(Colors.Black);
                LeftCtrl.Content = "Recording";
                recording = true;
                this.CaptureInstance = new WasapiLoopbackCapture();
                this.RecordedAudioWriter = new WaveFileWriter(saverecord,CaptureInstance.WaveFormat);
                this.CaptureInstance.DataAvailable += (s, a) =>
                {
                    this.RecordedAudioWriter.Write(a.Buffer, 0, a.BytesRecorded);
                };
                this.CaptureInstance.RecordingStopped += (s, a) =>
                {
                    this.RecordedAudioWriter.Dispose();
                    this.RecordedAudioWriter = null;
                    this.CaptureInstance.Dispose();
                };
                this.CaptureInstance.StartRecording();

            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mnu = sender as MenuItem;
            ContextMenu MyContextMenu = (ContextMenu)mnu.Parent;
            Button MyButton = MyContextMenu.PlacementTarget as Button;
            string saveuri = "";
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Audio File (*.wav)|*.wav;";
            if (saveFileDialog.ShowDialog() == true)
            {
                saveuri = saveFileDialog.FileName;
                string realUri = @"C:/Padsource/" + nowBank + "/" + "s" + MyButton.Tag.ToString() + ".wav";
                System.IO.File.Copy(realUri, saveuri, true);
            }

        }

        private void Panning_Click(object sender, RoutedEventArgs e)
        {
            Speedbar.Visibility = Visibility.Hidden;
            Volumebar.Visibility = Visibility.Hidden;
            OK.Visibility = Visibility.Visible;
            MenuItem mnu = sender as MenuItem;
            Button MyButton = null;
            ContextMenu MyContextMenu = (ContextMenu)mnu.Parent;
            MyButton = MyContextMenu.PlacementTarget as Button;
            nowvolumeSet = "Music" + MyButton.Tag;
            object target = MainScreen.FindName(nowvolumeSet);
            MediaElement targetvol = (MediaElement)target;
            Balancebar.Visibility = Visibility.Visible;
            Balancebar.Maximum = 1;
            Balancebar.Minimum = -1;
            Balancebar.Value = targetvol.Balance;
        }

        private void Speed_Click(object sender, RoutedEventArgs e)
        {
            Balancebar.Visibility = Visibility.Hidden;
            Volumebar.Visibility = Visibility.Hidden;
            OK.Visibility = Visibility.Visible;
            MenuItem mnu = sender as MenuItem;
            Button MyButton = null;
            ContextMenu MyContextMenu = (ContextMenu)mnu.Parent;
            MyButton = MyContextMenu.PlacementTarget as Button;
            nowvolumeSet = "Music" + MyButton.Tag;
            object target = MainScreen.FindName(nowvolumeSet);
            MediaElement targetvol = (MediaElement)target;
            Speedbar.Visibility = Visibility.Visible;
            Speedbar.Value = targetvol.SpeedRatio;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Speedbar.Visibility = Visibility.Hidden;
            Balancebar.Visibility = Visibility.Hidden;
            Volumebar.Visibility = Visibility.Hidden;
            OK.Visibility = Visibility.Hidden;
        }
        private void Sliderbar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) // 슬라이더바 조절시 불륨버튼과 연동시킴
        {
            try
            {
                if (sender == Volumebar)
                {
                    object target = MainScreen.FindName(nowvolumeSet);
                    MediaElement targetvol = (MediaElement)target;
                    targetvol.Volume = Volumebar.Value;
                }
                else if (sender == Balancebar)
                {
                    object target = MainScreen.FindName(nowvolumeSet);
                    MediaElement targetvol = (MediaElement)target;
                    targetvol.Balance = Balancebar.Value;
                }
                else if (sender == Speedbar)
                {
                    object target = MainScreen.FindName(nowvolumeSet);
                    MediaElement targetvol = (MediaElement)target;
                    targetvol.SpeedRatio = Speedbar.Value;
                }
            }
            catch(Exception) { }

        }

        private void FileDrop(object sender, DragEventArgs e)
        {
            for (int i = 0; i < 53; i++) // 버튼별로 소스 입력해줄것
            {
                MediaElement mu = functions.Findmedi(i);
                mu.LoadedBehavior = MediaState.Close;
                mu.UnloadedBehavior = MediaState.Close;
            }
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            Button MyButton = (Button)sender;
            string realUri = @"C:/Padsource/" + nowBank + "/" + "s" + MyButton.Tag.ToString() + ".wav";
            System.IO.File.Copy(files[0], realUri, true);
            MediaElement input = functions.Findmedi(Convert.ToInt32(MyButton.Tag));
            input.Source = new Uri(realUri); // 해당 버튼에 음악파일 연결
            functions.allstop();
        }

        //private void SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        //{
        //    ColorSet b = new ColorSet();
        //    b.Back = D1.Background.ToString();
        //    b.Bor = D1.BorderBrush.ToString();

        //    using (SQLiteConnection conn = new SQLiteConnection(App.databasePath))
        //    {
        //        conn.CreateTable<ColorSet>();
        //        SQLiteCommand cmd = new SQLiteCommand(conn);
        //        cmd.CommandText = "insert or replace into Colorset (ID, Name, Back, Bor) values ((select ID from Colorset where Name = @Name),@Name,@Back,@Bor)";
        //        cmd.Bind("@Name", "Default");
        //        cmd.Bind("@Back", b.Back);
        //        cmd.Bind("@Bor", b.Bor);
        //        cmd.ExecuteNonQuery();
        //        //id 값은 53이내로 유지되도록하면서 버튼이름,눌릴때 배경색,눌릴때 테두리색을 저장함
        //    }
        //    //using (SQLiteConnection conn = new SQLiteConnection(App.databasePath))
        //    //{
        //    //    conn.CreateTable<ColorSet>();
        //    //    SQLiteCommand cmd = new SQLiteCommand(conn);
        //    //    cmd.CommandText = "insert or replace into Colorset (ID, Name, Back, Bor) values ((select ID from Colorset where Name = @Name),@Name,@Back,@Bor)";
        //    //    cmd.Bind("@Name", "Default");
        //    //    cmd.Bind("@Back", BackColor.SelectedColor.ToString());
        //    //    if(BorderColor==null) cmd.Bind("@Bor","#f8f8ff");
        //    //    else cmd.Bind("@Bor", BorderColor.SelectedColor.ToString());
        //    //    cmd.ExecuteNonQuery();
        //    //    //id 값은 53이내로 유지되도록하면서 버튼이름,눌릴때 배경색,눌릴때 테두리색을 저장함
        //    //}
        //}
    }
}

