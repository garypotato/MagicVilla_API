using MagicVilla_VillaAPI;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Repository;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(option => 
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultSQLConnection"))
);
// Add auto mapper
builder.Services.AddAutoMapper(typeof(MappingConfig));
// add repository
builder.Services.AddScoped<IVillaRepository, VillaRepository>();
builder.Services.AddScoped<IVillaNumberRepository, VillaNumberRepository>();

builder.Services
.AddControllers(options =>
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // if no json, will return 406 
    //options.ReturnHttpNotAcceptable = true; 
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

})
.AddNewtonsoftJson()
.AddXmlDataContractSerializerFormatters(); // can return xml format when request header is set to application/xml

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

app.UseAuthorization();

app.MapControllers();

app.Run();
