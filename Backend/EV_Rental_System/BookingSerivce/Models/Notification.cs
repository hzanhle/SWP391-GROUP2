using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingSerivce.Models
{
    public class Notification
    {
        public int? Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string DataType { get; set; }
        public int? DataId { get; set; } // ID liên quan đến dữ liệu cụ thể (nếu có)
        public int? StaffId { get; set; }
        public int UserId { get; set; }
        public DateTime Created { get; set; }

        public Notification(string title, string description, string dataType, int? dataId, int? staffId, int userId, DateTime created)
        {
            Title = title;
            Description = description;
            DataType = dataType;
            DataId = dataId;
            StaffId = staffId;
            UserId = userId;
            Created = created;
        }

        public Notification()
        {
            
        }
    }
}
