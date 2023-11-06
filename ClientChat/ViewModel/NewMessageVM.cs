using ClientChat.Model;
using EntityChatDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ClientChat.ViewModel
{
    class NewMessageVM : INotifyPropertyChanged
    {
        /// <summary>
        /// Id пользователя
        /// </summary>
        int _userId = 0;
        public int UserId
        {
            get { return _userId; }
            set { _userId = value; GetUsers(); }
        }
        /// <summary>
        /// Список всех пользователей
        /// </summary>
        List<User> _users = new List<User>();
        
        string _host = "https://localhost:7777";

        HttpClient _httpClient = new HttpClient();
        public NewMessageVM() { }

        #region OBJECTS AND PROPERTIES DATAVM
        //Список открытых чатов


        /// <summary>
        /// Выбранный пользователя для отправки сообщений
        /// </summary>
        private User? _userTo;
        public User? UserTo
        {
            get { return _userTo; }
            set
            {
                _userTo = value;
                NotifyPropertyChanged(nameof(UserTo));
            }
        }

        /// <summary>
        /// Список отсортированных пользователей
        /// </summary>
        private List<User>? _listuser;
        public List<User>? ListUser
        {
            get { return _listuser; }
            set
            {
                _listuser = value;
                NotifyPropertyChanged(nameof(ListUser));
            }
        }


        /// <summary>
        /// Текущее сообщение
        /// </summary>
        private string? _message;
        public string? Message
        {
            get { return _message; }
            set
            {
                _message = value;
                NotifyPropertyChanged(nameof(Message));
            }
        }

        /// <summary>
        /// Фильтр пользователей
        /// </summary>
        private string? _searchUser;
        public string? SearchUser
        {
            get { return _searchUser; }
            set
            {
                _searchUser = value;
                NotifyPropertyChanged(nameof(SearchUser));
                Search();
            }
        }


        #endregion

        #region COMMAND DATAVM

        /// <summary>
        /// Команда отправки сообщения
        /// </summary>

        private MyCommand _sendMessageCD;
        public MyCommand SendMessageCD
        {
            get { return _sendMessageCD ?? new MyCommand(async (obj) => await SendMessageAsync(), (obj) => SendMessageValid()); }
        }


        #endregion

        #region METHOD DATAVM

        /// <summary>
        /// Отправка сообщений
        /// </summary>
        private async Task SendMessageAsync()
        {
            try
            {
                Message message = new Message { UserFrom = _userId, UserTo = UserTo.UserId, DataMessage = DateTime.Now, IsRead = false, Message1 = _message };
                StartAuth();
                using var responseMessage = await _httpClient.PostAsJsonAsync($"{_host}/api/message", message);
                Message = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Сообщение не отправлено!", "Состояние", MessageBoxButton.OK, MessageBoxImage.Information);
            }

        }

        /// <summary>
        /// Валидация проверки сообщений
        /// </summary>
        private bool SendMessageValid()
        {
            return _message != "" && _userTo != null;
        }


        /// <summary>
        /// Проверка авторизации пользователя на сервере
        /// </summary>
        private void StartAuth()
        {
            string? accessToken;
            using (StreamReader sw = new StreamReader($"Token_{UserId}.txt"))
            {
                accessToken = sw.ReadLine();
            }
            if (accessToken != null)
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            }
            else
            {
                MessageBox.Show("Ошибка чтения токена!");
            }
        }

        /// <summary>
        /// Получение списка пользователей с сервера
        /// </summary>
        private async void GetUsers()
        {
            StartAuth();
            List<User>? data = await _httpClient.GetFromJsonAsync<List<User>>($"{_host}/api/users/all/{_userId}");
            if (data is not null)
            {
                _users = data as List<User>;
            }
            else
            {
                MessageBox.Show("Ошибка доступа к защищенному ресурсу");
            }
        }

        /// <summary>
        /// Поиск пользователей из списка по фильтру
        /// </summary>
        private void Search()
        {
            if (_searchUser.Length==0) ListUser.Clear();
            else
            {
                ListUser = _users.Where(u => u.UserName.ToLower().Contains(_searchUser.ToLower())).ToList();
            }
        }

        #endregion

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
