using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SchoolMeal;

namespace MealWidget
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 SWP_NOACTIVATE = 0x0010;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                SetWindowPos(Process.GetCurrentProcess().MainWindowHandle, HWND_BOTTOM, 0, 0, 0, 0,
                    SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
                Meal meal = new Meal(Regions.Daejeon, SchoolType.High, "G100000170");
                var menu = meal.GetMealMenu();
//                var date = DateTime.Now.Day - 1;
                var date = 15;
                if (!menu[date].IsExistMenu)
                {
                    this.Breakfirst.Text = "급식 없어";
                }
                else
                {
                    string breakfirst = null;
                    string lunch = null;
                    string dinner = null;
                    foreach (var item in menu[date].Breakfast)
                    {
                        breakfirst += item.Remove(item.IndexOf('*')) + "\n";
                    }
                    this.Breakfirst.Text = breakfirst;

                    foreach (var item in menu[date].Lunch)
                    {
//                        lunch += item.Remove(item.IndexOf('*')) + "\n";
                        lunch += item + "\n";
                    }
                    this.Lunch.Text = lunch;
                    foreach (var item in menu[date].Dinner)
                    {
//                        dinner += item.Remove(item.IndexOf('*')) + "\n";
                        dinner += item + "\n";
                    }
                    this.Dinner.Text = dinner;
                }
            };
        }
    }
}