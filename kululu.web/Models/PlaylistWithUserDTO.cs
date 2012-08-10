using AutoMapper;
using Kululu.Entities;

namespace Kululu.Web.Models
{
    public class PlaylistWithUserDTO
    {
        public PlaylistDTO Playlist { get; set; }
        public UserDTO User { get; set; }
    }

    public static class PlaylistWithUserDTOFactory
    {   
        static PlaylistWithUserDTOFactory()
        {
            PlaylistDTOFactory.CreateMaps();
            Mapper.CreateMap<Playlist, PlaylistWithUserDTO>();
        }

        public static PlaylistWithUserDTO Map(Playlist playlist, UserDTO user)
        {
            var playlistDTO = new PlaylistWithUserDTO()
            {
                Playlist = Mapper.Map<Playlist, PlaylistDTO>(playlist),
                User = user
            };

            return playlistDTO;
        }
    }
}