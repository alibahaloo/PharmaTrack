using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Schedule.API.Data
{
    public class ScheduleDBContext : DbContext
    {
        public ScheduleDBContext(DbContextOptions<ScheduleDBContext> options) : base(options) { }
        public DbSet<ScheduleTask> ScheduleTasks { get; set; } = null!;
    }

    public class ScheduleTask
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string UserName { get; set; } = default!;
        [Required]
        public DateTime Start { get; set; }
        [Required]
        public DateTime End { get; set; }
        [Required]
        public string Description { get; set; } = default!;
    }
}
