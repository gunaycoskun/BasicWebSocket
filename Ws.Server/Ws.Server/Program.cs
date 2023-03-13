using Ws.Server;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();
var wsOptions = new WebSocketOptions {
    KeepAliveInterval=TimeSpan.FromSeconds(5),
};
app.UseWebSockets(wsOptions);
app.UseCustomMiddleware();

app.Run();

