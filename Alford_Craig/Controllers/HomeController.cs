using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alford_Craig.Controllers.DAL;
using Alford_Craig.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace Alford_Craig.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration configuration;
        public HomeController( IConfiguration config)
        {
           this.configuration = config;
        }

        public IActionResult AddPerson()
        {
            return View("PersonAdd");
        }

        public IActionResult DisplayPerson(Person p, string password)
        { 
            DALPerson dp = new DALPerson(configuration);
            int returnValue = dp.InsertPerson(p, password);
            switch (returnValue)
            {
                case -1:
                    ViewBag.AddPersonError = "User Name already in use.  Please Login or use a different name.";
                    return View("PersonAdd");
                case -2:
                    ViewBag.AddPersonError = "Email already in use.  Please Login or use a different email.";
                    return View("PersonAdd");
                default:
                    ViewBag.AddPersonError = null;
                    p.PersonID = returnValue;
                    HttpContext.Session.SetString("uID", p.PersonID.ToString());
                    HttpContext.Session.SetString("uFName", p.FName);

                    ViewBag.UserFirstName = p.FName;
                    ViewBag.HeaderWelome = "Welcome, " + p.FName + "!";
                    return View("PersonDisplay", p);
            }
        }

        public IActionResult EditPerson(Person p)
        {
            if (HttpContext.Session.GetString("uID") == null)
            {
                ViewBag.LoginError = "User not logged in.";
                return RedirectToAction("Index", "Alford");
            }

            int personID = Convert.ToInt32(HttpContext.Session.GetString("uID"));
            p.PersonID = personID;

            DALPerson dp = new DALPerson(configuration);
            p = dp.GetPerson(personID);

            return View("PersonEdit", p);
        }

        public IActionResult UpdatePerson(Person p)
        {
            int personID = Convert.ToInt32(HttpContext.Session.GetString("uID"));
            p.PersonID = personID;

            DALPerson dp = new DALPerson(configuration);
            dp.UpdatePerson(p);

            return View("PersonDisplay", p);
        }
    }
}