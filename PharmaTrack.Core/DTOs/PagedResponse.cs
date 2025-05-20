using System.Collections.Generic;

namespace PharmaTrack.Core.DTOs
{
    public class PagedResponse<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public int CurrentPage { get; set; }
        public int CurrentItemCount { get; set; }
        public int TotalPageCount { get; set; }
        public int TotalItemCount { get; set; }
    }
}
