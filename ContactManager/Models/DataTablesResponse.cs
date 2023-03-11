namespace ContactManager.Models
{
    public class DataTablesResponse<T>
    {
        public int TotalRecords { get; set; }
        public int TotalRecordsFiltered { get; set; }
        public List<T> Data { get; set; }
        public string Error { get; set; }
    }
}
