using System;
using System.Collections.Generic;
using System.Text;

namespace VAJukebox
{
    public class Track
    {
        string _name = string.Empty;
        string _artist = string.Empty;
        string _album = string.Empty;
        string _albumArtist = string.Empty;
        string _filename = string.Empty;
        int _trackNumber = 1;

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value.ToLower().Trim();
            }
        }

        public string Artist
        {
            get
            {
                return _artist;
            }
            set
            {
                _artist = value.ToLower().Trim();
            }
        }

        public string Album
        {
            get
            {
                return _album;
            }
            set
            {
                _album = value.ToLower().Trim();
            }
        }

        public string AlbumArtist
        {
            get
            {
                return _albumArtist;
            }
            set
            {
                _albumArtist = value.ToLower().Trim();
            }
        }

        public string Filename
        {
            get
            {
                return _filename;
            }
            set
            {
                _filename = value.ToLower().Trim();
            }
        }

        public int TrackNumber
        {
            get
            {
                return _trackNumber;
            }
            set
            {
                _trackNumber = value;
            }
        }

        public static Track RandomPick(List<Track> tracks)
        {
            Random rand = new Random();

            return tracks[rand.Next(tracks.Count)];
        }

        public static List<Track> RandomTracklist(List<Track> tracks)
        {
            List<Track> shuffledList = new List<Track>(tracks);

            Random rand = new Random();

            for (int i = shuffledList.Count - 1; i > 1; i--)
            {
                int swapIndex = rand.Next(i + 1);

                Track t = shuffledList[swapIndex];
                shuffledList[swapIndex] = shuffledList[i];
                shuffledList[i] = t;
            }

            return shuffledList;
        }


    }
}
