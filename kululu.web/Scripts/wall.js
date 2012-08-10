// Wall Prototype
function wall() {
    var that = this;
    $.jqotetag('*');
    var lambda = $.jqotec('#templateWall');
    this.render = function (startCount, callback) {
        Globals.ajax.getSongs(function (data) {
            var h = $.jqote(lambda, data);
            $('ul.wall').append(h);

            if (data.areMore) {
                $('.showMore a').show();
                $('.showMore .loading').hide();
                $('.showMore').show();
            }
            else {
                $('.showMore').hide();
            };
            if (typeof (callback) != 'undefined') {
                return callback();
            };
        }, { startCount: typeof (startCount) == 'undefined' ? 0 : startCount });
    };
    var showMoreSongs = function () {
        $('div.showMore a').hide();
        $('div.showMore .loading').show();
        var numOfCurrentSongs = $('ul.wall li').length;
        that.render(numOfCurrentSongs);
    };
    //Constructor
    (function () {
        $('div.showMore a').livedie('click', function () {
            showMoreSongs();
        });
        $('a.btnShare').livedie('click', function () {
            var post = $(this).closest('.post');
            FB.ui({
                method: 'feed',
                name: post.attr('data-fulltitle'),
                link: "http://www.youtube.com/watch?v=" + post.attr('data-YouTubeId')
            }, function (response) {
                if (response && response.post_id) {
                    //published successfully
                };
            });
        });
    })();
};