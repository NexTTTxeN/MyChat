using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

namespace ClientChat
{
    /// <summary>
    /// Логика взаимодействия для Autorization.xaml
    /// </summary>
    public partial class Authoriz : Window
    {
        public static Authoriz? Auth { get; private set; }
        public int UserId { get ; set; } = 0;
        public Authoriz()
        {
            InitializeComponent();
            Auth = this;
        }
        public string GetPassword()
        {
            string temp = AuthPassword.Password;
            AuthPassword.Password = "";
            return temp;
        }
    }
}
