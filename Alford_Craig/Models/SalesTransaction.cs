using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Alford_Craig.Models
{
    public class SalesTransaction
    {
        public int SaleID { get; set; }
        public Person Person { get; set; }
        public Products Product { get; set; }
        public double ListPrice { get; set; }
        public int PurchasedQuantity { get; set; }
        public DateTime SalesDataTime { get; set; }
    }
}
