using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VAJukebox
{
    public static class TrackLister
    {
        public static List<Track> GetTracks(string filepath, string rootPath)
        {
            List<Track> tracks = new List<Track>();

            foreach (string file in Directory.EnumerateFiles(filepath))
            {
                string path = file.ToLower();
                if (!path.EndsWith(@".mp3") && !path.EndsWith(@".flac"))
                    continue;

                Track track = new Track();
                track.Filename = file;

                path = path.Substring(rootPath.Length + 1);
                track.AlbumArtist = path.Substring(0, path.IndexOf('\\'));

                path = path.Substring(track.AlbumArtist.Length + 1);
                track.Album = path.Substring(0, path.IndexOf('\\'));

                path = path.Substring(track.Album.Length + 1);
                string filenameToParse = path.Trim();

                track.TrackNumber = int.Parse(filenameToParse.Substring(0, filenameToParse.IndexOf(' ')).Trim());
                
                filenameToParse = filenameToParse.Substring(filenameToParse.IndexOf(' '));
                filenameToParse = filenameToParse.Substring(0, filenameToParse.LastIndexOf('.'));

                track.Artist = filenameToParse.Substring(filenameToParse.LastIndexOf('-'));
                track.Name = filenameToParse.Substring(0, filenameToParse.LastIndexOf('-'));

                tracks.Add(track);
            }

            foreach (string dir in Directory.GetDirectories(filepath))
            {
                tracks.AddRange(GetTracks(dir, rootPath));
            }

            return tracks;
        }
    }
}
