namespace PharmaTrack.Core.DTOs
{
    public class InventoryRequest
    {
        public string? UPC { get; set; }
        public string? Name { get; set; }
        public string? Brand { get; set; }
        public string? DIN { get; set; }
        public string? NPN { get; set; }
    }
}
