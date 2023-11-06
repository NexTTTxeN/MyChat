using ClientChat.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Net.Http.Json;

namespace ClientChat.ViewModel
{
    internal class RegistrationVM : INotifyPropertyChanged
    {
        HttpClient _client;
        public RegistrationVM()
        {
            _client = new HttpClient();
        }

        string _host = "https://localhost:7777";


        /// <summary>
        /// Логин для регистрации
        /// </summary>
        private string _regLogin;
        public string RegLogin
        {
            get { return _regLogin; }
            set
            {
                _regLogin = value;
                NotifyPropertyChanged(nameof(RegLogin));
            }
        }


        /// <summary>
        /// Команда для регистрации
        /// </summary>
        private MyCommand _registrUserCD;
        public MyCommand RegistrUserCD
        {
            get { return _registrUserCD ?? new MyCommand(async (obj) => RegistrUser(), (obj) => RegistrUserValid()); }
        }

        /// <summary>
        /// Регистрация пользователя
        /// </summary>
        private async Task RegistrUser()
        {
            var loginData = new { Login = RegLogin, Password = Registration.Rigist.GetPassword() };
            if (loginData.Password.Length < 5 && loginData.Login.Length < 5)
            {
                MessageBox.Show("Логин и пароль должны быть 5 и более символов!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (loginData.Password.Length > 30 && loginData.Password.Length > 30)
            {
                MessageBox.Show("Логин и пароль должны не более 30 символов!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            try
            {
                var response = await _client.PostAsJsonAsync($"{_host}/api/regist", loginData);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Зарегестрирован пользователь {RegLogin} прошла успешно!");
                    Registration.Rigist.Close();
                }
                else
                {
                    MessageBox.Show("Ошибка при входе. Пожалуйста, проверьте логин и пароль");
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Валидация команды регистарции
        /// </summary>
        private bool RegistrUserValid()
        {
            return RegLogin != null && RegLogin.Length>0;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }

}
