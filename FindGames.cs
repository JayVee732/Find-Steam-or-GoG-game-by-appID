using Microsoft.Win32;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace FindSteamOrGOGGameByAppID
{
    class SteamGamePath
    {
        static string GetSteamPath()
        {
            return Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Valve\\Steam", "InstallPath", null).ToString();
        }

        static List<string> SteamLibraryPaths()
        {
            var libraryfoldersPath = new StringBuilder(GetSteamPath());

            if (libraryfoldersPath == null) // Steam not installed or Registry key is missing
            {
                return null;
            }

            var toReturn = new List<string>(); // We can have as little as 1 or up to an unknown amount of paths. 
            toReturn.Add(libraryfoldersPath.ToString()); // By default steam install path is a steam library location
            libraryfoldersPath.Append("\\steamapps\\libraryfolders.vdf"); // This file holds all paths

            var unsortedPaths = new StringBuilder();
            using (var sr = File.OpenText(libraryfoldersPath.ToString()))
            {
                for (int i = 1; i <= 4; ++i)
                {
                    sr.ReadLine(); // First 4 lines are useless
                }

                while (sr.Peek() >= 0 && sr.Peek() != 125) // We can stop after we hit }, as we don't need it.
                {
                    unsortedPaths.Append(sr.ReadLine());
                }
            }

            string[] strings = SplitByQuotes(unsortedPaths.ToString());
            for (var i = 1; i < strings.Length; i += 2) // We only need the paths, not the library number
            {
                toReturn.Add(strings[i].Trim('\"')); // Get rid of the quotes
            }

            return toReturn;
        }

        static string FindGameACFByAppID(string appID)
        {
            /* If we do not add this other games with the same number in 
             * its name can be returned instead.
             * ex - Try 730 (cs:go) and have 976730 (MCC) installed.
             * If we find MCC first we return that instead of csgo.
             * All appID in steam file start with _ and ends with .acf. */

            appID = $"_{appID}.acf";
            foreach (var path in SteamLibraryPaths())
            {
                var steamappPath = $"{path}\\steamapps\\"; // ACF files are in steamapp folder
                var filesInPath = Directory.GetFiles(steamappPath);
                foreach (var file in filesInPath)
                {
                    if (file.Contains(appID)) // If we find the appID we can stop looking
                    {
                        return file;
                    }
                }
            }

            return null; // Game not installed or something is wrong.
        }

        // I split these up so it looks neater.
        public static string FindGameByAppID(string appID)
        {
            var ACFFile = FindGameACFByAppID(appID); // ACF file has the install folder

            if (ACFFile == null)
            {
                return null;
            }

            using (var sr = File.OpenText(ACFFile))
            {
                string currentLine;
                while ((currentLine = sr.ReadLine()) != null)
                {
                    if (currentLine.Contains("installdir"))
                    {
                        string[] currentLineArr = SplitByQuotes(currentLine);
                        /* Instead of refinding the whole file path again I just remove the .acf file
                         * and add on common and the installdir.*/
                        //return ACFFile.Substring(0, ACFFile.LastIndexOf("\\") + 1) + "common\\" + currentLineArr[1].Trim('\"') /*+ "\\"*/;
                        return ACFFile.Substring(0, ACFFile.LastIndexOf("\\") + 1) + $"common\\{currentLineArr[1].Trim('\"')}";
                    }
                }
            }

            return null;
        }

        static string[] SplitByQuotes(string unsplitArray)
        {
            var re = new Regex("\"[^\"]*\"");
            return re.Matches(unsplitArray).Cast<Match>().Select(m => m.Value).ToArray(); // Split into an array using quotes to split
        }
    }

    class GOGGamePath
    {
        public static string FindGameByAppID(string appID)
        {
            return Registry.GetValue($"HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\GOG.com\\Games\\{appID}\\", "Path", null).ToString();
        }
    }
}
