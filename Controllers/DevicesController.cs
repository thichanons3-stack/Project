using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetworkManager.Data;
using NetworkManager.Models;

namespace NetworkManager.Controllers;

[Authorize]
public class DevicesController : Controller
{
    private readonly AppDbContext _db;

    public DevicesController(AppDbContext db)
    {
        _db = db;
    }

    // GET: /Devices
    public async Task<IActionResult> Index(string? search, string? type, string? status)
    {
        var query = _db.Devices.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(d => d.Name.Contains(search) || d.IpAddress.Contains(search) || (d.Location != null && d.Location.Contains(search)));

        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<DeviceType>(type, out var devType))
            query = query.Where(d => d.Type == devType);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<DeviceStatus>(status, out var devStatus))
            query = query.Where(d => d.Status == devStatus);

        ViewBag.Search = search;
        ViewBag.Type = type;
        ViewBag.Status = status;

        var devices = await query.OrderBy(d => d.Name).ToListAsync();
        return View(devices);
    }

    // GET: /Devices/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var device = await _db.Devices
            .Include(d => d.MaintenanceLogs.OrderByDescending(m => m.Date))
            .Include(d => d.IpAssignments)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (device == null) return NotFound();
        return View(device);
    }

    // GET: /Devices/Create
    public IActionResult Create() => View(new Device());

    // POST: /Devices/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Device device)
    {
        // Check for duplicate IP
        if (await _db.Devices.AnyAsync(d => d.IpAddress == device.IpAddress))
            ModelState.AddModelError(nameof(Device.IpAddress), "IP Address นี้มีในระบบแล้ว");

        if (!ModelState.IsValid) return View(device);

        device.CreatedAt = DateTime.Now;
        device.Status = DeviceStatus.Unknown;
        _db.Devices.Add(device);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"เพิ่มอุปกรณ์ '{device.Name}' เรียบร้อยแล้ว";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Devices/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var device = await _db.Devices.FindAsync(id);
        if (device == null) return NotFound();
        return View(device);
    }

    // POST: /Devices/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Device device)
    {
        if (id != device.Id) return BadRequest();

        // Check for duplicate IP (exclude current device)
        if (await _db.Devices.AnyAsync(d => d.IpAddress == device.IpAddress && d.Id != id))
            ModelState.AddModelError(nameof(Device.IpAddress), "IP Address นี้มีในระบบแล้ว");

        if (!ModelState.IsValid) return View(device);

        var existing = await _db.Devices.FindAsync(id);
        if (existing == null) return NotFound();

        existing.Name = device.Name;
        existing.Type = device.Type;
        existing.IpAddress = device.IpAddress;
        existing.MacAddress = device.MacAddress;
        existing.Location = device.Location;
        existing.Notes = device.Notes;

        await _db.SaveChangesAsync();
        TempData["Success"] = $"แก้ไขอุปกรณ์ '{existing.Name}' เรียบร้อยแล้ว";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Devices/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var device = await _db.Devices.FindAsync(id);
        if (device == null) return NotFound();

        _db.Devices.Remove(device);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"ลบอุปกรณ์ '{device.Name}' เรียบร้อยแล้ว";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Devices/PingNow/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PingNow(int id)
    {
        var device = await _db.Devices.FindAsync(id);
        if (device == null) return NotFound();

        try
        {
            using var ping = new System.Net.NetworkInformation.Ping();
            var reply = await ping.SendPingAsync(device.IpAddress, 3000);
            device.Status = reply.Status == System.Net.NetworkInformation.IPStatus.Success
                ? DeviceStatus.Online
                : DeviceStatus.Offline;
        }
        catch
        {
            device.Status = DeviceStatus.Unknown;
        }

        device.LastChecked = DateTime.Now;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"ตรวจสอบสถานะ '{device.Name}': {device.Status}";
        return RedirectToAction(nameof(Details), new { id });
    }
}
