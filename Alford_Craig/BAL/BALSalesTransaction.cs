using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Alford_Craig.Models;
using System.Data.SqlClient;
using Alford_Craig.Controllers.DAL;

namespace Alford_Craig.BAL
{
    public class BALSalesTransaction
    {
        private readonly IConfiguration configuration;

        public BALSalesTransaction(IConfiguration config)
        {
            this.configuration = config;
        }


        internal void InsertSalesTransaction(SalesTransaction st)
        {

            // create, open connection
            SqlConnection conn = new SqlConnection()
            {
                ConnectionString = configuration.GetConnectionString("BCConnString")
            };
            conn.Open();

            // create query
            String query = "INSERT INTO [dbo].[SalesTransaction] ([ProductID],[PersonID],[SalesDataTime],[PQuantity])"
                + " VALUES (@ProductID, @PersonID, GetDate(), @PQuantity)";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@ProductID", st.Product.PID);
            cmd.Parameters.AddWithValue("@PersonID", st.Person.PersonID);
            cmd.Parameters.AddWithValue("@PQuantity", st.PurchasedQuantity);

            // execute query get new key field
            cmd.ExecuteNonQuery();

            // close connection
            conn.Close();
        }

        internal LinkedList<SalesTransaction> GetSalesTransactions()
        {
            // create, open connection
            SqlConnection conn = new SqlConnection()
            {
                ConnectionString = configuration.GetConnectionString("BCConnString")
            };
            conn.Open();

            // create query
            String query = "SELECT [SalesID],[ProductID],[PersonID], Format([SalesDataTime], 'yyyy-MM-dd') as SoldDate,[PQuantity] " +
                "FROM [dbo].[SalesTransaction] " +
                "ORDER BY [SalesDataTime];";
            SqlCommand cmd = new SqlCommand(query, conn);

            // load data to LinkedList
            LinkedList<SalesTransaction> sts = new LinkedList<SalesTransaction>();

            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                SalesTransaction st = new SalesTransaction
                {
                    SaleID = Convert.ToInt32(reader["SalesID"]),
                    Product = new DALProducts(configuration).GetProduct(reader["ProductID"].ToString()),
                    Person = new DALPerson(configuration).GetPerson(Convert.ToInt32(reader["PersonID"])),
                    PurchasedQuantity = Convert.ToInt32(reader["PQuantity"]),
                    SalesDataTime = Convert.ToDateTime(reader["SoldDate"])
                };

                sts.AddLast(st);
            }

            // close connection
            conn.Close();

            return sts;
        }
    }
}
