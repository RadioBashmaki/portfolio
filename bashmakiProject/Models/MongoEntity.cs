using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace bashmakiProject.Models;

public class MongoEntity
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
}