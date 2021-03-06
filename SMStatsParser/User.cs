using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMStatsParser
{
    //A high score username pulled from the Stats.xml file
    class User
    {
        public User(string name, int highScores)
        {
            Name = name;
            HighScores = highScores;
        }

        //The player's name
        public string Name { get; set; }

        //The number of high scores this player's achieved
        public int HighScores { get; set; }
    }
}
