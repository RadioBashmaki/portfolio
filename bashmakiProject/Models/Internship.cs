using bashmakiProject.mongodb;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace bashmakiProject.Models;

[MongoCollection("internshipsCollection")]
public class Internship : MongoEntity
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; }
    public string Title { get; set; }
    public Experience Experience { get; set; }
    public Topic[] Topics { get; set; }
    public string Description { get; set; }
    
    [BsonDateTimeOptions(DateOnly = true)]
    public DateTime CreationDate { get; set; }
    
    public bool IsActive { get; set; }
    public FileDescriptionDatabase[]? Files { get; set; }
}

public enum Experience{
    Demanded,
    NotDemanded
}