//youtube public methods, cannot be contained in a class
function onYouTubePlayerAPIReady() { //IFRAME
    Globals.player.onYouTubePlayerReady();
};
function onYouTubePlayerReady(player) { //FLASH
    Globals.player.onYouTubePlayerReady(player);
};
function onytplayerAStateChange(data) { //FLASH
    Globals.player.onytplayerAStateChange(data);
};
function onytplayerBStateChange(data) { //FLASH
    Globals.player.onytplayerBStateChange(data);
};
function onytplayerError(error) {
    Globals.player.onytplayerError(error);
};
/**************************************************/
/**************************************************/
function player() {
    // Private variables
    var ytplayers = new Array();
    var ytplayersState = new Array();//-1-unstarted 0-ended 1-playing 2-paused 3-buffering 5-cued
    var that = this;
    var mode; //IFRAME or FLASH
    var observers = Globals.playerObservers;
    var ready = 0; //each player adds +1 when is ready
    /**************************************************/
    /**************************************************/
    // Public variables
    this.currentSongLength = 0; //length of current song loaded in the player
    this.playingNow = null; //song id of current playing song

    this.status = function () {
        if (ytplayersState[0] == 1 || ytplayersState[1] == 1) {
            return true; //playing
        }
        return false; // not playing
    };
    this.playlist = {
        songs: [],
        ids: [],
        add: function (songs) {
            var that = this;
            $(songs).each(function () { //update playlist array
                var id = parseInt(this.SongId);
                var indx = that.ids.indexOf(id);
                if (indx == -1) {//new song
                    that.songs.push(this);
                    that.ids.push(id);
                }
                else { //overwrite 
                    that.songs[indx] = this;
                    that.ids[indx] = id;
                };
            });
        },
        clear: function () {
            this.songs = [];
            this.ids = [];
        }
    };
    /**************************************************/
    /**************************************************/
    /** Public Methods  **/
    /** Youtube Callback **/
    this.onYouTubePlayerReady = function(player) { //used only for Flash Player
        _onYouTubePlayerReady(player);
    };
    this.onytplayerAStateChange = function(state) {
        ytplayersState[0] = state;
        onytplayerStateChange(state, 0);
    };
    this.onytplayerBStateChange = function (state) {
        ytplayersState[1] = state;
        onytplayerStateChange(state, 1);
    };
    this.onytplayerError = function(error) {
        _onytplayerError(error);
    };
    /**************************************************/
    this.loadSong = function (songId, callback) { //loads song in available player and start playing after buffering
        //check if el_song is defined

        var indx = that.playlist.ids.indexOf(parseInt(songId));
        if (indx == -1) {
            return callback(false); // song not found TODO: return some error
        };
        playersIsReady(function () {
            var videoID = that.playlist.songs[indx].YouTubeID;
            observers.changed({
                state: 7,
                nextSong: that.playlist.songs[indx].fulltitle,
                language: that.playlist.songs[indx].language
            });

            //        if (el_autoNextSong == null || (el_autoNextSong.find('.VideoID').val() != el_song.find('.VideoID').val())) {//normal loading and playing
            if (ytplayersState[0] != 1 && (ytplayersState[1] == 1 || ytplayersState[1] == 3)) {//player 0 is available and player 1 is busy
                ytplayers[0].loadVideoById(videoID, 0, 'large');
            };
            if ((ytplayersState[0] == 1 || ytplayersState[0] == 3) && ytplayersState[1] != 1) {//player 0 is busy and player 1 is available
                ytplayers[1].loadVideoById(videoID, 0, 'large');
            };
            if (ytplayersState[0] != 1 && ytplayersState[1] != 1) {//both players are available
                ytplayers[0].loadVideoById(videoID, 0, 'large');
            };
            if ((ytplayersState[0] == 1 || ytplayersState[0] == 3) && (ytplayersState[1] == 1 || ytplayersState[1] == 3)) { //both players are busy
                //stop player A first before loading.
                ytplayers[0].stopVideo();
                ytplayers[0].clearVideo();
                ytplayers[0].loadVideoById(videoID, 0, 'large');
            };
            //        }
            //        else {//playing autoNextSong that already loaded
            //            if (ytplayersState[0] == 5 && ytplayersState[1] != 5) {//player 0 is available and player 1 is busy
            //                ytplayers[0].playVideo();
            //            } else { ytplayers[1].playVideo(); }
            //        }
            that.playingNow = parseInt(songId);
            //el_song.addClass('playing');
            if (typeof(callback) != 'undefined') {
                return callback(true);
            }
        });
    };
    /**************************************************/
    this.pauseSong = function () {
      
        if (ytplayersState[0] == 1 || ytplayersState[0] == 3) {//if 0 is busy
            ytplayers[0].pauseVideo();
        };
        if (ytplayersState[1] == 1 || ytplayersState[1] == 3) {
            ytplayers[1].pauseVideo();
        };
        return;
    };
    /**************************************************/
    this.resumeSong = function (callback) {
        if (that.playingNow == null && that.playlist.songs.length > 0) { //load first song
            that.playNextSong(function (success) {
                return callback(success);
            });
        };
        if (ytplayersState[0] == 2) { //if paused
            ytplayers[0].playVideo();
            if (typeof callback != 'undefined') {
                return callback(true);
            }
        }
        else if (ytplayersState[1] == 2) { //if paused
            ytplayers[1].playVideo();
            if (typeof callback != 'undefined') {
                return callback(true);
            }
        }
        else {
            if (typeof callback != 'undefined') {
                return callback(false);
            }
        };
    };
    this.seekTo = function (seekedSecond) {
        if (ytplayersState[0] == 1 || ytplayersState[0] == 2 || ytplayersState[0] == 3) { // if playerA is playing or buffering or paused
            ytplayers[0].seekTo(seekedSecond, true);
        }
        else if (ytplayersState[1] == 1 || ytplayersState[1] == 2 || ytplayersState[1] == 3) { // if playerB is playing or buffering or paused
            ytplayers[1].seekTo(seekedSecond, true);
        };
    };
    this.getCurrentTime = function () {
        if (ytplayersState[0] == 1) { // if playerA is playing
            return ytplayers[0].getCurrentTime();
        }
        else if (ytplayersState[1] == 1) { // if playerB is playing
            return ytplayers[1].getCurrentTime();
        }
        else {
            return false;
        }
    };
    /**************************************************/
    this.playNextSong = function (callback) {
        var indx = that.playlist.ids.indexOf(that.playingNow);
        var indx_next = 0;
        if (indx != -1) {
            indx_next = indx + 1;
            if (indx_next >= that.playlist.ids.length) {
                indx_next = 0;
            };
        };
        if (indx_next == indx) {
            return callback(false)
        };
        var songId = that.playlist.songs[indx_next].SongId;
        observers.changed({ state: 6, songId: songId }); //start playing next song
        that.loadSong(songId, function (success) {
            if (typeof callback != 'undefined') {
                return callback(success);
            }
        });
    };
    /**************************************************/
    /**************************************************/
    /** Private Methods  **/
    var createYTPlayer = function (el) {//IFRAME
        return new YT.Player(el, {
            width: '520',
            height: '224',
            videoId: '',
            playerVars: {
                rel: 0,
                iv_load_policy: 3,
                controls: 0,
                show_info: 0,
                modestbranding: 1,
                origin: "https://www.kulu.lu",
                wmode: 'opaque'
            },
            events: { //TODO: Add onReady
                'onReady': onPlayerReady,
                'onError': onytplayerError,
                'onStateChange': function (event) {
                    ytplayersState[event.target.id - 1] = event.data;
                    onytplayerStateChange(event.data, event.target.id - 1); //0 indexed
                }
            }
        });
    };
    var _onYouTubePlayerReady = function (player) {
        if (mode == 'IFRAME') {
            ytplayers[0] = createYTPlayer("playerAwrapper");
            ytplayers[1] = createYTPlayer("playerBwrapper");
        }
        else { //FLASH

            if (player == "PlayerA") {
                if (typeof ytplayers[0] != 'undefined') {
                    return;
                }
                ready = ready + 1;
                //$('#PlayerA').append("<param name='wmode' value='opaque'>");
                //$('#PlayerA').addParam("wmode", "transparent");
                //var sf = document.getElementById('PlayerA');
                //sf.addParam("wmode", "transparent");

                ytplayers[0] = document.getElementById('PlayerA');
                ytplayers[0].addEventListener("onStateChange", "onytplayerAStateChange");
                ytplayers[0].addEventListener("onError", "onytplayerError");
            };
            if (player == "PlayerB") {
                if (typeof ytplayers[1] != 'undefined') {
                    return;
                }
                ready = ready + 1;
                ytplayers[1] = document.getElementById('PlayerB');
                ytplayers[1].addEventListener("onStateChange", "onytplayerBStateChange");
                ytplayers[1].addEventListener("onError", "onytplayerError");
            };
        };
    };
    /**************************************************/
    var _onytplayerError = function (error) {
        //evoke observer function
        observers.changed({ state: -2 });
    };
    /**************************************************/
    var onPlayerReady = function (event) {//HTML5
        ready = ready + 1;
        event.target.setVolume(100);
    };
    /**************************************************/
    var playersIsReady = function (callback) {
        if (ready == 2) {
            return callback();
        }
        else {
            console.log('wait');
            setTimeout(function () { playersIsReady(callback) }, 2000);
        }
    };
    /**************************************************/
    var onytplayerStateChange = function (state, playerID) {
        //Globals.PlayerObserver.changed(state);
        switch (state) {
            //there is some error in Youtube's javascript API that can make the player get stuck in buffer state, when using seekTo with allowSeekAhead = true        
            case 0: //ended
                observers.changed({ state: 0 });
                that.playNextSong();
                break;
            case 1: //video started to play
                var indx = that.playlist.ids.indexOf(that.playingNow);
                observers.changed({
                    state: 1,
                    songTitle: that.playlist.songs[indx].fulltitle,
                    language: that.playlist.songs[indx].language,
                    id: that.playingNow
                });
                var indx = that.playlist.ids.indexOf(that.playingNow);
                var indx_next = 0;
                if (indx != -1) {
                    indx_next = indx + 1;
                    if (indx_next >= that.playlist.ids.length) {
                        indx_next = 0;
                    };
                };
                if (playerID == 0) { //load the other player
                    $('#playerScreen').removeClass('playerB').addClass('playerA');
                    if (indx_next != indx) {
                        ytplayers[1].cueVideoById(that.playlist.songs[indx_next].YouTubeID, 0, 'large');
                    };
                }
                else {
                    $('#playerScreen').removeClass('playerA').addClass('playerB');
                    if (indx_next != indx) {
                        ytplayers[0].cueVideoById(that.playlist.songs[indx_next].YouTubeID, 0, 'large');
                    };
                };
                that.currentSongLength = ytplayers[playerID].getDuration();
                //stop the other player if is playing or buffering
                var indx = (playerID == 0) ? 1 : 0; //other player index
                if (ytplayersState[indx] == 1 || ytplayersState[indx] == 3) {
                    ytplayers[indx].stopVideo();
                    ytplayers[indx].clearVideo();
                }
                break;
            default:
                observers.changed({ state: state });
                break;
        }
    };
    /**************************************************/
    /**************************************************/
    // Constructor
    (function () {
        var loadScript = function (src) {
            var tag = document.createElement('script');
            tag.src = src;
            var firstScriptTag = document.getElementsByTagName('script')[0];
            firstScriptTag.parentNode.insertBefore(tag, firstScriptTag);
        };
        BrowserDetect.init();
        switch (BrowserDetect.browser) {
            case 'Chrome':
                mode = 'IFRAME';
                break;
            case 'Explorer':
                if (BrowserDetect.version >= 9) {
                    mode = 'IFRAME';
                };
                break;
            case 'Firefox':
                if (BrowserDetect.version >= 4) {
                    mode = 'IFRAME';
                }
                break;
            default:
                mode = 'FLASH';
        };
        if (mode == 'IFRAME') {//IFRAME API - HTML5 
            src = "https://www.youtube.com/player_api";
            loadScript(src);
        }
        else {//Javascript API - FLASH
            var swfUrlStr = "http://www.youtube.com/e/9Xvn_Ku55cI?&enablejsapi=1&version=3&rel=0&iv_load_policy=3&controls=0&showinfo=0&modestbranding=1&playerapiid=";
            var params = {
                allowScriptAccess: "always"
                //wmode: "opaque"
            };
            var atts = { id: "PlayerA" };
            swfobject.embedSWF(swfUrlStr + "PlayerA" + "&cachebuster=" + escape((new Date()).getTime()), "ytapiplayerA", "520", "224", "8", null, null, params, atts, function (e) {
                //TODO: if e.success is false do something.
            });
            atts = { id: "PlayerB" };
            swfobject.embedSWF(swfUrlStr + "PlayerB" + "&cachebuster=" + escape((new Date()).getTime()), "ytapiplayerB", "520", "224", "8", null, null, params, atts, function (e) {
            });
        };
    })();
}