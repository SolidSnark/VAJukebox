using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VAJukebox
{
    public static class LibraryBuilder
    {
        public static List<Track> GetTracks(string filepath, string rootPath, List<string> musicExtensions)
        {
            List<Track> tracks = new List<Track>();

            foreach (string file in Directory.EnumerateFiles(filepath))
            {
                string path = file.ToLower();
                
                string extension = Path.GetExtension(path);

                if (!musicExtensions.Contains(extension))
                    continue;

                TagLib.File tagFile = null;

                try
                {
                    tagFile = TagLib.File.Create(path);
                }
                catch (Exception)
                {
                    Console.WriteLine(path + " is corrupt");
                    continue;
                }

                const string unknownStr = "Unknown";

                Track track = new Track();                               
                track.Filename = file;

                string albumArtist = tagFile.Tag.AlbumArtists.Length > 0 ? 
                                    tagFile.Tag.AlbumArtists[0] ?? unknownStr :
                                    unknownStr;
                track.AlbumArtist = albumArtist;

                string album = !string.IsNullOrEmpty(tagFile.Tag.Album) ? 
                                    tagFile.Tag.Album :
                                    unknownStr;
                track.Album = album;

                track.TrackNumber = (int)tagFile.Tag.Track;

                string artist = tagFile.Tag.Performers.Length > 0 ?
                                    tagFile.Tag.Performers[0] ?? unknownStr :
                                    unknownStr;
                track.Artist = artist;

                string title = !string.IsNullOrEmpty(tagFile.Tag.Title) ?
                                    tagFile.Tag.Title :
                                    unknownStr;
                track.Name = title.Replace(" & ", " and ");
               
                tracks.Add(track);

                tagFile.Dispose();
            }

            foreach (string dir in Directory.GetDirectories(filepath))
            {
                tracks.AddRange(GetTracks(dir, rootPath, musicExtensions));
            }

            return tracks;
        }
    }
}
