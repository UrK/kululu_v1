// Playlist Prototype
function playlist() {
    var that = this;
    // public variables
    this.playlistActive = false; //is playlist active - before deadline
    this.playlistSortBy = 'Rating';
    /**************************************************/
    // Public Methods
    this.populatePlaylist = function () {
        Globals.ajax.getSongs(function (data, playSongId) {
            $('ul#playlist>li.back').hide();
            $('ul.info').show();
            $('ul#playlist>li.song').remove();
            $(data).each(function () {
                var rating = $('ul#playlist li.Current').length + 1; //TODO: get Rating from server/ajax.js
                if (rating.toString().length == 1) {
                    rating = '0' + rating;
                };
                this.rating = rating;
                h = createSongElement(this);
                $('ul#playlist').append(h);
            });
            $('ul#playlist').listview('refresh');

            if (playSongId != 0) {
                Globals.player.loadSong(playSongId);
            }
        });
    };
    /**************************************************/
    /**************************************************/
    // Private Methods
    var createSongElement = function (song) {
        var h = $('#songLITemplate').jqote(song, '*');
        return h;
    };
    /**************************************************/
    /**************************************************/
    //Constructor: Bindings - Static and Dynamic
    /**************************************************/
    (function () {
    })();
};