using System.Configuration;

namespace Kululu.Web.Common
{
    public class PlaylistConfiguration : ConfigurationSection
    {
        // Create a "remoteOnly" attribute.
        [ConfigurationProperty("songsLimitPerUser",  IsRequired = true)]
        public int SongsLimitPerUser
        {
            get
            {
                return (int)this["songsLimitPerUser"];
            }
            set
            {
                this["songsLimitPerUser"] = value;
            }
        }

        private PlaylistConfiguration()
        {
        }

        public static PlaylistConfiguration Configuration()
        {
            return ConfigurationManager.GetSection("playlistSettings") as PlaylistConfiguration;
        }
    }
}