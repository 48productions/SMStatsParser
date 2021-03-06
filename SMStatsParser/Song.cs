using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMStatsParser
{
    //A song pulled from Stats.xml
    class Song
    {
        public Song(string group, string name, int totalPlays)
        {
            Group = group;
            Name = name;
            TotalPlays = totalPlays;
        }

        //The group this song belongs to
        public string Group { get; set; }

        //The name of the song
        public string Name { get; set; }

        //The number of times this song has been played
        public int TotalPlays { get; set; }
    }
}
