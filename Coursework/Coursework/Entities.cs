using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Media.Imaging;

namespace Coursework.Entities
{
    public class User
    {
        [Key] 
        public int Id { get; set; }  
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
    }

    public class Category
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public string ProductImage { get; set; }

        [NotMapped]
        public BitmapImage ImageSource { get; set; }
    }

    public class Supplier
    {
        public int SupplierID { get; set; }
        public string SupplierName { get; set; }
        public string ContactName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
    }

    public class Customer
    {
        public int CustomerID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }

    public class Order
    {
        public int OrderID { get; set; }
        public int CustomerID { get; set; }

        public int EmployeeID { get; set; } 

        [ForeignKey("EmployeeID")]
        public virtual Employee Employee { get; set; }

        public int TotalAmount { get; set; }

        [NotMapped]
        public DateTime StartDate { get; set; }

        [NotMapped]
        public DateTime EndDate { get; set; }
    }


    public class Shipment
    {
        public int ShipmentID { get; set; }
        public int SupplierID { get; set; }

        public int EmployeeID { get; set; }

        public string TotalCost { get; set; }

        [NotMapped]
        public DateTime ShipmentDate { get; set; }

        [ForeignKey("EmployeeID")]
        public virtual Employee Employee { get; set; }
    }

    public class Product
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }

        public int? CategoryID { get; set; }
        public virtual Category Category { get; set; }
    }

    public class Employee
    {
        public int EmployeeID { get; set; }
        public int UserID { get; set; }  // Это внешний ключ, который указывает на сущность User

        // Навигационное свойство, которое будет ссылаться на сущность User
        public virtual User User { get; set; }
    }

}
