var _selectorsLive = []; //collects all selectors that are binded with .live(). used in destroy function

(function ($) {
    $.fn.livedie = function (eventType, handler) {
        _selectorsLive.push(this.selector);
        return this.live(eventType, handler);
    };
})(jQuery);
var destroyView = function () {
    $(_selectorsLive).each(function () {
        $(this + "").die();
    });
    _selectorsLive = []; //clear container
    $('#navsub').hide();
    $('.view').remove();
};

var relativePath = Globals.hfAppPath;
$.extend(Globals, {// Globals extension
    flashmessage: new flashmessage($('div#flash')),
    playerObservers: new subscription()
});
Globals.flashmessage.show(Globals.Strings.loading);

var changeView = function (vars, callback) {
    Globals.flashmessage.show(Globals.Strings.loading);
    //TODO: check which is current view
    if (typeof (Globals.chart) != 'undefined') {
        Globals.chart.destroy();
        delete Globals.chart;
    };
    if (typeof (Globals.wall) != 'undefined') {
        delete Globals.wall;
    };
    destroyView();
    $('.content').prepend("<div id=" + vars.view + " class='view group'></div>");
    $('#' + vars.view).load(vars.url, vars.data, function () {
        if (typeof (callback) != 'undefined') {
            callback();
        }
    });
};
var chartInit = function (playlistID, callback) {
    var vars = {
        view: 'chart',
        url: relativePath + '/Playlist/Chart',
        data: { id: playlistID }
    };
    changeView(vars, function () {
        $.extend(Globals, {// Globals extension
            chart: new chart() //TODO: check if has already been initiated
        });
        Globals.chart.populatePlaylist(function () {
            Globals.flashmessage.hide(); //hide loading message
            $('#navsub.chart').show();
            $('#chart').fadeIn(400);
            if (typeof(callback) != 'undefined') {
                return callback();
            };
        });
    });
};
var leaderboardInit = function () {
    var vars = {
        view: 'leaderboard',
        url: relativePath + '/LocalBusiness/GetLeaders',
        data: { localBussinesId: Globals.localBusiness.Id }
    };
    changeView(vars, function () {
        Globals.flashmessage.hide(); //hide loading message
        $('#leaderboard').fadeIn(400);
    });
};
var wallInit = function () {
    var vars = {
        view: 'Wall',
        url: relativePath + '/Playlist/Wall',
        data: null
    };
    changeView(vars, function () {
        $.extend(Globals, {
            wall: new wall()
        });
        Globals.wall.render(0, function () {
            Globals.flashmessage.hide(); //hide loading message
            $('#Wall').fadeIn(400);
        });
    });
};
var guidesInit = function () {
    //New User (not admin) - Before Connected
    if (Globals.User.role == "None") {
        guiders.createGuider({
            id: "welcome",
            next: "connect",
            title: decodeXml(Globals.Strings.guideTitle),
            description: decodeXml(Globals.Strings.guideWelcome),
            buttons: [{ name: decodeXml(Globals.Strings.guideLetsStart), classString: "ui-btnSubmit", onclick: guiders.next}],
            overlay: true,
            width: 350
        }).show();
        guiders.createGuider({
            id: "connect",
            attachTo: 'a#login',
            position: Globals.IsRtl ? 7 : 5,
            offset: { left: Globals.IsRtl ? 15 : -15, top: -10 },
            description: decodeXml(Globals.Strings.guideFirstStage),
            overlay: false,
            width: 200,
            xButton: true
        });
        return;
    };
    //New User (not admin) - Just After Connected
    if (Globals.User.summedScore == 0 && Globals.User.role == 'User') {
        guiders.createGuider({
            id: "newuser",
            next: "search",
            description: decodeXml(Globals.Strings.guideBuckleUp),

            buttons: [{ name: decodeXml(Globals.Strings.guideButtonGo), classString: "ui-btnSubmit", onclick: guiders.next },
                        { name: decodeXml(Globals.Strings.guideButtonCancel), classString: "ui-btnCancel", onclick: guiders.hideAll}],
            overlay: true,
            width: 300
        }).show();
        guiders.createGuider({
            id: "search",
            next: "points",
            attachTo: ".searchBar",
            position: Globals.IsRtl ? 5 : 7,
            offset: { left: Globals.IsRtl ? -35 : 35, top: -15 },
            description: decodeXml(Globals.Strings.guideTutorialStep1),
            buttons: [{ name: decodeXml(Globals.Strings.guideTutorialNext), classString: "ui-btnSubmit", onclick: guiders.next },
                        { name: decodeXml(Globals.Strings.guideTutorialStop), classString: "ui-btnCancel", onclick: guiders.hideAll}]
        });
        guiders.createGuider({
            id: "points",
            next: "newsongs",
            attachTo: ".points",
            position: Globals.IsRtl ? 7 : 5,
            offset: { left: Globals.IsRtl ? -8 : 8, top: -5 },
            description: decodeXml(Globals.Strings.guideTutorialStep2),
            buttons: [{ name: decodeXml(Globals.Strings.guideTutorialNext2), classString: "ui-btnSubmit", onclick: guiders.next },
                        { name: decodeXml(Globals.Strings.guideTutorialStop), classString: "ui-btnCancel", onclick: guiders.hideAll}]
        });
        guiders.createGuider({
            id: "newsongs",
            attachTo: ".newSongs",
            position: Globals.IsRtl ? 9 : 3,
            description: decodeXml(Globals.Strings.guideTutorialStep3),
            buttons: [{ name: decodeXml(Globals.Strings.guideTutorialLetsStart), classString: "ui-btnSubmit", onclick: guiders.hideAll}]
        });
    };
};

Globals.onLoadComplete.addObserver(function (success) {
    chartInit(Globals.playlistSettings.playlistID, function () {
        guidesInit();
    });
});
/**************************************************/
/**************************************************/
$(document).ready(function () {
    //init player
    $.extend(Globals, {
        player: new player() //player class should be declared before chart
    });
    $('div#timeline').timeline();

    $('a#login').live('click', function () {
        guiders.hideAll();
        Globals.facebookUtils.login();
    });
    $('a.feedback').click(function () {
        Globals.blockmessage.load({ dialogName: 'feedback' }, function (response) {
            if (response.success) {
                var vars = {
                    dialogName: 'feedbackThanks',
                    feedback: response.data['comment']
                };
                Globals.blockmessage.load(vars, function () { });
            };
        });
    });

    //parse URLs bbcode in description
    //$("div.playlistDescription span").html(parseBBCode($("div.playlistDescription span").html()));
    $('ul.main_nav>li>a').click(function () {

        $('ul.main_nav>li.selected').removeClass('selected');
        $(this).closest('li').addClass('selected');

        if ($(this).closest('li').hasClass('wall')) {
            wallInit();
        };
        if ($(this).closest('li').hasClass('playlist')) {
            chartInit(Globals.playlistSettings.playlistID);
        };
        if ($(this).closest('li').hasClass('leaderboard')) {
            leaderboardInit();
        };

    });
    $('ul.main_nav>li.playlist').mouseover(function () {
        $(this).find('.dropdown').show();
    });
    $('ul.main_nav>li.playlist').mouseout(function () {
        $(this).find('.dropdown').hide();
    });
    $('ul.main_nav ul.dropdown li a').click(function () {
        $('ul.main_nav>li.selected').removeClass('selected');
        $('ul.main_nav>li.playlist').addClass('selected');
        $('ul.main_nav ul.dropdown').hide();
        $('ul.main_nav ul.dropdown li.selected').removeClass('selected');
        $(this).closest('li').addClass('selected');
        var playlistId = parseInt($(this).closest('li').attr('data-id'));
        chartInit(playlistId);
    });
});