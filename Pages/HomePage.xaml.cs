using NapierBankApplication.Classes;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace NapierBankApplication.Pages
{
    public partial class HomePage : Page
    {
        #region CONSTRUCTOR
        public HomePage()
        {
            InitializeComponent();
            Lists lists = new Lists(); //lists object used to call non-static methods within the Lists class
            lists.RetrieveStoredMessages();

            lists.RetrieveMentionsList(); //every time a new HomePage is created, we get the latest list
            UpdateMentionsTextBox(); //then write that to the text box

            //Now we carry out the same process for Trending and SIR lists
            lists.RetrieveTrendingList();
            UpdateTrendingTextBox();

            lists.RetrieveSIRList();
            UpdateSIRTextBox();

        }
        #endregion

        #region BUTTON EVENTS
        private void btnInputMessagesPage_Click(object sender, RoutedEventArgs e)
        {
            InputMessagesPage page = new InputMessagesPage(); //Input messages page is displayed in the window
            this.NavigationService.Navigate(page);
        }

        private void btnViewMessagesPage_Click(object sender, RoutedEventArgs e)
        {
            if (Lists.StoredMessages.Count == 0) //If the JSON string is empty (no messages have been inputted)
            {
                MessageBoxResult result = MessageBox.Show("You have not entered any messages yet", "", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            else
            {
                ViewMessagesPage page = new ViewMessagesPage(); //View messages page is displayed in the window
                this.NavigationService.Navigate(page);
            }
            
        }

        private void btnExit_Click(object sender, RoutedEventArgs e) //closes the application
        {
            Application.Current.Shutdown();
        }
        #endregion

        #region PRIVATE METHODS
        private void UpdateTrendingTextBox() //displays each hashtag in the trending text box
        {
            /*To satisfy the requirement of ordering the trending list from most to least frequency, the Linq
             * OrderByDescending feature sorts the list by value, which in this case represents the count of each
             * hashtag across all stored messages
             */
            foreach (KeyValuePair<string, int> hashtag in Lists.TrendingList.OrderByDescending(key => key.Value))
            {
                /*For semantics, it is good to know whether to say "mention" or "mentions" for each hashtags, because if
                 * the hashtag has only been used once, we should say "1 mention". If the hashtags has appeared more than once 
                 * we should say "n mentions".
                 */
                string singleOrPlural = (hashtag.Value == 1) ? "mention" : "mentions"; //ternary operator

                txtTrendingList.Text += $"{hashtag.Key}{Environment.NewLine}{hashtag.Value} " +
                    $"{singleOrPlural}{Environment.NewLine}{Environment.NewLine}"; 
            }
        }

        private void UpdateMentionsTextBox() //displays twiiter ID in the mentions text box
        {           
            foreach(string twitterID in Lists.MentionsList)
            {
                txtMentionsList.Text += twitterID + Environment.NewLine; 
            }
        }

        private void UpdateSIRTextBox() //displays each SIR incident in the SIR text box
        {
            foreach(KeyValuePair<string, string> incident in Lists.SIRList)
            {
                txtSIRList.Text += $"Sort Code: {incident.Key} Nature of incident: {incident.Value}" +
                    $"{Environment.NewLine}{Environment.NewLine}"; 
            }
        }
        #endregion

    }
}
