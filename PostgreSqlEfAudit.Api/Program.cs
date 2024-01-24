using Microsoft.EntityFrameworkCore;
using PostgreSqlEfAudit.Api.Data;
using PostgreSqlEfAudit.Api.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/my-entity", async (AppDbContext context) =>
{
    return Results.Ok(await context.MyEntities.ToListAsync());
})
.WithName("GetMyEntities")
.WithOpenApi();


app.MapPost("/my-entity", async (AppDbContext context) =>
    {
        var entity = new MyEntity
        {
            Id = Guid.NewGuid(),
            Name = "Entity Name",
            MyProperties = new MyProperties
            {
                Properties = new List<MyNestedProperty>
                {
                    new MyNestedProperty
                    {
                        Value = "Prop1"
                    },
                    new MyNestedProperty
                    {
                        Value = "Prop2"
                    }
                }
            }
        };
        await context.MyEntities.AddAsync(entity);
        await context.SaveChangesAsync();
        
        return Results.Ok();
    })
    .WithName("AddMyEntity")
    .WithOpenApi();


app.MapPut("/my-entity", async (AppDbContext context) =>
    {
        var entity = await context.MyEntities.FirstOrDefaultAsync();

        if (entity is null)
        {
            return Results.Ok();
        }

        entity.Name += "***";
        entity.MyProperties.Properties.Add(new MyNestedProperty() { Value = "New property"});
        
        context.MyEntities.Update(entity);
        await context.SaveChangesAsync();
        
        return Results.Ok();
    })
    .WithName("EditMyEntity")
    .WithOpenApi();

using (var dbScope = app.Services.CreateScope())
{
    var dbContext = dbScope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();