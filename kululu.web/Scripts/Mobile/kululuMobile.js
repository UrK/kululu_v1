/*****************************************
Kululu.js
Main Script
*****************************************/
//Prototypes & Classes

var User = { //User prototype
    name: null,
    role: 'None',
    FBID: 0,
    numOfSongsLeft: -1,
    score: 0,
    email: null
}
var Globals = {
    User: User,
    playlistID: parseInt($('input#hfCurrentPlaylistId').val()),
    onLoadComplete: new subscription(), //observer
    ajax: new ajax(),
    facebookUtils: new facebookUtils(),
    Browser: null
};

/**************************************************/
/**************************************************/
$(document).ready(function () {
    var vars = {
        'playlistId': 1,
        'type': 0
    };

    //init functions and bindings on load - global only!
    $('a#login').live('click', function () {
        Globals.facebookUtils.login();
    });
    $('input').live('focus click', function () {
        $(this).closest('.inputWrapper').addClass('ui-focus');
    });
    $('input').live('blur', function () {
        $(this).closest('.inputWrapper').removeClass('ui-focus');
    });
    $('select').live('focus click', function () {
        $(this).closest('.selectWrapper').addClass('ui-focus');
    });
    $('select').live('blur', function () {
        $(this).closest('.selectWrapper').removeClass('ui-focus');
    });
    //doesn't exist...TODO
    $('.clearInput').click(function () {
        var p = $(this).parent().find('.textinput');
        p.val(p.attr('defaultValue'));
        $(this).hide();
        p.addClass('defaultInput');
        p = $(this).parent().find('img');
        p.attr('src', '../img/genericMaven.png');
        p.css('margin-top', '0');
        p.css('width', '38px');
        p.css('height', '38px');
    });
});