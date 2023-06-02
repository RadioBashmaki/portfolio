using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace bashmakiProject.Models;

public class MongoEntity
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
}