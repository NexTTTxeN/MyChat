using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ClientChat.Model;
using System.ComponentModel;
using System.Net.WebSockets;

namespace ClientChat.ViewModel
{
    /// <summary>
    /// Класс view model для окна Авторизации
    /// </summary>
    internal class AuthorizationVM : INotifyPropertyChanged
    {
        string _host = "https://localhost:7777";
        HttpClient _client;
        Authoriz _autorization;

        public AuthorizationVM()
        {
            _client = new HttpClient();
        }

        /// <summary>
        /// Переменная хранящая введенный логин
        /// </summary>
        private string _authLogin;
        public string AuthLogin
        {
            get { return _authLogin; }
            set
            {
                _authLogin = value;
                NotifyPropertyChanged(nameof(AuthLogin));
            }
        }


        /// <summary>
        /// Команда для кнопки запуска авторизации
        /// </summary>
        private MyCommand _authUserCD;
        public MyCommand AuthUserCD
        {
            get { return _authUserCD ?? new MyCommand(async (obj) => await AuthUser(), (obj) => AuthUserValid()); }
        }


        /// <summary>
        /// Метод авторизации пользователя
        /// </summary>
        private async Task AuthUser()
        {
            if(_autorization==null) _autorization = Authoriz.Auth;
            try
            {
                if (_autorization is null) _autorization = Authoriz.Auth;
                var loginData = new { Login = AuthLogin, Password = _autorization.GetPassword() };
                var response = await _client.PostAsJsonAsync($"{_host}/api/auth/login", loginData);
                if (response.IsSuccessStatusCode)
                {
                    string token = await response.Content.ReadAsStringAsync();

                    var tokenData = JsonConvert.DeserializeAnonymousType(token, new { access_token = "", userId = 0 });

                    string accessToken = tokenData.access_token;
                    using (StreamWriter sw = new StreamWriter($"Token_{tokenData.userId}.txt"))
                    {
                        sw.Write(accessToken);
                    }
                    _autorization.UserId = tokenData.userId;
                    MessageBox.Show($"С возвращением {AuthLogin} !");
                    _autorization.DialogResult = true;
                    _autorization.Close();

                }
                else
                {
                    _autorization.UserId = 0;
                    MessageBox.Show("Ошибка при входе. Пожалуйста, проверьте логин и пароль");
                }
            }
            catch (HttpRequestException ex)
            {
                _autorization.UserId = 0;
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Проверка для определения состояния кнопки
        /// </summary>
        private bool AuthUserValid()
        {
            return AuthLogin != "";
        }



        /// <summary>
        /// событие для обновение переменных в view model
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;


        /// <summary>
        /// Метод для обновение переменных в view model
        /// </summary>
        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
