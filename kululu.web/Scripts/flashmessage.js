var flashmessage = function (_el) {
    var options = {
        el: _el, //HTML element
        el_text: 'span.message',
        el_btnClose: 'a.btnClose'
    };
    var btnTimer;
    var textTimer;
    var textTimeout = 15000;
    var that = this;

    this.getTextTimeout = function () {
        return textTimeout;
    }

    this.hide = function () {
        $(options.el).slideUp(50);
    };
    this.show = function (text) {
        clearTimeout(textTimer);
        $(options.el).find(options.el_text).html(text).fadeIn(800);
        $(options.el).slideDown(50);
        setTextTimer();
    };
    var setTextTimer = function () {
        textTimer = setTimeout(function () {
            that.hide();
        }, textTimeout);
    };
    (function () {//constructor
        $(options.el).append("<div class='inner clearfix'></div>");
        $(options.el).find('.inner').append("<span class='message'></span>")
            .append("<a class='btnClose oppositeLanguageFloat'>" + Globals.Strings.close + "</a>");
        $(options.el).find(options.el_btnClose).live('click', function () {
            $(options.el).find(options.el_text).fadeOut(400);
            that.hide();
        });
        $(options.el).live('mouseenter', function () {
            clearTimeout(btnTimer);
            clearTimeout(textTimer);
            $(options.el).find(options.el_btnClose).show().stop().animate({ 'opacity': 1 }, 300);
        });
        $(options.el).live('mouseleave', function () {
            btnTimer = setTimeout(function () {
                $(options.el).find(options.el_btnClose).show().stop().animate({ 'opacity': 0 }, 500);
            }, 500);
            setTextTimer();
        });
    })();
};