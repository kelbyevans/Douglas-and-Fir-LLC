﻿using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using MIVisitorCenter.Models;
using Newtonsoft.Json.Linq;
using MIVisitorCenter.Data.Abstract;
using Microsoft.AspNetCore.Http;

namespace MIVisitorCenter.Controllers
{
    public class BusinessesController : Controller
    {
        private readonly MIVisitorCenterDbContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly IBusinessRepository _businessRepo;
        private readonly IPhotoCollectionRepository _photoRepo;


        public BusinessesController(MIVisitorCenterDbContext context, 
                                    IAuthorizationService authorizationService, 
                                    IBusinessRepository businessRepo,
                                    IPhotoCollectionRepository photoRepo)
        {
            _context = context;
            _authorizationService = authorizationService;
            _businessRepo = businessRepo;
            _photoRepo = photoRepo;
        }

        // GET: Businesses
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Index(string sortOption, string cityFilter = null, string categoryFilter = null)
        {
            ViewBag.NameSortOption = string.IsNullOrEmpty(sortOption) ? "name_desc" : "";
            
            var addresses = _context.Addresses.ToArray();
            var cities = new ArrayList();

            foreach (var address in addresses)
            {
                if (!cities.Contains(address.City))
                    cities.Add(address.City);
            }

            cities.Sort();

            ViewData["Cities"] = cities;
            ViewData["Categories"] = _context.Categories.OrderBy(c => c.Name).ToArray();

            var businesses = _context.Businesses.Include(b => b.Address);
            var filteredBusinesses = new List<Business>();
            IQueryable filteredCategories = null;

            if (!string.IsNullOrEmpty(cityFilter) && !string.IsNullOrEmpty(categoryFilter))
            {
                filteredCategories = _context.Categories.Where(b => b.Name == categoryFilter)
                                        .Include(c => c.BusinessCategories)
                                        .ThenInclude(d => d.Business)
                                        .ThenInclude(e => e.Address);
                foreach (Category c in filteredCategories)
                {
                    foreach (BusinessCategory b in c.BusinessCategories)
                        filteredBusinesses = filteredBusinesses.Append(b.Business).ToList();
                }
                filteredBusinesses = filteredBusinesses.Where(a => a.Address.City == cityFilter).ToList();
            }
            else if (!string.IsNullOrEmpty(cityFilter))
                filteredBusinesses = await businesses.Where(a => a.Address.City == cityFilter).ToListAsync();
            else if (!string.IsNullOrEmpty(categoryFilter))
            {
                filteredCategories = _context.Categories.Where(b => b.Name == categoryFilter)
                                                        .Include(c => c.BusinessCategories)
                                                        .ThenInclude(d => d.Business)
                                                        .ThenInclude(e => e.Address);
                foreach (Category c in filteredCategories)
                {
                    foreach (BusinessCategory b in c.BusinessCategories)
                        filteredBusinesses = filteredBusinesses.Append(b.Business).ToList();
                }
            }

            if (filteredBusinesses.Any())
                return View(filteredBusinesses);
            if (string.IsNullOrEmpty(sortOption)) 
                return View(await businesses.ToListAsync());

            var sortedBusinesses = businesses.OrderByDescending(c => c.Name);

            return View(await sortedBusinesses.ToListAsync());
        }

        //[HttpPost]
        //[Authorize(Roles = "admin")]
        //public async Task<IActionResult> Index(string cityFilter = null, string categoryFilter = null)
        //{
        //    var addresses = _context.Addresses.ToArray();
        //    var cities = new ArrayList();

        //    foreach (var address in addresses)
        //    {
        //        if (!cities.Contains(address.City))
        //            cities.Add(address.City);
        //    }

        //    cities.Sort();

        //    ViewData["Cities"] = cities;
        //    ViewData["Categories"] = _context.Categories.OrderBy(c => c.Name).ToArray();

        //    var businesses = _context.Businesses.Include(b => b.Address);

        //    if (!string.IsNullOrEmpty(cityFilter))
        //        return View(await businesses.Where(a => a.Address.City == cityFilter).ToListAsync());
        //    return View(await businesses.ToListAsync());
        //}

        public IActionResult EatAndDrink()
        {
            var businesses = _context.Categories
                                    .Where(n => n.Name == "Restaurants" || n.Name == "Coffee" || n.Name == "Wineries" || n.Name == "Bars")
                                    .Include(b => b.BusinessCategories)
                                    .ThenInclude(b => b.Business)
                                    .ThenInclude(a => a.Address)
                                    .AsEnumerable()
                                    .GroupBy(c => c.Name);
            return View(businesses);
        }

        public IActionResult ArtAndCulture()
        {
            var businesses = _context.Categories
                                    .Where(n => n.Name == "Historic Sites & Museums" || n.Name == "Art Galleries" || n.Name == "Festivals & Events" || n.Name == "Cinemas & Performing Arts")
                                    .Include(b => b.BusinessCategories)
                                    .ThenInclude(b => b.Business)
                                    .ThenInclude(a => a.Address)
                                    .AsEnumerable()
                                    .GroupBy(c => c.Name);
            return View(businesses);
        }

        public IActionResult OutdoorRecreation()
        {
            var businesses = _context.Categories
                                    .Where(n => n.Name == "Hiking" || n.Name == "Cycling" || n.Name == "Birding" || n.Name == "Fishing" || n.Name == "Golf" || n.Name == "Disc Golf" || n.Name == "Skating")
                                    .Include(b => b.BusinessCategories)
                                    .ThenInclude(b => b.Business)
                                    .ThenInclude(a => a.Address)
                                    .AsEnumerable()
                                    .GroupBy(c => c.Name);
            return View(businesses);
        }

        public IActionResult WaterTrail()
        {
            //ViewData["Lodging"] = _context.Lodgings.Include(l => l.LodgingAmenities).ThenInclude(l => l.Amenities).Include(l => l.Business).ThenInclude(b => b.Address).Include(l => l.Business).ThenInclude(b => b.BusinessCategories).ThenInclude(b => b.Category);
            var businesses = _context.Categories
                                    .Where(n => n.Name == "WaterTrailRestaurants")
                                    .Include(b => b.BusinessCategories)
                                    .ThenInclude(b => b.Business)
                                    .ThenInclude(a => a.Address)
                                    .AsEnumerable();

            var lodging = _context.Categories
                                    .Where(n => n.Name == "WaterTrailLodging")
                                    .Include(b => b.BusinessCategories)
                                    .ThenInclude(b => b.Business)
                                    .ThenInclude(a => a.Address)
                                    .AsEnumerable();
            ViewBag.Lodging = lodging;
            return View(businesses);
        }

        [HttpGet]
        public IActionResult Lodging()
        {
            ViewData["Amenities"] = _context.Amenities.OrderBy(a => a.Name).ToArray();

            var businesses = _context.Lodgings.Include(l => l.LodgingAmenities).ThenInclude(l => l.Amenities).Include(l => l.Business).ThenInclude(b => b.Address);

            return View(businesses);
        }

        [HttpPost]
        public ActionResult Lodging(string[] tags)
        {
            ViewData["Amenities"] = _context.Amenities.OrderBy(a => a.Name).ToArray();
            var businesses = _context.Lodgings.Include(l => l.LodgingAmenities).ThenInclude(l => l.Amenities).Include(l => l.Business).ThenInclude(b => b.Address);

            if (tags.Length == 0)
            {
                return View("Lodging", businesses);
            }

            var filtered = new List<Lodging>();

            foreach (var b in businesses)
            {
                int tagCount = 0;
                foreach (var la in b.LodgingAmenities)
                {
                    for (var i = 0; i < tags.Length; i++)
                    {
                        if (la.Amenities.Name == tags[i])
                        {
                            tagCount++;
                            if (tags.Length == tagCount)
                            {
                                filtered.Add(b);
                            }
                        }
                    }
                }
            }

            return View("Lodging", filtered);
        }

        // GET: Businesses/Details/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var business = await _context.Businesses
                .Include(b => b.Address)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (business == null)
            {
                return NotFound();
            }

            return View(business);
        }

        // GET: Businesses/Create
        [Authorize(Roles = "admin")]
        public IActionResult Create()
        {
            ViewData["AddressId"] = new SelectList(_context.Addresses, "Id", "City");
            return View();
        }

        // POST: Businesses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Phone,Website,PictureFileName,AddressId")] Business business)
        {
            if (ModelState.IsValid)
            {
                _context.Add(business);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AddressId"] = new SelectList(_context.Addresses, "Id", "City", business.AddressId);
            return View(business);
        }

        // GET: Businesses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var business = await _context.Businesses.FindAsync(id);
            if (business == null) return NotFound();

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, business, "BusinessOwner");
            if (!authorizationResult.Succeeded) return NotFound();

            ViewData["AddressId"] = new SelectList(_context.Addresses, "Id", "StreetAddress", business.AddressId);
            ViewData["Photos"] = _photoRepo.GetAll().Where(i => i.BusinessId == id).ToArray();
            return View(business);

        }

        // POST: Businesses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Phone,Website,PictureFileName,AddressId,PhotoCollections")] Business business, IFormFile PictureFileName, IFormCollection PhotoCollections)
        {
            if (id != business.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {  
                    // Save image to business record using function from BusinessRepository.cs
                    await _businessRepo.UpdateBusiness(business, PictureFileName, PhotoCollections);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BusinessExists(business.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Business", new {id = business.Id});
            }
            ViewData["AddressId"] = new SelectList(_context.Addresses, "Id", "StreetAddress", business.AddressId);
            return View(business);
        }

        // GET: Businesses/Delete/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var business = await _context.Businesses
                .Include(b => b.Address)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (business == null)
            {
                return NotFound();
            }

            return View(business);
        }

        // POST: Businesses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var business = await _context.Businesses.FindAsync(id);
            _context.Businesses.Remove(business);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BusinessExists(int id)
        {
            return _context.Businesses.Any(e => e.Id == id);
        }

        [HttpGet]
        public string GetAllBusinesses() {
            var businesses = _context.BusinessCategories.Include(b => b.Business).Include(c => c.Category);

            JArray array = new JArray(
                businesses.Select(b => new JObject
                {
                    { "Id", b.Business.Id },
                    { "Name", b.Business.Name },
                    { "Category", b.Category.Name }
                })
            );

            string json = array.ToString();
            return json;
        }

        public async Task<IActionResult> Business(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ViewData["Photos"] = _photoRepo.GetAll().Where(i => i.BusinessId == id).ToArray();
            var business = await _context.Businesses
                .Include(b => b.Address)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (business == null)
            {
                return NotFound();
            }

            return View(business);
        }
    }
}
