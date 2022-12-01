using System;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Text.RegularExpressions;

namespace NapierBankApplication.Classes
{
    public class Validate
    {
        #region VARIABLES
        private string header = string.Empty;
        private string sender = string.Empty;
        private string subject = string.Empty;
        private string messageText = string.Empty;
        public string Header { get; set; }
        public string Sender { get; set; }
        public string Subject { get; set; }
        public string MessageText { get; set; }
        public bool IsSMS { get; private set; } = false;
        public bool IsEmail { get; private set; } = false;
        public bool IsTweet { get; private set; } = false;
        #endregion

        #region PUBLIC METHODS
        public bool ValidateMessageHeader(string messageHeader)
        {
            /*This method is called from the InputMessagesPage class to validate that the header the user has written
             * is not invalid. If the header is valid, the method returns true, otherwise it returns false. 
             * The category of the message is also detected by the system here based on the first letter of the header.
             */
            header = messageHeader.Trim().ToUpper();

            //Check 1 - Ensure header is 10 chars (10 since a valid header is 'S', 'E' or 'T' plus 9 numeric values)
            if(header.Length != 10)
            {
                return false;
            }

            /*Check 2 - Ensure all characters after first character are all numeric by checking whether
            * the string could be parsed to an integer or not.
            */

            int nums = 0;
            string numericValues = messageHeader.Substring(1, 9);
            bool canConvert = int.TryParse(numericValues, out nums);
            if(!canConvert)
            {
                return false;
            }

            //Check 3 - Message categorisation - SMS, email or tweet
            string firstValue = header[0].ToString();
            if(firstValue.ToUpper().Equals("S"))
            {
                IsSMS = true;
                return true;
            }
            else if(firstValue.ToUpper().Equals("E"))
            {
                IsEmail = true;
                return true;
            }
            else if(firstValue.ToUpper().Equals("T"))
            {
                IsTweet = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool ValidateSMSBody(string messageBody)
        {
            PhoneAttribute validatePhoneNumber = new PhoneAttribute(); //used to check if sender is valid international tel no
            string[] values = Regex.Split(messageBody, Environment.NewLine); //first line = sender, second line = message text

            if (messageBody.StartsWith("Sender")) //the information text is still present
            {
                return false;
            }

            if (values.Length == 0 || values.Length == 1) //sender and message not present
            {
                return false;
            }

            sender = values[0];
            messageText = values[1];

            if (sender.Length < 10) //international telephone number should be at least 10 chars
            {
                return false;
            }

            if (!validatePhoneNumber.IsValid(sender)) //return false if sender is not a valid phone number
            {
                return false;
            }

            //Check 2 - Ensure message text does not exceed the 140 character limit
            if (messageText.Length > 140)
            {
                MessageBox.Show("SMS character limit (140) exceeded. Please shorten your message");
                return false;
            }

            //If we get to this stage, all is good with the message.
            Header = header;
            Sender = sender;
            MessageText = messageText;
            return true;
        }

        public bool ValidateEmailBody(string messageBody)
        {
            if (messageBody.StartsWith("Sender")) //the information text is still present
            {
                return false;
            }

            EmailAddressAttribute validateEmailAddress = new EmailAddressAttribute(); //used to check sender is a valid email
            /*Line 1 = sender, line 2 = subject
             * IF Standard email: line 3 = message text
             * IF SIR: line 3 = Sort code, line 4 = Nature of Incident, line 5 = message text
             */

            string[] values = Regex.Split(messageBody, Environment.NewLine); 

            /*The message body should be at least three lines - sender, subject and message text.
             */
            if (values.Length < 3)
            {
                return false;
            }

            sender = values[0].Trim(); //sender is first line of message body
            subject = values[1].Trim(); //subject is second line of message body

            /*String is created depending on whether the email is an SIR or not. If it is an SIR, the message
             * should be at least 5 lines - sender, subject sort code, nature of incident and the message text.
             * For future processing of an SIR, a caret symbol is placed between the new lines of each section of the SIR.
             * Otherwise, if en email is not an SIR, then the email should just be three lines - sender, subject and message text.
             */
            if (subject.StartsWith("SIR"))
            {
                messageText = $"{values[2].Trim()}^{values[3].Trim()}^{values[4].Trim()}";
            }
            else
            {
                messageText = values[2].Trim(); //message text on third line
            }

            //Check 1 - Find a valid email address. This should be the first part of the message body
            if (!validateEmailAddress.IsValid(sender))
            {
                MessageBox.Show("No valid email found for sender");
                return false;
            }

            /*Check 2 - Ensure the subject does not exceed the allowed limit. The size limit changes depending on if the email
             *is an SIR or standard email.
             */
            if (subject.Length > 20)
            {
                return false;
            }
            else if (subject.StartsWith("SIR"))
            {
                if (subject.Length != 12) //All SIR subjects should be exactly 12 chars in length
                {
                    return false;
                }

                /*The next check ensures the nature of incident (NOI) the user has entered as part of the SIR
                 * is a valid one from the list of options
                 */

                string natureOfIncident = values[3]; //NOI should be on the fourth line of msg body
                bool validIncidentFound = false;
                Lists lists = new Lists();
                lists.PopulateIncidentsList(); //ensures list has been populated

                foreach (string incident in Lists.NatureOfIncidents)
                {
                    if (string.Equals(natureOfIncident, incident, StringComparison.OrdinalIgnoreCase))
                    {
                        validIncidentFound = true;
                        break;
                    }

                }

                if (!validIncidentFound) //if the NOI is not one from the valid list given in the project brief, return false
                {
                    MessageBox.Show("Incident is not valid");
                    return false;
                }

            }


            //Check 3 - Ensure message text does not exceed the 1028 character limit
            if (messageText.Length > 1028)
            {
                MessageBox.Show("Email character limit (1028) exceeded. Please shorten your message");
                return false;
            }

            //If we get here, the message body has passed validation
            Header = header;
            Sender = sender;
            Subject = subject;
            MessageText = messageText;
            return true;
        }

        public bool ValidateTweetBody(string messageBody)
        {
            if (messageBody.StartsWith("Sender")) //informative text is still present in the text box
            {
                return false;
            }

            string[] values = Regex.Split(messageBody, Environment.NewLine);
            sender = values[0];
            messageText = values[1];

            //Check 1 - ensure the sender is a valid twitter ID and does not exceed character limit
            if (!sender.StartsWith("@") && sender.Length < 16)
            {
                MessageBox.Show("Invalid twiiter ID");
                return false;
            }
            else if (sender.StartsWith("@") && sender.Length > 16)
            {
                MessageBox.Show("Twitter ID is too long");
                return false;
            }

            //Check 2 - Ensure message text does not exceed 140 character limit
            if (messageText.Length > 140)
            {
                MessageBox.Show("Tweet character limit (140) exceeded. Please shorten your message");
                return false;
            }

            //If we reach here, message is validated
            Header = header;
            Sender = sender;
            MessageText = messageText;
            return true;
        }
        #endregion

    }
}
