using GCDService.Controllers.Post;
using GCDService.DB;
using GCDService.Helpers;
using GCDService.Managers.Cash;
using GCDService.Managers.Events;
using GCDService.Managers.Permission;
using GCDService.Managers.Session;

var MyAllowSpecificOrigins = "allowAll";
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                      });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseHttpsRedirection();

app.UseCors(MyAllowSpecificOrigins);
app.UseAuthorization();

app.MapControllers();

RSAHelper.Initialize();
WebsiteDB.Initialize();
GameDB.Initialize();
EventDB.Initialize();
    

PermissionManager.Initialize();
SessionManager.Initialize();
CashProductManager.Initialize();
GameEventManager.Initialize();

app.Run();
