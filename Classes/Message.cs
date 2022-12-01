
namespace NapierBankApplication.Classes
{
    public class Message
    {
        #region PROPERTIES
        public string Header { get; set; }
        public string Sender { get; set; }
        public string Subject { get; set; }
        public string MessageText { get; set; }
        #endregion

        #region CONSTRUCTOR
        public Message() //default constructor
        {

        }
        #endregion
    }
}
