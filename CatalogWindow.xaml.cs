using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Media;

namespace DolgoStrou
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public string Name { get; set; }
        public string Manufacturer { get; set; }
        public string Composition { get; set; }
        public decimal Price { get; set; }
        public string ImagePath { get; set; }

        public int TotalQuantity { get; set; }
        public string QuantityLabel { get; set; }
        public bool HasDiscount { get; set; }

        public string OldPrice => HasDiscount ? Price.ToString("F0") : "";
        public string NewPrice => HasDiscount ? (Price * 0.75m).ToString("F2") : Price.ToString("F2");
        public Visibility OldPriceVisibility => HasDiscount ? Visibility.Visible : Visibility.Collapsed;
        public Visibility DiscountBadgeVisibility => HasDiscount ? Visibility.Visible : Visibility.Collapsed;

        public Brush CardBackground { get; set; }
    }

    public partial class CatalogWindow : Window
    {
        private string userRole;
        private string userFio;

        public CatalogWindow(string fio, string role)
        {
            InitializeComponent();

            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                return;

            userFio = fio;
            userRole = role;
            FioText.Text = fio; // Отображаем ФИО в правом верхнем углу (по ТЗ)
            Title = $"Чудо Обувь — Каталог ({role})";

            LoadProducts();
        }

        private void LoadProducts()
        {
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                // ЕБУЧИЙ УМНЫЙ ЗАПРОС:
                // 1. SUM(si.Quantity) считает общее кол-во по всем размерам модели
                // 2. EXISTS проверяет, были ли заказы на этот товар в ПРЕДЫДУЩЕМ календарном месяце
                string query = @"
                    DECLARE @PrevMonth INT = MONTH(DATEADD(month, -1, GETDATE()));
                    DECLARE @PrevYear INT = YEAR(DATEADD(month, -1, GETDATE()));

                    SELECT 
                        p.Id,
                        c.Name AS Category,
                        p.Name,
                        m.Name AS Manufacturer,
                        p.Composition,
                        p.Price,
                        p.Image,
                        ISNULL(SUM(si.Quantity), 0) AS TotalQuantity,
                        CASE 
                            WHEN EXISTS (
                                SELECT 1 FROM Orders o
                                JOIN OrderDetails od ON o.Id = od.OrderId
                                WHERE od.ProductId = p.Id
                                  AND MONTH(o.OrderDate) = @PrevMonth
                                  AND YEAR(o.OrderDate) = @PrevYear
                            ) THEN 0 
                            ELSE 1 
                        END AS HasDiscount
                    FROM Products p
                    JOIN Categories c ON p.CategoryId = c.Id
                    JOIN Manufacturers m ON p.ManufacturerId = m.Id
                    LEFT JOIN Stock_Items si ON p.Id = si.ProductId
                    GROUP BY p.Id, c.Name, p.Name, m.Name, p.Composition, p.Price, p.Image
                    ORDER BY p.Name";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                var products = new List<ProductViewModel>();
                foreach (DataRow row in dt.Rows)
                {
                    int totalQty = Convert.ToInt32(row["TotalQuantity"]);
                    bool hasDiscount = Convert.ToInt32(row["HasDiscount"]) == 1;

                    // Логика "много" / "мало" (порог 5 по ТЗ)
                    string qtyLabel = totalQty > 5 ? "много" : "мало";

                    // Логика подсветки: #ff8080 если остаток <= 3
                    Brush bg = Brushes.White;
                    if (totalQty <= 3)
                    {
                        bg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff8080"));
                    }

                    // Путь к картинке (с заглушкой picture.png)
                    string imageName = row["Image"].ToString();
                    string imagePath = string.IsNullOrEmpty(imageName)
                        ? "pack://application:,,,/Resources/picture.png"
                        : $"pack://application:,,,/Resources/{imageName}";

                    products.Add(new ProductViewModel
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Category = row["Category"].ToString(),
                        Name = row["Name"].ToString(),
                        Manufacturer = row["Manufacturer"].ToString(),
                        Composition = row["Composition"].ToString(),
                        Price = Convert.ToDecimal(row["Price"]),
                        ImagePath = imagePath,
                        TotalQuantity = totalQty,
                        QuantityLabel = qtyLabel,
                        HasDiscount = hasDiscount,
                        CardBackground = bg
                    });
                }

                ProductsItemsControl.ItemsSource = products;
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show(); // Возвращаемся на экран входа
            this.Close();
        }
    }
}