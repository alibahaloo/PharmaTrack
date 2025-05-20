using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PharmaTrack.Core.DBModels
{
    public class ScheduleTask : IValidatableObject
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Username is required")]
        public string UserName { get; set; } = default!;
        [Required(ErrorMessage = "Start date & time is required")]
        public DateTime Start { get; set; }
        [Required(ErrorMessage = "End date & time is required")]
        public DateTime End { get; set; }
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = default!;
        public IEnumerable<ValidationResult> Validate(ValidationContext ctx)
        {
            // 1) Start.Date must be today or later
            if (Start.Date < DateTime.Today)
            {
                yield return new ValidationResult(
                    "Date cannot be in the past.",
                    new[] { nameof(Start) }
                );
            }

            // 2) End must be strictly after Start
            if (End <= Start)
            {
                yield return new ValidationResult(
                    "End time must be later than Start time.",
                    new[] { nameof(End) }
                );
            }
        }
    }
}
