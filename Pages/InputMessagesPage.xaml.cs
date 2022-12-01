using NapierBankApplication.Classes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NapierBankApplication.Pages
{
    public partial class InputMessagesPage : Page
    {
        #region VARIABLES
        private Validate validateMessage = new Validate();
        private Sanitise sanitiseMessage = new Sanitise();
        private Lists lists = new Lists();
        #endregion

        #region CONSTRUCTOR
        public InputMessagesPage()
        {
            InitializeComponent();

            //this is used to ensure the Lists class has read in the json file before we write to it
            lists.RetrieveStoredMessages();
        }
        #endregion

        #region BUTTON EVENTS
        private void btnHomePage_Click(object sender, RoutedEventArgs e)
        {
            HomePage page = new HomePage();
            this.NavigationService.Navigate(page);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            /*The methods for validating header and message body return false if not valid, otherwise
             * they return true.
             */

            //message HEADER validation
            if (!validateMessage.ValidateMessageHeader(txtMessageHeader.Text.Trim()))
            {
                MessageBox.Show("You have not entered a valid message header");
                txtMessageHeader.Focus();
                return;
            }

            /*message BODY validation
             * As part of determining whether a message header is valid, the message type is discovered. Based on this,
             * the correct method to validate the message body can be called
             */

            if(validateMessage.IsSMS)
            {
                if(!validateMessage.ValidateSMSBody(txtMessageBody.Text.Trim()))
                {
                    MessageBox.Show("You have not entered a valid message body");
                    return;
                }
            }
            else if(validateMessage.IsEmail)
            {
                if(!validateMessage.ValidateEmailBody(txtMessageBody.Text.Trim()))
                {
                    MessageBox.Show("You have not entered a valid message body");
                    return;
                }
                
            }
            else
            {
                if(!validateMessage.ValidateTweetBody(txtMessageBody.Text.Trim()))
                {
                    MessageBox.Show("You have not entered a valid message body");
                    return;
                }
                
            }

            /*If we get to this point in the method, the message has passed both the header and 
             * body validation, so we can proceed to take the header and body from the text boxes, store the
             * info in local variables, and send them off to be sanitised.
             */
            string messageText = validateMessage.MessageText;
            string header = validateMessage.Header;
            string messageSender = validateMessage.Sender;
            string subject = validateMessage.Subject;

            /*Call method to sanitise message, depending on the type the system has detected it is.
             * The memory location of the message text is passed to the method by using the 'ref' keyword. This means the actual
             * message can be changed within the sanitise methods, rather than passing by value and creating new copy
             * of the message text.
             */

            if(validateMessage.IsSMS)
            {
                sanitiseMessage.SanitiseSMS(ref messageText);
            }
            else if (validateMessage.IsEmail)
            {
                sanitiseMessage.SanitiseEmail(ref messageText, header, subject);
            }
            else
            {
                sanitiseMessage.SanitiseTweet(ref messageText);
            }

            //Output sanitised version of the message to the Output boxes
            txtOutputHeader.Text = header;
            txtOutputSender.Text = messageSender;
            txtOutputSubject.Text = subject;
            txtOutputBody.Text = messageText;

            /*Here we create a Message object, with the sanitised version of the message
             * set as the property values for that Message. Then the new Message object is passed to the 
             * Lists class where it will be added to List<Message> and JSON string updated.
             */
            Message message = new Message();
            message.Header = header;
            message.Sender = messageSender;
            message.Subject = subject;
            message.MessageText = messageText;

            lists.UpdateMessagesList(ref message); //list containing all messages is updated
        }

        private void btnClear_Click(object sender, RoutedEventArgs e) //clears all text boxes
        {
            InputMessagesPage page = new InputMessagesPage();
            this.NavigationService.Navigate(page);
        }

        /*By default, a helpful message is displayed in the text box for entering the message body which describes
         * how the message should be formatted by the user - each section of the message is written on a new line. The two private
         * methods below hide and show the message depending on when the user's cursor enters and leaves the text box. If the user
         * has written a message and the default text is no longer in the text box, when the cursor leaves the text box the message
         * that the user has written will stay
         */
        private void txtMessageBody_MouseEnter(object sender, MouseEventArgs e)
        {
            if (txtMessageBody.Text.StartsWith("Sender"))
            {
                txtMessageBody.Text = string.Empty;
            }

        }

        private void txtMessageBody_MouseLeave(object sender, MouseEventArgs e)
        {
            if (txtMessageBody.Text == "")
            {
                txtMessageBody.Text = "Sender\nSubject (if applicable)\nMessage body";
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            /*When user clicks exit, the lists on the home page should be displayed as per the functional requirement that
             * stipulates this. Once the user has had the chance to see the lists, they can click Ok to close the app
             */
            HomePage page = new HomePage();
            this.NavigationService.Navigate(page);
            MessageBoxResult result = MessageBox.Show(Title = "Exit Confirmation", "Application is closing", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if(result == MessageBoxResult.OK)
            {
                Application.Current.Shutdown();
            }
        }

        #endregion
    }
}
