using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMStatsParser
{
    //A group/folder of songs found in Stats.xml
    class Group
    {
        public Group(string name)
        {
            Name = name;
        }

        //This group's name
        public string Name { get; set; }

        //A list of songs in this group
        public Dictionary<string, Song> Songs { get; set; }

        //A total count of how many times songs in this group got played
        public int TotalSongPlays { get; set; }

        //A total count of how many high scores this song has
        public int TotalHighScores { get; set; }

    }
}
