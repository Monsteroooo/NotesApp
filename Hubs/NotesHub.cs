using Microsoft.AspNetCore.SignalR;
using NotesApp.Data;
using NotesApp.Models;

namespace NotesApp.Hubs;

public class NotesHub : Hub
{
    private readonly ApplicationDbContext context;

    public NotesHub(ApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task SendNote(string note, string noteId)
    {
        var myNote = new Note
        {
            Id = Guid.NewGuid(),
            Content = note,
            Title = "New Note",
            CreatedAt = DateTime.UtcNow
        };

        this.context.Notes.Add(myNote);
        await this.context.SaveChangesAsync();
        await Clients.Group(noteId).SendAsync("ReceiveNote", myNote);
    }

    public async Task JoinNoteGroup(string noteId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, noteId);
    }
}
