using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using NotesApp.Data;
using NotesApp.Models;
using System.Text.Json;

namespace NotesApp.Hubs;

[Authorize]
public class NotesHub : Hub
{
    private readonly ApplicationDbContext context;

    public NotesHub(ApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task SendNote(string noteJson, string groupId)
    {
        string title = "Без назви";
        string content = string.Empty;

        try
        {
            var dto = JsonSerializer.Deserialize<JsonElement>(noteJson);
            if (dto.TryGetProperty("title", out var t))   title   = t.GetString() ?? title;
            if (dto.TryGetProperty("content", out var c)) content = c.GetString() ?? content;
        }
        catch
        {
            content = noteJson;
        }

        var noteId = Guid.Parse(groupId);
        var note = await context.Notes.FirstOrDefaultAsync(n => n.Id == noteId);

        if (note == null) return;

        var currentUserId = Context.UserIdentifier;

        if (currentUserId != note.OwnerId)
        {
            var access = await context.NoteAccesses.FirstOrDefaultAsync(na => na.NoteId == noteId && na.UserId == currentUserId && na.CanEdit == true);
            if (access == null)
            {
                return;
            }
        }
        
        note.Title = title;
        note.Content = content;
        await context.SaveChangesAsync();

        await Clients.GroupExcept(groupId, Context.ConnectionId).SendAsync("ReceiveNote", new
        {
            note.Id,
            note.Title,
            note.Content,
            note.CreatedAt,
        });
    }

    public async Task JoinNoteGroup(string groupId)
    {
        var noteId = Guid.Parse(groupId);
        var note = await context.Notes.FirstOrDefaultAsync(n => n.Id == noteId);
        if (note == null) return;

        var CurrentUserId = Context.UserIdentifier;

        if (CurrentUserId != note.OwnerId)
        {
            var access = await context.NoteAccesses.FirstOrDefaultAsync(na => na.NoteId == noteId && na.UserId == CurrentUserId);
            if (access == null)
            {
                return;
            }
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
    }
}
