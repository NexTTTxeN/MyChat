using EntityChatDB;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Text;
using System.Net;
using Microsoft.Extensions.FileSystemGlobbing.Internal.Patterns;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace ChatServer
{
    public class Program
    {
        static Connecting conn = new Connecting();
        public static async Task Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<ChatDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddAuthorization();
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = AuthOptions.ISSUER,
                    ValidateAudience = true,
                    ValidAudience = AuthOptions.AUDIENCE,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey()
                };
            });

            var app = builder.Build();

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapGet("/", (ChatDbContext context) =>
            {

            });


            /// <summary>
            /// Аунтификация пользователей
            /// </summary>
            app.MapPost("/api/auth/login", (Person loginData, ChatDbContext context) =>
            {
                User? person = context.Users.FirstOrDefault(p => p.UserName.ToLower() == loginData.Login.ToLower() && p.UserPassword == loginData.Password);
                if (person is null)
                {
                    return Results.Unauthorized();
                }

                var claims = new List<Claim> { new Claim(ClaimTypes.Name, person.UserId.ToString()) };

                var token = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(1),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
                );

                var encodedToken = new JwtSecurityTokenHandler().WriteToken(token);

                var response = new
                {
                    access_token = encodedToken,
                    userId = person.UserId
                };
                return Results.Json(response);
            });


            /// <summary>
            /// Регистрация пользователей
            /// </summary>
            app.MapPost("/api/regist", async (Person loginData, ChatDbContext context) =>
            {
                if (context.Users.FirstOrDefault(u => u.UserName.ToLower() == loginData.Login.ToLower()) != null) return Results.Problem("Логин занят");
                try
                {
                    User user = new User { UserId = NextUserId(context), UserName = loginData.Login, UserPassword = loginData.Password };
                    context.Users.Add(user);
                    await context.SaveChangesAsync();
                    return Results.Ok();
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex);
                }
            });


            /// <summary>
            /// Возвращение информации по пользователю
            /// </summary>
            app.MapGet("/api/user/{id}", [Authorize] (int id, ChatDbContext context) =>
            {

                User? auth = GetUser(id, context);
                if (auth is not null)
                {
                    auth.UserPassword = "";
                    return Results.Json(auth);
                }
                return Results.NotFound(new { Message = "Пользователь не найден" });
            });


            /// <summary>
            /// Замена изображения пользователя
            /// </summary>
            app.MapPut("/api/user/images/{id}", [Authorize] async (int id, byte[]? image_byte, ChatDbContext context) =>
            {
                try
                {
                    User? us = context.Users.FirstOrDefault(u => u.UserId == id);
                    if (us is null) return Results.NotFound(new { Message = "Пользователь не найден" });
                    Image image = new Image { Screen = image_byte, ImagesId = NextImageId(context)};
                    context.Images.Add(image);
                    us.ScreenId = image.ImagesId;
                    await context.SaveChangesAsync();
                    User auth = GetUser(id, context);
                    auth.UserPassword = "";
                    return Results.Ok(null);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex);
                }
            });
            /// <summary>
            /// Отправка изображения
            /// </summary>
            app.MapGet("/api/user/images/{id}", [Authorize] async (int id, ChatDbContext context) =>
            {
                User? us = context.Users.FirstOrDefault(u => u.UserId == id);
                byte[]? image_byte = null;
                if (us!=null && us.ScreenId!=null)
                image_byte = context.Images.FirstOrDefault(i => i.ImagesId == us.ScreenId).Screen;
                return Results.Json(image_byte);
            });

            /// <summary>
            /// Получение списка сообзений пользователя
            /// </summary>
            app.MapGet("/api/message/{id}", [Authorize] (int id, ChatDbContext context) =>
            {
                User? auth = context.Users.FirstOrDefault(u => u.UserId == id);
                if (auth == null) return Results.NotFound(new { Message = "Пользователь не найден" });
                List<Message> messages = context.Messages.Where(u => u.UserFrom == auth.UserId || u.UserTo == auth.UserId).ToList();
                return Results.Json(messages);
            });


            /// <summary>
            /// Запись сообщения в БД
            /// </summary>
            app.MapPost("/api/message", [Authorize] async (Message message, ChatDbContext context) =>
            {
                try
                {
                    message.MessageId = NextMessageId(context);
                    context.Messages.Add(message);
                    await context.SaveChangesAsync();
                    return Results.Ok();
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex);
                }
            });


            /// <summary>
            /// Получение и оработка прочитанных сообщений
            /// </summary>
            app.MapPut("/api/message/isread", [Authorize] async (List<int> IdMessages, ChatDbContext context) =>
            {
                foreach (int id in IdMessages)
                {
                    Message? ms = context.Messages.FirstOrDefault(m => m.MessageId == id);
                    if (ms is not null) ms.IsRead = true;
                }
                await context.SaveChangesAsync();
                return Results.Ok();

            });


            /// <summary>
            /// Отправка списка всех авторизованных пользователей
            /// </summary>
            app.MapGet("/api/users/all/{id}", [Authorize] (int id, ChatDbContext context) =>
            {
                User? auth = context.Users.FirstOrDefault(u => u.UserId == id);
                if (auth == null) return Results.NotFound(new { Message = "Пользователь не найден" });
                List<User> users = context.Users.Where(u => u.UserId != auth.UserId).ToList();
                foreach (User user in users)
                {
                    user.UserPassword = "";
                }
                return Results.Json(users);
            });


            /// <summary>
            /// Отправка списка с пользоватей с которыми открыты чаты
            /// </summary>
            app.MapGet("/api/musers/{id}", [Authorize] (int id, ChatDbContext context) =>
            {
                User? auth = context.Users.FirstOrDefault(u => u.UserId == id);
                if (auth == null) return Results.NotFound(new { Message = "Пользователь не найден" });
                List<UserConnect> usersMessages = new List<UserConnect>();
                conn.Connect(id);
                List<Message> messages = context.Messages.Where(u => u.UserFrom == auth.UserId || u.UserTo == auth.UserId).ToList();
                foreach (Message message in messages)
                {
                    User? tempUser = null;
                    if (message.UserFrom != id && usersMessages.FirstOrDefault(um => um.UserTo.UserId == message.UserFrom) == null)
                        tempUser = context.Users.FirstOrDefault(u => u.UserId == message.UserFrom).Clone() as User;
                    else if (message.UserTo != id && usersMessages.FirstOrDefault(um => um.UserTo.UserId == message.UserTo) == null)
                        tempUser = context.Users.FirstOrDefault(u => u.UserId == message.UserTo).Clone() as User;
                    if (tempUser is not null)
                    {
                        tempUser.UserPassword = "";
                        tempUser.MessageUserFromNavigations = null;
                        tempUser.MessageUserToNavigations = null;
                        usersMessages.Add(new UserConnect ( tempUser, conn.UserTime.FirstOrDefault(u=>u.UserId == tempUser.UserId)!=null));
                    }
                }
                return Results.Json(usersMessages);
            });


            /// <summary>
            /// Отправка списка сообщений с пользователями у которых открыты чаты
            /// </summary>
            app.MapGet("/api/umessage/{id}", [Authorize] (int id, ChatDbContext context) =>
            {
                User? auth = context.Users.FirstOrDefault(u => u.UserId == id);
                if (auth == null) return Results.NotFound(new { Message = "Пользователь не найден" });
                List<Message> messages = context.Messages.Where(u => u.UserFrom == auth.UserId || u.UserTo == auth.UserId).ToList();
                foreach (Message message in messages)
                {
                    message.UserFromNavigation = null;
                    message.UserToNavigation = null;
                }
                return Results.Json(messages);
            });
            app.Run();
        }


        /// <summary>
        /// Получение копии пользователя из базы
        /// </summary>
        static private User? GetUser(int id, ChatDbContext context)
        {
            if (context == null) return null;
            User? tempUser = context.Users.FirstOrDefault(u => u.UserId == id);
            if (tempUser == null) return null;
            return tempUser.Clone() as User;
        }

        /// <summary>
        /// Получение ID для нового пользователя
        /// </summary>
        static public int NextUserId(ChatDbContext db)
        {
            try
            {
                int id = db.Users.Max(u => u.UserId);
                return ++id;
            }
            catch (Exception ex)
            {
                return 1000;
            }
        }

        /// <summary>
        /// Получение ID для нового сообщения
        /// </summary>
        static public int NextMessageId(ChatDbContext db)
        {
            try
            {
                int id = db.Messages.Max(u => u.MessageId);
                return ++id;
            }
            catch (Exception ex)
            {
                return 1000;
            }
        }
        /// <summary>
        /// Получение ID для нового изобрадения
        /// </summary>
        static public int NextImageId(ChatDbContext db)
        {
            try
            {
                int id = db.Images.Max(u => u.ImagesId);
                return ++id;
            }
            catch (Exception ex)
            {
                return 1000;
            }
        }
    }


    public class AuthOptions
    {
        public const string ISSUER = "MyAuthServer";
        public const string AUDIENCE = "MyAuthClient";
        /*!!!
         Для алгоритма SecurityAlgorithms.HmacSha256 длина ключа должна быть не меньше 256 бит
         !!!*/
        const string KEY = "my_secret_key_udmy_secret_key_ud";
        public static SymmetricSecurityKey GetSymmetricSecurityKey() => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
    }


    /// <summary>
    /// Метод проверки активности пользователей
    /// </summary>
    class Connecting
    {

        /// <summary>
        /// хранит пользователя и его последние обращение к серверу
        /// </summary>
        public class UserTimeConnect
        {
            public int UserId { get; set; }

            /// <summary>
            /// Время последнего обращения к серверу
            /// </summary>
            public DateTime? Time { get; set; }
        }

        /// <summary>
        /// Список активных пользователей
        /// </summary>
        public List<UserTimeConnect> UserTime { get; private set; }

        Timer _timer;


        /// <summary>
        /// Период срабатывания таймера
        /// </summary>
        int _tick;

        public Connecting()
        {
            UserTime = new List<UserTimeConnect>();
            _tick = 5000;
            _timer = new Timer(Disconnecting, null, 0, _tick);
        }

        /// <summary>
        /// Проявление активности пользователя
        /// </summary>
        public void Connect(int userId)
        {
            UserTimeConnect? userTime = UserTime.FirstOrDefault(t => t.UserId == userId);
            if (userTime is not null) UserTime.Remove(userTime);
            else userTime = new UserTimeConnect { UserId = userId };
            userTime.Time = DateTime.Now;
            UserTime.Add(userTime);
        }
        /// <summary>
        /// Отключение пользователей время ожидания которых превысило допускаемое
        /// </summary>
        public void Disconnecting(object obj)
        {
            if (UserTime.Count > 0)
            {
                TimeSpan? t = DateTime.Now - UserTime[0].Time;
                int ms = t.Value.Seconds;
                if (ms > 5)
                {
                    UserTime.Remove(UserTime[0]);
                    Disconnecting(obj);
                }
            }
        }
    }
    /// <summary>
    /// Класс для регистрации и аутнтификации
    /// </summary>
    record class Person(string Login, string Password);
}