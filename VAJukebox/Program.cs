using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Newtonsoft.Json;

namespace VAJukebox
{
    public enum SearchType
    {
        Track,
        Album,
        Artist
    }

    class Program
    {
        // 1) Destination File Name
        // 2) Source Directory
        // 3) Search Type
        // 4) Search Phrase
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Invalid number of arguments.");
                return;
            }

            string destinationFile = args[0];
            string sourcePath = args[1];

            SearchType searchType = SearchType.Artist;
            string matchString = args.Length == 4 ? args[3].ToLower() : string.Empty;

            try
            {
                string destDirctory = Path.GetDirectoryName(destinationFile);
                if (!Directory.Exists(destDirctory))
                {
                    Directory.CreateDirectory(destDirctory);
                }

                if (File.Exists(destinationFile))
                {
                    File.Delete(destinationFile);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to create target file.");
                return;
            }

            if (args[2] == "Rebuild")
            {
                GenerateIndex(sourcePath, destinationFile);
                return;
            }

            if (!File.Exists(sourcePath))
            {
                Console.WriteLine("Invalid index file specified.");
                return;
            }

            if (!Enum.TryParse<SearchType>(args[2], out searchType))
            {
                Console.WriteLine("Invalid search type.");
                return;
            }

            if (string.IsNullOrEmpty(matchString))
            {
                Console.WriteLine("Search term must be specified.");
                return;
            }

            string trackJson = File.ReadAllText(sourcePath);
            List<Track> tracks = JsonConvert.DeserializeObject<List<Track>>(trackJson);

            matchString = matchString.Replace(" & ", " and ");
            string byMatchString = matchString;

            bool artistSearch = false;
            const string byStr = " by ";

            if (matchString.LastIndexOf(byStr) > 1)
            {
                string searchName = matchString.Substring(0, matchString.LastIndexOf(byStr));
                string searchArtist = matchString.Substring(matchString.LastIndexOf(byStr) + 3);

                matchString = searchName + " - " + searchArtist;
                artistSearch = true;
            }

            Dictionary<string, List<Track>> byDictionary = new Dictionary<string, List<Track>>();
            Dictionary<string, List<Track>> tracknameDictionary = new Dictionary<string, List<Track>>();
            foreach (Track track in tracks)
            {
                string keyName = string.Empty;

                switch (searchType)
                {
                    case SearchType.Track:
                        keyName = track.Name;
                        if (artistSearch)
                        {
                            AddToDictionary(byDictionary, track, keyName);
                            keyName += " - " + track.Artist;
                        }
                        break;

                    case SearchType.Album:
                        keyName = track.Album;
                        if (artistSearch)
                        {
                            AddToDictionary(byDictionary, track, keyName);
                            keyName += " - " + track.AlbumArtist;
                        }
                        break;

                    case SearchType.Artist:
                        keyName = track.Artist;
                        break;
                }

                AddToDictionary(tracknameDictionary, track, keyName);
            }

            List<Track> searchResults = null;

            // Quick check for exact match
            if (tracknameDictionary.ContainsKey(matchString))
            {
                searchResults = tracknameDictionary[matchString];
            }
            else
            {
                int shortestByDistance = int.MaxValue;
                List<Track> bySearchResults = null;

                if (artistSearch)
                {
                    // Check against bys
                    bySearchResults = PerformSearch(byMatchString, byDictionary, out shortestByDistance);
                }

                int shortestDistance;
                searchResults = PerformSearch(matchString, tracknameDictionary, out shortestDistance);

                if (artistSearch && shortestByDistance < shortestDistance)
                {
                    // The bys have it
                    searchResults = bySearchResults;
                }
            }

            searchResults = FormatResultsBySearchType(searchType, searchResults);

            File.WriteAllLines(destinationFile, searchResults.Select(x => x.Filename));
        }

        private static void AddToDictionary(Dictionary<string, List<Track>> dictionary, Track track, string keyName)
        {
            if (!dictionary.ContainsKey(keyName))
            {
                dictionary.Add(keyName, new List<Track>());
            }

            dictionary[keyName].Add(track);
        }

        private static List<Track> FormatResultsBySearchType(SearchType searchType, List<Track> searchResults)
        {
            if (searchType == SearchType.Track)
            {
                searchResults = new List<Track> { Track.RandomPick(searchResults) };
            }
            else if (searchType == SearchType.Artist)
            {
                searchResults = Track.RandomTracklist(searchResults);
            }
            else if (searchType == SearchType.Album)
            {
                searchResults.Sort((x, y) => x.TrackNumber.CompareTo(y.TrackNumber));
            }

            return searchResults;
        }

        private static List<Track> PerformSearch(string matchString, Dictionary<string, List<Track>> tracknameDictionary, out int shortestDistance)
        {
            List<Track> searchResults = new List<Track>();
            shortestDistance = int.MaxValue;

            foreach (string key in tracknameDictionary.Keys)
            {
                int distance = StringDistance.LevenshteinDistance(key, matchString);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    searchResults = tracknameDictionary[key];
                }
            }

            return searchResults;
        }

        private static void GenerateIndex(string sourceDirectory, string targetFile)
        {
            List<string> musicExtensions = (
                new List<string> { ".aa", ".aax", ".aac", ".aiff", ".ape", ".dsf", ".flac", ".m4a", ".m4b", 
                    ".m4p", ".mp3", ".mpc", ".mpp", ".ogg", ".oga", ".wav", ".wma", ".wv", ".webm" 
                }
            );

            List<Track> tracks = LibraryBuilder.GetTracks(sourceDirectory, sourceDirectory, musicExtensions);

            string output = JsonConvert.SerializeObject(tracks);
            File.WriteAllText(targetFile, output);
            Console.WriteLine("Index completed");
        }
    }
}