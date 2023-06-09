using bashmakiProject.mongodb;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace bashmakiProject.Models;

[MongoCollection("projectsCollection")]
public class Project : MongoEntity
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; }
    public string Title { get; set; }
    public Topic[] Topics { get; set; }
    public string Description { get; set; }
    [BsonDateTimeOptions(DateOnly = true)]
    public DateTime CreationDate { get; set; }

    public bool IsPinned { get; set; } = false;

    public FileDescriptionDatabase[]? Files;
}

public class FileDescriptionDatabase
{
    public string Name { get; set; }
    public string Description { get; set; }
    public byte[] File { get; set; }
}

public class EnumFieldDescriptionAttribute : Attribute
{
    public readonly string Name;
    public readonly string ShortName;
    public readonly string HexColor;

    public EnumFieldDescriptionAttribute(string name, string shortName, string hexColor)
    {
        Name = name;
        ShortName = shortName;
        HexColor = hexColor;
    }
}

public enum Topic
{
    [EnumFieldDescription("WebTechnologies", "Web", "#3EDC17")]
    WebTechnologies,

    [EnumFieldDescription("Design", "Design", "#14F1BD")]
    Design,

    [EnumFieldDescription("Development", "Dev", "#305EFF")]
    Development,

    [EnumFieldDescription("3D", "3D", "#5200FF")]
    ThreeDimensional,

    [EnumFieldDescription("Other", "Other", "#FFE500")]
    Other,
}

public static class TopicsEnumExtensions
{
    public static TopicDescription? GetDescription(this Topic value)
    {
        var attr = value.GetType().GetField(value.ToString())!
            .GetCustomAttributes(false).OfType<EnumFieldDescriptionAttribute>().FirstOrDefault();
        return attr != null ? new TopicDescription(attr.Name, attr.ShortName, attr.HexColor) : null;
    }
}

public record class TopicDescription(string Name, string ShortName, string HexColor);