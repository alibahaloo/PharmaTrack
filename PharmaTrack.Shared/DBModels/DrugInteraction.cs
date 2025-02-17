using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaTrack.Shared.DBModels
{
    public class DrugInteraction
    {
        [Key]
        public int Id { get; set; }

        [Column("HASH")]
        public string? Hash { get; set; }

        [Column("DDInterID_A")]
        public string DrugAID { get; set; } = default!;

        [Column("DDInterID_B")]
        public string DrugBID { get; set; } = default!;

        [Column("Drug_A")]
        public string DrugA { get; set; } = default!;

        [Column("Drug_B")]
        public string DrugB { get; set; } = default!;
        
        [Column("Level")]
        public string Level { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string Management { get; set; } = default!;
        public int InteractionID { get; set; }
    }
}
