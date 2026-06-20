using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetworkManager.Models;

public enum WorkType
{
    Repair,
    Upgrade,
    Inspection,
    Configuration,
    Replacement
}

public class MaintenanceLog
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "อุปกรณ์")]
    public int DeviceId { get; set; }

    [ForeignKey(nameof(DeviceId))]
    public Device? Device { get; set; }

    [Required(ErrorMessage = "กรุณาระบุวันที่")]
    [Display(Name = "วันที่ดำเนินการ")]
    public DateTime Date { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "กรุณาระบุชื่อผู้ดำเนินการ")]
    [StringLength(100)]
    [Display(Name = "ผู้ดำเนินการ")]
    public string Technician { get; set; } = string.Empty;

    [Required(ErrorMessage = "กรุณาเลือกประเภทงาน")]
    [Display(Name = "ประเภทงาน")]
    public WorkType WorkType { get; set; }

    [Required(ErrorMessage = "กรุณาระบุรายละเอียด")]
    [StringLength(1000)]
    [Display(Name = "รายละเอียด")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "บันทึกเมื่อ")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
