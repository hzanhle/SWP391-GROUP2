// Thêm validation
using System.ComponentModel.DataAnnotations;

public class StationDashboardDTO
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    [Required]
    public string Location { get; set; } = null!;

    public bool IsActive { get; set; }
    public int? ManagerId { get; set; }

    public string Status => IsActive ? "Hoạt động" : "Ngừng hoạt động";
}