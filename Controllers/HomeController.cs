using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NotesApp.Models;
using NotesApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

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

    [Authorize]
    [HttpGet("api/notes")]
    public async Task<IActionResult> GetNotes()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var notes = await _context.Notes
            .Where(n => n.OwnerId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new { n.Id, n.Title, n.CreatedAt })
            .ToListAsync();
        return Json(notes);
    }

    [Authorize]
    [HttpGet("api/notes/{id}")]
    public async Task<IActionResult> GetNote(Guid id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var note = await _context.Notes.FirstOrDefaultAsync(n => n.Id == id && n.OwnerId == userId);
        if (note == null) return NotFound();
        return Json(new { note.Id, note.Title, note.Content, note.CreatedAt });
    }

    [Authorize]
    [HttpPut("api/notes/{id}")]
    public async Task<IActionResult> UpdateNote(Guid id, [FromBody] UpdateNoteDto dto)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var note = await _context.Notes.FirstOrDefaultAsync(n => n.Id == id && n.OwnerId == userId);
        if (note == null) return NotFound();
        note.Title = dto.Title ?? note.Title;
        note.Content = dto.Content ?? note.Content;
        await _context.SaveChangesAsync();
        return Json(new { note.Id, note.Title, note.Content, note.CreatedAt });
    }

    [Authorize]
    [HttpDelete("api/notes/{id}")]
    public async Task<IActionResult> DeleteNote(Guid id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var note = await _context.Notes.FirstOrDefaultAsync(n => n.Id == id && n.OwnerId == userId);
        if (note == null) return NotFound();
        _context.Notes.Remove(note);
        await _context.SaveChangesAsync();
        return Ok();
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
    
    [Authorize]
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
