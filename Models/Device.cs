using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetworkManager.Models;

public enum DeviceType
{
    Router,
    Switch,
    Server
}

public enum DeviceStatus
{
    Online,
    Offline,
    Unknown
}

public class Device
{
    public int Id { get; set; }

    [Required(ErrorMessage = "กรุณาระบุชื่ออุปกรณ์")]
    [StringLength(100)]
    [Display(Name = "ชื่ออุปกรณ์")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "กรุณาเลือกประเภทอุปกรณ์")]
    [Display(Name = "ประเภท")]
    public DeviceType Type { get; set; }

    [Required(ErrorMessage = "กรุณาระบุ IP Address")]
    [RegularExpression(@"^(\d{1,3}\.){3}\d{1,3}$", ErrorMessage = "รูปแบบ IP Address ไม่ถูกต้อง")]
    [Display(Name = "IP Address")]
    public string IpAddress { get; set; } = string.Empty;

    [StringLength(17)]
    [Display(Name = "MAC Address")]
    public string? MacAddress { get; set; }

    [StringLength(200)]
    [Display(Name = "ตำแหน่ง/สถานที่")]
    public string? Location { get; set; }

    [StringLength(500)]
    [Display(Name = "หมายเหตุ")]
    public string? Notes { get; set; }

    [Display(Name = "สถานะ")]
    public DeviceStatus Status { get; set; } = DeviceStatus.Unknown;

    [Display(Name = "ตรวจสอบล่าสุด")]
    public DateTime? LastChecked { get; set; }

    [Display(Name = "วันที่เพิ่ม")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public ICollection<MaintenanceLog> MaintenanceLogs { get; set; } = new List<MaintenanceLog>();
    public ICollection<IpAssignment> IpAssignments { get; set; } = new List<IpAssignment>();
}
