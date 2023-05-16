using BDSManager.WebUI.Services;
using BDSManager.WebUI.Hubs;
using BDSManager.WebUI.IO;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddSingleton<ConsoleHub>();
builder.Services.AddSingleton<CommandHub>();
builder.Services.AddSingleton<MinecraftServerService>();
builder.Services.AddSingleton<ServerProperties>();
builder.Services.AddSingleton<BDSUpdater>();
builder.Services.AddSingleton<BDSAddon>();
builder.Services.AddSingleton<BDSBackup>();
builder.Services.AddSingleton<OptionsIO>();
builder.Services.AddSingleton<DirectoryIO>();
builder.Services.AddHostedService<BackupAndUpdateService>();

var app = builder.Build();

AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
{
    Console.WriteLine(e.ExceptionObject.ToString());
};

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

app.UseAuthorization();

app.MapRazorPages();
app.MapHub<ConsoleHub>("/consoleHub");
app.MapHub<CommandHub>("/commandHub");

app.Run();
