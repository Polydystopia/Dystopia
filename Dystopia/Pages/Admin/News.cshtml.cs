using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Dystopia.Database;
using Dystopia.Database.News;
using Microsoft.EntityFrameworkCore;
using PolytopiaBackendBase.Game;

namespace Dystopia.Pages.Admin;

public class NewsModel : PageModel
{
    [BindProperty]
    public NewsEntity NewItem { get; set; } = new();

    public DbSet<NewsEntity> NewsItems { get; set; }

    private PolydystopiaDbContext _dbContext;

    public NewsModel(PolydystopiaDbContext dbContext)
    {
        _dbContext = dbContext;
        NewsItems = _dbContext.News;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult>  OnPostAdd()
    {
        if (!ModelState.IsValid)
        {
            foreach (var entry in ModelState)
            {
                foreach (var error in entry.Value.Errors)
                {
                    
                }
            }
        }
        NewItem.Id = NewsItems.Count() + 1;
        NewItem.CreatedAt = DateTime.UtcNow;
        NewItem.UpdatedAt = DateTime.UtcNow;
        _dbContext.News.Add(NewItem);
        await _dbContext.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDelete(int id)
    {
        var item = _dbContext.News.FirstOrDefault(x => x.Id == id);
        if (item != null)
        {
            _dbContext.News.Remove(item);
        }

        await _dbContext.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEdit(int id)
    {
        // Redirect to edit page or show edit UI (depends on design)
        return RedirectToPage("EditNews", new { id });
    }
}