using System;
using System.Collections.Generic;
using System.Configuration;
using NHibernate.Tool.hbm2ddl;
using Kululu.Entities;
using Dror.Common.Data.Contracts;
using Kululu.Entities.Common;
using Dror.Common.Data.NHibernate;
using Configuration = NHibernate.Cfg.Configuration;

namespace Kululu.DBCreator
{
    class Program
    {
        //http://aabs.wordpress.com/2008/01/18/c-by-contract-using-expression-trees/
        static void Main(string[] args)
        {
            Console.WriteLine("Creating Tables...");

            var sessionFactory = new SessionFactory();

            Console.WriteLine("Finished Creating Tables!");
            Console.WriteLine("inserting new data...");
            IRepository repository = new NHibernateRepository(sessionFactory);
            foreach (var str in ConfigurationManager.ConnectionStrings)
            {
                if (!str.ToString().Contains("server=localhost") &&
                    !str.ToString().Contains(".\\SQLEXPRESS") &&
                    !str.ToString().Equals(String.Empty))
                {
                    Console.WriteLine("WTF? Don't run this on a remote server!");
                    Console.ReadKey();
                    return;
                }
            }
            try
            {
                repository.BeginTransaction();
                
                //stas 664118894
                var fbUser1 = new FbUser { Id = 585701767, JoinDate = DateTime.Now, Name = "anna", Status = UserStatus.Joined };
                var fbUser2 = new FbUser { Id = 806905557, JoinDate = DateTime.Now, Name = "stas", Status = UserStatus.Joined };
                var fbUser3 = new FbUser { Id = 716712905, JoinDate = DateTime.Now, Name = "dror", Status = UserStatus.Joined };
                //var fbUser3 = new FbUser { Id = 806905557, JoinDate = DateTime.Now, Name = "ran", Status = UserStatus.Joined };
                var fbUser4 = new FbUser { Id = 100000431166374, JoinDate = DateTime.Now, Name = "yadid", Status = UserStatus.Joined };

                repository.Save(fbUser1);
                repository.Save(fbUser2);
                repository.Save(fbUser3);
                repository.Save(fbUser4);

                var localBusiness1 = new LocalBusiness
                {
                    AddressCity = "Yehud",
                    AddressStreet = "Levi Eschol 1",
                    Category = "Music",
                    CreatedDate = DateTime.Now,
                    FacebookUrl = "http://www.facebook.com/pages/%D7%A7%D7%95%D7%9C%D7%95%D7%9C%D7%95/178456328876503",
                    FanPageId = 186841648018387,
                    //FanPageId = 258829897475960,
                    LastModified = DateTime.Now,
                    Name = "קולולו",
                    PublishUserContentToWall = false,
                    PublishAdminContentToWall = true,
                    Owners = new List<FbUser> { fbUser1, fbUser2, fbUser3, fbUser4 }
                };

                var uriBusiness = new LocalBusiness
                    {
                        AddressCity = "Faraway city",
                        AddressStreet = "Back alley",
                        Category = "Music",
                        CreatedDate = DateTime.Now,
                        FacebookUrl =
                            "https://www.facebook.com/pages/%D7%96%D7%9E%D7%A8-test/245866165438909",
                        FanPageId = 245866165438909,
                        FBFanPageAccessToken = "157721837628308|afd07c5be8ae8e01047a1156.1-662326561|juC8P9wsNLP5XirWXPCzEMF7qPU",
                        ImageUrl =
                            "https://fbcdn-profile-a.akamaihd.net/static-ak/rsrc.php/v1/yA/r/gPCjrIGykBe.gif",
                        LastModified = DateTime.Now,
                        Name = "זמר טסט",
                        PublishUserContentToWall = true,
                        PublishAdminContentToWall = true,
                        Owners =
                            new List<FbUser>
                                {fbUser1, fbUser2, fbUser3, fbUser4}
                    };

                var uriRadio = new Playlist
                    {
                        Name = "Radio Uri",
                        Description = "",
                        Image = "",
                        CreationDate = DateTime.Now,
                        LastModifiedDate = DateTime.Now,
                        NextPlayDate = DateTime.Parse("23/07/2013 08:55:00"),
                        IsUserModifyable = true,
                        NumOfSongsLimit = 5
                    };

                uriBusiness.AddPlaylist(uriRadio);
                uriBusiness.ImportPlaylist = uriRadio;
                repository.Save(uriBusiness);

                var radio1 = new Playlist
                {
                    Name = "Kululu-FM",
                    Description = "",
                    Image = "",
                    CreationDate = DateTime.Now,
                    LastModifiedDate = DateTime.Now,
                    NextPlayDate = DateTime.Parse("23/07/2012 08:55:00"),
                    IsUserModifyable = true,
                    NumOfSongsLimit = 5
                };
                
                localBusiness1.AddPlaylist(radio1);
                localBusiness1.ImportPlaylist = radio1;
                repository.Save(localBusiness1);

                var localBusiness2 = new LocalBusiness()
                {
                    Owners = new List<FbUser> { fbUser1 },
                    AddressCity = "Haifa",
                    AddressStreet = "Rauel Wallenberg",
                    Category = "Music",
                    CreatedDate = DateTime.Now,
                    FacebookUrl = "http://www.facebook.com/pages/%D7%A7%D7%95%D7%9C%D7%95%D7%9C%D7%95/178456328876503",
                    FanPageId = 1,
                    ImageUrl = "http://profile.ak.fbcdn.net/hprofile-ak-snc4/195725_178456328876503_4279627_n.jpg",
                    LastModified = DateTime.Now,
                    PublishAdminContentToWall = true,
                    PublishUserContentToWall = false,
                    Name = "Kululu Test"
                };

                var radio2 = new Playlist
                {
                    Description = "",
                    Image = "",
                    CreationDate = DateTime.Now,
                    LastModifiedDate = DateTime.Now,
                    Name = "Radius-100FM",
                    NextPlayDate = DateTime.Parse("25/07/2011"),
                    IsUserModifyable = true,
                    NumOfSongsLimit = 10
                };

                localBusiness2.AddPlaylist(radio2);
                repository.Save(localBusiness2);

                var playlistSongRating = SaveSong(repository, radio1, fbUser1, "Cee Lo Green", "Bright Lights Bigger City", 231, "UBhdIcb84Hw", 1, "http://userserve-ak.last.fm/serve/126s/62075863.png");
                //radio1.AddRating(song, fbUser3, -1);
                radio1.RateSong(playlistSongRating, fbUser4, 1);
                radio1.RateSong(playlistSongRating, fbUser2, 1);
                uriRadio.AddSong(playlistSongRating.Song, fbUser1, true);
                uriRadio.RateSong(playlistSongRating, fbUser4, 1);

                playlistSongRating = SaveSong(repository, radio1, fbUser1, "מוש בן ארי", "סתכל לי בעיניים", 249, "HwgVLsnaAoU", 1, "http://userserve-ak.last.fm/serve/126s/20443621.jpg");
                //radio1.AddRating(song, fbUser3, 1);
                radio1.RateSong(playlistSongRating, fbUser4, 1);
                radio1.RateSong(playlistSongRating, fbUser2, 1);
                uriRadio.AddSong(playlistSongRating.Song, fbUser1, true);
                uriRadio.RateSong(playlistSongRating, fbUser4, 1);
                uriRadio.RateSong(playlistSongRating, fbUser2, 1);

                playlistSongRating = SaveSong(repository, radio1, fbUser1, "J.LO FT. Pitbull", "On The Floor", 267, "t4H_Zoh7G5A", 1, null);
                uriRadio.AddSong(playlistSongRating.Song, fbUser1, true);

                playlistSongRating = SaveSong(repository, radio1, fbUser1, "גלעד שגב", "חנה'לה התבלבלה", 214, "vAlFB4Z7P2Y", 1, "http://userserve-ak.last.fm/serve/126s/163895.jpg");
                uriRadio.AddSong(playlistSongRating.Song, fbUser1, true);

                playlistSongRating = SaveSong(repository, radio1, fbUser1, "Adele", "Someone Like You", 306, "7AW9C3-qWug", 1, "http://userserve-ak.last.fm/serve/126s/58297847.png");
                uriRadio.AddSong(playlistSongRating.Song, fbUser1, true);

                SaveSong(repository, radio1, fbUser1, "Britney Spears", "Till The World Ends", 236, "qzU9OrZlKb8", 1, "http://userserve-ak.last.fm/serve/126s/59558187.png");
                uriRadio.AddSong(playlistSongRating.Song, fbUser1, true);

                SaveSong(repository, radio1, fbUser1, "Enrique Iglesias", "Tonight", 299, "Jx2yQejrrUE", 1, "http://userserve-ak.last.fm/serve/126s/65262750.png");
                uriRadio.AddSong(playlistSongRating.Song, fbUser1, true);

                playlistSongRating = SaveSong(repository, radio1, fbUser1, "Pitbull", "Give Me Everything", 267, "EPo5wWmKEaI", 1, "http://userserve-ak.last.fm/serve/126s/64990460.png");
                uriRadio.AddSong(playlistSongRating.Song, fbUser1, true);

                //radio1.AddRating(song, fbUser3, -1);
                radio1.RateSong(playlistSongRating, fbUser4, 1);
                radio1.RateSong(playlistSongRating, fbUser2, -1);
                uriRadio.RateSong(playlistSongRating, fbUser4, 1);
                uriRadio.RateSong(playlistSongRating, fbUser2, -1);

                playlistSongRating = SaveSong(repository, radio1, fbUser1, "Lady Gaga", "Born This Way", 440, "wV1FrqwZyKw", 1, "http://userserve-ak.last.fm/serve/126s/63387017.png");
                uriRadio.AddSong(playlistSongRating.Song, fbUser1, true);

                playlistSongRating = SaveSong(repository, radio1, fbUser1, "ברי סחרוף", "זמן של מספרים", 303, "PCwDDGYely0", 1, "http://userserve-ak.last.fm/serve/126s/784256.jpg");
                uriRadio.AddSong(playlistSongRating.Song, fbUser1, true);

                playlistSongRating = SaveSong(repository, radio1, fbUser1, "Depeche Mode", "Personal Jesus (The Stargate Mix)", 252, "3xLvArgSp3k", 1, "http://userserve-ak.last.fm/serve/126s/2126897.jpg");
                uriRadio.AddSong(playlistSongRating.Song, fbUser1, true);

                playlistSongRating = SaveSong(repository, radio1, fbUser1, "Diddy Dirty Money ft. Skylar Grey", "Coming Home", 251, "k-ImCpNqbJw", 1, null);
                uriRadio.AddSong(playlistSongRating.Song, fbUser1, true);

                playlistSongRating = SaveSong(repository, radio1, fbUser1, "The Black Eyed Peas", "Just Can't Get Enough", 236, "OrTyD7rjBpw", 1, "http://userserve-ak.last.fm/serve/126s/65970878.png");
                uriRadio.AddSong(playlistSongRating.Song, fbUser1, true);

                playlistSongRating = SaveSong(repository, radio1, fbUser1, "Bob Sinclar & Raffaella Carrà", "Far l'Amore", 220, "rSmdeqxxLLk", 1, "http://userserve-ak.last.fm/serve/126s/26932877.jpg");
                uriRadio.AddSong(playlistSongRating.Song, fbUser1, true);

                playlistSongRating = SaveSong(repository, radio1, fbUser1, "Jessie J ft. B.o.B.", "Price Tag", 247, "qMxX-QOV9tI", 1, null);
                uriRadio.AddSong(playlistSongRating.Song, fbUser1, true);

                //radio1.AddRating(song, fbUser3, -1);
                radio1.RateSong(playlistSongRating, fbUser4, -1);
                radio1.RateSong(playlistSongRating, fbUser2, -1);
                uriRadio.RateSong(playlistSongRating, fbUser4, -1);
                uriRadio.RateSong(playlistSongRating, fbUser2, -1);
                //radio2.AddRating(song, fbUser3, 1);

                playlistSongRating = SaveSong(repository, radio1, fbUser1, "Lady Gaga", "The Edge Of Glory", 328, "QeWBS0JBNzQ", 1, "http://userserve-ak.last.fm/serve/126s/63387017.png");
                uriRadio.AddSong(playlistSongRating.Song, fbUser1, true);

                //SaveSong(repository, radio1, fbUser1, "הפרוייקט של עידן רייכל - אִמָּא, אַבָּא וכל הַשְּׁאָר", "D98E89oUo6o", 1);
                playlistSongRating = SaveSong(repository, radio1, fbUser1, "אדיר גץ", "איך היא רוקדת", 236, "Pu2s7bboV9M", 1, "http://userserve-ak.last.fm/serve/126s/51539323.jpg");
                uriRadio.AddSong(playlistSongRating.Song, fbUser1, true);

                playlistSongRating = SaveSong(repository, radio1, fbUser1, "שלומי סרנגה", "זה רק נדמה לך", 218, "MaZIYKtIfnM", 1, "http://userserve-ak.last.fm/serve/126s/83144.jpg");
                uriRadio.AddSong(playlistSongRating.Song, fbUser1, true);

                playlistSongRating = SaveSong(repository, radio1, fbUser1, "מאור כהן", "ישראל", 183, "svjdhfKJbUc", 1, "http://userserve-ak.last.fm/serve/126s/311813.jpg");
                uriRadio.AddSong(playlistSongRating.Song, fbUser1, true);

                //radio2.RateSong(song, fbUser2, 1);
                //radio2.AddRating(song, fbUser3, 1);

                repository.CommitTransaction();
            }
            catch (Exception e)
            {
                repository.RollbackTransaction();
                repository.CloseSession();

                Console.WriteLine("*** Failure ({0}): {1}", e.GetType(), e.Message);
            }
            Console.WriteLine("Finished!");
            Console.ReadKey();
        }

        private static PlaylistSongRating SaveSong(IRepository repository, Playlist playlist, FbUser votingUser,
                                    string artistName, string songName, double duration, string videoId, short rating, string imageUrl)
        {
            var song = new Song
                {
                    Name = songName,
                    ArtistName = artistName,
                    Duration = duration,
                    VideoID = videoId,
                    ImageUrl = imageUrl
                };
            repository.Save(song);
            var playlistRating = playlist.AddSong(song, votingUser, playlist.IsOwner(votingUser.Id));
            repository.SaveOrUpdate(playlistRating);
            return playlistRating;
        }

        private static void BuildSchema(Configuration config)
        {
            // this NHibernate tool takes a configuration (with mapping info in)
            // and exports a database schema from it
            new SchemaExport(config)
                    .Create(true, true);
        }
    }
}