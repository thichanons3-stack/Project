using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetworkManager.Models;

public class IpAssignment
{
    public int Id { get; set; }

    [Required(ErrorMessage = "กรุณาระบุ IP Address")]
    [RegularExpression(@"^(\d{1,3}\.){3}\d{1,3}$", ErrorMessage = "รูปแบบ IP Address ไม่ถูกต้อง")]
    [Display(Name = "IP Address")]
    public string Address { get; set; } = string.Empty;

    [StringLength(15)]
    [Display(Name = "Subnet Mask")]
    public string? SubnetMask { get; set; }

    [StringLength(15)]
    [Display(Name = "Default Gateway")]
    public string? Gateway { get; set; }

    [StringLength(100)]
    [Display(Name = "หมายเหตุ")]
    public string? Description { get; set; }

    [Display(Name = "สถานะการใช้งาน")]
    public bool IsAssigned { get; set; } = false;

    [Display(Name = "วันที่เพิ่ม")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // FK nullable - IP อาจยังไม่ถูก assign
    [Display(Name = "อุปกรณ์")]
    public int? DeviceId { get; set; }

    [ForeignKey(nameof(DeviceId))]
    public Device? Device { get; set; }
}
