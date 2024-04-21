namespace application.Master_Services
{
    public class Response
    {
        public Response()
        {
            bool success = Status >= 100 && Status <= 399;

            if (success)
                Message = "Success";
            else
                Message = "An occured unexpected error";

            IsSuccess = Status >= 200 && Status <= 299;
        }

        public int Status { get; internal set; } = 500;
        public bool IsSuccess { get; private set; }
        public string Message { get; internal set; }
        public object? ObjectData { get; internal set; }
    }
}
