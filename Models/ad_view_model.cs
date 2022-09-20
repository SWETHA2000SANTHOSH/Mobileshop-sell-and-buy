using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobileShopWebsite.Models
{
    public class ad_view_model
    {
        public int pdt_id { get; set; }
        public string pdt_name { get; set; }
        public string pdt_img { get; set; }
        public Nullable<int> pdt_price { get; set; }
        public string pdt_desc { get; set; }

        public Nullable<int> cat_id_fk { get; set; }
        public Nullable<int> pdt_user_id_fk { get; set; }
        public int cat_id { get; set; }
        public string cat_name { get; set; }
        public string u_name { get; set; }
        public string u_img { get; set; }
        public string u_contact { get; set; }
    }
}