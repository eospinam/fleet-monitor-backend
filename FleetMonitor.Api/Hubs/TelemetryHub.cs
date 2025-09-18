using Microsoft.AspNetCore.SignalR;

namespace FleetMonitor.Api.Hubs {
    public class TelemetryHub : Hub {
        public override async Task OnConnectedAsync()
        {
            if (Context.User?.IsInRole("admin") ?? false)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
            }
            await base.OnConnectedAsync();
        }
    }
}
