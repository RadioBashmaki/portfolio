using bashmakiProject.mongodb;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace bashmakiProject.Models;


public class InternshipRequest : MongoEntity
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string StudentId { get; set; }
    [BsonRepresentation(BsonType.ObjectId)]
    public string InternshipId { get; set; }
    public InternshipRequestStatus Status { get; set; }
    public InternshipRequest()
    {
        Id = ObjectId.GenerateNewId().ToString();
    }
}

public enum InternshipRequestStatus
{
    Pending,
    Accepted,
    Declined
}