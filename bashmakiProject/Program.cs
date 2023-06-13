using System.Security.Claims;
using bashmakiProject.Models;
using bashmakiProject.mongodb;
using Microsoft.AspNetCore.Authentication.Cookies;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Configuration.AddJsonFile("MongoConfig.json");
var mongoConfig = new MongoConfig();
builder.Configuration.Bind("Mongo", mongoConfig);
builder.Services.AddSingleton(mongoConfig);

var mongoClient = new MongoClient(mongoConfig.ConnectionString);
builder.Services.AddSingleton<IMongoClient>(mongoClient);

var mongoRepo = new MongoDbRepository(mongoClient, mongoConfig.DatabaseName);
builder.Services.AddSingleton<IMongoDbRepository>(mongoRepo);
BsonSerializer.RegisterSerializer(new EnumSerializer<Role>(BsonType.String));
BsonSerializer.RegisterSerializer(new EnumSerializer<Gender>(BsonType.String));
BsonSerializer.RegisterSerializer(new EnumSerializer<Topic>(BsonType.String));
BsonSerializer.RegisterSerializer(new EnumSerializer<Experience>(BsonType.String));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opts =>
    {
        opts.LoginPath = new PathString("/account/auth");
        opts.AccessDeniedPath = new PathString("/account/auth");
        opts.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    });

builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("OnlyForRepresentatives", policy =>
    {
        policy.RequireClaim(ClaimTypes.Role, "Representative");
    });
    opts.AddPolicy("OnlyForStudents", policy =>
    {
        policy.RequireClaim(ClaimTypes.Role, "Student");
    });
});

var app = builder.Build();

// if (!app.Environment.IsDevelopment())
// {
//     app.UseExceptionHandler("/Home/Error");
//     app.UseHsts();
// }

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();