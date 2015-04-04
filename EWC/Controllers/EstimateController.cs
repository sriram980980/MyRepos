﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EWC.Models;

namespace EWC.Controllers
{
    public class EstimateController : Controller
    {
        private DCDBset db = new DCDBset();

        //
        // GET: /Estimate/

        public ActionResult Index()
        {
            return View(db.estimates.Include(i => i.customer).Where(i => i.isActive == true).ToList());
        }

        //
        // GET: /Estimate/Details/5

        public ActionResult Details(int id = 0)
        {
            Estimate invoice = db.estimates.Include(i => i.Items).Include(i => i.customer).SingleOrDefault(i => i.DeliveryChalanID == id);
            if (invoice == null)
            {
                return HttpNotFound();
            }
            return View(invoice);
        }
        public ActionResult Print(int id = 0)
        {
            Estimate invoice = db.estimates.Include(i => i.Items).Include(i => i.customer).SingleOrDefault(i => (i.DeliveryChalanID == id && i.isActive));
            if (invoice == null)
            {
                return HttpNotFound();
            }
            return View(invoice);
        }
        //
        // GET: /Estimate/Create

        public ActionResult Create(int id = 0)
        {
            var dc = db.waybills.Include("Items").Include("customer").SingleOrDefault(d => d.DeliveryChalanID == id);
            var Invoicee = new Estimate(dc);

            if (id == 0 || dc == null)
            {
                return HttpNotFound();
            }
            var existinvoice = db.estimates.Include("Items").Include("customer").SingleOrDefault(d => d.DCNo == dc.DCNo && d.isActive);
            if (existinvoice != null)
                return RedirectToAction("Print", "Estimate", new { id = existinvoice.DeliveryChalanID });
            Customer c = dc.customer;
            c.CustomerID = 0;
            Invoicee.customer = c;
            foreach (DcItems itm in dc.Items)
            {
                Invoicee.Items.Add(new InvoiceItem(itm));
            }
            return View(Invoicee);
        }

        //
        // POST: /Estimate/Create

        [HttpPost]
        public ActionResult Create(Estimate invoice)
        {
            if (ModelState.IsValid)
            {
                invoice.addedby = User.Identity.Name;
                invoice.isActive = true;
                db.estimates.Add(invoice);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(invoice);
        }

        //
        // GET: /Estimate/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Estimate invoice = db.estimates.Include(i => i.Items).Include(i => i.customer).FirstOrDefault(i => i.DeliveryChalanID == id);
            if (invoice == null)
            {
                return HttpNotFound();
            }
            return View(invoice);
        }

        //
        // POST: /Estimate/Edit/5

        [HttpPost]
        public ActionResult Edit(Estimate invoice)
        {
            if (ModelState.IsValid)
            {
                var invindb = db.estimates.Include(dc => dc.customer).Include(dc => dc.Items).SingleOrDefault(dc => dc.DeliveryChalanID == invoice.DeliveryChalanID);
                db.Entry(invindb).CurrentValues.SetValues(invoice);
                var itemsindb = invindb.Items.ToList();
                invindb.isActive = true;
                foreach (var item in itemsindb)
                {
                    var itemi = invoice.Items.SingleOrDefault(i => i.InvoiceItemID == item.InvoiceItemID);
                    if (itemi != null)
                        db.Entry(item).CurrentValues.SetValues(itemi);
                }
                var cindb = db.customers.SingleOrDefault(i => i.CustomerID == invoice.customer.CustomerID);

                db.Entry(cindb).CurrentValues.SetValues(invoice.customer);
                invindb.isActive = true;
                db.SaveChanges();
                db.Dispose();
                return RedirectToAction("Index");
            }
            return View(invoice);
        }

        //
        // GET: /Estimate/Delete/5
        [Authorize(Users="Admin")]
        public ActionResult Delete(int id = 0)
        {
            Estimate invoice = db.estimates.Find(id);
            if (invoice == null)
            {
                return HttpNotFound();
            }
            return View(invoice);
        }

        //
        // POST: /Estimate/Delete/5
        [Authorize(Users = "Admin")]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var invoice = db.estimates.Include(dc => dc.Items).Include(dc => dc.customer).FirstOrDefault(dc => dc.DeliveryChalanID == id);
            if (invoice != null)
            {
                invoice.isActive = false;
                db.Entry(invoice).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}