using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using EntityChatDB;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.IO;
using ClientChat.Model;
using System.Threading;
using System.Windows.Media;
using System.Drawing;
using System.Linq.Expressions;
using System.Net;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;

namespace ClientChat.ViewModel
{
    /// <summary>
    ///View model для основного окна программы 
    /// </summary>
    class DataVM : INotifyPropertyChanged
    {
        string _host = "https://localhost:7777";

        HttpClient _client;
        /// <summary>
        ///Таймер обновления сообщений
        /// </summary>
        Timer? _timer;
        public DataVM()
        {
            _client = new HttpClient();
            ThisUser = new User();
            _isAuth = false;
        }

        #region OBJECTS AND PROPERTIES DATAVM

        /// <summary>
        ///Список открытых чатов
        /// </summary>
        private List<UsersMessage>? _listuser;
        public List<UsersMessage>? ListUser
        {
            get { return _listuser; }
            set
            {
                _listuser = value;
                NotifyPropertyChanged(nameof(ListUser));
            }
        }
        /// <summary>
        ///Текущий пользователь
        /// </summary>
        static private User _thisUser;
        public User ThisUser
        {
            get { return _thisUser; }
            set
            {
                _thisUser = value;
                NotifyPropertyChanged(nameof(ThisUser));
            }
        }


        /// <summary>
        ///Показывает авторизован ли пользователь
        /// </summary>
        static private bool _isAuth;
        public bool IsAuth
        {
            get { return _isAuth; }
            set
            {
                _isAuth = value;
                NotifyPropertyChanged(nameof(IsAuth));
            }
        }

        /// <summary>
        /// Открытый чат
        /// </summary>
        static private UsersMessage _userTo;
        public UsersMessage UserTo
        {
            get { return _userTo; }
            set
            {
                if (value != null)
                {
                    _userTo = value;
                    NotifyPropertyChanged(nameof(UserTo));
                    ImageUserTo = UserTo.ImageUser;
                }
            }
        }

        /// <summary>
        /// Введенное сообщение
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

        ///<summary>
        ///Изображение пользователя
        ///</summary>
        ///
        private byte[]? _imageUser;
        public byte[]? ImageUser
        {
            get { return _imageUser; }
            set
            {
                _imageUser = value;
                NotifyPropertyChanged(nameof(ImageUser));
            }
        }

        ///<summary>
        ///Изображение пользователя
        ///</summary>
        ///
        private byte[]? _imageUserTo;
        public byte[]? ImageUserTo
        {
            get { return _imageUserTo; }
            set
            {
                _imageUserTo = value;
                NotifyPropertyChanged(nameof(ImageUserTo));
            }
        }

        #endregion

        #region COMMAND DATAVM



        /// <summary>
        /// Команда открытия окна нового сообщения
        /// </summary>
        private MyCommand _openNewMessWindowCD;
        public MyCommand OpenNewMessWindowCD
        {
            get { return _openNewMessWindowCD ?? new MyCommand((obj) => OpenNewMessage(), (obj) => AuthUserValid()); }
        }

        /// <summary>
        /// Команда отправки сообщений
        /// </summary>
        private MyCommand _sendMessageCD;
        public MyCommand SendMessageCD
        {
            get { return _sendMessageCD ?? new MyCommand(async (obj) => await SendMessageAsync(), (obj) => SendMessageValid()); }
        }


        /// <summary>
        /// Команда смены пользователя
        /// </summary>
        private MyCommand _exitUserCD;
        public MyCommand ExitUserCD
        {
            get { return _exitUserCD ?? new MyCommand((obj) => ExitUser(),(obj) => AuthUserValid()); }
        }


        /// <summary>
        /// Команда открытия окна авторизации
        /// </summary>
        private MyCommand _authorizationCD;
        public MyCommand AuthorizationCD
        {
            get { return _authorizationCD ?? new MyCommand((obj) => Authorization(), (obj) => AutorizationValid()); }
        }


        /// <summary>
        /// Команда открытия окна регистрации
        /// </summary>
        private MyCommand _openRegWindowCD;
        public MyCommand OpenRegWindowCD
        {
            get { return _openRegWindowCD ?? new MyCommand((obj) => OpenRegWindow(), (obj) => OpenRegWindowValid()); }
        }

        /// <summary>
        /// Изменение изображения
        /// </summary>
        private MyCommand _setImageCD;
        public MyCommand SetImageCD
        {
            get { return _setImageCD ?? new MyCommand((obj) => SetImage()); }
        }

        
        private MyCommand _dropImageCD;
        public MyCommand DropImageCD
        {
            get { return _dropImageCD ?? new MyCommand((obj) => DropMessage()); }
        }
        #endregion

        #region METHOD DATAVM

        /// <summary>
        /// Обновление списка сообщений и списка активных пользователей
        /// </summary>
        private async void UpdateListUser(object? obj)
        {
            if (IsAuth)
            {
                try
                {
                    StartAuth();
                    List<UserConnect>? data = await _client.GetFromJsonAsync<List<UserConnect>>($"{_host}/api/musers/{ThisUser.UserId}");
                    List<Message> lM = await _client.GetFromJsonAsync<List<Message>>($"{_host}/api/umessage/{ThisUser.UserId}");
                    if (data != null && lM != null)
                    {
                        List<UsersMessage> uM = new List<UsersMessage>();
                        foreach (var d in data)
                        {
                            UsersMessage users = new UsersMessage(d.UserTo);
                            users.IsConnect = d.IsConnect;
                            users.UpdateMessage(lM, ThisUser);
                            byte[] im_byte = await _client.GetFromJsonAsync<byte[]>($"{_host}/api/user/images/{users.TUser.UserId}");
                            if(im_byte==null) im_byte=new byte[0];
                            users.ImageUser = im_byte;
                            uM.Add(users);
                        }
                        uM.Sort(delegate (UsersMessage x, UsersMessage y)
                        {
                            if (x.Messages != null && y.Messages != null)
                            {
                                if (x.Messages[x.Messages.Count - 1].DataMessage < y.Messages[y.Messages.Count - 1].DataMessage) return 1;
                                else if (x.Messages[x.Messages.Count - 1].DataMessage > y.Messages[y.Messages.Count - 1].DataMessage) return -1;
                            }
                            return 0;
                        });
                        ListUser = uM;
                        if (UserTo != null && UserTo.TUser != null)
                        {
                            UserTo = ListUser.FirstOrDefault(u => u.TUser.UserId == UserTo.TUser.UserId);
                            IsReadAsync();
                        }
                    }
                    else
                    {
                        _timer.Dispose();
                        MessageBox.Show("Ошибка при загрузке данных!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (HttpRequestException ex)
                {
                    _timer.Dispose();
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Открытие окна новых сообщений
        /// </summary>
        private void OpenNewMessage()
        {
            NewMessageVM vM = new NewMessageVM {UserId = ThisUser.UserId};
            NewMessage nM = new NewMessage();
            nM.DataContext = vM;
            SetCentreAndOpenWindow(nM);
        }

        /// <summary>
        /// Проверка аунтификации на сервере
        /// </summary>
        private void StartAuth()
        {
            string? accessToken;
            using (StreamReader sw = new StreamReader($"Token_{ThisUser.UserId}.txt"))
            {
                accessToken = sw.ReadLine();
            }
            if (accessToken != null)
            {
                _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            }
            else
            {
                MessageBox.Show("Ошибка чтения токена!");
            }
        }


        /// <summary>
        /// Открытие окна регистраии
        /// </summary>
        private void OpenRegWindow()
        {
            Registration reg = new Registration();
            SetCentreAndOpenWindow(reg);
        }

        /// <summary>
        /// Проверка активности команды открытия окна регистрации
        /// </summary>
        private bool OpenRegWindowValid()
        {
            return ThisUser.UserId == 0;
        }

        /// <summary>
        /// Открытие окна регистрации
        /// </summary>
        private async void Authorization()
        {
            try
                {
                ThisUser.UserId = 0;
                Authoriz auth = new Authoriz();
                SetCentreAndOpenWindow(auth);
                bool? result = auth.DialogResult;
                ThisUser.UserId = auth.UserId;
                if (result is not null && result == true)
                {
                    StartAuth();
                    object? sender = await _client.GetFromJsonAsync($"{_host}/api/user/{ThisUser.UserId}", typeof(User));
                    if (sender != null)
                    {
                        ThisUser = sender as User;
                        IsAuth = true;
                        ImageUser = await _client.GetFromJsonAsync<byte[]>($"{_host}/api/user/images/{ThisUser.UserId}");
                    }
                    _timer = new Timer(UpdateListUser, null, 0, 2000);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Проверка активности команды открытия окна авторизации
        /// </summary>
        private bool AutorizationValid()
        {
            return !IsAuth;
        }

        /// <summary>
        /// Отправка сообщения
        /// </summary>
        private async Task SendMessageAsync()
        {
            try
            {
                Message message = new Message { UserFrom = ThisUser.UserId, UserTo = UserTo.TUser.UserId, DataMessage = DateTime.Now, IsRead = false, Message1 = _message };
                StartAuth();
                var responseMessage = await _client.PostAsJsonAsync($"{_host}/api/message", message);
                Message = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Сообщение не отправлено!", "Состояние", MessageBoxButton.OK, MessageBoxImage.Information);
            }

        }

        /// <summary>
        /// Проверка активности команды открытия отправки сообщения
        /// </summary>
        private bool SendMessageValid()
        {
            return _message != "" && _userTo != null && IsAuth;
        }

        /// <summary>
        /// Проверка авторизован ли пользователь
        /// </summary>
        private bool AuthUserValid()
        {
            return IsAuth;
        }

        /// <summary>
        /// Смена пользователя
        /// </summary>
        private void ExitUser()
        {
            IsAuth = false;
            ThisUser = new User { UserId = 0 };
            ThisUser = new User();
            ListUser = new List<UsersMessage>();
            UserTo = new UsersMessage();
            ImageUser = null;
            if (_timer is not null)
            {
                _timer.Dispose();
                _timer = null;
            }
            Authorization();
        }

        /// <summary>
        /// Отправка сообщения на сервер, что сообщения были прочитаны
        /// </summary>
        private async void IsReadAsync()
        {
            List<int> IdMessage = UserTo.Messages.Where(u=>u.IsRead==false && u.UserTo == ThisUser.UserId).Select(u=>u.MessageId).ToList();
            if (IdMessage is null) return;
            StartAuth();
            var response = await _client.PutAsJsonAsync($"{_host}/api/message/isread", IdMessage);
        }
        
        private async void SetImage()
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Images Files|*.jpg;*.jpeg;*.png";
                byte[] image_byte;
                if (openFileDialog.ShowDialog() == true)
                {
                    image_byte = await File.ReadAllBytesAsync(openFileDialog.FileName);
                    StartAuth();
                    var response = await _client.PutAsJsonAsync<byte[]>($"{_host}/api/user/images/{ThisUser.UserId}", image_byte);
                    ImageUser = image_byte;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void DropMessage()
        {
            try
            {
                    byte[] image_byte = null;
                    StartAuth();
                    var response = await _client.PutAsJsonAsync<byte[]?>($"{_host}/api/user/images/{ThisUser.UserId}", image_byte);
                    ImageUser = image_byte;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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

        /// <summary>
        /// Метод открытия и центрирования окна
        /// </summary>
        private void SetCentreAndOpenWindow(Window window)
        {
            window.Owner = Application.Current.MainWindow;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.ShowDialog();
        }
    }
}
