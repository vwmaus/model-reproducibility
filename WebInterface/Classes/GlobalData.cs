namespace WebInterface.Classes
{
    using System.Collections.Generic;
    using WebInterface.Models;

    public static class GlobalData
    {
        private static List<MessageModel> messages;

        public static List<MessageModel> Messages
        {
            get
            {
                if (!messages.IsNull())
                {
                    return messages;
                }

                messages = new List<MessageModel>();
                return messages;

            }
            set => messages = value;
        }
    }
}