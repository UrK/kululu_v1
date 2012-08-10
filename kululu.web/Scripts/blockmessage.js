var blockmessage = function () {
    var options = {
        callback: null,
        offset: 0
    };
    var resize = function () {
        var h = $(document).height();
        var w = $(document).width();
        setOffset(function () {
            $('.blockOverlay').css('height', h).css('width', w);
            $('.blockContainer').css('top', options.offset + 230 + 'px')
            $('.blockContainer').css("left", ($(window).width() - $('.blockContainer').width()) / 2 + "px");
        });
    };
    var setOffset = function (callback) {
        if (Globals.isInIFrame) {
            FB.Canvas.getPageInfo(function (o) {
                options.offset = parseInt(o.scrollTop) - parseInt(o.offsetTop);
                if (options.offset < 0) {
                    options.offset = 0;
                };
                callback();
            });
        };
        callback();
    };

    this.update = function (vars, callback) {
        createDialog(vars, function (el) {
            replace(el, callback);
        });
    }
    this.block = function () {
        show($('#templateDialogue').jqote(null, '*'));
    };
    this.unblock = function () {
        destroy();
    };
    this.load = function (vars, callback) {
        //show loading message
        show($('#templateDialogue').jqote(null, '*'));
        createDialog(vars, function (el) {
            replace(el, callback);
        });
    };
    this.show = function (message, callback) {
        this.load({ dialogName: 'default', message: message }, callback);
    }
    var createDialog = function (vars, callback) {
        //check for input variables
        switch (vars.dialogName) {
            case 'default':
                return callback($('#templateDialogDefault').jqote({ message: vars.message }, '*'));
            case 'connect':
                return callback($('#templateConnect').jqote(null, '*'));
            case 'harvestPreDialog':
                return callback($('#templateHarvestPreDialogue').jqote(null, '*'));
            case 'synchronize':
                return callback($('#templateSynchronizePreDialogue').jqote(null, '*'));
            case 'harvestFBContent':
                var rslt = $('#templateHarvestDialogue').jqote(vars.data, '*');
                return callback(rslt);
            case 'votes':
                Globals.ajax.getVotes(vars.data, function (data) {
                    return callback($('#templateVotesDialogue').jqote(data, '*'));
                });
                break;
            case 'editSong':
                Globals.ajax.getArtistImages(vars.data.artist, 5, function (images) {
                    $.extend(vars.data, { images: images });
                    return callback($('#DialogueTemplate_EditSong').jqote(vars.data, '*'));
                });
                break;
            case 'deleteSong':
                return callback($('#DialogueTemplate_DeleteSong').jqote(null, '*'));
                break;
            case 'addSongClient':
                return callback($('#DialogueTemplate_PostClient').jqote(vars.data, '*'));
                break;
            case 'addSong':
                Globals.ajax.querySongs(vars.data.videoId, function (data) {
                    if (data.length > 0) { //video already exists in playlist
                        return callback($('#DialogueTemplate_PostError').jqote(null, '*'));
                    }
                    else {//video does not exist
                        return callback($('#DialogueTemplate_Post').jqote(vars.data, '*'));
                    };
                });
                break;
            case 'showComments':
                Globals.ajax.getComments(vars.data.postId, function (data) {
                    var h = $('#DialogComments').jqote({ postid: vars.data.postId, comments: data }, '*');
                    return callback(h);
                });
                break;
            case 'songLimit':
                return callback($('#DialogueTemplate_SongLimit').jqote(null, '*'));
            case 'feedback':
                return callback($('#DialogFeedback').jqote(null, '*'));
            case 'feedbackThanks':
                Globals.ajax.SendFeedback(vars.feedback, function (response) {
                    //TODO: check if success
                    return callback($('#DialogFeedbackThanks').jqote(null, '*'));
                });
                break;
            default:
                return callback([]);
        };
    };
    var show = function (el) {
        var h = $(document).height();
        var w = $(document).width();
        $('body').append('<div class="blockOverlay"></div>');
        $('.blockOverlay').css('height', h).css('width', w).fadeIn(100);
        $('body').append("<div class='blockContainer'><div class='blockContent'></div></div>");
        $('.blockContent').append(el);
        resize();
        $('.blockContainer').fadeIn(150);
    };
    var replace = function (el, callback) {
        if (typeof (callback) == 'undefined') {
            options.callback = null;
        }
        else {
            options.callback = callback;
        };
        $('.blockContent').empty().append(el);
        $('.blockContent').find('textarea.elastic').elastic();
        resize();
    };
    var destroy = function (status, data) {
        $('.blockContainer').remove();
        $('.blockOverlay').remove();
        if (options.callback) {
            options.callback({ success: status, data: data });
        };
    };
    //Constructor
    (function () {
        $('.blockContainer').find('.ui-btnSubmit').live('click', function () {
            var data = new Array();
            $('.blockContainer').find(':input').each(function () {
                var name = $(this).attr('name');
                var value = $(this).attr('value')
                if (typeof (name) != 'undefined' &&
                    typeof (value) != 'undefined') {
                    data[name] = value;
                };
            });
            destroy(true, data);
        });
        $('.blockContainer').find('.ui-btnCancel').live('click', function () {
            destroy(false);
        });
        $('.blockContainer').find('.ui-btnSubmit, .ui-btnCancel')
            .live('focus', function () {
                $(this).addClass('selected');
            })
            .live('blur', function () {
                $(this).removeClass('selected');
            });
    })();
};