using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoobarElf
{
    class Song
    {
        public string title;

        public string album;

        public string albumArtist;

        public string trackArtist;
        
        public int discNumber;

        public int trackNumber;

        public string codec;

        public int bitRate;

        public int sampleRate;

        // stereo, mono, etc
        public string channels;

        public Song(Dictionary<string, string> songDict)
        {
            if (!songDict.ContainsKey("title") || !songDict.ContainsKey("album") || !songDict.ContainsKey("albumArtist"))
            {
                return;
            }
            title = songDict["title"];
            album = songDict["album"];
            albumArtist = songDict["albumArtist"];
            trackArtist = songDict["trackArtist"];
            discNumber = Utility.ToInt(songDict["CD"]);
            trackNumber = Utility.ToInt(songDict["track"]);
            codec = songDict["codec"];
            bitRate = Utility.ToInt(songDict["bitRate"]);
            sampleRate = Utility.ToInt(songDict["sampleRate"]);
            channels = songDict["channels"];
        }

        public override string ToString()
        {
            return String.Format("{0} {{\n\ttitle: {1},\n\talbum: {2},\n\talbumArtist: {3},\n\ttrackArtist:{4},\n\tCD: {5},\n\ttrack: {6},\n\tcodec: {7},\n\tbitRate: {8},\n\tsampleRate: {9},\n\tchannels: {10}\n}}", base.ToString(), title, album, albumArtist, trackArtist, discNumber, trackNumber, codec, bitRate, sampleRate, channels);
        }

        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            Song s = (Song)obj;
            return s.title.Equals(title) && s.album.Equals(album) && s.albumArtist.Equals(albumArtist) && s.trackArtist.Equals(trackArtist) && s.codec.Equals(codec) && s.channels.Equals(channels) && s.discNumber == discNumber && s.trackNumber == trackNumber && s.bitRate == bitRate && s.sampleRate == sampleRate;
        }

        public override int GetHashCode()
        {
            return title.GetHashCode() ^ album.GetHashCode() ^ albumArtist.GetHashCode() ^ trackArtist.GetHashCode() ^ codec.GetHashCode() ^ channels.GetHashCode() + discNumber + trackNumber * 7 + bitRate * 2 + sampleRate * 11;
        }
    }
}
