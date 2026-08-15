using Microsoft.AspNetCore.SignalR;

namespace NotesApp.Hubs;

public class NotesHub : Hub
{
    public async Task SendNote(string note)
    {
        await Clients.All.SendAsync("ReceiveNote", note);
    }
}
