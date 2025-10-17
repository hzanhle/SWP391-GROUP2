﻿namespace StationService.DTOs
{
    public class StationDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Location { get; set; } = null!;
        public int? ManagerId { get; set; } // User với role là Employee
        public bool IsActive { get; set; }
        public ICollection<FeedbackDTO> Feedbacks { get; set; } = new List<FeedbackDTO>();
        public StationDTO() { }
        public StationDTO(int id, string name, string location, int? managerId, bool isActive)
        {
            Id = id;
            Name = name;
            Location = location;
            ManagerId = managerId;
            IsActive = isActive;
        }

    }
}
