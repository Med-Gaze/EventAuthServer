namespace med.common.library.database.paging
{
    public class PagingArgs
    {
        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public PagingStrategy PagingStrategy { get; set; }
    }
}
