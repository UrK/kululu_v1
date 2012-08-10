function chartToolbar() {

    el = $('.toolbar');
    el_filterbar = $('#filterBar');
    el_searchbar = $('div.searchBar');
    el_switchPlaylist = $('div.switchPlaylists');
    el_countdown = $('div.countdown');
    
    (function () {//Constructor
        $(el_filterbar).filterbar();
        $(el_searchbar).searchbar();
        $(el_countdown).countdown();
    })();
};
/**************************************************/
/**************************************************/
(function ($) {
    $.fn.filterbar = function () {
        return this.each(function () {
            var that = this;
            $(this).find('a').click(function () {
                $(that).find('a.selected').removeClass('selected');
                $(this).addClass('selected');
                filter($(this));
            });
        });
    };

    var filter = function (elementClicked) {//TODO: use ajax
        var sortType = elementClicked.attr('id');
        Globals.chart.playlistSortBy = sortType;
        $('ul#filterBar li.loading img').show();
        Globals.chart.populatePlaylist(function(){
            $('ul#filterBar li.loading img').hide();
        });
    };
})(jQuery);
/**************************************************/
/**************************************************/
(function ($) {
    $.fn.searchbar = function () {
        return this.each(function () {
            // private variables
            var elSearchBar = $(this);
            var elInputWrapper = elSearchBar.find('.inputWrapper');
            var elInput = elSearchBar.find('input#search');
            var elFilterBar = $('ul#filterBar'); //hiding filter bar when search bar expands
            var elBtnDelete = elSearchBar.find('a.delete');

            var searchTimer; //used for search timing
            var timerLastSearch; //used to count time since last search, when the timer finishes timeout gets to zero to get fast search results
            var timeout = 0;
            var previousQuery = "";
            var that = this;
            
            $(this).bind('clear', function () {
                previousQuery = "";
                elInput.val('').trigger('blur');
                elBtnDelete.hide();
            });
            elBtnDelete.bind('click', function(event){
                elInput.val('').focus();
                elBtnDelete.hide();
            });
            elInput.keyup(function () {
                elInputWrapper.addClass('loading');
                elBtnDelete.show();
                search($(this).val());
            });
            elInput.focus(function () {
                if (this.value == this.defaultValue) {
                    this.value = '';
                    $(this).removeClass('defaultInput');
                };
            });
            elInput.blur(function () {
                if (this.value == '') {
                    this.value = this.defaultValue;
                    $(this).addClass('defaultInput');
                }
            });
            var search = function (query) {
                if (query == "") {
                    elInputWrapper.removeClass('loading');
                    clearTimeout(searchTimer); //stop any search
                    clearTimeout(timerLastSearch); //stop any search
                    previousQuery = "";
                    //Globals.chart.populatePlaylist();
                    return;
                };
                if (query == previousQuery) {
                    elInputWrapper.removeClass('loading');
                    //console.log('same query. exiting...');
                    return;
                };
                //query is not empty
                $('.showMore').hide();
                $('ul.info').hide();
                $('.back').find('a').show().end().show(); //add back button list-item
                clearTimeout(searchTimer);
                clearTimeout(timerLastSearch);
                timerLastSearch = setTimeout(function () {
                    that.timeout = 0;
                }, 500);

                //TODO: put outside in some central messaging system
                if (Globals.User.role == 'Owner') {
                    Globals.flashmessage.show(Globals.Strings.adminNoLimit);
                }
                else {
                    if (Globals.playlistSettings.NumOfSongsLimit == 0 && Globals.User.role != 'Owner') {
                        //Globals.flashmessage.show('לא ניתן להוסיף שירים לפלייליסט זה.');
                    }

                    if (!Globals.playlistSettings.playlistActive) {
                        Globals.flashmessage.show(Globals.Strings.votingEnded);
                    }
                    else {
                        if (Globals.User.numOfSongsLeft > 0 && Globals.playlistSettings.NumOfSongsLimit > 0) {
                            Globals.flashmessage.show(Globals.Strings.votingRemaining.format(Globals.User.numOfSongsLeft ));
                        }
                    };
                };
                //check if query is not the same as previous
                if (query == previousQuery) {
                    elInputWrapper.removeClass('loading');
                    return;
                };
                previousQuery = query;

                searchTimer = setTimeout(function () {
                    that.timeout = 50;
                    //perform 2 searches at the same time asynchronously
                    Globals.ajax.querySongs(query, function (songs) {//perform local search
                        if (query != previousQuery){
                            return; //current search was invoked in past
                        };
                        Globals.chart.renderPlaylist(songs, 'searchLocal');
                    });

                    if ((Globals.playlistSettings.NumOfSongsLimit > 0 && Globals.playlistSettings.playlistActive) || Globals.User.role == 'Owner') {
                        Globals.ajax.youtubeGetSongs(query, function (data) {//perform Youtube Search
                            if (query != previousQuery){
                                return; //current search was invoked in past
                            };
                            Globals.chart.renderPlaylist(data, 'searchRemote', function(){
                                elInputWrapper.removeClass('loading');
                            });
                        });
                    }
                    else {
                            elInputWrapper.removeClass('loading');
                    };
                }, that.timeout);
            };
        });
    };
})(jQuery);
/**************************************************/
/**************************************************/
(function ($) {
    $.fn.countdown = function (options) {
        var defaults = {
            fromDate: Globals.playlistSettings.timeStamp,
            toDate: Globals.playlistSettings.nextPlayDate,
            intervalDelay: 1000
        };
        var interval = 0;
        var options = $.extend(defaults, options);

        var start = function (element) {
            write(element);
            interval = setInterval(function () { write(element) }, defaults.intervalDelay)
        }

        var stop = function () {
            clearInterval(interval);
        };

        var write = function (element) {
            var inSeconds = 0;
            var inMinutes = 0;
            var inHours = 0;
            var inDays = 0;
            var inYears = 0;

            var toDate = new Date(parseInt(options.toDate));
            var toDate = new Date(Date.parse(toDate));

            var currentDate = new Date(parseInt(options.fromDate));
            currentDate = new Date(Date.parse(currentDate));
            options.fromDate = parseInt(options.fromDate) + parseInt(defaults.intervalDelay);

            var dt = toDate - currentDate.getTime();
            
            if (dt > 0) {
                Globals.playlistSettings.playlistActive = true;
                var inSeconds = Math.floor(dt / 1000 % 60);
                var inMinutes = Math.floor(dt / 1000 / 60 % 60);
                var inHours = Math.floor(dt / 1000 / 60 / 60 % 24);
                var inDays = Math.floor(dt / 1000 / 60 / 60 / 24 % 365);
                var daysSpan = "<span class='days'> {0} {1} </span>".format(inDays, Globals.Strings.days);
                var hoursSpan = "<span class='hours'> {0} {1} </span>".format(inHours, Globals.Strings.hours);
                var mintuesSpan = "<span class='minutes'> {0} {1} </span>".format(inMinutes, Globals.Strings.minutes);
                var secondsSpan = "<span class='seconds'> {0} {1} </span>".format(inSeconds, Globals.Strings.seconds);

                $(element).html('<label> {0} </label>'.format(Globals.Strings.votingWillEndIn));
                $(element).append(daysSpan);
                $(element).append(hoursSpan);
                $(element).append(mintuesSpan);
                $(element).append(secondsSpan);            
            } else {
                Globals.playlistSettings.playlistActive = false;
                $(element).html(Globals.Strings.votingEnded);
                stop();
            }
        }

        var parseDate = function (date) {
            var dateParts = date.split(' ');
            var dateComponent = dateParts[0];
            var timeComponent = dateParts[1];

            var dt1 = dateComponent.substring(0, 2);
            var mon1 = dateComponent.substring(3, 5);
            var yr1 = dateComponent.substring(6, 10);
            var hour = timeComponent.substring(0, 2);
            var minute = timeComponent.substring(3, 5);
            var second = timeComponent.substring(6, 10);
            var temp1 = mon1 + "/" + dt1 + "/" + yr1 + " " + hour + ":" + minute + ":" + second;

            var cfd = Date.parse(temp1);
            var date1 = new Date(cfd);
            return date1;
        }

        return this.each(function () {
            start(this);
        });
    };
})(jQuery);
/**************************************************/
/**************************************************/