using System.ComponentModel.DataAnnotations;
using System.Globalization;
using bashmakiProject.mongodb;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
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
    public PersonalData PersonalData { get; set; }
}

public class PersonalData
{
    [StringLength(50, ErrorMessage = "Слишком длинная строка")]
    public string? Name { get; set; }
    [StringLength(50, ErrorMessage = "Слишком длинная строка")]
    public string? Surname { get; set; }
    public byte[]? Avatar { get; set; }
    [BsonDateTimeOptions(DateOnly = true)]
    public DateTime? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    [StringLength(50, ErrorMessage = "Слишком длинная строка")]
    public string? City { get; set; }
    [StringLength(50, ErrorMessage = "Слишком длинная строка")]
    public string? Career { get; set; }
    [StringLength(50, ErrorMessage = "Слишком длинная строка")]
    public string? Company { get; set; }
    [StringLength(100, ErrorMessage = "Слишком длинная строка")]
    public string? Education { get; set; }
    [StringLength(350, ErrorMessage = "Слишком длинная строка")]
    public string? About { get; set; }
    public Links Links { get; set; }
}

public class Links
{
    [Url(ErrorMessage = "Неправильная ссылка")]
    public string? Vk { get; set; }
    [Url(ErrorMessage = "Неправильная ссылка")]
    public string? Telegram { get; set; }
    [Url(ErrorMessage = "Неправильная ссылка")]
    public string? Discord { get; set; }
    [Url(ErrorMessage = "Неправильная ссылка")]
    public string? Github { get; set; }
}
