using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NotesApp.Models;
using NotesApp.Data;
using Microsoft.EntityFrameworkCore;

namespace NotesApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        this._context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpGet ("download/{id}")]
    public async Task<IActionResult> DownloadNote(Guid id)
    {
        var note = await _context.Notes.FindAsync(id);
        if (note == null)
        {
            return NotFound();
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(note.Content);

        return File(bytes, "text/markdown", $"{note.Title}.md");
    }
}
