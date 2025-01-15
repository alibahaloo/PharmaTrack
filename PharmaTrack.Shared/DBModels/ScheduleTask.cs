using System.ComponentModel.DataAnnotations;

namespace PharmaTrack.Shared.DBModels
{
    public class ScheduleTask
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string UserName { get; set; } = default!;
        [Required]
        public DateTime Start {  get; set; }
        [Required]
        public DateTime End { get; set; }
        [Required]
        public string Description { get; set; } = default!;
    }
}
