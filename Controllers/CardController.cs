﻿using BiletPortal.Data;
using BiletPortal.Dto;
using BiletPortal.Models;
using BiletPortal.Session;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BiletPortal.Controllers
{
    public class CardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<AppUser> _signInManager;

        public CardController(ApplicationDbContext context, SignInManager<AppUser> signInManager)
        {
            _context = context;
            _signInManager = signInManager; 
        }
        public IActionResult Index()
        {
            List<CardItem> items = HttpContext.Session.GetJson<List<CardItem>>("Card") ?? new List<CardItem>();
            CardViewModel viewModel = new()
            {
                CardItems = items,
                GrandTotal = items.Sum(x => x.Quantity * x.Price)
            };
            return View(viewModel);
            // this will be returned as result.
        }
       
        public async Task<IActionResult>Add(int? id)
        {
            if (_signInManager.IsSignedIn(User))
            {
                Products? product = await _context.Products.FindAsync(id);
                // Convert Json Format To Object and Checked null or non null
                List<CardItem> items = HttpContext.Session.GetJson<List<CardItem>>("Card") ?? new List<CardItem>();
                CardItem cardItem = items.FirstOrDefault(x => x.ProductId == id);
                if (cardItem == null)
                {
                    items.Add(new CardItem(product));
                }
                else
                {
                    cardItem.Quantity += 1;
                }
                HttpContext.Session.SetJson("Card", items);   // Json Format was Converted.
                return Json(new { success = true, message = "Bilet Sepete Eklendi" });
                
            }

            return Json(new { success = false, message = "Oturum açmanız gerekiyor." });
           
        }

        public async Task<IActionResult> Decrease(int? id)
        {
            List<CardItem> card = HttpContext.Session.GetJson<List<CardItem>>("Card");
            CardItem cardItem = card.Where(x=>x.ProductId == id).FirstOrDefault();
            if(cardItem.Quantity>1)
            {
                cardItem.Quantity -= 1; 
            }
            else
            {
                card.RemoveAll(c=> c.ProductId == id);
            }
            if(card.Count > 0)
            {
                HttpContext.Session.SetJson("Card", card);
            }
            TempData["Message"] = "The Ticket was Removed.";
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Remove(int? id)
        {
            List<CardItem> card = HttpContext.Session.GetJson<List<CardItem>>("Card");
            card.RemoveAll(c=> c.ProductId == id);
            if ((card.Count > 0))
            {
                HttpContext.Session.Remove("Card");


            }
            {
                HttpContext.Session.SetJson("Card", card);
            }
            TempData["Message"] = "The ticket was removed.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Clear()
        {
            HttpContext.Session.Remove("Card");
            return RedirectToAction("Index");
        }
    }
}
