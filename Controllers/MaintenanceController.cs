using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NetworkManager.Data;
using NetworkManager.Models;

namespace NetworkManager.Controllers;

[Authorize]
public class MaintenanceController : Controller
{
    private readonly AppDbContext _db;

    public MaintenanceController(AppDbContext db)
    {
        _db = db;
    }

    // GET: /Maintenance
    public async Task<IActionResult> Index(int? deviceId, string? workType)
    {
        var query = _db.MaintenanceLogs.Include(m => m.Device).AsQueryable();

        if (deviceId.HasValue)
            query = query.Where(m => m.DeviceId == deviceId.Value);

        if (!string.IsNullOrWhiteSpace(workType) && Enum.TryParse<WorkType>(workType, out var wt))
            query = query.Where(m => m.WorkType == wt);

        ViewBag.DeviceId = deviceId;
        ViewBag.WorkType = workType;
        await LoadDeviceSelectListAsync(deviceId);

        var list = await query.OrderByDescending(m => m.Date).ToListAsync();
        return View(list);
    }

    // GET: /Maintenance/Create?deviceId=5
    public async Task<IActionResult> Create(int? deviceId)
    {
        await LoadDeviceSelectListAsync(deviceId);
        return View(new MaintenanceLog
        {
            DeviceId = deviceId ?? 0,
            Date = DateTime.Now,
            Technician = "Admin"
        });
    }

    // POST: /Maintenance/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MaintenanceLog log)
    {
        if (!ModelState.IsValid)
        {
            await LoadDeviceSelectListAsync(log.DeviceId);
            return View(log);
        }

        log.CreatedAt = DateTime.Now;
        _db.MaintenanceLogs.Add(log);
        await _db.SaveChangesAsync();

        TempData["Success"] = "บันทึกประวัติการดูแลเรียบร้อยแล้ว";

        // Redirect back to device details if came from there
        return RedirectToAction(nameof(Index), new { deviceId = log.DeviceId });
    }

    // GET: /Maintenance/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var log = await _db.MaintenanceLogs.FindAsync(id);
        if (log == null) return NotFound();
        await LoadDeviceSelectListAsync(log.DeviceId);
        return View(log);
    }

    // POST: /Maintenance/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MaintenanceLog log)
    {
        if (id != log.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            await LoadDeviceSelectListAsync(log.DeviceId);
            return View(log);
        }

        var existing = await _db.MaintenanceLogs.FindAsync(id);
        if (existing == null) return NotFound();

        existing.DeviceId = log.DeviceId;
        existing.Date = log.Date;
        existing.Technician = log.Technician;
        existing.WorkType = log.WorkType;
        existing.Description = log.Description;

        await _db.SaveChangesAsync();
        TempData["Success"] = "แก้ไขบันทึกการดูแลเรียบร้อยแล้ว";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Maintenance/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var log = await _db.MaintenanceLogs.FindAsync(id);
        if (log == null) return NotFound();

        _db.MaintenanceLogs.Remove(log);
        await _db.SaveChangesAsync();
        TempData["Success"] = "ลบบันทึกการดูแลเรียบร้อยแล้ว";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadDeviceSelectListAsync(int? selectedId = null)
    {
        var devices = await _db.Devices.OrderBy(d => d.Name).ToListAsync();
        ViewBag.DeviceList = new SelectList(devices, "Id", "Name", selectedId);
    }
}
