using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NotesApp.Models;

namespace NotesApp.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public DbSet<Note> Notes { get; set; }
    public DbSet<NoteAccess> NoteAccesses { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}
