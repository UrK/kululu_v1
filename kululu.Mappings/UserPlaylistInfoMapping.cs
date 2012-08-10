using Kululu.Entities;
using FluentNHibernate.Mapping;

namespace Kululu.Mappings
{
    public class UserPlaylistInfoMapping : ClassMap<UserPlaylistInfo>
    {
        public UserPlaylistInfoMapping()
        {
            Table("user_playlist_info");

            Map(o => o.Points);
            Map(o => o.NumOfSongsUsed);
            Map(o => o.NumOfVotes);
            Map(o => o.LastEntrance);

            //http://devlicio.us/blogs/anne_epstein/archive/2009/11/20/nhibernate-and-composite-keys.aspx
            CompositeId()
                .KeyReference(o => o.Playlist, "Playlist_id")
                .KeyReference(o => o.User,"FbUser_id");
        }
    }
}
