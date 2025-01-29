using PharmaTrack.Shared.DBModels;

namespace PharmaTrack.Shared.APIModels
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
