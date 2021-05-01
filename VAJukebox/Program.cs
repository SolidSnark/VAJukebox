using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            
            string destinationFile = args[0];
            string sourceDirectory = args[1];
            SearchType searchType = SearchType.Artist;
            string matchString = args[3].ToLower();

            if (args.Length != 4)
            {
                Console.WriteLine("Invalid number of arguments.");
                return;
            }

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

            if (!Directory.Exists(sourceDirectory))
            {
                Console.WriteLine("Invalid Source Directory.");
                return;
            }

            switch (args[2])
            {
                case "Track":
                    searchType = SearchType.Track;
                    break;

                case "Album":
                    searchType = SearchType.Album;
                    break;

                case "Artist":
                    searchType = SearchType.Artist;
                    break;

                default:
                    Console.WriteLine("Invalid search type.");
                    return;
            }

            matchString = matchString.Replace(" & ", " and ");

            List<Track> tracks = TrackLister.GetTracks(sourceDirectory, sourceDirectory);
                                    
            bool artistSearch = false;
            const string byStr = " by ";

            if (matchString.LastIndexOf(byStr) > 1)
            {
                string searchName = matchString.Substring(0, matchString.LastIndexOf(byStr));
                string searchArtist = matchString.Substring(matchString.LastIndexOf(byStr) + 3);

                matchString = searchName + " - " + searchArtist;
                artistSearch = true;
            }
                        
            Dictionary<string, List<Track>> tracknameDictionary = new Dictionary<string, List<Track>>();
            foreach (Track track in tracks)
            {
                string keyName = string.Empty;

                switch (searchType)
                {
                    case SearchType.Track:
                        if (artistSearch)
                        {
                            keyName = track.Name + " - " + track.Artist;
                        }
                        else
                        {
                            keyName = track.Name;
                        }                        
                        break;

                    case SearchType.Album:
                        if (artistSearch)
                        {
                            keyName = track.Album + " - " + track.AlbumArtist;
                        }
                        else
                        {
                            keyName = track.Album;
                        }
                        break;

                    case SearchType.Artist:
                        keyName = track.Artist;
                        break;
                }

                if (!tracknameDictionary.ContainsKey(keyName))
                {
                    tracknameDictionary.Add(keyName, new List<Track>());
                }

                tracknameDictionary[keyName].Add(track);
            }

            List<Track> searchResults = new List<Track>();
            if (tracknameDictionary.ContainsKey(matchString))
            {
                searchResults = tracknameDictionary[matchString];
            }
            else
            {
                int shortestDistance = int.MaxValue;

                foreach (string key in tracknameDictionary.Keys)
                {
                    int distance = StringDistance.LevenshteinDistance(key, matchString);
                    if (distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        searchResults = tracknameDictionary[key];
                    }
                }
            }

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

            File.WriteAllLines(destinationFile, searchResults.Select(x => x.Filename));
        }
    }
}
