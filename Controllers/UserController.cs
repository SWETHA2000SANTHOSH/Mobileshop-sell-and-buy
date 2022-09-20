using MobileShopWebsite.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList.Mvc;
using System.Xml;

namespace MobileShopWebsite.Controllers
{
    public class UserController : Controller
    {
        MobileShopDataEntities db = new MobileShopDataEntities();
        // GET: User
        public ActionResult Index(int? page)
        {
            if (TempData["cart"] != null)
            {
                double x = 0;
                List<cart> li2 = TempData["cart"] as List<cart>;
                foreach(var item in li2)
                {
                    x += Convert.ToInt32(item.o_bill);
                }
                TempData["total"] = x;
            }
            TempData.Keep();
            int pagesize = 7, pageindex = 1;
            pageindex = page.HasValue ? Convert.ToInt32(page) : 1;
            var list = db.categories.Where(x => x.cat_status == 1).OrderByDescending(x => x.cat_id).ToList();
            IPagedList<category> cate = list.ToPagedList(pageindex, pagesize);
            return View(cate);
        }
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(tbl_user us, HttpPostedFileBase imgfile)
        {
            string path = uploadimage(imgfile);
            if (path.Equals(-1))
            {
                ViewBag.error = "Image could not be uploaded.";
            }
            else
            {
                tbl_user u = new tbl_user();
                u.u_name = us.u_name;
                u.u_password = us.u_password;
                u.u_contact = us.u_contact;
                u.u_email = us.u_email;
                u.u_img = path;
                db.tbl_user.Add(u);
                db.SaveChanges();

                return RedirectToAction("Login");
            }
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(tbl_user svm)
        {
            tbl_user ad = db.tbl_user.Where(x => x.u_email == svm.u_email && x.u_password == svm.u_password).SingleOrDefault();
            if (ad != null)
            {
                Session["u_id"] = ad.u_id.ToString();
                Session["User"] = ad.u_name;
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.error = "Invalid email or password.";
            }
            return View();
        }

        [HttpGet]
        public ActionResult CreateAdd()
        {
            List<category> li = db.categories.ToList();
            ViewBag.categorylist = new SelectList(li, "cat_id", "cat_name");


            return View();
        }

        [HttpPost]
        public ActionResult CreateAdd(product p, HttpPostedFileBase imgfile)
        {
            List<category> li = db.categories.ToList();
            ViewBag.categorylist = new SelectList(li, "cat_id", "cat_name");


            string path = uploadimage(imgfile);

            if (path.Equals("-1"))
            {
                ViewBag.error = "image could not be uploaded";

            }
            else

            {


                product pr = new product();
                pr.pdt_name = p.pdt_name;
                pr.pdt_price = p.pdt_price;
                pr.pdt_img = path;
                pr.cat_id_fk = p.pdt_user_id_fk;
                pr.pdt_desc = p.pdt_desc;
                pr.pdt_user_id_fk = Convert.ToInt32(Session["u_id"].ToString());
                db.products.Add(pr);
                db.SaveChanges();

                Response.Redirect("DisplayAdd");
            }
            return View();
        }

        public ActionResult DisplayAdd(int? id, int? page)
        {
            TempData.Keep();
            int pagesize = 7, pageindex = 1;
            pageindex = page.HasValue ? Convert.ToInt32(page) : 1;
            var list = db.products.Where(x => x.cat_id_fk == id).OrderByDescending(x => x.pdt_id).ToList();
            IPagedList<product> cate = list.ToPagedList(pageindex, pagesize);
            return View(cate);
        }
        [HttpPost]
        public ActionResult DisplayAdd(int? id, int? page, string search)
        {
            int pagesize = 7, pageindex = 1;
            pageindex = page.HasValue ? Convert.ToInt32(page) : 1;
            var list = db.products.Where(x => x.pdt_name.Contains(search)).OrderByDescending(x => x.pdt_id).ToList();
            IPagedList<product> cate = list.ToPagedList(pageindex, pagesize);
            return View(cate);
        }

        public ActionResult ViewAdd(int? id, int? page)
        {
            ad_view_model adm = new ad_view_model();
            product p = db.products.Where(x => x.pdt_id == id).SingleOrDefault();
            adm.pdt_id = p.pdt_id;
            adm.pdt_name = p.pdt_name;
            adm.pdt_img = p.pdt_img;
            adm.pdt_price = p.pdt_price;
            adm.pdt_desc = p.pdt_desc;

            category cat = db.categories.Where(x => x.cat_id == p.cat_id_fk).SingleOrDefault();
            adm.cat_name = cat.cat_name;

            tbl_user u = db.tbl_user.Where(x => x.u_id == p.pdt_user_id_fk).SingleOrDefault();
            adm.u_name = u.u_name;
            adm.u_img = u.u_img;
            adm.u_contact = u.u_contact;
            adm.pdt_user_id_fk = u.u_id;
            return View(adm);
        }

        public ActionResult Ad_tocart(int? id)
        {
            product p = db.products.Where(x => x.pdt_id == id).SingleOrDefault();
            return View(p);
        }

        List<cart> li = new List<cart>();
        [HttpPost]
        public ActionResult Ad_tocart(product pr, string qty, int id)
        {
            product p = db.products.Where(x => x.pdt_id == id).SingleOrDefault();
            cart c = new cart();
            c.pdt_id = p.pdt_id;
            c.pdt_name = p.pdt_name;
            c.pdt_price = p.pdt_price;
            c.o_qty = Convert.ToInt32(qty);
            c.o_bill = p.pdt_price * c.o_qty;
            if (TempData["cart"] == null)
            {
                li.Add(c);
                TempData["cart"] = li;
            }
            else
            {
                List<cart> li2 = TempData["cart"] as List<cart>;
                int flag = 0;
                foreach(var item in li2)
                {
                    if(item.pdt_id == c.pdt_id) // if item already exist in cart
                    {
                        item.o_qty += c.o_qty;
                        item.o_bill += c.o_bill;
                        flag = 1;
                    }
                }
                if(flag == 0) // if item is new
                {
                    li2.Add(c);
                }
                TempData["cart"] = li2;
             }
        TempData.Keep();
        
            return RedirectToAction("Index");
        }
        public ActionResult remove(int ? id)
        {
            List<cart> li2 = TempData["cart"] as List<cart>;
            cart c = li2.Where(x => x.pdt_id == id).SingleOrDefault();
            li2.Remove(c);
            double h = 0;
            foreach(var item in li2)
            {
                h += (double) item.o_bill;
            }
            TempData["total"] = h;
            return RedirectToAction("checkout");
        }
        public ActionResult checkout()
        {
            TempData.Keep();
            return View();
        }

        [HttpPost]
        public ActionResult checkout(order_table O)
        {
            List<cart> li = TempData["cart"] as List<cart>;
            tbl_invoice iv = new tbl_invoice();
            iv.in_date = System.DateTime.Now;
            iv.in_fk_us = Convert.ToInt32(Session["u_id"].ToString());
            iv.in_totalbill = (double)TempData["total"];
            db.tbl_invoice.Add(iv);
            db.SaveChanges();

            foreach(var item in li)
            {
                order_table od = new order_table();
                od.o_fk_pdt = item.pdt_id;
                od.o_fk_invoice = iv.in_id;
                od.o_date = System.DateTime.Now;
                od.o_qty = item.o_qty;
                od.o_unitprice = item.pdt_price;
                od.o_bill = item.o_bill;
                db.order_table.Add(od);
                db.SaveChanges();
            }

            TempData.Remove("total");
            TempData.Remove("cart");
            TempData["msg"] = "Order Placed.";
            TempData.Keep();
            return RedirectToAction("Index");
        }

        public ActionResult Total_Bill()
        {
            return View(db.order_table.ToList());
        }

        //image upload
        public string uploadimage(HttpPostedFileBase file)
        {

            Random r = new Random();

            string path = "-1";

            int random = r.Next();

            if (file != null && file.ContentLength > 0)
            {

                string extension = Path.GetExtension(file.FileName);

                if (extension.ToLower().Equals(".jpg") || extension.ToLower().Equals(".jpeg") || extension.ToLower().Equals(".png"))
                {

                    try
                    {



                        path = Path.Combine(Server.MapPath("~/Content/upload"), random + Path.GetFileName(file.FileName));

                        file.SaveAs(path);

                        path = "~/Content/upload/" + random + Path.GetFileName(file.FileName);



                        //    ViewBag.Message = "File uploaded successfully";

                    }

                    catch (Exception ex)
                    {

                        path = "-1";

                    }

                }

                else
                {

                    Response.Write("<script>alert('Only jpg ,jpeg or png formats are acceptable....'); </script>");
                    path = "-1";

                }

            }



            else
            {

                Response.Write("<script>alert('Please select a file'); </script>");

                path = "-1";

            }







            return path;


        }
        //end

        public ActionResult SignOut()
        {
            Session.RemoveAll();
            Session.Abandon();
            return RedirectToAction("Index");
        }

        public ActionResult Ad_Delete(int ? id)
        {
            product p = db.products.Where(x => x.pdt_id == id).SingleOrDefault();
            db.products.Remove(p);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}