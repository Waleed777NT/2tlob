namespace _2tlob.ViewModels.Common
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public string? Message { get; set; }
    }
}
