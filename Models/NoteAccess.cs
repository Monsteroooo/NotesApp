namespace NotesApp.Models;

    public class NoteAccess
    {
        public Guid Id { get; set; }
        public Guid NoteId { get; set; }
        public string UserId { get; set; }
        public bool CanEdit { get; set; } = false;
    }

