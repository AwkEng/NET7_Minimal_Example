using BlazorApp1;
using BlazorApp1.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

builder.Services.AddDbContextFactory<ApplicationDbContext>(builder =>
    builder.UseNpgsql("Host=localhost;Database=TestDb;Username=postgres;Password=EZSP1234!;CommandTimeout=0;IncludeErrorDetails=true;", 
        x => x.UseNetTopologySuite()
    ).EnableSensitiveDataLogging()
);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var applicationDbFactory = services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();

    //rebuild database
    using (var dbContext = applicationDbFactory.CreateDbContext())
    {
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    var statesFilePath = Path.Combine(Directory.GetCurrentDirectory(), "us_states.geojson");

    //add states without geometries
    var statesWithoutGeometry = await ImportService.GetStatesWithoutGeometry(statesFilePath);
    await Parallel.ForEachAsync(statesWithoutGeometry, async (state, _) =>
    {
        using var dbContext = applicationDbFactory.CreateDbContext();
        await dbContext.StatesWithoutGeometry.AddAsync(state);
        await dbContext.SaveChangesAsync();
    });

    //add states with geometries
    var statesWithGeometry = await ImportService.GetStatesWithGeometry(statesFilePath);
    await Parallel.ForEachAsync(statesWithGeometry, async (state, _) =>
    {
        using var dbContext = applicationDbFactory.CreateDbContext();
        await dbContext.StatesWithGeometry.AddAsync(state);
        await dbContext.SaveChangesAsync();
    });
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
