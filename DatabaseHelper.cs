using Microsoft.Data.SqlClient;

namespace DolgoStrou
{
    // Статический класс, чтобы не создавать его через new
    public static class DatabaseHelper
    {
        // СТРОКА ПОДКЛЮЧЕНИЯ
        // Data Source=PULSEANDREY - это имя твоего сервера (мы выяснили это ранее)
        // Initial Catalog=ChudoObuv - имя нашей новой базы
        // Encrypt=False - отключает ебучую ошибку SSL-сертификата
        public static string ConnectionString =
            "Data Source=PULSEANDREY;Initial Catalog=ChudoObuv;Integrated Security=True;Encrypt=False;TrustServerCertificate=True";

        // Метод, который создает и СРАЗУ открывает соединение
        public static SqlConnection GetConnection()
        {
            SqlConnection conn = new SqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }
    }
}