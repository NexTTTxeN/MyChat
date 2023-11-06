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

namespace ClientChat
{
    /// <summary>
    /// Логика взаимодействия для Registration.xaml
    /// </summary>
    public partial class Registration : Window
    {
        public static Registration? Rigist { get; private set; }
        public Registration()
        {
            InitializeComponent();
            Rigist = this;
        }
        public string GetPassword()
        {
            string temp = RegPassword.Password;
            RegPassword.Password = "";
            return temp;
        }
    }
}
