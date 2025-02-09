namespace PharmaTrack.Shared.APIModels
{
    public class DrugInfoRequest
    {
        public int? DrugCode { get; set; }
        public string? BrandName { get; set; }
        public string? DIN {  get; set; }
    }
}
