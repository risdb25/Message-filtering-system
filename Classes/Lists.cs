using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace NapierBankApplication.Classes
{
    public class Lists
    {
        #region VARIABLES
        public static List<string> URLQuarantineList = new List<string>();
        public static Dictionary<string, int> TrendingList = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase); //key = the hashtag, value = frequency
        public static List<string> MentionsList = new List<string>();
        public static Dictionary<string, string> SIRList = new Dictionary<string, string>(); //key = sort code, value = nature of incident
        public static List<Message> StoredMessages = new List<Message>();
        public static List<string> NatureOfIncidents = new List<string>();
        public static Dictionary<string, string> Abbreviations = new Dictionary<string, string>();

        private string jsonString; //used to keep a local copy of the deserialised json string
        private bool incidentsListIsPopulated = false; //becomes true the first time the list is populated
        private bool abbreviationsAreImported = false; //becomes true once abbreviations have been read from file once
        #endregion

        #region PUBLIC METHODS
        /*The methods below are accessed by different parts of the application. For instance, whenever the HomePage is displayed
         * in the window, the latest version of the trending, mentions and SIR lists are retrieved so they can be
         * displayed in the text boxes within the GUI. This classes provides a central place for all the lists
         * necessary to satify the requirements of the application. These include:
         * 
         * URL Quarantine list - list of all URLs which have been removed from email messages
         * Trending list - list of hashtags from tweets and their number of uses
         * Mentions list - list of twitter IDs mentioned in tweets
         * SIR list - sort code and nature of incident taken from emails which are SIRs
         * Stored messages - list of all valid messages the user has inputted
         */

        public void RetrieveSIRList()
        {
            /*The list of sort codes and nature of incidents stored on file are
             * retrieved and added into the the SIR list dictionary. 
             * Key = sort code (string)
             * Value = nature of incident (string)
             */

            string filePath = Directory.GetCurrentDirectory() + "\\" + "SIRList.csv";

            using(StreamReader reader = new StreamReader(filePath))
            {
                while(!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    //retrieves each SIR incident from file and inputs it into the SIR dictionary
                    if(!SIRList.ContainsKey(values[0]))
                    {
                        SIRList.Add(values[0], values[1]);
                    }
                    
                }
            }
        }

        public void RetrieveTrendingList()
        {
            /*The hashtags and their respective number of uses stored on file are retrieved and added into
             * the mentions list dictionary. 
             * Key = hashtag (string)
             * Value = number of uses (int)
             */
            
            string filePath = Directory.GetCurrentDirectory() + "\\" + "Hashtags.csv";

            using (StreamReader reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    //retrieves each hashtag (and their count) from the file and stores them in the dictionary
                    if(!TrendingList.ContainsKey(values[0]))
                    {
                        TrendingList.Add(values[0], Convert.ToInt32(values[1]));
                    }
                    
                }
            }
        }

        public void RetrieveMentionsList()
        {
            //The twitter IDs stored on file are written into the string List of mentions

            string filePath = Directory.GetCurrentDirectory() + "\\" + "MentionsList.csv";

            using (StreamReader reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();

                    //retrieves each twitter ID from mentions list file and adds them into list
                    if(!MentionsList.Contains(line))
                    {
                        MentionsList.Add(line);
                    }
                    
                }
            }
        }

        public void RetrieveStoredMessages()
        {
            /*In this method, the JSON file is deserialised and messages the JSON has stored
             * are added into a List of Message objects.
             * We check firstly that a JSON file exists
             * Then we check if the string we've just read from file is the same as the last time
             * it was read from file or not. If it is the same we dont need to rewrite the List, 
             * otherwise the List is populated and we update the local copy of the JSON string.
             */

            string filePath = Directory.GetCurrentDirectory() + "\\" + "StoredMessages.json";
            string jsonString = string.Empty;

            //JSON file is read into a string variable
            try
            {
                jsonString = File.ReadAllText(filePath);
            }
            catch(FileNotFoundException e)
            {
                File.Create(filePath);
                return;
            }         
            

            //List of messages is populated by deserialising the json string read in from file
            if(this.jsonString != jsonString) //only update list if JSON file has changed
            {
                if(string.IsNullOrEmpty(jsonString))
                {
                    return;
                }
                StoredMessages = JsonSerializer.Deserialize<List<Message>>(jsonString); //Message objects are added to the List
                this.jsonString = jsonString; //update local copy, used for comparing whether the JSON string from file has changed
            }
            
        }

        public void UpdateSIRList(string sortCode, string natureOfIncident)
        {
            /*This method is called from within the sanitise class once the sanitise process has completed. If an SIR
             * has been inputted by the user, we need to add its sort code and NOI to the list - if the sort code is unique.
             */

            //only add SIR to list if sortCode is unique
            if(!SIRList.ContainsKey(sortCode))
            {
                SIRList.Add(sortCode, natureOfIncident);
            }

            //write to file
            string filePath = Directory.GetCurrentDirectory() + "\\" + "SIRList.csv";

            using(StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (KeyValuePair<string, string> entry in SIRList) 
                {
                    writer.WriteLine($"{entry.Key},{entry.Value}"); //write each SIR to file
                }
            }
        }

        public void UpdateQuarantineList(string url)
        {
            /*This method is called from within the sanitise class when an email has been inputted by the user with a URL
             * in it. We need to add the URL to the quarantine list
             */

            //Update local quarantine list
            URLQuarantineList.Add(url);

            //Write detected url to file
            string filePath = Directory.GetCurrentDirectory() + "\\" + "URLQuarantineList.csv";
            
            using(StreamWriter writer = new StreamWriter(filePath, append: true))
            {
                writer.WriteLine(url);
            }
        }

        public void UpdateTrendingList(string hashtag)
        {
            /*This method is called within the sanitise class when a tweet contains a hashtag. The hashtag is added
             * to the dictionary if it is unique, and its value (which represents the count of the number of times 
             * the hashtag has appeared) is set to 1. 
             * If the hashtag (which is the dictionary key) is already in the dictionary, its value is incremented by 1. 
             * This is how the system controls what hashtags are trending
             * (although it is not actually sorted until it is being displayed within the HomePage)
             */

            if (TrendingList.ContainsKey(hashtag)) //if hashtag is already in the dictionary
            {
                TrendingList[hashtag] += 1; //Increment its value by 1
            }
            else //if it hasnt appeared in a tweet before
            {
                TrendingList.Add(hashtag, 1); //add it it dictonary with a value (count/frequency) of 1
            }

            //write to file
            string filePath = Directory.GetCurrentDirectory() + "\\" + "Hashtags.csv";

            using(StreamWriter writer = new StreamWriter(filePath))
            {
                foreach(KeyValuePair<string, int> entry in TrendingList)
                {
                    writer.WriteLine($"{entry.Key},{entry.Value}");
                }
            }
        }

        public void UpdateMentionsList(string twitterID)
        {
            /*This method is called within the sanitise class when a twitter ID has been found within the message text
             * of a tweet. We only add the twitter ID to the list if it is not in the list already
             */
            if (!MentionsList.Contains(twitterID)) //only add twitter ID to list if it isn't already in it
            {
                MentionsList.Add(twitterID);
            }

            //write to file
            string filePath = Directory.GetCurrentDirectory() + "\\" + "MentionsList.csv";

            using(StreamWriter writer = new StreamWriter(filePath))
            {
                foreach(string id in MentionsList) //write each twitter ID to the file
                {
                    writer.WriteLine(id);
                }
            }
        }

        public void UpdateMessagesList(ref Message message)
        {
            /*This method is called from within the InputMessagesPage class after the message has been validated
             * and sanitised. The message is only added to the list of Message objects if it is unique. The list
             * of all messages is then serialised and written to a JSON file.
             */

            if(!StoredMessages.Contains(message)) //only add messages that are unique
            {
                StoredMessages.Add(message);
            }

            //update json file
            var options = new JsonSerializerOptions
            {
                WriteIndented = true //Indents each Message object in the string
            };

            string filePath = Directory.GetCurrentDirectory() + "\\" + "StoredMessages.json";
            jsonString = JsonSerializer.Serialize(StoredMessages, options);

            File.WriteAllText(filePath, jsonString);
        }

        public void PopulateIncidentsList()
        {
            if(!incidentsListIsPopulated)
            {
                NatureOfIncidents.Add("Theft");
                NatureOfIncidents.Add("Staff Attack");
                NatureOfIncidents.Add("ATM Theft");
                NatureOfIncidents.Add("Raid");
                NatureOfIncidents.Add("Customer Attack");
                NatureOfIncidents.Add("Staff Abuse");
                NatureOfIncidents.Add("Bomb Threat");
                NatureOfIncidents.Add("Terrorism");
                NatureOfIncidents.Add("Suspicious Incident");
                NatureOfIncidents.Add("Intelligence");
                NatureOfIncidents.Add("Cash Loss");

                incidentsListIsPopulated = true; //ensures list is not re-populated
            }         

            
        }

        public void ImportAbbreviations()
        {
            if(!abbreviationsAreImported)
            {
                string directory = Directory.GetCurrentDirectory();
                string fileLocation = directory + "\\" + "textwords.csv";

                try
                {
                    using (var reader = new StreamReader(fileLocation))
                    {
                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            var values = line.Split(',');

                            Abbreviations.Add(values[0], values[1]);
                        }
                    }
                }
                catch(Exception e) { }
                

                abbreviationsAreImported = true;
            }
            
        }

        #endregion
    }
}
