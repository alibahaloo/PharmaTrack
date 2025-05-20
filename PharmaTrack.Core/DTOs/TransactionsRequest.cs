using PharmaTrack.Core.DBModels;
namespace PharmaTrack.Core.DTOs
{
    public class TransactionsRequest
    {
        public string? UPC { get; set; }
        public string? Product { get; set; }
        public string? Brand { get; set; }
        public string? CreatedBy { get; set; }
        public TransactionType? Type { get; set; }
    }
}
