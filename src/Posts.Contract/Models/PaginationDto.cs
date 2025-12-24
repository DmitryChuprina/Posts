namespace Posts.Contract.Models
{
    public class PaginationRequestDto
    {
        public int From { get; set; } = 0;
        public int Limit { get; set; } = 10;
    }

    public class PaginationResponseDto<TItem>
    {
        public int? Total { get; set; }

        public IEnumerable<TItem> Items { get; set; } = [];
    }
}
