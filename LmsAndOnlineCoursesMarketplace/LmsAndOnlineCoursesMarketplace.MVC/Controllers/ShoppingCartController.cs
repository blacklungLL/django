using LmsAndOnlineCoursesMarketplace.Domain.Entities;
using LmsAndOnlineCoursesMarketplace.MVC.Models.ShoppingCart;
using LmsAndOnlineCoursesMarketplace.Persistence.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LmsAndOnlineCoursesMarketplace.MVC.Controllers;

[Authorize]
public class ShoppingCartController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ApplicationDbContext _context;

    public ShoppingCartController(
        UserManager<IdentityUser> userManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Index(int? courseId)
    {
        var identityUser = await _userManager.GetUserAsync(User);
        if (identityUser == null) return Challenge();

        var user = await _context.Users
            .Include(u => u.PurchasedCourses)
            .ThenInclude(up => up.Course)
            .FirstOrDefaultAsync(u => u.IdentityUserId == identityUser.Id);

        if (user == null) return NotFound();

        List<CourseSummaryVM> courses = new();

        if (courseId.HasValue)
        {
            var course = await _context.Courses
                .Where(c => c.Id == courseId.Value)
                .Select(c => new CourseSummaryVM
                {
                    Id = c.Id,
                    Title = c.Title,
                    Price = c.Price,
                    ImageLink = c.ImageLink,
                    Category = c.Category,
                    Language = c.Language,
                    Duration = c.Duration,
                    Views = c.Views,
                    AuthorName = c.User.Name ?? "Unknown"
                })
                .SingleOrDefaultAsync();

            if (course != null && !user.PurchasedCourses.Any(up => up.CourseId == course.Id))
            {
                courses.Add(course);
            }
        }

        var model = new ShoppingCartVM
        {
            Courses = courses,
            Balance = user.Balance
        };
        
        User? curUser = null;

        if (identityUser != null)
        {
            curUser = await _context.Users
                .FirstOrDefaultAsync(u => u.IdentityUserId == identityUser.Id);
        }
            
        if (curUser != null)
        {
            ViewBag.CurrentUserId = curUser.Id;
            ViewBag.CurrentUserName = curUser.Name;
            ViewBag.CurrentEmail = curUser.Email;
        }

        return View(model);
    }

    [HttpPost("ShoppingCart/BuyNow")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BuyNow(int courseId)
    {
        var identityUser = await _userManager.GetUserAsync(User);
        if (identityUser == null) return Challenge();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.IdentityUserId == identityUser.Id);

        if (user == null) return NotFound();

        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null) return NotFound();

        if (user.Balance < course.Price)
        {
            TempData["Message"] = "Your balance is too low to buy this course.";
            return RedirectToAction("Index", new { courseId = courseId });
        }

        if (await _context.UserCoursePurchases
            .AnyAsync(up => up.UserId == user.Id && up.CourseId == courseId))
        {
            TempData["Message"] = "You already bought this course";
            return RedirectToAction("Index", new { courseId = courseId });
        }
        
        int authorId = course.UserId;
        
        var author = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == authorId);
        
        if (author == null)
        {
            Console.Error.WriteLine($"Author with ID {courseId} not found");
            return NotFound("Author not found");
        }

        user.Balance -= course.Price;
        author.Balance += course.Price;

        var purchase = new UserCoursePurchase
        {
            UserId = user.Id,
            CourseId = courseId
        };

        await _context.UserCoursePurchases.AddAsync(purchase);
        _context.Users.Update(user);
        _context.Users.Update(author);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Course '{course.Title}' was successfully bought!";
        return RedirectToAction("Index", "Profile");
    }
}