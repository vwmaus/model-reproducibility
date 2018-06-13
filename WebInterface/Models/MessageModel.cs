namespace WebInterface.Models
{
    public class MessageModel
    {
        public int Id
        {
            get
            {
                counter++;
                return counter;
            }
        }

        public string Message { get; set; }

        private static int counter;
    }
}
