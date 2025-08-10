var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddHttpsRedirection(Options =>
{
    Options.HttpsPort = 5001;
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


app.UseWebSockets();

app.UseAuthorization();

app.MapControllers(); 

app.Run();
