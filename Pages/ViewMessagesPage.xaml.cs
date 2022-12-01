using NapierBankApplication.Classes;
using System.Windows;
using System.Windows.Controls;

namespace NapierBankApplication.Pages
{
    public partial class ViewMessagesPage : Page
    {
        #region VARIABLES
        private int messageCounter;
        #endregion

        #region CONSTRUCTOR
        public ViewMessagesPage()
        {
            InitializeComponent();

            Lists lists = new Lists(); //object for using List's non-static methods

            lists.RetrieveStoredMessages(); //ensures the list of messages is up-to-date

            messageCounter = 0; //index for navigating the list of messages

            //Sets the text boxes as the details from the first message in the list
            txtHeader.Text = Lists.StoredMessages[messageCounter].Header;
            txtSender.Text = Lists.StoredMessages[messageCounter].Sender;
            txtSubject.Text = Lists.StoredMessages[messageCounter].Subject;
            txtMessageText.Text = Lists.StoredMessages[messageCounter].MessageText;
        }
        #endregion

        #region BUTTON EVENTS
        private void btnHomePage_Click(object sender, RoutedEventArgs e)
        {
            HomePage page = new HomePage();
            this.NavigationService.Navigate(page);
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if(messageCounter == Lists.StoredMessages.Count - 1) //minus 1 because List elements are 0-based
            {
                MessageBox.Show("This is the last message");
            }
            else //show the next message in the List
            {
                messageCounter++; //indexer is incremented

                txtHeader.Text = Lists.StoredMessages[messageCounter].Header;
                txtSender.Text = Lists.StoredMessages[messageCounter].Sender;
                txtSubject.Text = Lists.StoredMessages[messageCounter].Subject;
                txtMessageText.Text = Lists.StoredMessages[messageCounter].MessageText;
            }            

        }

        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            if(messageCounter == 0)
            {
                MessageBox.Show("This is the first message");
            }
            else //show the previous message in the list
            {
                messageCounter--; //indexer is decremented

                txtHeader.Text = Lists.StoredMessages[messageCounter].Header;
                txtSender.Text = Lists.StoredMessages[messageCounter].Sender;
                txtSubject.Text = Lists.StoredMessages[messageCounter].Subject;
                txtMessageText.Text = Lists.StoredMessages[messageCounter].MessageText;

            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            /*User is given a final chance to look at the trending, mentions and SIR lists before exiting
             */
            HomePage page = new HomePage();
            this.NavigationService.Navigate(page);
            MessageBoxResult result = MessageBox.Show(Title = "Exit Confirmation", "Application is closing", 
                MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (result == MessageBoxResult.OK)
            {
                Application.Current.Shutdown();
            }
        }
        #endregion

    }
}
