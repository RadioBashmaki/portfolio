using bashmakiProject.mongodb;
using MongoDB.Bson;

namespace bashmakiProject.Models;

[MongoCollection("filesDescriptionsCollection")]
public class FileDescriptionDatabase : MongoEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public byte[] File { get; set; }
    public string Extension { get; set; }
    public string ContentType { get; set; }

    public FileDescriptionDatabase()
    {
        Id = ObjectId.GenerateNewId().ToString();
    }
}