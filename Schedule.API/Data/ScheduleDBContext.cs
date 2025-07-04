﻿using Microsoft.EntityFrameworkCore;
using PharmaTrack.Core.DBModels;

namespace Schedule.API.Data
{
    public class ScheduleDBContext : DbContext
    {
        public ScheduleDBContext(DbContextOptions<ScheduleDBContext> options) : base(options) { }
        public DbSet<ScheduleTask> ScheduleTasks { get; set; } = null!;
    }
}
