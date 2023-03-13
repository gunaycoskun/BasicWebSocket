using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace Ws.Server {
    public class CustomMiddleware {
        private readonly RequestDelegate _next;

        public CustomMiddleware(RequestDelegate next) {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext) {

            if (httpContext.Request.Path == "/send") {

                if (httpContext.WebSockets.IsWebSocketRequest) {
                    using (WebSocket webSocket = await httpContext.WebSockets.AcceptWebSocketAsync()) {
                        await Send(httpContext, webSocket);
                    }
                }
            }
            else {
                httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            await _next(httpContext);
        }
        private async Task Send(HttpContext context, WebSocket webSocket) {
            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result != null) {
                while (!result.CloseStatus.HasValue) {
                    string msg = Encoding.UTF8.GetString(new ArraySegment<byte>(buffer, 0, result.Count));
                    await Console.Out.WriteLineAsync($"Client message: {msg}");
                    await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes($"Server message: {DateTime.UtcNow:f}")), result.MessageType, result.EndOfMessage, System.Threading.CancellationToken.None);
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), System.Threading.CancellationToken.None);
                }
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

    }
  
  
    public static class CustomMiddlewareExtensions {
        public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder builder) {
            return builder.UseMiddleware<CustomMiddleware>();
        }
    }
}
