function ajax() {
    /**************************************************/
    /** Public Properties**/
    /**************************************************/
    if (typeof Globals == 'undefined') {
        relativePath = hfAppPath;
    }
    else {
        relativePath = Globals.hfAppPath;
    }
    var settings =
    {
        getStartCount:0,
        getSongsIncrement: 20,
        sortBy: 'DateAdded'
    };
    var that = this;
    /**************************************************/
    /** Public Methods  **/
    /**************************************************/
    this.refresh = function () {
        if (typeof Globals.playlistSettings == 'undefined') //if no playlist has been created, just reload the page
        {
            return location.reload();
        }

        //http://stackoverflow.com/questions/503093/how-can-i-make-a-redirect-page-in-jquery/506004#506004
        var relocateHref = relativePath + '/Account/ShowDefaultScreen?playlistId=' + Globals.playlistSettings.playlistID;
        window.location.replace(relocateHref);
        location.href = relocateHref;
    }

    this.create = function (pageData, callback) {
        $.ajax({
            type: 'POST',
            url: relativePath + '/Playlist/CanvasCreate',
            data: pageData,
            success: function (rslt) {
                callback(rslt);
            }
        });
    }

    this.login = function (accessToken, callback) {
        var vars = {
            'accessToken': accessToken,
            'playlistId': null
        };
        $.post(relativePath + '/Account/Login', vars, function (userResponse) { //TODO: should contain UserRole
            if (userResponse.Status != 1) {
                return callback(false);
            }
            return callback(true);
        });
    };

    this.logout = function (callback) {
        $.post(relativePath + '/Account/logout', function () {
            if (typeof (callback) != 'undefined') {
                return callback();
            };
        });
    };
//    this.getPlaylists = function (callback) {
//        var url = relativePath + '/LocalBusiness/GetPlaylists/';
//        var vars = {
//            id: Globals.localBusiness.Id
//        };
//        $.post(url, vars, function (data) {
//            if (data.error==""){//check for errors
//                var dataout = [];
//                $(data.data).each(function(){
//                    dataout.push({
//                        name: this.Name,
//                        id: this.Id,
//                        selected: this.Id == Globals.playlistSettings.playlistID
//                    });
//                });
//                callback(dataout);
//            }
//            else{
//                callback([]);
//            };
//        });
//    }

//    /**************************************************/
//     this.GetSong = function(songID, callback) {
//        var vars = {
//            'playlistId': Globals.playlistSettings.playlistID,
//            'songId': songID
//        };

//        $.post(relativePath + "/Playlist/GetSong", vars, function (data) {
//            if(!callback){
//                return;
//            }
//            callback(data);
//        });
//    };

    /**************************************************/
    this.getSongs = function (callback, options) {
        var getSongsVars = {
            'localBussinesId': Globals.localBusiness.Id,
            'startCount': settings.getStartCount,
            'increment': settings.getSongsIncrement,
            'playlistId': null,
            'sort': settings.sortBy,
            'descending': 'true',
            'referringPlaylistSongRatingId': Globals.referringPlaylisingSongRating
        };
        $.extend(getSongsVars, options);
        $.post(relativePath + '/Playlist/GetSongs', getSongsVars, function (response) {
            response.Data.songs = createSongs(response.Data.songs, "Current");
            callback(response.Data);
        }, 'json');
    };
    /**************************************************/
    this.getVotes = function (vars, callback) {
        var getVoteVars = {
            'playlistId': Globals.playlistSettings.playlistID,
            'playlistSongRatingId': parseInt(vars.songId)
        };
        var votesPositive = [];
        var votesNegative = [];
        var getLikeString = function (Gender, RatingValue) {
            switch (Gender) {
                case 'male':
                    return (RatingValue == 1 ? Globals.Strings.maleLiked : Globals.Strings.maleNotLiked);
                    break;
                case 'female':
                    return (RatingValue == 1 ? Globals.Strings.femaleLiked : Globals.Strings.femaleNotLiked);
                    break;
                default:
                    return (RatingValue == 1 ? Globals.Strings.maleLiked : Globals.Strings.maleNotLiked);
            }
        };
        $.post(relativePath + '/Playlist/GetAllRatingsOfSong', getVoteVars, function (response) {
            $(response.Data.Rating).each(function () {
                var vote = {
                    user: this.Member.name,
                    id: this.Member.FBID,
                    photo: "https://graph.facebook.com/" + this.Member.FBID + "/picture",
                    like: getLikeString(this.Member.Gender, this.RatingValue),
                    date: relativeDate(this.Date)
                };
                (this.RatingValue == 1) ? votesPositive.push(vote) : votesNegative.push(vote);
            });
            callback({ votesPositive: votesPositive, votesNegative: votesNegative });
        });
      
    };
    /**************************************************/
    this.getNumOfVotes = function (callback) {
        var vars = {
            'playlistId': Globals.playlistSettings.playlistID
        };

        $.post(relativePath + "/Playlist/GetNumOfVotes", vars, function (response) {
            callback(response.Data);
        });
    }

    /**************************************************/
    this.RateSong = function RateSong(songID, likeValue, callback) {
        var vars = {
            'playlistId': Globals.playlistSettings.playlistID,
            'playlistSongRatingId': songID,
            'rating': likeValue
        };

        $.post(relativePath + "/Playlist/RateSong", vars, function (response) {
            SetFbLike(Globals.playlistSettings.playlistID, songID, likeValue);

            //TODO: make sure these graph actions work
            FB.api("/me/kululu-radio:vote", { song: "http://samples.ogp.me/219201131480378" }, function (response) {
            });

            response.Data.Song = createSongs(response.Data.SongInfo, "Current");
            callback(response.Data);
        });
    };
    /**************************************************/
    this.querySongs = function querySongs(query, callback) {
        var vars = {
            'playlistId': Globals.playlistSettings.playlistID,
            'queryStr': query
        };
        $.post(relativePath + '/Playlist/QuerySongs', vars, function (response) {
            var songs = [];
            if (response.Data.songsInplaylist.length > 0) {
                songs = createSongs(response.Data.songsInplaylist, "SearchResultLocal");
            }
            callback(songs);
        });
    };
    this.getComments = function (postId, callback) {
        var id = Globals.localBusiness.fanPageId + "_" + postId;
        FB.api('/' + id + '/comments', { date_format: 'U' }, function (response) {
            $(response.data).each(function () {
                this.created_time = relativeDate("/Date(" + this.created_time + "000)/");
            });
            return callback(response.data);
        });
    };
    this.addComment = function (id, comment, callback) {
        var postId = Globals.localBusiness.fanPageId + "_" + id;
        FB.api('/' + postId + '/comments', 'post', { message: comment }, function(response) {
            return callback(response);
        });
    };
    /**************************************************/
    var splitTitle = function(title){
        //split song title to artist and title
        var names = title.split("-");
        if (names.length == 2) { //artist and title information seperated with hyphen
            artist = names[0];
            title = names[1];
        }
        else {
            title = title;
            artist = "";
        }
        return {artist: artist, title: title};
    };
    var parseVideoURL = function(url) {
        var text = url.split(/\s+/);
        for (var i = 0; i < text.length; i++) {
            var url = text[i];
            if (/^https?\:\/\/.+/i.test(url)) {
                var temp = /[\?\&]v=([^\?\&]+)/.exec(url);
                if (temp) { //found
                    return temp[1];
                } else {
                    // text[i] = "URL found but does not contain video id";
                }
            }
        }
        return;
    };

    this.youtubeGetSongs = function (query, callback) {
        var createYouTubeData = function (data) {
            var songInfo = splitTitle(data.title);
            return {
                SongVideoID: data.id,
                SongDuration: data.duration,
                SongFulltitle: data.title.replace(/"/g, '&quot;'),
                SongName: songInfo.title,
                SongArtistName: songInfo.artist,
                SongImageUrl: null
            };
        };
        var youtubeSearchVars = {
            'v': '2',
            'format': '5',
            'alt': 'jsonc',
            'key': 'AI39si5Oc1PU30CEks9gd5D2tzMGPtuUlEPWSX38K8ouKsUI7p8l35abbwFLSUluss5WEu5a5c4seL4SPLcEWvX87iD3yX6_xw'
        };
        var videoID = parseVideoURL(query);
        if (typeof (videoID) != 'undefined') {//check for value URL and YouTube video ID
            var youtubeServiceCall = "https://gdata.youtube.com/feeds/api/videos/" + videoID + "?&callback=?";
            $.getJSON(youtubeServiceCall, youtubeSearchVars, function (data) {
                //check if error i.e. typeof(data.error) == 'undefined'
                if (typeof (data.error) != 'undefined') {//video not found
                    return callback([]);
                };

                var song = constructSongPrototype(createYouTubeData(data.data), "SearchResultRemote");
                song.SongId = -1;
                callback(song);
            });
        }
        else {
            var youtubeServiceCall = "https://gdata.youtube.com/feeds/api/videos/?callback=?";
            $.extend(youtubeSearchVars, {
                'q': query,
                'category': 'Music',
                'start-index': '1',
                'max-results': '10'
            });
            $.ajax({
                url: youtubeServiceCall,
                dataType: 'json',
                data: youtubeSearchVars,
                async: false,
                success: function (data) {
                    if (typeof (data.data.items) == 'undefined') { //no songs found
                        return callback([]);
                    }
                    var songsData = []
                    var songs = [];
                    $(data.data.items).each(function (index) {
                        var song = constructSongPrototype(createYouTubeData(this), "SearchResultRemote");
                        song.SongId = index * -1 - 1;
                        songs.push(song);
                    });
                    callback(songs);
                }
            });
        }
    };

    /**************************************************/
    this.importFBStream = function (localBusinessId, callback, type) {
        var vars = {
            localBusinessId: localBusinessId,
            type: type
        };

        $.post(relativePath + "/FbStreamHarvest/Import", vars, function (data) {
            if (!callback) {
                return;
            }
            callback(data);
        });
    };

    this.synchronizeFBStream = function (localBusinessId, callback, type) {
        var vars = {
            localBusinessId: localBusinessId,
            type: type
        };

        $.post(relativePath + "/FbStreamHarvest/Synchronize", vars, function (data) {
            if (!callback) {
                return;
            }
            callback(data);
        });
    };

    this.getImportStatus = function (callback) {
        $.post(relativePath + "/FbStreamHarvest/GetImportStatus", function (data) {
            if (!callback) {
                return;
            }
            callback(data);
        });
    };

    this.getImportLikesComplete = function (callback) {
        $.post(relativePath + "/FbStreamHarvest/GetImportLikesComplete", function (data) {
            if (!callback) {
                return;
            }
            callback(data);
        });
    };

    this.areUsersPageAdmins = function (localBusinessId, userIds, rows, callback) {
        var vars =
        {
            localbusinessId: localBusinessId,
            userIds: userIds
        }

        $.post(relativePath + "/Facebook/ArePageAdmins", vars, function (data) {
            if (!callback) {
                return;
            }
            callback(data, rows);
        });
    };

    

    /**************************************************/
    this.getArtistImages = function (artist, limit, callback) {
        if (artist.length == 0) {
            return callback([]); //TODO: check if callback is defined
        };
        $.getJSON('https://ws.audioscrobbler.com/2.0/?callback=?', {//last.fm API
            format: 'json',
            method: 'artist.getimages',
            artist: artist,
            limit: limit,
            api_key: '11558012cfa9e625b0a74c79fb4e5ab4'
        }, function (data) {
            if ((typeof (data.error) == 'undefined') && (typeof (data.images.image) != 'undefined')) { // artist image found
                if (!$.isArray(data.images.image)) {//one image
                    return callback(data.images.image.sizes.size[2]["#text"]);
                }
                var images = []; //more that one image
                for (var i = 0; i < data.images.image.length; i++) {
                    images.push(data.images.image[i].sizes.size[2]["#text"]);
                };
                return callback(images);
            }
            else {
                return callback([]);
            };
        });
    };
    this.addSong = function (songData, attachement, callback) {//TODO: it's not a good place to get DOM element. instead use Objects
        $.post(relativePath + '/Playlist/AddSong', songData, function (response) {
            if (response.Status == -1 || response.Status == -2) { //some error 
                return callback(response);
            };
            postToWall(response.Data.song.SongId, attachement);
            response.Data.song = createSongs(response.Data.song, "Current");
            callback(response);
        });
    };

    var SetFbLike = function (playlistID, songId, rating) {
     var vars =
        {
            'playlistID': playlistID,
            'playlistSongRatingId': songId,
            'rating': rating
        };

        $.post(relativePath + '/Facebook/SetFbLike', vars, function (data) {
        });
    }

    var postToWall = function (songId, attachement) {
        var replaceNewlines = function (msg) {
            return msg.replace(/\n/g, "<center></center>");
        }
        //        msg = replaceNewlines(msg).toString();
        //console.log(msg);
        var vars =
        {
            'playlistSongRatingId': songId,
            'playlistID': Globals.playlistSettings.playlistID,
            'msg': attachement.msg,
            'description': attachement.description
        };
        $.post(relativePath + '/Facebook/PostToWall', vars, function (data) {
        });
    }

    /**************************************************/
    this.DeleteSong = function (songID, callback) {
        var songId = parseInt(songID);
        var vars = {
            'playlistSongRatingId': songId,
            'playlistId': Globals.playlistSettings.playlistID
        };

        //removing facebook wall post
        removePost(songId,
        //removing song locally
           function () {
               $.post(relativePath + "/Playlist/DeleteSong", vars, function (response) {
                   callback(response);
               });
           }
       )
    };

    /**************************************************/
    var removePost = function (songID, callback) {
        var vars = {
            'playlistSongRatingId': parseInt(songID),
            'playlistId': Globals.playlistSettings.playlistID
        };

        $.post(relativePath + '/Facebook/RemovePost', vars, function (data) {
            callback();
        });
    }

    /**************************************************/
//    this.getPlaylistDetails = function (callback) {
//        var vars = {
//            'playlistId': Globals.playlistSettings.playlistID
//        };
//        $.post(relativePath + "/Playlist/GetDetails", vars, function (data) {
//            callback(data.data);
//        });
//    };
    /**************************************************/
    this.UpdateSongDetails = function(data, callback) {
        var vars = {
            playlistId: Globals.playlistSettings.playlistID,
            playlistSongRatingId: data.songId,
            songName: data.title,
            artistName: data.artist,
            imageUrl: data.thumbnail
        };
        $.post(relativePath + "/Playlist/UpdateSong", vars, function (response) {
            callback(response);
        });
    };
    /**************************************************/
    this.SendFeedback = function (feedback, callback) {
        var vars = {
            feedback: feedback,
            playlistId: Globals.playlistSettings.playlistID
        };
        $.post(relativePath + "/Facebook/SendFeedback", vars, function (response) {
            callback(response);
    });
};

    this.addOwner = function (businessId, ownerId, callback) {
        var vars = {
            localBusinessId: businessId,
            ownerId: ownerId
        };
        $.post(relativePath + "/LocalBusiness/AddOwner", vars, function (data) {
            callback(data);
        });
    };

    this.removeOwner = function(businessId, ownerId, callback) {
        var vars = {
            localBusinessId: businessId,
            ownerId: ownerId
        };
        $.post(relativePath + "/LocalBusiness/RemoveOwner", vars, function (data) {
            callback(data);
        });
    };

    this.setSocialSettings = function (businessId, properties, callback) {
        var vars = {
            id: businessId,
            properties: JSON.stringify(properties)
        };

        $.post(relativePath + "/LocalBusiness/SetSocialSettings", vars, function (data) {
            if (!callback) {
                return;
            }
            callback(data);
        });
    }

    this.setLocalBusinessProperty = function (id, propertyName, value, callback) {
        var vars = {
            id: id,
            propertyName: propertyName,
            value: value
        };

        $.post(relativePath + "/LocalBusiness/CheckProperty", vars, function (data) {
            if (!callback) {
                return;
            }

            callback(data);
        });
    }

    /**************************************************/
    /**************************************************/
    /** Private Methods **/
    var constructSongPrototype = function (data, type) {
        //create simple song prototype - used both for search results and default entries
        var language;
        var artist;
        var title;
        var addDisabled = false;
        var fulltitle = data.SongFulltitle;
        if (data.SongName.match(/[א-ת]/g) != null) {
            language = 'hebrew';
        }
        else {
            language = 'english';
        }
        title = data.SongName;
        artist = data.SongArtistName;
        if (typeof (fulltitle) == 'undefined') {
            if (artist == "") {
                fulltitle = title;
            }
            else {
                fulltitle = artist + ' - ' + title;
            };
        };

        if (Globals.User.role == 'Owner') {
            addDisabled = false;
        }
        else {
            if (Globals.User.numOfSongsLeft == 0 || (!Globals.playlistSettings.playlistActive)) {
                addDisabled = true;
            }
        }
        var thumbnail = data.SongImageUrl == null
                         ? "https://img.youtube.com/vi/" + data.SongVideoID + "/default.jpg" //0.jpg is larger 
                         : data.SongImageUrl;
        var song = {
            YouTubeID: data.SongVideoID,
            YouTubeThumbnail: "https://img.youtube.com/vi/" + data.SongVideoID + "/default.jpg", //0.jpg is larger
            thumbnail: thumbnail,
            artist: artist,
            title: title,
            fulltitle: fulltitle,
            duration: data.SongDuration,
            SongId: 0, // no local db Id for searh results
            rating: 0, //absolute Rating 
            Votes: 0,
            showOptions: false,
            language: language,
            type: type,
            addDisabled: addDisabled
        };
        return song;
    };
    /**************************************************/
    var createSongs = function (data, type) {
        var songs = [];

        var defaultMessage = Globals.Strings.createSongDefaultMessage + Globals.playlistSettings.playlistName + '.';
        var defaultDescription = Globals.Strings.createSongDefaultDescription;
        $(data).each(function (index) {
            var song = constructSongPrototype(this, type);
            song.SongId = this.SongId;
            song.Timeago = relativeDate(this.CreationDate); //convert date to relative...
            var date = new Date(parseInt(this.CreationDate.substr(6)));
            song.Message = (this.Message != null) ? this.Message : defaultMessage;
            song.Description = (this.Description != null) ? this.Description : defaultDescription;
            song.CreationDate = new Date(Date.parse(date));
            song.Creator = this.Member.name;
            song.CreatorId = this.Member.FBID;
            song.UserRating = this.Rating;
            song.showOptions = (this.Member.FBID == Globals.User.FBID) || (Globals.User.role == 'Owner');
            song.PositiveVotes = this.SummedPositiveRating;
            song.NegativeVotes = this.SummedNegativeRating;
            song.Votes = this.SummedPositiveRating + this.SummedNegativeRating;
            song.numOfComments = this.NumOfComments;
            song.fbPageId = this.FBOnlyPageId;
            song.fbPostId = this.FBOnlyPostId;

            songs.push(song);
        });
        return songs;
    };

    /**************************************************/
    function relativeDate(dateParam) {
        var date = new Date(parseInt(dateParam.substr(6)));
        var d = new Date(Date.parse(date));
        var dateFunc = new Date();
        var timeSince = dateFunc.getTime() - d;
        if (timeSince < 0) {
            timeSince = 0;
        }
        var inSeconds = timeSince / 1000;
        var inMinutes = timeSince / 1000 / 60;
        var inHours = timeSince / 1000 / 60 / 60;
        var inDays = timeSince / 1000 / 60 / 60 / 24;
        var inYears = timeSince / 1000 / 60 / 60 / 24 / 365;

        // in seconds
        if (Math.round(inSeconds) == 1) {
            return Globals.Strings.aSecondAgo;
        }
        else if (inMinutes < 1.01) {
            return Globals.Strings.ago + ' ' + Math.round(inSeconds) + ' ' + Globals.Strings.seconds;
        }

        // in minutes
        else if (Math.round(inMinutes) == 1) {
            return Globals.Strings.aMinuteAgo;
        }
        else if (inHours < 1.01) {
            return Globals.Strings.ago + ' ' + Math.round(inMinutes) + ' ' + Globals.Strings.minutes;
        }

        // in hours
        else if (Math.round(inHours) == 1) {
            return Globals.Strings.anHoursAgo;
        }
        else if (inDays < 1.01) {
            return Globals.Strings.ago + ' ' + Math.round(inHours) + ' ' + Globals.Strings.hours;
        }

        // in days
        else if (Math.round(inDays) == 1) {
            return Globals.Strings.aDayAgo;
        }
        else if (inYears < 1.01) {
            return Globals.Strings.ago + ' ' + Math.round(inDays) + ' ' + Globals.Strings.days;
        }

        // in years
        else if (Math.round(inYears) == 1) {
            return Globals.Strings.aYearAgo;
        }
        else {
            return Globals.Strings.ago + ' ' + Math.round(inYears) + ' ' + Globals.Strings.years;
        }
    };
};
