using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobileShopWebsite.Models
{
    public class cart
    {
        public int pdt_id { get; set; }
        public string pdt_name { get; set; }
        public Nullable<int> pdt_price { get; set; }
        public Nullable<int> o_qty { get; set; }
        public Nullable<double> o_bill { get; set; }
    }
}