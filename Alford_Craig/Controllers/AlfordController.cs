using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Alford_Craig.Models;
using Microsoft.Extensions.Configuration;
using Alford_Craig.Controllers.DAL;
using Microsoft.AspNetCore.Http;
using Alford_Craig.BAL;

namespace Alford_Craig.Controllers
{
    public class AlfordController : Controller
    {

        private readonly IConfiguration configuration;

        public AlfordController(IConfiguration config)
        {
            this.configuration = config;
        }

        public IActionResult Index()
        {
            @ViewBag.Welcome = "";
            return View();
        }

        public IActionResult LogMeIn(LoginCredentials lic)
        {
            DALPerson dp = new DALPerson(configuration);
            PersonShort ps = dp.CheckLoginCredentials(lic);
            ViewBag.LoginMessage = "";
            if (ps == null) 
            {
                // bad credentials
                ViewBag.LoginError = "User or password incorrect.";
                return View("Index");
            }
            else
            {
                // good credentials
                HttpContext.Session.SetString("uID", ps.PersonID.ToString());
                HttpContext.Session.SetString("uFName", ps.FName);

                ViewBag.UserFirstName = ps.FName;
                return View("Home", ViewBag.UserFirstName);
            }
        }

        public IActionResult EnterNewProduct()
        {
            #region if not logged in, send to login page

            if (HttpContext.Session.GetString("uID") == null)
            {
                ViewBag.LoginError = "User not logged in.";
                return View("Index");
            }

            #endregion

            return View("ProductAdd");
        }

        public IActionResult InsertProduct(Products p)
        {
            #region if not logged in, send to login page

            if (IsUserLoggedIn())
            {
                ViewBag.LoginError = "User not logged in.";
                return View("Index");
            }

            #endregion

            DALProducts dp = new DALProducts(configuration);
            p = dp.InsertProduct(p);
            return View("ProductDisplay", p);
        }

        public IActionResult EditProduct(Products p)
        {
            #region if not logged in, send to login page

            if (IsUserLoggedIn())
            {
                ViewBag.LoginError = "User not logged in.";
                return View("Index");
            }

            #endregion

            DALProducts dp = new DALProducts(configuration);
            p = dp.EditProduct(p);
            return View("ProductDisplay");
        }

        public IActionResult ViewAllProducts()
        {
            DALProducts dp = new DALProducts(configuration);
            LinkedList<Products> products = dp.GetProducts();

            return View("ProductDisplayAll", products);
        }

        public IActionResult SalesReport()
        {
            // verify user is logged in
            if (HttpContext.Session.GetString("uID") == null)
            {
                ViewBag.LoginError = "User not logged in.";
                return View("Index");
            }

            BALSalesTransaction bst = new BALSalesTransaction(configuration);
            LinkedList<SalesTransaction> st = bst.GetSalesTransactions();

            return View("SalesTransactionDisplayAll", st);
        }

        public IActionResult Checkout()
        {
            #region if not logged in, send to login page

            if (IsUserLoggedIn())
            {
                ViewBag.LoginError = "User not logged in.";
                return View("Index");
            }

            #endregion

            return View();
        }

        public IActionResult Home()
        {
            if (HttpContext.Session.GetString("uFName") == null)
            {
                ViewBag.UserFirstName = "Visitor";
            }
            else
            {
                String firstName = HttpContext.Session.GetString("uFName");
                ViewBag.UserFirstName = firstName;
            }
            
            return View("Home", ViewBag.UserFirstName);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return View("Index");
        }

        public IActionResult ClickToBuy(int PID)
        {
            // verify user is logged in
            if (HttpContext.Session.GetString("uID") == null)
            {
                ViewBag.LoginError = "User not logged in.";
                return View("Index");
            }

            // retrive personID, Person
            int personID = Convert.ToInt32(HttpContext.Session.GetString("uID"));
            Person person = new DALPerson(configuration).GetPerson(personID);

            // retrieve product
            Products product = new DALProducts(configuration).GetProduct(PID.ToString());

            // decrease inventory
            int quantityPurchased = 1;
            new DALProducts(configuration).DecreaseInventoryByOne(product.PID, quantityPurchased);

            // create Sales Transaction
            SalesTransaction st = new SalesTransaction()
            {
                Person = person,
                Product = product,
                PurchasedQuantity = quantityPurchased
            };

            // insert Sales Transaction
            BALSalesTransaction bst = new BALSalesTransaction(configuration);
            bst.InsertSalesTransaction(st);

            // return view of sales transaction
            return View("ClickToBuy", st);
        }

        public Boolean IsUserLoggedIn()
        {
            return (HttpContext.Session.GetString("uID") == null);
        }

    }
}