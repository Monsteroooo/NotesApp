using Microsoft.AspNetCore.SignalR;

namespace NotesApp.Hubs;

public class NotesHub : Hub
{
    public async Task SendNote(string note, string noteId)
    {
        await Clients.Group(noteId).SendAsync("ReceiveNote", note);
    }

    public async Task JoinNoteGroup(string noteId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, noteId);
    }
}
