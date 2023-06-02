using bashmakiProject.mongodb;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace bashmakiProject.Models;

[MongoCollection("usersCollection")]
public class User : MongoEntity
{
    public string Email { get; set; }
    public byte[] Password { get; set; }
    public byte[] PasswordKey { get; set; }
    public Role? Role { get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    [BsonDateTimeOptions(DateOnly = true)]
    public DateTime? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    public string? Company { get; set; }
}
