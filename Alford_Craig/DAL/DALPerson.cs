using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Alford_Craig.Models;
using Microsoft.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace Alford_Craig.Controllers.DAL
{
    public class DALPerson
    {
        private readonly IConfiguration configuration;
        private readonly string personConnectionString;

        public DALPerson(IConfiguration config)
        {
            this.configuration = config;
            this.personConnectionString = configuration.GetConnectionString("BCConnString");
        }

        internal int InsertPerson(Person p, string pwd)
        { 
            #region open connection

            SqlConnection conn = new SqlConnection
            {
                ConnectionString = personConnectionString
            };
            conn.Open();

            #endregion

            #region check for existing username

            String query = "SELECT PersonID " +
                " FROM Person" +
                " WHERE UserName = @UserName";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Username", p.UserName);

            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return -1; //User Name already exists
            }
            reader.Close();

            #endregion

            #region check for existing email

            cmd.CommandText = "SELECT PersonID " +
                " FROM Person" +
                " WHERE email = @email";

            cmd.Parameters.AddWithValue("@email", p.email);

            reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return -2; //email already exists
            }
            reader.Close();

            #endregion

            #region add person to database
            cmd.CommandText = "INSERT INTO dbo.Person(FName, LName, email, phone, address, Username)"
                + " Values(@FName, @LName, @email, @phone, @address, @Username)"
                + " SELECT Scope_Identity();";

            cmd.Parameters.AddWithValue("@FName", p.FName);
            cmd.Parameters.AddWithValue("@LName", p.LName);
            cmd.Parameters.AddWithValue("@phone", p.phone);
            cmd.Parameters.AddWithValue("@address", p.address);

            // execute query get new key field
            reader = cmd.ExecuteReader();
            reader.Read();

            int personID = Convert.ToInt32(reader[0]);

            p.PersonID = personID;

            // close reader
            reader.Close();

            #endregion

            #region insert credentials

            // change cmd.CommandText
            cmd.CommandText = "INSERT INTO dbo.Credentials(PersonID, Password)"
                + " VALUES (@PersonID, @Password);";
            cmd.Parameters.AddWithValue("@PersonID", personID);
            cmd.Parameters.AddWithValue("@Password", pwd);

            // execute command
            cmd.ExecuteNonQuery();

            #endregion

            #region close connection

            conn.Close();

            #endregion

            return personID;
        }

        internal PersonShort CheckLoginCredentials(LoginCredentials lic)
        {
            // create, open connection
            SqlConnection conn = new SqlConnection
            {
                ConnectionString = personConnectionString
            };
            conn.Open();

            // create query
            String query = "SELECT [Person].[PersonID],[FName]" +
                " FROM [dbo].[Person]" +
                " JOIN [dbo].[Credentials] ON [Person].[PersonID] = [Credentials].[PersonID]" +
                " WHERE [Person].Username = @UserName" +
                " AND [Credentials].Password = @Password;";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserName", lic.UserName);
            cmd.Parameters.AddWithValue("@Password", lic.Password);

            PersonShort ps = null;

            // execute query get new key field
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read()) {
                // create PersonShort to fill and return
                ps = new PersonShort();
                ps.PersonID = Convert.ToInt32(reader["PersonID"]);
                ps.FName = reader["FName"].ToString();
            }

            // close connection
            conn.Close();

            // return Person p
            return ps;
        }

        internal Person GetPerson(int personID)
        {
            // create, open connection
            SqlConnection conn = new SqlConnection
            {
                ConnectionString = personConnectionString
            };
            conn.Open();

            // create query
            String query = "SELECT PersonID, FName, LName, email, phone, address, UserName"
                + " FROM dbo.Person"
                + " WHERE PersonID = @personID;";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@personID", personID);

            // execute query get new key field
            SqlDataReader reader = cmd.ExecuteReader();
            reader.Read();

            // create Person to fill and return
            Person p = new Person
            {
                FName = reader["FName"].ToString(),
                LName = reader["LName"].ToString(),
                email = reader["email"].ToString(),
                phone = reader["phone"].ToString(),
                address = reader["address"].ToString(),
                UserName = reader["UserName"].ToString(),
                PersonID = Convert.ToInt32(reader["PersonID"])
            };

            // close connection
            conn.Close();

            // return Person p
            return p;
        }

        internal void UpdatePerson(Person p)
        {
            // create, open connection
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = personConnectionString;
            conn.Open();

            // create query
            String query = "UPDATE [dbo].[Person] SET" +
                " [FName] = @FName " +
                ",[LName] = @LName " +
                ",[email] = @email " +
                ",[phone] = @phone " +
                ",[address] = @address" +
                " WHERE PersonID = @PersonID";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@FName", p.FName);
            cmd.Parameters.AddWithValue("@LName", p.LName);
            cmd.Parameters.AddWithValue("@email", p.email);
            cmd.Parameters.AddWithValue("@phone", p.phone);
            cmd.Parameters.AddWithValue("@address", p.address);
            cmd.Parameters.AddWithValue("@PersonID", p.PersonID);

            // execute command
            cmd.ExecuteNonQuery();

            // close connection
            conn.Close();
        }

        internal void DeletePerson(int personID)
        {
            // create, open connection
            SqlConnection conn = new SqlConnection
            {
                ConnectionString = personConnectionString
            };
            conn.Open();

            // create query
            /// delete credential
            String query = "DELETE FROM dbo.Credentials"
                + " WHERE PersonID = @personID;";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@personID", personID);

            // execute query get new key field
            cmd.ExecuteNonQuery();
            
            /// delete person
            cmd.CommandText = "Update dbo.Credentials SET " +
                "Password = 'deleted on ' + GetDate()" +
                " WHERE PersonID = @personID;";
            //cmd.Parameters.AddWithValue("@personID", personID);

            // execute query get new key field
            cmd.ExecuteNonQuery();

            // close connection
            conn.Close();
        }
    }
}
