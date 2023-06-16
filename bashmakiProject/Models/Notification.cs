using bashmakiProject.mongodb;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace bashmakiProject.Models;

[MongoCollection("notificationsCollection")]
public class Notification : MongoEntity
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string FromWhomId { get; set; }
    [BsonRepresentation(BsonType.ObjectId)]
    public string ToWhomId { get; set; }
    public Role ToWhomRole { get; set; }
    public Role FromWhomRole { get; set; }
    [BsonRepresentation(BsonType.ObjectId)]
    public string InternshipId { get; set; }
    public DateTime CreationDate { get; set; }
    
    public NotificationType Type { get; set; }
}

public enum NotificationType
{
    InternshipRequestAccepted,
    InternshipRequestDeclined,
    InternshipRequestCreated,
}