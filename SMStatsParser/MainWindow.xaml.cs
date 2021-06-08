using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using System.Xml;
using System.Xml.XPath;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;

namespace SMStatsParser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public static string[] gradeTierName = { //Ghetto AF grade tier to string lookup table (using SL's grade tier names)
            "★★★★", //Tier01
            "★★★", //Tier02
            "★★", //Tier03
            "★", //Tier04
            "S+", //Tier05
            "S", //Tier06
            "S-", //Tier07
            "A+", //Tier08
            "A", //Tier09
            "A-", //Tier10
            "B+", //Tier11
            "B", //Tier12
            "B-", //Tier13
            "C+", //Tier14
            "C", //Tier15
            "C-", //Tier16
            "D", //Tier17
        };

        public static int TopSongsMax = 100; //Hard-coded number of songs to cap the "top songs" list at


        public bool sortAlphabet = false; //Whether we're sorting by alphabet or popularity

        Dictionary<string, Group> groups = new Dictionary<string, Group>(); //A list of groups we've found
        Dictionary<string, User> users = new Dictionary<string, User>(); //A list of users we've found
        List<Song> allSongs = new List<Song>(); //A list of all the songs we've found (songs are normally stored in groups)
        int totalHighScores = 0; //The total number of high scores counted

        public PlotModel PlotModelTopDays { get; private set; }


        private void buttonLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Stats.xml file|*.xml";
            if (dialog.ShowDialog() == true) //Try loading the file
            {
                groups.Clear(); //Clear all our lists from previous stats loads
                users.Clear();
                allSongs.Clear();
                totalHighScores = 0;


                //Setup for the top days/times graph
                PlotModelTopDays = new PlotModel { Title = "Top Days" };
                CategoryAxis axis = new CategoryAxis();
                axis.ActualLabels.Add("Sun");
                axis.ActualLabels.Add("Mon");
                axis.ActualLabels.Add("Tues");
                axis.ActualLabels.Add("Wed");
                axis.ActualLabels.Add("Thurs");
                axis.ActualLabels.Add("Fri");
                axis.ActualLabels.Add("Sat");
                PlotModelTopDays.Axes.Add(axis);

                ColumnSeries seriesTopDay = new ColumnSeries();
                PlotModelTopDays.Series.Add(seriesTopDay);

                int[] topDays = { 0, 0, 0, 0, 0, 0, 0 };



                XmlDocument statsFile = new XmlDocument();
                statsFile.Load(new StreamReader(dialog.FileName));


                XmlNode generalData = statsFile.GetElementsByTagName("GeneralData")[0]; //First: Grab the <GeneralData> tag, a few of its children are used


                //**Grade data**
                //Here, we find what percentage of all plays each grade is obtained (among some other grade-related stats)
                XmlNode gradeStats = generalData.SelectSingleNode("NumStagesPassedByGrade");
                int[] gradeData = new int[17];
                float[] gradePercent = new float[17];
                int gradeSum = 0;
                for (int i = 0; i < 17; i++)
                {
                    gradeData[i] = int.Parse(FindXmlNode(gradeStats, "Tier" + (i + 1).ToString("D2")));
                    gradeSum += gradeData[i];
                }

                labelGradeStats.Content = gradeSum + " recorded grades\n";
                for (int i = 0; i < 17; i++) //Find percentage of each grade
                {
                    gradePercent[i] = (float)gradeData[i] / gradeSum * 100;
                    labelGradeStats.Content += gradeTierName[i] + ": " + Math.Round(gradePercent[i], 1) + "% (" + gradeData[i] + ")\n";
                }



                //**Song data**
                //*Every* song and group mentioned in this Stats file, as well as their play counts will be tallied here
                XmlNode songData = statsFile.GetElementsByTagName("SongScores")[0];
                foreach (XmlNode songNode in songData.SelectNodes("Song"))
                {
                    if (songNode.Attributes["Dir"] != null)
                    {
                        string[] rawDir = new string[3];
                        rawDir = songNode.Attributes["Dir"].Value.Split('/'); //Songs are stored in stats.xml by their path (Songs/AdditionalSongs/Group/Song Name) - let's split our path string by '/' so we can grab the group/song names

                        if (rawDir[0] == "@mem") //Paths starting with @mem belong to the memory card group (Formatted as @mem/Song name) - move the actual song title/group over an index so they match the format of normal groups
                        {
                            rawDir[2] = rawDir[1];
                            rawDir[1] = "@mem";
                        }

                        //At this point the dir strings should be laid out as:
                        //0: Base dir (songs/additionalsongs)
                        //1: Group
                        //2: Song
                        if (!groups.ContainsKey(rawDir[1])) //If this song is in a new group, create a new group
                        {
                            groups.Add(rawDir[1], new Group(rawDir[1]));
                            groups[rawDir[1]].Songs = new Dictionary<string, Song>();
                            //groups[rawDir[1]].Name = rawDir[1];
                        }


                        //Make a new song to add to this song's group
                        Group group = groups[rawDir[1]];


                        //Find total plays for this song
                        int totalPlays = 0;
                        XmlNodeList steps = songNode.SelectNodes("Steps"); //Iterate through each stepchart in this song...
                        //Console.WriteLine(steps.Count);
                        foreach (XmlNode step in steps)
                        {
                            XmlNodeList highScoreLists = step.SelectNodes("HighScoreList"); //...to iterate through the high score lists for each chart
                            //Console.WriteLine(highScoreLists.Count);
                            foreach (XmlNode highScoreList in highScoreLists)
                            {
                                totalPlays += int.Parse(highScoreList.SelectSingleNode("NumTimesPlayed").InnerText); //First tally our total plays counter for this song based on NumTimesPlayed

                                XmlNodeList highScores = highScoreList.SelectNodes("HighScore"); //Now iterate through the high scores logged for this chart
                                if (highScores.Count >= 1)
                                {
                                    foreach (XmlNode highScore in highScores)
                                    {
                                        totalHighScores += 1; //Tally the total high scores logged
                                        XmlNode name = highScore.SelectSingleNode("Name");
                                        if (name != null)
                                        {
                                            if (users.ContainsKey(name.InnerText)) //Also log which user got this score, and add to the tally for their total high score count
                                            {
                                                users[name.InnerText].HighScores += 1;
                                            } else
                                            {
                                                users.Add(name.InnerText, new User(name.InnerText, 1));
                                            }
                                        }

                                        XmlNode dateTime = highScore.SelectSingleNode("DateTime");
                                        if (dateTime != null)
                                        {
                                            //Take the raw Date string from the XML node
                                            string rawValue = dateTime.InnerText;//.Split(' ');

                                            //Then: Parse it via DateTime to find the day of week it happened on.
                                            //Last we'll convert that day of week to an int, index the topDays list, and increment that day by 1
                                            topDays[(int) DateTime.Parse(rawValue).DayOfWeek]++;
                                        }
                                    }
                                }
                            }
                        }


                        //Now finally create a new song entry
                        Song song = new Song(rawDir[1], rawDir[2], totalPlays);

                        //Now add this song to its group and our total song counter
                        group.Songs.Add(rawDir[2], song);
                        group.TotalSongPlays += totalPlays;
                        allSongs.Add(song);
                    }
                    //break;
                }




                //**General data**
                //Now grab some other general data
                //Console.WriteLine(generalData.SelectSingleNode("Guid").InnerText);
                XmlNode songsByMode = generalData.SelectSingleNode("NumSongsPlayedByPlayMode");
                if (FindXmlNode(generalData, "IsMachine") == "1") //If we can find the IsMachine tag and it's 1, note that this is the machine profile
                {
                    labelGeneralStats.Content = "Machine profile\n(All song plays/scores on this install)\n\n";
                } else
                {
                    labelGeneralStats.Content = FindXmlNode(generalData, "DisplayName") + "'s profile" +
                        "\n\nDisplay name: " + FindXmlNode(generalData, "DisplayName") +
                        "\nLast high score name: " + FindXmlNode(generalData, "LastUsedHighScoreName");
                }
                labelGeneralStats.Content +=
                    "\nTotal session count: " + FindXmlNode(generalData, "TotalSessions") +
                    "\nTotal song plays: " + (int.Parse(FindXmlNode(songsByMode, "Regular")) + int.Parse(FindXmlNode(songsByMode, "Nonstop"))) +
                    "\nTotal recorded grades: " + gradeSum +
                    "\nTotal high scores: " + totalHighScores +
                    "\n\nTotal session time: " + Math.Round(TimeSpan.FromSeconds(int.Parse(FindXmlNode(generalData, "TotalSessionSeconds"))).TotalHours, 1) + " hours" +
                    "\nTotal gameplay time: " + Math.Round(TimeSpan.FromSeconds(int.Parse(FindXmlNode(generalData, "TotalGameplaySeconds"))).TotalHours, 1) + " hours";




                ListBoxItem item;

                RefreshTopSongs(allSongs); //Now refresh the top song and group boxes in the UI
                RefreshTopGroups();


                //Unique users/high score counts box
                List<User> topUsers = users.Values.OrderByDescending(o => o.HighScores).ToList();
                listboxHighScores.Items.Clear();
                item = new ListBoxItem();
                item.Content = topUsers.Count + " unique high score names";
                listboxHighScores.Items.Add(item);
                foreach (User user in topUsers)
                {
                    item = new ListBoxItem();
                    string userName = user.Name;
                    item.Content = user.HighScores + " - " + userName;
                    listboxHighScores.Items.Add(item);
                }


                //Top Days chart
                for (int day = 0; day <= 6; day++) {
                    seriesTopDay.Items.Add(new ColumnItem(topDays[day], day));
                }


                plotTopDay.Model = PlotModelTopDays;

    }
        }

        //Some vaguely safe way to grab the inner text from an XML node with a specific name
        public string FindXmlNode(XmlNode baseNode, string nodeName)
        {
            try
            {
                return baseNode.SelectSingleNode(nodeName).InnerText;
            }
            catch (NullReferenceException)
            {
                return "0";
            }
        }

        //We've selected a new top group, filter top songs to show only songs from this group
        private void listboxTopGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBoxItem selectedItem = (ListBoxItem)listboxTopGroups.SelectedItem;
            if (selectedItem != null) {
                string selectedGroup = selectedItem.Content.ToString();
                if (selectedGroup != null)
                {
                    string[] selectedGroupA = selectedGroup.Split(new[] { '-' }, 2);  //Remove the leading "total song play count" from the group entries - split at the first "-"
                    if (selectedGroupA.Length >= 2) //If there's two parts of the string (the number part and the group name part), set our selected group to the group name part minus the leading ' '
                    {
                        selectedGroup = selectedGroupA[1].Substring(1);
                    }
                    if (selectedGroup == "@mem (USB Custom Songs)") { selectedGroup = "@mem"; } //Special case @mem as well

                    Console.WriteLine("Filtering top songs to " + selectedGroup);
                    if (groups.ContainsKey(selectedGroup))
                    {
                        RefreshTopSongs(groups[selectedGroup].Songs.Values.ToList<Song>());
                    }
                    else
                    {
                        RefreshTopSongs(allSongs);
                    }
                }
            }
        }

        //Change whether to sort the song/group lists by alphabet or by play count
        private void rbSortAlphabet_Checked(object sender, RoutedEventArgs e)
        {
            sortAlphabet = true;
            RefreshTopGroups();
        }

        private void rbSortPlayCount_Checked(object sender, RoutedEventArgs e)
        {
            sortAlphabet = false;
            RefreshTopGroups();
        }


        //Refresh the top groups box in the UI
        private void RefreshTopGroups()
        {
            listboxTopGroups.Items.Clear();
            List<Group> topGroups;
            if (sortAlphabet)
            {
                topGroups = groups.Values.OrderBy(o => o.Name).ToList();
            } else
            {
                topGroups = groups.Values.OrderByDescending(o => o.TotalSongPlays).ToList();
            }
            ListBoxItem item = new ListBoxItem();
            item.Content = "[Filter all groups]";
            listboxTopGroups.Items.Add(item);
            foreach (Group group in topGroups) //Add each group we've found and its total play count as an item in the top groups list
            {
                item = new ListBoxItem();
                string groupName = group.Name;
                if (group.Name == "@mem") { group.Name = "@mem (USB Custom Songs)"; } //Special case the @mem group when adding the USB Customs group to explain what @mem is
                item.Content = group.TotalSongPlays + " - " + groupName;
                listboxTopGroups.Items.Add(item);
            }
            labelTopGroupsHeader.Content = topGroups.Count + " groups - Select a group to filter top songs";

            listboxTopGroups.SelectedIndex = 0;
            //RefreshTopSongs(allSongs);
        }

        //Refresh the top songs box in the UI
        //Accepts a list of songs as the list of songs to consider displaying (so we can only display songs from a certain group, or all groups)
        private void RefreshTopSongs(List<Song> songs)
        {
            List<Song> topSongs;
            if (sortAlphabet)
            {
                topSongs = songs.OrderBy(o => o.Name).ToList();
            }
            else
            {
                topSongs = songs.OrderByDescending(o => o.TotalPlays).ToList();
            }
            //ListBoxItem item = new ListBoxItem();
            //item.Content = allSongs.Count + " songs (showing " + TopSongsCount + ")";
            //listboxTopSongs.Items.Add(item);
            listboxTopSongs.Items.Clear();
            for (int i = 0; i <= TopSongsMax; i++) //Find the top <TopSongsMax> songs in the selected group to add to the top songs list
            {
                if (topSongs.Count > i) //Limit the number of top songs we show to the specified max
                {
                    Song song = topSongs[i];
                    ListBoxItem item = new ListBoxItem();
                    item.Content = song.TotalPlays + " - " + song.Group + "/" + song.Name;
                    listboxTopSongs.Items.Add(item);
                }
            }
            labelTopSongsHeader.Content = allSongs.Count + " songs (showing " + listboxTopSongs.Items.Count + ")";
        }

        
    }
    
}
