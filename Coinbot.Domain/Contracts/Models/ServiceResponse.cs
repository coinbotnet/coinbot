namespace Coinbot.Domain.Contracts.Models
{
    public class ServiceResponse
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public bool Success
        {
            get
            {
                return Status == 0;
            }
        }

        public ServiceResponse(int status, string msg = null)
        {
            this.Status = status;
            this.Message = msg;
        }
    }

    public class ServiceResponse<T> : ServiceResponse
    {
        public T Data { get; set; }
        public ServiceResponse(int status, T data, string msg = null) : base(status, msg)
        {
            this.Data = data;
        }
    }
}