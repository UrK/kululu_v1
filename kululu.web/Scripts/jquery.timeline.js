(function ($) {
    $.fn.timeline = function () {
        this.each(function () {
            return _timeline(this);
        });
    };
    var _timeline = function (el) {
        /**************************************************/
        /**************************************************/
        // private variables
        var defaults = {
            el_timeline: $(el).find('.timeline'),
            el_innertimeline: $(el).find('.fulltime'),
            el_title: $(el).find('span.title'),
            el_titleWrapper: $(el).find('.titleWrapper'),
            el_btnPlay: $(el).find('.btnPlayerPlay'),
            el_btnPause: $(el).find('.btnPlayerPause'),
            el_btnVote: $(el).find('.btnVote'),
            el_btnNext: $(el).find('.btnPlayerNext'),
            el_slider: $(el).find('div.slider'),
            el_movingBar: $(el).find('span.time'),
            el_startTime: $(el).find('.startTime'),
            el_stopTime: $(el).find('.stopTime'),
            el_showOnTarget: $('div.header') //when mouse enters this target show the timeline
        }
        var sliderDrag = false; //indication if timeline is dragged
        var updateTimer;
        /**************************************************/
        /**************************************************/
        defaults.el_showOnTarget.mouseover(function () {
            defaults.el_timeline.show();
            $('#playerScreen').addClass('cut');
        }).mouseleave(function () {
            defaults.el_timeline.hide();
            $('#playerScreen').removeClass('cut');
        });
        defaults.el_btnPlay.click(function () {
            Globals.player.resumeSong(function (status) {
                if (status) {
                    defaults.el_btnPlay.hide();
                    defaults.el_btnPause.show();
                };
            });
        });
        defaults.el_btnVote.click(function () {
            var likeValue = parseInt($(this).attr('data-value')) + 1;
            if (likeValue = 2) {
                likeValue = -1;
            };
            $(this).attr('data-value', likeValue);
            var songId = Globals.player.playingNow;
            Globals.chart.vote(songId, likeValue);
        });
        defaults.el_btnPause.click(function () {
            Globals.player.pauseSong();
        });
        defaults.el_btnNext.click(function () {
            Globals.player.playNextSong();
        });
        defaults.el_slider.mousedown(function () { //dregging slider event
            sliderDrag = true;
            if (Globals.player.playingNow == null) { //nosong
                return;
            };
            var slider_mousemove_handler = function (e) {
                var relX = e.pageX - defaults.el_innertimeline.offset().left;
                setPlayingTime(relX);
            };
            var slider_mouseup_handler = function () {
                sliderDrag = false;
                $(document).unbind('mousemove', slider_mousemove_handler);
                $(document).unbind('mouseup', slider_mouseup_handler);
            };
            $(document).bind('mousemove', slider_mousemove_handler);
            $(document).bind('mouseup', slider_mouseup_handler);
        }).onselectstart = function () {
            return false;
        };
        defaults.el_timeline.mousedown(function (e) {
            clearInterval(updateTimer);
            if (Globals.player.playingNow == null) { //nosong
                updateTimer = setInterval(updatePlayingTime, 1000); //resume timer
                return;
            }
            var parentOffset = defaults.el_innertimeline.offset();
            var relX = e.pageX - parentOffset.left;
            setPlayingTime(relX, function () {
                updateTimer = setInterval(updatePlayingTime, 1000); //resume timer
            });
        });
        var setPlayingTime = function (currentPosition, callback) {
            var completePercentage = currentPosition / defaults.el_innertimeline.width();
            if (completePercentage < 0) { //lower limiting
                completePercentage = 0;
            }
            if (completePercentage > 1) { //upper limiting
                completePercentage = 1;
            }
            var seekedSecond = Math.ceil(completePercentage * Globals.player.currentSongLength);
            Globals.player.seekTo(seekedSecond);
            defaults.el_movingBar.css('width', completePercentage * 100 + "%");
            defaults.el_slider.css('left', completePercentage * 100 + "%");
            if (typeof callback != 'undefined') {
                callback();
            }
        };
        var updatePlayingTime = function () {
            var t = Globals.player.getCurrentTime();
            if (t) {
                var formatTime = function (t) {
                    var minutes = Math.floor(t / 60);
                    var seconds = Math.floor(t - minutes * 60);
                    if (seconds <= 9) { //zero padding
                        seconds = '0' + seconds.toString();
                    };
                    return { minutes: minutes, seconds: seconds };
                };
                if (!sliderDrag) { // if slider is dragged don't update timeline sliders
                    defaults.el_movingBar.css('width', t / Globals.player.currentSongLength * 100 + "%");
                    defaults.el_slider.css('left', t / Globals.player.currentSongLength * 100 + "%");
                }
                o = formatTime(t); //format time
                defaults.el_startTime.text(o.minutes + ':' + o.seconds);
                var stopTime = Math.round(Globals.player.currentSongLength) - t;
                if (stopTime < 0) { //sometimes this might happen because second timing isn't completely synchronized with youtube player
                    return;
                }
                o = formatTime(stopTime);
                defaults.el_stopTime.text('-' + o.minutes + ':' + o.seconds);
            };
        };
        var timelineObserver = function (data) {
            switch (data.state) {
                case -2: //error - couldn't load the song
                    break;
                case 0: //song ended
                    defaults.el_title.html("");
                    defaults.el_btnPause.hide();
                    defaults.el_btnPlay.show();
                    break;
                case 1: //song started to play
                    defaults.el_title.html(data.songTitle);
                    defaults.el_titleWrapper.removeClass('english hebrew').addClass(data.language);
                    defaults.el_btnPlay.hide();
                    defaults.el_btnPause.show();
                    break;
                case 2: //song paused
                    defaults.el_btnPause.hide();
                    defaults.el_btnPlay.show();
                    break;
                case 3: //buffering
                    defaults.el_titleWrapper.removeClass('english hebrew').addClass('hebrew');
                    defaults.el_title.html("טוען...");
                    break;
                case 6: //start playing next song
                    break;
                case 7: //loading song
                    defaults.el_titleWrapper.removeClass('english hebrew').addClass('hebrew');
                    defaults.el_title.html("טוען: " + data.nextSong);
                    break;
            };
        };
        /** Constructor **/
        (function () {
            updateTimer = setInterval(updatePlayingTime, 1000);
            Globals.playerObservers.addObserver(timelineObserver);
        })();
    };
})(jQuery);
