using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
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
            // fallback: treat plain string as content
            content = noteJson;
        }

        var note = new Note
        {
            Id        = Guid.NewGuid(),
            Title     = title,
            Content   = content,
            CreatedAt = DateTime.UtcNow,
            OwnerId   = Context.UserIdentifier,
        };

        context.Notes.Add(note);
        await context.SaveChangesAsync();

        await Clients.Group(groupId).SendAsync("ReceiveNote", new
        {
            note.Id,
            note.Title,
            note.CreatedAt,
        });
    }

    public async Task JoinNoteGroup(string groupId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
    }
}
