namespace PharmaTrack.Shared.APIModels
{
    public class PagedResponse<T>
    {
        public List<T> Data { get; set; } = [];
        public int CurrentPage { get; set; }
        public int CurrentItemCount { get; set; }
        public int TotalPageCount { get; set; }
        public int TotalItemCount { get; set; }
    }
}
