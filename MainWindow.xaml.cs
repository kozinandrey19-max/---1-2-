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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Data.SqlClient;

namespace DolgoStrou
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. Получаем логин из TextBox и убираем лишние пробелы
            string login = LoginTextBox.Text.Trim();

            // 2. Проверка на пустоту
            if (string.IsNullOrEmpty(login))
            {
                MessageBox.Show("Пожалуйста, введите логин!", "Ошибка ввода",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 3. Используем using, чтобы соединение ЗАКРЫЛОСЬ автоматически после работы
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    // 4. ЕБУЧИЙ SQL-ЗАПРОС
                    // Мы берем Фамилию, Имя и Отчество, склеиваем их в одну строку (AS FIO)
                    // И берем название роли из таблицы Roles через JOIN
                    string query = @"SELECT u.LastName + ' ' + u.FirstName + ' ' + u.MiddleName AS FIO, 
                                            r.Name AS RoleName 
                                     FROM Users u 
                                     JOIN Roles r ON u.RoleId = r.Id 
                                     WHERE u.Login = @login";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // 5. Защита от SQL-инъекций: передаем логин как параметр, а не склеиваем строки
                        cmd.Parameters.AddWithValue("@login", login);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            // 6. Если reader.Read() вернул true, значит пользователь с таким логином НАЙДЕН
                            if (reader.Read())
                            {
                                string fio = reader["FIO"].ToString();
                                string role = reader["RoleName"].ToString();

                                // ОТКРЫВАЕМ КАТАЛОГ И ЗАКРЫВАЕМ ОКНО ВХОДА
                                CatalogWindow catalog = new CatalogWindow(fio, role);
                                catalog.Show();
                                this.Close();
                            }
                            else
                            {
                                // Если логина нет в базе
                                MessageBox.Show("Пользователь с таким логином не найден!\nПроверьте правильность ввода.",
                                    "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Если база лежит или строка подключения неверна
                MessageBox.Show($"Ошибка подключения к базе данных:\n{ex.Message}",
                    "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        
        }

        private void GuestButton_Click(object sender, RoutedEventArgs e)
        {
            // Для гостя просто передаем фиктивные данные
            MessageBox.Show("Вход выполнен как ГОСТЬ.\nДоступен только просмотр каталога.",
                "Гостевой вход", MessageBoxButton.OK, MessageBoxImage.Information);

            CatalogWindow catalog = new CatalogWindow("Гость", "Гость");
            catalog.Show();
            this.Close();
        }
    }
}
