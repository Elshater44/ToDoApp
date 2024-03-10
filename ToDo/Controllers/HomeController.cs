using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using ToDoList.Context;
using ToDoList.Data;
using ToDoList.Models;

namespace ToDoList.Controllers
{
    public class HomeController : Controller
    {
        private ToDoContext _context;
        public HomeController(ToDoContext context)
        {
            _context = context;
        }

        public IActionResult Index(string id)
        {
            var filters = new Filters(id);
            ViewBag.Filter = filters;
            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Statuses = _context.Statuses.ToList();
            ViewBag.DueFilters = Filters.DueFilterValues;

            IQueryable<ToDo> query = _context.ToDos
                .Include(t => t.Category)
                .Include(t => t.Status);

            if (filters.HasCategory)
            {
                query = query.Where(t => t.CategoryId == filters.CategoryId);
            }

            if (filters.HasStatus)
            {
                query = query.Where(t => t.StatusId == filters.StatusId);
            }

            if (filters.HasDue)
            {
                var today = DateTime.Today;
                if (filters.IsPast)
                {
                    query = query.Where(t => t.DueDate < today);
                }

                if (filters.IsFuture)
                {
                    query = query.Where(t => t.DueDate > today);
                }

                if (filters.IsToday)
                {
                    query = query.Where(t => t.DueDate == today);
                }

            }
            var tasks = query.OrderBy(t => t.DueDate).ToList();
            return View(tasks);
        }

        [HttpGet]
        public IActionResult Add()
        {
            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Statuses = _context.Statuses.ToList();
            var task = new ToDo { StatusId = "open" };
            return View(task);
        }

        [HttpPost]
        public IActionResult Add(ToDo task)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _context.Categories.ToList();
                ViewBag.Statuses = _context.Statuses.ToList();
                return View(task);
            }

            _context.ToDos.Add(task);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Filter(string[] filter)
        {
            string id = string.Join('-', filter);
            return RedirectToAction("Index", new { ID = id });
        }

        [HttpPost]
        public IActionResult MarkAsComplete([FromRoute] string id,ToDo selected)
        {
            selected = _context.ToDos.Find(selected.Id)!;
            
            if(selected != null)
            {
                selected.StatusId = "closed";
                _context.SaveChanges();
            }

            return RedirectToAction("Index", new {ID = id});
        }

        public IActionResult DeleteComplete(string id)
        {
            var toDelete = _context.ToDos.Where(t => t.StatusId == "closed").ToList();

            foreach (var task in toDelete)
            {
                _context.ToDos.Remove(task);
            }
            _context.SaveChanges();
            return RedirectToAction("Index", new { ID = id });
        }

    }
}
