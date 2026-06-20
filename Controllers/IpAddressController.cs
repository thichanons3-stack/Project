using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NetworkManager.Data;
using NetworkManager.Models;

namespace NetworkManager.Controllers;

[Authorize]
public class IpAddressController : Controller
{
    private readonly AppDbContext _db;

    public IpAddressController(AppDbContext db)
    {
        _db = db;
    }

    // GET: /IpAddress
    public async Task<IActionResult> Index(string? search, bool? assigned)
    {
        var query = _db.IpAssignments.Include(i => i.Device).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(i => i.Address.Contains(search) || (i.Description != null && i.Description.Contains(search)));

        if (assigned.HasValue)
            query = query.Where(i => i.IsAssigned == assigned.Value);

        ViewBag.Search = search;
        ViewBag.Assigned = assigned;

        var list = await query.OrderBy(i => i.Address).ToListAsync();
        return View(list);
    }

    // GET: /IpAddress/Create
    public async Task<IActionResult> Create()
    {
        await LoadDeviceSelectListAsync();
        return View(new IpAssignment());
    }

    // POST: /IpAddress/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(IpAssignment ip)
    {
        if (await _db.IpAssignments.AnyAsync(i => i.Address == ip.Address))
            ModelState.AddModelError(nameof(IpAssignment.Address), "IP Address นี้มีในระบบแล้ว");

        if (!ModelState.IsValid)
        {
            await LoadDeviceSelectListAsync();
            return View(ip);
        }

        ip.IsAssigned = ip.DeviceId.HasValue;
        ip.CreatedAt = DateTime.Now;
        _db.IpAssignments.Add(ip);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"เพิ่ม IP '{ip.Address}' เรียบร้อยแล้ว";
        return RedirectToAction(nameof(Index));
    }

    // GET: /IpAddress/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var ip = await _db.IpAssignments.FindAsync(id);
        if (ip == null) return NotFound();
        await LoadDeviceSelectListAsync();
        return View(ip);
    }

    // POST: /IpAddress/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, IpAssignment ip)
    {
        if (id != ip.Id) return BadRequest();

        if (await _db.IpAssignments.AnyAsync(i => i.Address == ip.Address && i.Id != id))
            ModelState.AddModelError(nameof(IpAssignment.Address), "IP Address นี้มีในระบบแล้ว");

        if (!ModelState.IsValid)
        {
            await LoadDeviceSelectListAsync();
            return View(ip);
        }

        var existing = await _db.IpAssignments.FindAsync(id);
        if (existing == null) return NotFound();

        existing.Address = ip.Address;
        existing.SubnetMask = ip.SubnetMask;
        existing.Gateway = ip.Gateway;
        existing.Description = ip.Description;
        existing.DeviceId = ip.DeviceId;
        existing.IsAssigned = ip.DeviceId.HasValue;

        await _db.SaveChangesAsync();
        TempData["Success"] = $"แก้ไข IP '{existing.Address}' เรียบร้อยแล้ว";
        return RedirectToAction(nameof(Index));
    }

    // POST: /IpAddress/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var ip = await _db.IpAssignments.FindAsync(id);
        if (ip == null) return NotFound();

        _db.IpAssignments.Remove(ip);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"ลบ IP '{ip.Address}' เรียบร้อยแล้ว";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadDeviceSelectListAsync()
    {
        var devices = await _db.Devices.OrderBy(d => d.Name).ToListAsync();
        ViewBag.DeviceList = new SelectList(devices, "Id", "Name");
    }
}
