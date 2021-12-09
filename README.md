# Stepmania Stats Parser

SM Stats Parser reads player statistics files (`Stats.xml`) from StepMania 5.x and gives you some fun data, like your average grades and most played songs!

Written in C# using WPF for the UI. You probably need .NET Framework 4.6 installed to run this.


(Outdated screenshot)
![Program Screenshot](https://i.imgur.com/jVDnkdZ.png)

## Features

 * Supports Stats.xml files from Stepmania 5.x (5.1/5.3 are supported!)
 * Supports reading stats from individual player profiles, or the global Machine profile
---
 * Gives stats on playtime, session count, etc
 * Tallies recorded grades from every song, shows how often each grade is achieved (using SL's grading standards)
 * Shows how many unique high score names are in the profile, and a score count from each (most useful for machine profiles)
 * Shows the most played songs in each group, and overall most played songs (with play counts for each)
 * Shows graphs of the top days/times high scores are recorded at


## Locating Stats.xml

To use this tool, you'll need to find your profile's `Stats.xml` file

1. First, find your `Save` directory. It's found under:
    * Windows: `%APPDATA%/StepMania 5.x`
    * Linux: `~/.stepmania-5.x`
    * MacOS: `~/Library/Application Support/StepMania 5.x`
    * Any OS: `Save` is found in the game directory when in portable mode
3. Find the profile to load
    * Individual player profiles are under `Save/LocalProfiles/xxxxxxxx/`
    * The global "Machine" profile for all players is under `Save/MachineProfile/`
4. Load the `Stats.xml` file inside your profile's folder!

Some themes may separate stats files based on play modes, etc. (Simply Love separates ITG and ECFA stats, for example)
Any of these stats files can be loaded without issue.


## Loading options

There's only one loading option, currently:

### "Only load scores between:"
**If enabled, will only fully load a high score if it was recorded between two specified dates. All affected fields update automatically!**

This goes off of the date/time recorded for individual HighScore entries, and thus only affects areas where individual high score entries are used (Top Days/Times, Total High Scores counter, and the High Scores by Name counters).

**This does not affect fields not based on high scores (grade stats, Top Songs/Groups, etc)**, as SM doesn't store the needed date/time information.