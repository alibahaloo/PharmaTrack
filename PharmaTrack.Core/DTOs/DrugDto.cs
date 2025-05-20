using System;

namespace PharmaTrack.Core.DTOs
{
    public class DrugDto
    {
        public int Id { get; set; }

        public string Hash { get; set; }

        public int DrugCode { get; set; }

        public string ProductCategorization { get; set; }

        public string Class { get; set; }

        public string DrugIdentificationNumber { get; set; }

        public string BrandName { get; set; }

        public string Descriptor { get; set; }

        public string PediatricFlag { get; set; }

        public string AccessionNumber { get; set; }

        public string NumberOfAis { get; set; }

        public DateTime LastUpdateDate { get; set; }
        public string AiGroupNo { get; set; }
    }
}
