# Stepmania Stats Parser

SM Stats Parser reads player statistics files (`Stats.xml`) from StepMania and gives you some useful data.

Any Stats.xml from Stepmania 5.x (5.1/5.3 included!) should work, and individual player profiles and the global Machine profile should also work.

## Features

 * Supports Stats.xml files from Stepmania 5.x (5.1/5.3 are supported!)
 * Supports reading stats from individual player profiles, or the global Machine profile
---
 * Gives stats on playtime, session count, etc
 * Tallies recorded grades from every song, shows how often each grade is achieved (using SL's grading standards)
 * Shows how many unique high score names are in the profile, and a score count from each (most useful for machine profiles)
 * Shows the most played songs in each group, and overall most played songs (with play counts for each)


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