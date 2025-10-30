﻿using System.ComponentModel.DataAnnotations;

namespace TwoWheelVehicleService.DTOs
{
    public class VehicleRequest
    {
        [Required(ErrorMessage = "ModelId không được để trống")]
        public int ModelId { get; set; }
        [Required(ErrorMessage = "StationId không được để trống")]
        public int StationId { get; set; }
        [Required(ErrorMessage = "Màu xe không được để trống")]
        [StringLength(30, ErrorMessage = "Màu xe không được vượt quá 30 ký tự")]
        public string Color { get; set; }

        [RegularExpression(@"^[0-9]{2}[A-Z]{1,2}[0-9]-[0-9]{3,4}(\.[0-9]{2})?$",
    ErrorMessage = "Biển số xe không hợp lệ. Ví dụ: 59X3-123.45 hoặc 30H1-5678")]
        public string LicensePlate { get; set; }


    }
}
