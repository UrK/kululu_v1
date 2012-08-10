// Playlist Prototype
function chart() {
    var that = this;
    var toolBar;//new toolbar
    // public variables
    this.playlistSortBy = $('ul#filterBar li a.selected').attr('id');
    //'DateAdded';
    /**************************************************/
    // Public Methods
    this.populatePlaylist = function (callback) {
        Globals.ajax.getSongs(function (data) {
            $('.back').hide();
            $('.searchBar a.delete').hide();
            $('ul.info').show();
            $('.song').remove();
            Globals.player.playlist.clear();
            that.renderPlaylist(data.songs, 'Current', callback);
            if (!Boolean(data.areMore)) {
                $('.showMore').hide();
            }
            else {
                $('.showMore a').show();
                $('.showMore').show();
            };
            updatePlaylist(data.playlist);
            if (Globals.referringPlaylisingSongRating != 0) {
                Globals.player.loadSong(Globals.referringPlaylisingSongRating);
                Globals.referringPlaylisingSongRating = 0; //clearing, referral ended
            }

        }, { sort: that.playlistSortBy, playlistId: Globals.playlistSettings.playlistID });
    };
    this.renderPlaylist = function (songs, type, callback) {
        h = createSongElement(songs);
        Globals.player.playlist.add(songs);
        switch (type) {
            case 'Current':
                $('ul.playlist.current').append(h);
                break;
            case 'searchLocal':
                //hide current playlist and local search results
                $('ul.playlist.current').empty();
                $('ul.playlist.current').append(h);
                break;
            case 'searchRemote':
                //hide current playlist and remote search results
                $('ul.playlist.searchResults').empty(); //remove previous search results
                $('ul.playlist.searchResults').append(h);
        };
        updatePlayingStatus();
        updateRatings();
        if (typeof (callback) != 'undefined') {
            callback();
        };
    };
    /**************************************************/
    /**************************************************/
    // Private Methods
    $.jqotetag('*');
    var lambda = $.jqotec('#songLIChartWrapperTemplate');
    var createSongElement = function (song) {
        return $.jqote(lambda, song);
    };
    var updateSongActions = function (el, song) {
        $(el).find('a.btnVotes').html(parseInt(song.Votes) + ' ' + Globals.Strings.ratings);
        $(el).find('input.Votes').val(parseInt(song.Votes));
        $(el).find('input.PositiveVotes').val(parseInt(song.PositiveVotes));
        $(el).find('input.NegativeVotes').val(parseInt(song.NegativeVotes));
    };
    var updatePlayingStatus = function(){
        if (Globals.player.playingNow != null) {
            $('.song.playing').removeClass('playing');
            var song = $('.song[data-id=' + Globals.player.playingNow + ']');
            if (Globals.player.status()) {
                song.addClass('playing')
                    .find('.player').addClass('pause');
            }
            else //paused
            {
                song.addClass('playing')
                    .find('.player').addClass('play');
            };
        };
    };
    var updateRatings = function () {
        if (that.playlistSortBy == 'Rating') {
            $('ul.playlist.current li').each(function (index) {
                var strIndex = (index + 1).toString();
                if (strIndex.length == 1) {
                    strIndex = '0' + strIndex;
                };
                $(this).find('.rating').show().html(strIndex);
                switch (index) {
                    case 0:
                        $(this).find('.rating').removeClass('top1 top2 top3').addClass('top1');
                        break;
                    case 1:
                        $(this).find('.rating').removeClass('top1 top2 top3').addClass('top2');
                        break;
                    case 2:
                        $(this).find('.rating').removeClass('top1 top2 top3').addClass('top3');
                        break;
                };
            });
        };
    };
    var updatePlaylist = function (data) { //TODO: move somewhere else
        Globals.playlistSettings.NumberOfVotes = data.NumberOfVotes;
        Globals.playlistSettings.NumberOfSongs = data.NumberOfSongs;
        $('ul.info span.numOfVotes').html(Globals.playlistSettings.NumberOfVotes);
        $('ul.info span.numOfSongs').html(Globals.playlistSettings.NumberOfSongs);
    };
    /**************************************************/
    /**************************************************/
    var play = function (songId) {
        var song = $('.song[data-id=' + songId + ']');
        if ($(song).hasClass('playing')) {
            Globals.player.resumeSong();
        }
        else {
            $('.song.playing').find('.player')
                .removeClass('loading pause')
                .addClass('play').end()
                .removeClass('playing')
            $(song).find('a.player').removeClass('play pause')
                .addClass('loading').end()
                .addClass('playing');
            Globals.player.loadSong(songId, function (success) {
                if (success) {
                }
            });
        };
    };
    /**************************************************/
    /**************************************************/
    this.addSong = function (song) {
        if (!Globals.playlistSettings.playlistActive && Globals.User.role != 'Owner') {
            Globals.flashmessage.show(Globals.Strings.votingEnded);
            return;
        };
        if (Globals.User.numOfSongsLeft == 0 && Globals.User.role != 'Owner') {
            Globals.blockmessage.load({ dialogName: 'songLimit' }, function () {
                return;
            });
            return;
        };
        if (!Globals.facebookUtils.isConnected()) { // if user is not connected
            Globals.facebookUtils.login();
            return;
        };
        //post message to wall if settings allow so
        //***************
        //hardcoded for OZ
        //only
        //***************
        if ((Globals.localBusiness.fanPageId == 258829897475960 ||
            Globals.localBusiness.fanPageId == 231917093631 ||
            Globals.localBusiness.fanPageId == 245866165438909) && Globals.User.IsPageAdmin) {
            var vars = {
                dialogName: 'addSongClient',
                data: {
                    playlistName: Globals.playlistSettings.playlistName,
                    img: $(song).find('img.thumbnail').attr('src'),
                    videoId: $(song).data('YouTubeId'),
                    link: $(song).data('fulltitle'),
                    caption: "www.youtube.com",
                    language: $(song).data('language')
                }
            };
            Globals.blockmessage.load(vars, function (response) {
                var link = "https://kulu.lu/" + Globals.hfAppPath  + "/Playlist/GetVideoInfo/" + $(song).data('youtubeid');
                if (response.success) {
                    FB.ui({
                        method: 'feed',
                        from: Globals.localBusiness.fanPageId,
                        name: response.data["title"],
                        link: link,
                        caption: "www.youtube.com",
                        description: response.data['description']
                    }, function (response) {
                        if (response && response.post_id) {
                            //published successfully
                        };
                    });
                    //doAddSong(song, response.data["title"], response.data['msg'], response.data['description']);
                }
                else {
                    return;
                };
            });
        }
        else {
            if ((Globals.localBusiness.isAdminPostRequired && Globals.User.IsPageAdmin) ||
                    (Globals.localBusiness.isUserPostRequired && !Globals.User.IsPageAdmin && Globals.User.HasLikedPage)) {
                var vars = {
                    dialogName: 'addSong',
                    data: {
                        playlistName: Globals.playlistSettings.playlistName,
                        img: $(song).find('img.thumbnail').attr('src'),
                        videoId: $(song).attr('data-YouTubeId'),
                        link: $(song).attr('data-fulltitle'),
                        caption: "www.youtube.com",
                        language: $(song).attr('data-language')
                    }
                };
                Globals.blockmessage.load(vars, function (response) {
                    if (response.success) {
                        doAddSong(song, response.data["title"], response.data['msg'], response.data['description']);
                    }
                    else {
                        return;
                    };
                });
            }
            else {//no post to wall
                doAddSong(song, "", "", "");
            };
        };
    };
    /**************************************************/
    /**************************************************/
    var doAddSong = function (song, title, msg, description) {
        updateSongActions(song, { UserRating: 1, Votes: 1, PositiveVotes: 1, NegativeVotes: 0 });
        // TODO: pass songId instead of song
        var songArtist;
        var songName;
        if (title == "") {//title was not modified when posting to wall
            songArtist = $(song).find('span.artist').html();
            songName = $(song).find('a.title').html(); //TODO: escape name!
        }
        else {//modified title
            var songInfo = splitTitle(title);
            songName = songInfo.title;
            songArtist = songInfo.artist;
        };
        var songData = {
            playlistId: Globals.playlistSettings.playlistID,
            videoId: $(song).attr('data-YouTubeId'),
            songName: songName,
            songArtist: songArtist,
            songImage: [],
            songDuration: $(song).attr('data-duration'),
            newRatingValue: 1 //when adding a new song, +1 vote automatically
        };
        var attachement = {
            msg: msg,
            description: description
        };

        Globals.ajax.addSong(songData, attachement, function (response) {
            if (response.Status == 0 || response.Status == 1) { //succeded
                if ($(song).hasClass('playing')) {
                    Globals.player.playingNow = response.Data.song[0].SongId;
                };                
                //update song
                h = createSongElement(response.Data.song);
                $(song).replaceWith(h);
                $(song).hide().fadeIn(400);
                updatePlayingStatus();
                updatePlaylist(response.Data.playlist); // update playlist stats
                updateUser(response.Data.user);

                if (Globals.User.role == 'Owner') {
                }
                else { //show how many songs left to user who is not Owner
                    if (Globals.User.numOfSongsLeft == 0) {
                        Globals.flashmessage.show(Globals.Strings.votingLimit);
                        $('button.btnAddSong').addClass('disabled');
                    }
                    if (Globals.User.numOfSongsLeft > 0) {
                        Globals.flashmessage.show(Globals.Strings.votingRemaining.format(Globals.User.numOfSongsLeft));
                    }
                }
            }
            else if (response.Status == -2) {
                Globals.flashmessage.show(Globals.Strings.votingLimit);
            };
        });
    }
    var splitTitle = function (title) {
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
        return { artist: artist, title: title };
    };
    /**************************************************/
    /**************************************************/
    var editSong = function (songId) {
        var song = $('.song[data-id=' + songId + ']');
        var vars = {
            dialogName: 'editSong',
            data: {
                artist: $(song).find('span.artist').html().replace(/"/g, '&quot;'),
                title: $(song).find('a.title').html().replace(/"/g, '&quot;'),
                thumbnail: $(song).find('img.thumbnail').attr('src'),
                language: $(song).attr('data-language')
            }
        };
        Globals.blockmessage.load(vars, function (response) {
            if (response.success) { //save details
                var vars = {
                    songId: songId,
                    artist: response.data['artist'],
                    title: response.data['title'],
                    thumbnail: response.data['thumbnail']
                    //image URL
                }
                Globals.ajax.UpdateSongDetails(vars, function (response) {
                    if (response.Status == 0) {
                        //update artist+title if changed
                        if ($(song).find('a.title').html() != vars.title) {
                            $(song).find('a.title').fadeOut(200, function () {
                                $(this).html(vars.title).fadeIn(200);
                            })
                        };
                        if ($(song).find('span.artist').html() != vars.artist) {
                            $(song).find('span.artist').fadeOut(200, function () {
                                $(this).html(vars.artist).fadeIn(200);
                            });
                        };
                        if ($(song).find('img.thumbnail').attr('src') != vars.thumbnail) {
                            $(song).find('img.thumbnail').fadeOut(200, function () {
                                $(song).find('img.thumbnail').attr('src', vars.thumbnail)
                                    .fadeIn(200);
                            });

                        };
                    }
                    else {
                    }
                });
            }
            else {
            }
        });
    };
    /**************************************************/
    /**************************************************/
    var deleteSong = function (songId) {
        var vars = { dialogName: 'deleteSong' }
        Globals.blockmessage.load(vars, function (response) {
            if (response.success) {
                Globals.blockmessage.block();
                Globals.ajax.DeleteSong(songId, function (response) {
                    if (response.Status == 0) {
                        Globals.blockmessage.unblock();
                        var song = $('.song[data-id=' + songId + ']');
                        $(song).slideUp(500, function () {
                            $(song).remove();
                            updateRatings();
                        });
                        //update User object
                        updatePlaylist(response.Data.playlist); // update playlist stats
                        updateUser(response.Data.user);
                    }
                    else {
                        Globals.blockmessage.unblock();
                    };
                });
            }
            else {
            };
        });
    };
    /**************************************************/
    /**************************************************/
    this.vote = function (songId, likeValue) {
        if (!Globals.facebookUtils.isConnected()) { // if user is not connceted
            Globals.facebookUtils.login();
        }
        else {
            doVote(songId, likeValue);
        }
    };
    /*************************************************/
    /*************************************************/
    var doVote = function (songId, likeValue) {
        var song = $('.song[data-id=' + songId + ']');

        //var previousSong = $(this).closest('li.song').prev();
        //var nextSong = $(this).closest('li.song').next();
        updateSongActions(song, {
            UserRating: likeValue,
            Votes: $(song).find('input.Votes').val(),
            PositiveVotes: $(song).find('input.PositiveVotes').val(),
            NegativeVotes: $(song).find('input.NegativeVotes').val()
        });
        Globals.ajax.RateSong(songId, likeValue, function (data) {
            if (data.Success) {
                updatePlaylist(data.playlist); // update playlist stats
                updateUser(data.CurrentUser);

                data.Song[0].Rating = $(song).find('.rating').html();
                updateSongActions(song, data.Song[0]);

                //                    var newRating = data.Song[0].PositiveRating;
                //                    var previousRating = parseInt(previousSong.find('input.positiveRating').val());
                //                    var nextRating = parseInt(nextSong.find('input.positiveRating').val());

                //                    //meaning that current rating is heigher or equal to the previously higher vote
                //                    if (newRating >= previousRating) {
                //                        c = $(previousSong).clone();
                //                        $(previousSong).slideUp(300, function () {
                //                            $(this).remove();
                //                        });
                //                        $(song).replaceWith(h);
                //                        c.hide().insertBefore(nextSong).slideDown(700);
                //                    }
                //                    else if (newRating <= nextRating) {
                //                        c = $(nextSong).clone();
                //                        $(nextSong).slideUp(300, function () {
                //                            $(this).remove();
                //                        });
                //                        $(song).replaceWith(h);
                //                        c.hide().insertAfter(previousSong).slideDown(700);
                //                    }
                //                    else {
                //                        $(song).replaceWith(h);
                //                    }
            }
            else {
            }
        });
    };
    /**************************************************/
    /**************************************************/
    var showVotes = function (songId) {
        var vars = { 
            dialogName: 'votes',
            data: {songId: songId}
        };
        Globals.blockmessage.load(vars, function () { });
    };
//    var toggleFacebookOptions = function (song) {
//        var el = $(song).find('div.facebookOptions');
//        if (el.length == 0) {
//            $(song).find('img.smallLike').hide();
//            $(song).find('img.smallLikeLoading').show();
//            var vars = {
//                'SongId': $(song).attr('data-id'),
//                'PlaylistId': Globals.playlistSettings.playlistID,
//                'appId': Globals.appId,
//                'relativePath': $('#hfAbsoluteAppUri').val()
//            };
//            el = $('#songLIFacebookOptionsTemplate').jqote(vars, '*');
//            $(song).find('.facebookOptionsDivider').after(el);
//            var h = $(song).find('div.facebookOptions');
//            $(this).addClass('open');
//        }
//        else {
//            $(el).slideUp(150, function () {
//                $(this).remove();
//            });
//            $(this).removeClass('open');
//        };
//    };
//    this.showFacebookOptions = function (songId) {
//        var song = $('.song[data-id=' + songId + ']');
//        $(song).find('img.smallLike').show();
//        $(song).find('div.facebookOptions').slideDown(200);
//        $(song).find('img.smallLikeLoading').hide();
//    };
    var showMoreSongs = function () {
        $('.showMore a').hide();
        var numOfCurrentSongs = $('ul.playlist.current li').length;
        Globals.ajax.getSongs(function (data) {
            that.renderPlaylist(data.songs, 'Current');
            if (!Boolean(data.areMore)) {
                $('.showMore').hide();
            }
            else {
                $('.showMore a').show();
                $('.showMore').show();
            };
            updatePlaylist(data.playlist);
        }, { startCount: numOfCurrentSongs, sort: that.playlistSortBy, playlistId: Globals.playlistSettings.playlistID });
    };
    this.destroy = function () {
        Globals.playerObservers.removeObserver(chartObserver);
    };
    var chartObserver = function (data) {
        switch (data.state) {
            case -2: //error - couldn't load the song
                Globals.flashmessage.show(Globals.Strings.errorPlayingSong);
                //playing next song
                setTimeout(function () {
                    Globals.flashmessage.hide();
                    Globals.player.playNextSong();
                }, Globals.flashmessage.getTextTimeout() / 2);
                break;
            case 0: //song ended
                $('.song.playing') //update the btn on the list
                    .find('a.player').removeClass('loading play pause').end()
                    .removeClass('playing');
                break;
            case 1: //song started to play
                var song = $('.song[data-id="' + data.id + '"]');
                //update the btn on the list
                song.addClass('playing')
                    .find('a.player').removeClass('loading play').addClass('pause');
                break;
            case 2: //song paused
                $('.song.playing') //update the btn on the list
                    .find('a.player').removeClass('loading pause')
                    .addClass('play');
                break;
            case 3: //buffering
                //will happens only once

                $('div.logo').slideUp(250);
                $('#player').animate({ height: 224 }, 300);
                $('#timeline').slideDown(300);

                $('.song.playing') //update the btn on the list
                    .find('a.player').removeClass('play pause')
                    .addClass('loading');
                break;
            case 6: //start playing next song
                $('.song.playing').find('a.player')
                    .removeClass('play pause loading').end()
                    .removeClass('playing');
                $('.song[data-id=' + data.songId + ']')
                    .find('a.player').addClass('loading').end()
                    .addClass('playing');
                break;
        };
    };
    var updateUser = function (User) {
        $.extend(Globals.User, User);
        $('.points span').html(Globals.User.summedScore); //Updates user's rating        
        var newSongs = Globals.playlistSettings.NumberOfSongs - Globals.User.numOfVotes;
        $('.newSongs span.number').html(newSongs);
        if (newSongs == 0) {
            $('.newSongs').removeClass('new');
        }
        else {
            $('.newSongs').addClass('new');
        };
    };
    var showComments = function (postId) {
        Globals.blockmessage.load({ dialogName: 'showComments', data: { postId: postId} }, function () {
        });
    };
    /**************************************************/
    /**************************************************/
    //Constructor: Bindings - Static and Dynamic
    /**************************************************/
    (function () {
        $('.newSongs').tipsy({ gravity: Globals.IsRtl ? 'e' : 'w',
            fade: true, title: function () {
                var n = Globals.playlistSettings.NumberOfSongs - Globals.User.numOfVotes;
                if (n == 0) {
                    return Globals.Strings.noNewSongs;
                };
                if (n == 1) {
                    return Globals.Strings.newSongs;
                };
                if (n >= 2) {
                    return Globals.Strings.didntVoteFor.format(n);
                };
            }
        });

        toolBar = new chartToolbar();
        Globals.playerObservers.addObserver(chartObserver);

        $('div.back a').livedie('click', function (event) {
            $('div.searchBar').trigger('clear'); //clear search input textbox
            $(this).hide();
            Globals.chart.populatePlaylist(); //show playlist
        });
        $('div.showMore a').livedie('click', function () { //TODO: use ajax
            showMoreSongs();
        });
        $('ul.playlist img.thumbnail').livedie('click', function () {
            var songId = $(this).closest('li.song').attr('data-id');
            play(songId);
        });
        $('ul.playlist a.comments').livedie('click', function () {
            //var postId = $(this).closest('li.song').data('postid');
            //showComments(postId);
        });
        $('ul.playlist a.title').livedie('click', function () {
            var songId = $(this).closest('li.song').attr('data-id');
            play(songId);
        });
        $('ul.playlist a.play').livedie('click', function () {
            var songId = $(this).closest('li.song').attr('data-id');
            play(songId);
        });
        $('ul.playlist a.pause').livedie('click', function () {
            Globals.player.pauseSong();
        });
        $('.song').livedie('mouseenter', function () {
        });
        //hover out on song
        $('.song').livedie('mouseleave', function () {
        });
        //add song from remote search results
        $('button.userVote').livedie('click', function () {
            if ($(this).hasClass('disabled')) {
                return;
            };
            var likeValue;
            var song = $(this).closest('.song');
            if ($(this).hasClass('selected')) {
                $(this).removeClass('selected');
                likeValue = 0;
            }
            else {
                //remove other selected
                $(this).siblings().removeClass('selected');
                $(this).addClass('selected');
                if ($(this).hasClass('love')) {
                    likeValue = 1;
                }
                if ($(this).hasClass('hate')) {
                    likeValue = -1;
                }
            };
            if ($(song).hasClass('SearchResultRemote')) {//add new song + vote
                that.addSong(song);
            }
            else {
                var songId = $(song).attr('data-id');
                that.vote(songId, likeValue);
            };
        });
        $('ul.playlist a.btnDelete').livedie('click', function () {
            var songId = $(this).closest('.song').attr('data-id');
            deleteSong(songId);
        });
        $('ul.playlist a.btnEdit').livedie('click', function () {
            if ($(this).hasClass('disabled')) {
                return;
            }
            var songId = $(this).closest('.song').attr('data-id');
            editSong(songId);
        });
        //Show details of votes on votes click
        $('ul.playlist a.btnVotes').livedie('click', function () {
            if ($(this).hasClass('disabled')) {
                return;
            };
            var songId = $(this).closest('.song').attr('data-id');
            showVotes(songId);
        });
        $('ul.playlist a.btnOptions').livedie('click', function () {
            var song = $(this).closest('.song');
            el = $(song).find('.options ul');
            if (el.length == 0) {
                $(song).find('.options').css('position', 'relative');
                var h = $('#songLIChartOptionsTemplate').jqote(null, '*');
                $(song).find('.options').append(h);
                el = $(song).find('.options ul');
                $(document).one('click', { el: el }, function (event) {
                    event.data.el.remove();
                    $(song).find('.options').css('position', 'static');
                });
            } else {
                $(el).remove();
                $(song).find('.options').css('position', 'static');
            }
        });
    })();                               //End Constructor
};