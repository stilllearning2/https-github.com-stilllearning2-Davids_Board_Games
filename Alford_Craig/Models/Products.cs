using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Alford_Craig.Models
{
    public class Products
    {
        public string PID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public int InventoryAmount { get; set; }
    }
}
