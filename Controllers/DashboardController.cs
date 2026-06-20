using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetworkManager.Data;
using NetworkManager.Models;

namespace NetworkManager.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var devices = await _db.Devices.ToListAsync();

        ViewBag.TotalDevices = devices.Count;
        ViewBag.OnlineDevices = devices.Count(d => d.Status == DeviceStatus.Online);
        ViewBag.OfflineDevices = devices.Count(d => d.Status == DeviceStatus.Offline);
        ViewBag.UnknownDevices = devices.Count(d => d.Status == DeviceStatus.Unknown);
        ViewBag.RouterCount = devices.Count(d => d.Type == DeviceType.Router);
        ViewBag.SwitchCount = devices.Count(d => d.Type == DeviceType.Switch);
        ViewBag.ServerCount = devices.Count(d => d.Type == DeviceType.Server);

        var totalIps = await _db.IpAssignments.CountAsync();
        var assignedIps = await _db.IpAssignments.CountAsync(i => i.IsAssigned);
        ViewBag.TotalIps = totalIps;
        ViewBag.AssignedIps = assignedIps;
        ViewBag.FreeIps = totalIps - assignedIps;

        // Recent offline devices
        var offlineDevices = devices
            .Where(d => d.Status == DeviceStatus.Offline)
            .OrderByDescending(d => d.LastChecked)
            .Take(5)
            .ToList();
        ViewBag.OfflineList = offlineDevices;

        // Recent maintenance logs
        var recentLogs = await _db.MaintenanceLogs
            .Include(m => m.Device)
            .OrderByDescending(m => m.Date)
            .Take(5)
            .ToListAsync();
        ViewBag.RecentLogs = recentLogs;

        return View(devices);
    }
}
