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
    [HttpPost("api/notes/{id}/share")]
    public async Task<IActionResult> ShareNote(Guid id, [FromBody] ShareNoteDto dto)
    {
        var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var note = await _context.Notes.FirstOrDefaultAsync(n => n.Id == id && n.OwnerId == currentUserId);
        if (note == null) return NotFound();

        var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (targetUser == null) return BadRequest("User not found");

        var existingAccess = await _context.NoteAccesses.FirstOrDefaultAsync(na => na.NoteId == id && na.UserId == targetUser.Id);

        if (existingAccess != null)
        {
            existingAccess.CanEdit = dto.CanEdit;
        }
        else
        {
            var NoteAccess = new NoteAccess
            {
                NoteId = id,
                UserId = targetUser.Id,
                CanEdit = dto.CanEdit
            };

            _context.NoteAccesses.Add(NoteAccess);
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    [Authorize]
    [HttpDelete("api/notes/{id}/share")]
    public async Task<IActionResult> UnshareNote(Guid id, string email)
    {
        var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var note = await _context.Notes.FirstOrDefaultAsync(n => n.Id == id && n.OwnerId == currentUserId);
        if (note == null) return NotFound();

        var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (targetUser == null) return NotFound("User not found");

        var existingAccess = await _context.NoteAccesses.FirstOrDefaultAsync(na => na.NoteId == id && na.UserId == targetUser.Id);

        if (existingAccess != null)
        {
            _context.NoteAccesses.Remove(existingAccess);
            await _context.SaveChangesAsync();
            return Ok();
        }

        return NotFound("Access not found");
    }

    [Authorize]
    [HttpGet ("download/{id}")]
    public async Task<IActionResult> DownloadNote(Guid id)
    {
        var note = await _context.Notes.FindAsync(id);
        if (note == null) return NotFound();
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (note.OwnerId != userId)
        {
            var access = await _context.NoteAccesses.FirstOrDefaultAsync(na => na.NoteId == id && na.UserId == userId);
            if (access == null) return Unauthorized();
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(note.Content);

        return File(bytes, "text/markdown", $"{note.Title}.md");
    }

    [Authorize]
    [HttpGet("api/notes")]
    public async Task<IActionResult> GetNotes()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var notes = await _context.Notes
            .Where(n => n.OwnerId == userId || _context.NoteAccesses.Any(na => na.NoteId == n.Id && na.UserId == userId))
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
        var note = await _context.Notes.FirstOrDefaultAsync(n => n.Id == id);
        if (note == null) return NotFound();

        if (note.OwnerId != userId)
        {
            var access = await _context.NoteAccesses.FirstOrDefaultAsync(na => na.NoteId == id && na.UserId == userId);
            if (access == null) return Unauthorized();
        }
        return Json(new { note.Id, note.Title, note.Content, note.CreatedAt });
    }

    [Authorize]
    [HttpPut("api/notes/{id}")]
    public async Task<IActionResult> UpdateNote(Guid id, [FromBody] UpdateNoteDto dto)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var note = await _context.Notes.FirstOrDefaultAsync(n => n.Id == id);
        if (note == null) return NotFound();
        if (note.OwnerId != userId)
        {
            var access = await _context.NoteAccesses.FirstOrDefaultAsync(na => na.NoteId == id && na.UserId == userId && na.CanEdit == true);
            if (access == null) return Unauthorized();
        }

        note.Title = dto.Title ?? note.Title;
        note.Content = dto.Content ?? note.Content;
        await _context.SaveChangesAsync();
        return Json(new { note.Id, note.Title, note.Content, note.CreatedAt });
    }

    [Authorize]
    [HttpPost("api/notes")]
    public async Task<IActionResult> CreateNote([FromBody] UpdateNoteDto dto) // Можна використати той самий DTO
    {
        var note = new Note
        {
            Id = Guid.NewGuid(),
            Title = dto.Title ?? "Без назви",
            Content = dto.Content ?? "",
            CreatedAt = DateTime.UtcNow,
            OwnerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        };

        _context.Notes.Add(note);
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
}
