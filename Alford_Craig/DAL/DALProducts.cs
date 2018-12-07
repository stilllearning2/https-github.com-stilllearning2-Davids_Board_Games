using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Alford_Craig.Models;
using System.Data.SqlClient;

namespace Alford_Craig.Controllers.DAL
{
    public class DALProducts
    {
        private readonly IConfiguration configuration;
        private readonly string productConnectionString;

        public DALProducts(IConfiguration config)
        {
            configuration = config;
            productConnectionString = configuration.GetConnectionString("BCConnString");
        }

        internal Products InsertProduct(Products p)
        {
            // create, open connection
            SqlConnection conn = new SqlConnection()
            {
                ConnectionString = productConnectionString
            };
            conn.Open();

            // create query
            String query = "INSERT INTO [dbo].[Products] ([Name], [Description], [Price], [InventoryAmount])"
                + " VALUES (@Name, @Description, @Price, @InventoryAmount)"
                + " SELECT Scope_Identity() as pid;";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Name", p.Name);
            cmd.Parameters.AddWithValue("@Description", p.Description);
            cmd.Parameters.AddWithValue("@Price", p.Price);
            cmd.Parameters.AddWithValue("@InventoryAmount", p.InventoryAmount);

            // execute query get new key field
            SqlDataReader reader = cmd.ExecuteReader();
            reader.Read();

            // Add key field to Products object
            p.PID = reader[0].ToString();

            // close connection
            conn.Close();

            return p;
        }

        internal Products GetProduct(string productID)
        {

            // create, open connection
            SqlConnection conn = new SqlConnection()
            {
                ConnectionString = productConnectionString
            };
            conn.Open();

            // create query
            String query = "SELECT [PID], [Name], [Description], [Price], [InventoryAmount]"
                + " FROM [dbo].[Products]"
                + " WHERE PID = @PID;";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@PID", productID);

            // execute query get new key field
            SqlDataReader reader = cmd.ExecuteReader();
            reader.Read();

            // Instantiate a product to contain the results
            Products p = new Products
            {
                Name = reader["Name"].ToString(),
                Description = reader["Description"].ToString(),
                Price = Convert.ToDouble(reader["Price"]),
                InventoryAmount = Convert.ToInt32(reader["InventoryAmount"]),
                PID = reader["PID"].ToString()
            };

            // close connection
            conn.Close();

            return p;
        }

        internal Products EditProduct(Products p)
        {

            // create, open connection
            SqlConnection conn = new SqlConnection()
            {
                ConnectionString = productConnectionString
            };
            conn.Open();

            // create query
            String query = "UPDATE [dbo].[Products] SET" +
                " [Name] = @Name>" +
                ",[Description] = @Description" +
                ",[Price] = @Price" +
                ",[InventoryAmount] = @InventoryAmount" +
                " WHERE PID = @PID;";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Name", p.Name);
            cmd.Parameters.AddWithValue("@Description", p.Description);
            cmd.Parameters.AddWithValue("@Price", p.Price);
            cmd.Parameters.AddWithValue("@InventoryAmount", p.InventoryAmount);
            cmd.Parameters.AddWithValue("@PID", p.PID);

            // execute query get new key field
            cmd.ExecuteNonQuery();

            // close connection
            conn.Close();

            return p;
        }

        internal void DecreaseInventoryByOne(string pid, int quantityPurchased)
        {

            // create, open connection
            SqlConnection conn = new SqlConnection()
            {
                ConnectionString = productConnectionString
            };
            conn.Open();

            // create query
            String query = "UPDATE [dbo].[Products] SET" +
                " [InventoryAmount] = [InventoryAmount] - " + quantityPurchased.ToString() +
                " WHERE PID = @PID;";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@PID", pid);

            // execute query get new key field
            cmd.ExecuteNonQuery();

            // close connection
            conn.Close();
        }

        internal LinkedList<Products> GetProducts()
        {
            // create, open connection
            SqlConnection conn = new SqlConnection()
            {
                ConnectionString = productConnectionString
            };
            conn.Open();

            // create query
            String query = "SELECT PID, Name, Description, Price, InventoryAmount " +
                "FROM [dbo].[Products] " +
                "ORDER BY Name;";
            SqlCommand cmd = new SqlCommand(query, conn);

            // load data to LinkedList
            LinkedList<Products> products = new LinkedList<Products>();

            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Products p = new Products
                {
                    PID = reader["PID"].ToString(),
                    Name = reader["Name"].ToString(),
                    Description = reader["Description"].ToString(),
                    Price = Convert.ToDouble(reader["Price"]),
                    InventoryAmount = Convert.ToInt32(reader["InventoryAmount"])
                };

                products.AddLast(p);
            }

            // close connection
            conn.Close();

            return products;
        }
    }
}


