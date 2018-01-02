using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Reflection;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;

namespace DSM_MealWidget
{
    public class Meal
    {
        public string[] mealData = new string[3];

        public string Morning { get => mealData[0]; }
        public string Lunch { get => mealData[1]; }
        public string Dinner { get => mealData[2]; }
    }

    public partial class MainWindow : Window
    {
        private NotifyIcon notify;

        private Meal meal = new Meal();
        private DateTime nowDate = DateTime.Today;
        private WebClient client = new WebClient();

        private int nowYear;
        private int nowMonth;
        private int nowDay;

        public MainWindow()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            key.SetValue("DmsMeal", System.Windows.Forms.Application.ExecutablePath.ToString());

            //System.Windows.MessageBox.Show("실행 성공");
            client.Encoding = Encoding.UTF8;
            
            Resources.Add("mealResource", meal);

            try
            {
                InitializeComponent();
            }
            catch
            {

            }
            //System.Windows.MessageBox.Show("WPF 윈도우 이니셜라이즈 완료");

            ReloadForm();
            NotifySetting();

            //System.Windows.MessageBox.Show("위치 조정중");
            var desktopWorkingArea = Screen.PrimaryScreen.WorkingArea;

            Left = desktopWorkingArea.Right - Width - 12;
            Top = desktopWorkingArea.Bottom - Height - 12;
            //System.Windows.MessageBox.Show("위치 조정 완료");
            //System.Windows.MessageBox.Show("설정이 종료되었습니다.");
        }

        public void NotifySetting()
        {
            //System.Windows.MessageBox.Show("Notify 설정중");
            Uri iconUri = new Uri("pack://application:,,,/Logo.ico", UriKind.RelativeOrAbsolute);
            Stream iconStream = System.Windows.Application.GetResourceStream(iconUri).Stream;
            notify = new NotifyIcon();
            notify.Text = "DMS Meal";
            notify.Icon = new System.Drawing.Icon(iconStream);
            notify.ContextMenu = new System.Windows.Forms.ContextMenu(
                    new System.Windows.Forms.MenuItem[] {
                        new System.Windows.Forms.MenuItem("Exit", (x, y) =>
                        {
                            System.Windows.Application.Current.Shutdown();
                        })
                    }
                );
            notify.Visible = true;
            notify.DoubleClick += Notify_DoubleClick;
            //System.Windows.MessageBox.Show("Notify 설정 완료");
        }

        private void ReloadForm()
        {
            nowYear = nowDate.Year;
            nowMonth = nowDate.Month;
            nowDay = nowDate.Day;

            //System.Windows.MessageBox.Show("급식 정보 다운로드중");
            string s = string.Empty;
            string url = string.Format("http://dsm2015.cafe24.com:3000/meal/{0}-{1}-{2}",
                nowYear.ToString().PadLeft(4, '0'),
                nowMonth.ToString().PadLeft(2, '0'),
                nowDay.ToString().PadLeft(2, '0'));
            try
            {
                s = client.DownloadString(url);
            }
            catch (WebException)
            {
                morningText.Text = string.Empty;
                lunchText.Text = "네트워크 연결에 실패하였습니다.";
                dinnerText.Text = string.Empty;
                return;
            }
            //System.Windows.MessageBox.Show("다운로드 완료");
            
            ParseJSONText(s);

            morningText.Text = meal.Morning;
            lunchText.Text = meal.Lunch;
            dinnerText.Text = meal.Dinner;

            DateText.Text = string.Format("{0}년 {1}월 {2}일 {3}요일", nowYear, nowMonth, nowDay, nowDate.ToString("ddd", CultureInfo.CurrentCulture));
            //System.Windows.MessageBox.Show("급식 정보 적용 완료");
        }

        public void ParseJSONText(string original)
        {
            //System.Windows.MessageBox.Show("JSON 파싱중");
            JObject jsonObject;
            try
            {
                jsonObject = JObject.Parse(original);
            }
            catch
            {
                meal.mealData[0] = string.Empty;
                meal.mealData[1] = "급식 정보를 불러오지 못했습니다.";
                meal.mealData[2] = string.Empty;
                return;
            }
            JArray breakfastArray = jsonObject["breakfast"] as JArray;
            JArray lunchArray = jsonObject["lunch"] as JArray;
            JArray dinnerArray = jsonObject["dinner"] as JArray;

            string temp = string.Empty;
            for(int i = 0; i < breakfastArray.Count - 1; i++)
            {
                temp += $"{breakfastArray[i]}, ";
            }
            meal.mealData[0] = temp + breakfastArray[breakfastArray.Count - 1];

             temp = string.Empty;
            for (int i = 0; i < lunchArray.Count - 1; i++)
            {
                temp += $"{lunchArray[i]}, ";
            }
            meal.mealData[1] = temp + lunchArray[lunchArray.Count - 1];

             temp = string.Empty;
            for (int i = 0; i < dinnerArray.Count - 1; i++)
            {
                temp += $"{dinnerArray[i]}, ";
            }
            meal.mealData[2] = temp + dinnerArray[dinnerArray.Count - 1];
            //System.Windows.MessageBox.Show("파싱 완료");
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            ShowInTaskbar = false;
            Hide();
        }

        private void Notify_DoubleClick(object sender, EventArgs e)
        {
            nowDate = DateTime.Today;
            ReloadForm();
            ShowInTaskbar = true;
            Show();
            Activate();
        }
        
        private void LeftDateButton_Click(object sender, RoutedEventArgs e)
        {
            nowDate -= new TimeSpan(1, 0, 0, 0);
            ReloadForm();
        }

        private void RightDateButton_Click(object sender, RoutedEventArgs e)
        {
            nowDate += new TimeSpan(1, 0, 0, 0);
            ReloadForm();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
