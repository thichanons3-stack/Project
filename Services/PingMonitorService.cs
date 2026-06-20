using Microsoft.EntityFrameworkCore;
using NetworkManager.Data;
using NetworkManager.Models;
using System.Net.NetworkInformation;

namespace NetworkManager.Services;

public class PingMonitorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PingMonitorService> _logger;
    private readonly IConfiguration _config;

    public PingMonitorService(
        IServiceScopeFactory scopeFactory,
        ILogger<PingMonitorService> logger,
        IConfiguration config)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PingMonitorService started.");

        // Wait a bit before first ping (give app time to fully start)
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        var intervalMinutes = _config.GetValue<int>("PingMonitor:IntervalMinutes", 5);

        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckAllDevicesAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
        }
    }

    private async Task CheckAllDevicesAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var devices = await db.Devices.ToListAsync(stoppingToken);
        _logger.LogInformation("Pinging {Count} devices...", devices.Count);

        foreach (var device in devices)
        {
            if (stoppingToken.IsCancellationRequested) break;

            var status = await PingDeviceAsync(device.IpAddress);
            device.Status = status;
            device.LastChecked = DateTime.Now;
        }

        await db.SaveChangesAsync(stoppingToken);
        _logger.LogInformation("Ping check complete.");
    }

    private async Task<DeviceStatus> PingDeviceAsync(string ipAddress)
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(ipAddress, timeout: 3000);
            return reply.Status == IPStatus.Success
                ? DeviceStatus.Online
                : DeviceStatus.Offline;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Ping failed for {IP}: {Msg}", ipAddress, ex.Message);
            return DeviceStatus.Unknown;
        }
    }
}
