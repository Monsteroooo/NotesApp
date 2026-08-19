using System;
using Microsoft.Data.Sqlite;

var connectionStringBuilder = new SqliteConnectionStringBuilder();
connectionStringBuilder.DataSource = "./app.db";

using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
{
    connection.Open();
    var command = connection.CreateCommand();
    command.CommandText = "SELECT UserId, NoteId, CanEdit FROM NoteAccesses";
    using (var reader = command.ExecuteReader())
    {
        while (reader.Read())
        {
            Console.WriteLine($"UserId: {reader.GetString(0)}, NoteId: {reader.GetString(1)}, CanEdit: {reader.GetBoolean(2)}");
        }
    }
}
