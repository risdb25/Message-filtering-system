using System;
using System.Collections.Generic;

namespace NapierBankApplication.Classes
{
    public class Sanitise
    {
        #region VARIABLES
        private Lists lists = new Lists(); //Lists object for interacting with non-static methods 
        #endregion

        #region PUBLIC METHODS
        /*These methods are called within the InputMessagesPage after message validation has taken place. If a message
         * the user has entered passes validation, the next requirement is to sanitise the message so it is ready for being
         * redisplayed in the message output section of the application, as well as written to the JSON file. The validated
         * version of the message that the user has entered is passed to the respective method for sanitising the type that
         * the message is
         */
        public void SanitiseSMS(ref string messageText)
        {
            /*SMS is sanitised when any abbreviations contained in the message text are expanded to full form
             * and written into the message directly after where the abbreviation is
             */

            lists.ImportAbbreviations(); //ensures abbrevs have been imported

            // This part of the method checks the message for abbreviations             
            string[] splitMessageText = messageText.Trim().Split(' '); //message is split into an array, each word is in its own index
            List<string> messageList = new List<string>();

            foreach (string word in splitMessageText) //populate List from the words in the array
            {
                messageList.Add(word);
            }

            foreach (string word in splitMessageText)
            {
                foreach (KeyValuePair<string, string> entry in Lists.Abbreviations) //goes thru each abbreviation
                {
                    if (word.ToUpper() == entry.Key) //if word from message is an abbreviation that's in the list
                    {
                        int index = messageList.IndexOf(word) + 1; //find one place past the point in the message where the abbreviation is
                        messageList.Insert(index, $"<{entry.Value}>"); //writes the abbreviation in full form
                    }
                }
            }

            //Now convert the list, which contains the sanitised version of the SMS, back to a string
            string sanitisedMessage = string.Empty;
            foreach (string word in messageList)
            {
                sanitisedMessage += word + " ";
            }

            messageText = sanitisedMessage.Trim(); //message is now sanitised
        }


        public void SanitiseEmail(ref string messageText, string header, string subject)
        {
            /*Email is sanitised when any URLs within the message are replaed by <URL Quarantined> and written to a
             * quarantine list. 
             * Secondly, if the email is an SIR, the sort code and nature of incident must be added to the SIR list.
             */
         
            /*First, we will check if the message text contains any URLs, since both standard emails and SIR emails
             * can contain these.
             */
            string[] splitMessageText = messageText.Trim().Split(' '); //creates an array where each word of the message is in its own index
            List<string> messageList = new List<string>();

            foreach(string word in splitMessageText)
            {
                messageList.Add(word); //adds each word in the message to a List
            }

            foreach(string word in splitMessageText)
            {
                if(Uri.IsWellFormedUriString(word, UriKind.Absolute)) //if word is a valid URL
                {
                    int index = messageList.IndexOf(word); //find where in the message the URL is
                    messageList.RemoveAt(index); //remove URL
                    messageList.Insert(index, "<URL Quarantined>"); //Insert <URL Quarantined> in place

                    //Add URL to quarantine list file
                    lists.UpdateQuarantineList(word);
                }
            }

            string sanitisedMessage = string.Empty;
            foreach (string word in messageList)
            {
                sanitisedMessage += word + " ";
            }

            messageText = sanitisedMessage.Trim(); //Email is now sanitised

            /*This last section of the method checks if the email is an SIR and writes the sort code and nature of incident
             * to the SIR list if so.
             */
            if(subject.StartsWith("SIR"))
            {
                lists.PopulateIncidentsList(); //ensures incidents list has been populated

                /*We added the caret to a show a new line of the message body during message validation. This makes it easier to
                 * separate the sender, subject, sort code, nature of incident and message text.
                 */
                string[] lines = messageText.Split('^');
                string sortCode = lines[0];
                string natureOfIncident = lines[1];

                //Add sort code and NOI to list
                lists.UpdateSIRList(sortCode, natureOfIncident);

                /*Lastly, we need to remove the carets from the message text which were used to split the message into sections
                 * where there is a caret, this shows where in the original message a new line was detected. So we replace
                 * the caret with a new line again. This will put the sort code, nature of incident and message text back onto
                 * new lines.
                 */
                messageText = String.Join("\n", messageText.Split('^'));             
            }
        }
        

        public void SanitiseTweet(ref string messageText)
        {
            /* Tweet is sanitised when:
             * 1 - abbreviations are expanded to full form and inserted into the message
             * 2 - any hashtags in the message are stored in the trending list
             * 3 - any twitter IDs within the message are added to mentions list
             */

            lists.ImportAbbreviations(); //ensures abbreviations have been imported

            string[] splitMessageText = messageText.Split(' '); //each word in the message is inserted into an array index
            List<string> messageList = new List<string>();

            //Check 1 - abbreviations in the message
            foreach (string word in splitMessageText) //populate list from words in the array
            {
                messageList.Add(word);
            }

            foreach (string word in splitMessageText)
            {
                foreach (KeyValuePair<string, string> entry in Lists.Abbreviations)
                {
                    if (word.ToUpper() == entry.Key) //if we have found an abvreviation in the message
                    {
                        int index = messageList.IndexOf(word) + 1; //find the point in the message where the abbreviation is
                        messageList.Insert(index, $"<{entry.Value}>"); //expanded version of abbrev is written into the message
                    }
                }
            }

            string sanitisedMessage = string.Empty;
            foreach(string word in messageList)
            {
                sanitisedMessage += word + " ";
            }

            messageText = sanitisedMessage.Trim(); //message is now sanitised

            /* Finally, we will scour the message for any hashtags or twitter IDs:
             * Any hashtags or twitter IDs we find within the message are added to the
             * trending list or mentions list respectively
             */

            foreach(string word in messageList)
            {
                if(word.StartsWith("#")) //if hashtag is found in the message
                {
                    lists.UpdateTrendingList(word); //pass it to method within Lists class to deal with
                } 
                else if(word.StartsWith("@")) //likewise if twitter ID is found in the message
                {
                    lists.UpdateMentionsList(word);
                }
            }

        }
        #endregion

    }
}
