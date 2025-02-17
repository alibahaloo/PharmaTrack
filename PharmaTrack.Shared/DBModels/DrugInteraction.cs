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
        public string? DrugAID { get; set; }

        [Column("DDInterID_B")]
        public string? DrugBID { get; set; }

        [Column("Drug_A")]
        public string? DrugA { get; set; }

        [Column("Drug_B")]
        public string? DrugB { get; set; }
        
        [Column("Level")]
        public string? Level { get; set; }
        public string? Description { get; set; }
        public string? Management { get; set; }
        public int InteractionID { get; set; }
    }
}
