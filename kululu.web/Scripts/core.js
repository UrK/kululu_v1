/**************************************************/
/**************************************************/
// Misc
/**************************************************/
/**************************************************/
if (!Globals.isInIFrame) {
    $('body').removeClass('clearfix');
};

$.extend(Globals, {
    onLoadComplete: new subscription() //observer
});
// Disabling console.log in non-supporting browsers
if (typeof console == "undefined" || typeof console.log == "undefined") var console = { log: function () { } };

// Extending Array prototype for IE
if (!Array.indexOf) {
    Array.prototype.indexOf = function (obj) {
        for (var i = 0; i < this.length; i++) {
            if (this[i] == obj) {
                return i;
            }
        }
        return -1;
    }
}
/**************************************************/
/**************************************************/
// Misc
/**************************************************/
/**************************************************/
$('input, textarea').live('focus click', function () {
    $(this).closest('.inputWrapper').addClass('ui-focus');
});
$('input, textarea').live('blur', function (event) {
    $(this).closest('.inputWrapper').removeClass('ui-focus');
});
$('select').live('focus click', function () {
    $(this).closest('.selectWrapper').addClass('ui-focus');
});
$('select').live('blur', function () {
    $(this).closest('.selectWrapper').removeClass('ui-focus');
});
/**************************************************/
/**************************************************/
// Dialogs functionlality
/**************************************************/
/**************************************************/
$('.postMessage span.description').live('click', function () {
    $(this).hide();
    $('.postMessage div.description .inputWrapper').find('textarea').val($(this).html())
        .end().show()
        .find('textarea').trigger('focus');
});
$('.postMessage .link a').live('click', function () {
    $(this).hide();
    $('.postMessage .link .inputWrapper').find('input').val($(this).html())
        .end().show()
        .find('input').trigger('focus');
});
$('.editSongDetails .img').live('mouseover', function () {
    var dialog = $(this).closest('.editSongDetails');
    var Images = $(dialog).data('images');
    if (!$.isArray(Images)) {
        return;
    }
    if (Images.length <= 1) {
        return;
    };
    $(this).addClass('pointer');
    $(this).find('a').show();

});
$('.editSongDetails .img').live('mouseout', function () {
    $(this).removeClass('pointer');
    $(this).find('a').hide();
});
$('.editSongDetails .img').live('click', function () {
    var dialog = $(this).closest('.editSongDetails');
    var Images = $(dialog).data('images');
    if (!$.isArray(Images)) {
        return;
    }
    if (Images.length <= 1) {
        return;
    };
    var indx = $(dialog).data('imageindx');
    indx = indx + 1;
    if (indx == Images.length) {
        indx = 0;
    };
    $(dialog).data('imageindx', indx);
    $(dialog).find('img').attr('src', Images[indx]);
    $(dialog).find('input.thumbnail').val(Images[indx]);
});
$('ul.comments textarea').live('focus', function () {
    if (this.value == this.defaultValue) {
        this.value = '';
        $(this).removeClass('defaultInput');
    };
});
$('ul.comments textarea').live('blur', function () {
    if (this.value == '') {
        this.value = this.defaultValue;
        $(this).addClass('defaultInput');
    };
});
$('ul.comments textarea').live('keydown', function (e) {
    var postid = $(this).data('postid');
    if (e.keyCode == 13) { //Enter key
        if (this.value == "") {
            return;
        };
        Globals.ajax.addComment(postid, this.value, function () {
        });
    };
});
/**************************************************/
/**************************************************/
// Browser Detect
/**************************************************/
/**************************************************/
var BrowserDetect = {
    init: function () {
        this.browser = this.searchString(this.dataBrowser) || "An unknown browser";
        this.version = this.searchVersion(navigator.userAgent)
			|| this.searchVersion(navigator.appVersion)
			|| "an unknown version";
        this.OS = this.searchString(this.dataOS) || "an unknown OS";
    },
    searchString: function (data) {
        for (var i = 0; i < data.length; i++) {
            var dataString = data[i].string;
            var dataProp = data[i].prop;
            this.versionSearchString = data[i].versionSearch || data[i].identity;
            if (dataString) {
                if (dataString.indexOf(data[i].subString) != -1)
                    return data[i].identity;
            }
            else if (dataProp)
                return data[i].identity;
        }
    },
    searchVersion: function (dataString) {
        var index = dataString.indexOf(this.versionSearchString);
        if (index == -1) return;
        return parseFloat(dataString.substring(index + this.versionSearchString.length + 1));
    },
    dataBrowser: [
		{
		    string: navigator.userAgent,
		    subString: "Chrome",
		    identity: "Chrome"
		},
		{ string: navigator.userAgent,
		    subString: "OmniWeb",
		    versionSearch: "OmniWeb/",
		    identity: "OmniWeb"
		},
		{
		    string: navigator.vendor,
		    subString: "Apple",
		    identity: "Safari",
		    versionSearch: "Version"
		},
		{
		    prop: window.opera,
		    identity: "Opera"
		},
		{
		    string: navigator.vendor,
		    subString: "iCab",
		    identity: "iCab"
		},
		{
		    string: navigator.vendor,
		    subString: "KDE",
		    identity: "Konqueror"
		},
		{
		    string: navigator.userAgent,
		    subString: "Firefox",
		    identity: "Firefox"
		},
		{
		    string: navigator.vendor,
		    subString: "Camino",
		    identity: "Camino"
		},
		{		// for newer Netscapes (6+)
		    string: navigator.userAgent,
		    subString: "Netscape",
		    identity: "Netscape"
		},
		{
		    string: navigator.userAgent,
		    subString: "MSIE",
		    identity: "Explorer",
		    versionSearch: "MSIE"
		},
		{
		    string: navigator.userAgent,
		    subString: "Gecko",
		    identity: "Mozilla",
		    versionSearch: "rv"
		},
		{ 		// for older Netscapes (4-)
		    string: navigator.userAgent,
		    subString: "Mozilla",
		    identity: "Netscape",
		    versionSearch: "Mozilla"
		}
	],
    dataOS: [
		{
		    string: navigator.platform,
		    subString: "Win",
		    identity: "Windows"
		},
		{
		    string: navigator.platform,
		    subString: "Mac",
		    identity: "Mac"
		},
		{
		    string: navigator.userAgent,
		    subString: "iPhone",
		    identity: "iPhone/iPod"
		},
		{
		    string: navigator.platform,
		    subString: "Linux",
		    identity: "Linux"
		}
	]
};

/**************************************************/
/**************************************************/
// Observer
/**************************************************/
/**************************************************/
//implementing a super simple version of the observer pattern, where there is only a single subject
//and all subjects are methods, with no parameter

function subscription() {
    var observers = new Array();
    this.publicObservers = function () {
        return observers;
    };

    this.addObserver = function (observer) {
        observers.push(observer);
    }

    this.removeObserver = function (observer) {
        var index = observers.indexOf(observer);
        if (index == -1) throw new Error("Observer not found");
        observers.splice(index, 1);
    }

    this.changed = function (options) {
        notifyObservers(options);
        return;
    }

    var notifyObservers = function (options) {
        for (var i = 0, observer; observer = observers[i]; i++) {
            observer(options);
        }
    }
};

/**************************************************/
/**************************************************/
// Resize Event
/**************************************************/
/**************************************************/
/*
* jQuery resize event - v1.1 - 3/14/2010
* http://benalman.com/projects/jquery-resize-plugin/
* 
* Copyright (c) 2010 "Cowboy" Ben Alman
* Dual licensed under the MIT and GPL licenses.
* http://benalman.com/about/license/
*/
(function ($, h, c) { var a = $([]), e = $.resize = $.extend($.resize, {}), i, k = "setTimeout", j = "resize", d = j + "-special-event", b = "delay", f = "throttleWindow"; e[b] = 250; e[f] = true; $.event.special[j] = { setup: function () { if (!e[f] && this[k]) { return false } var l = $(this); a = a.add(l); $.data(this, d, { w: l.width(), h: l.height() }); if (a.length === 1) { g() } }, teardown: function () { if (!e[f] && this[k]) { return false } var l = $(this); a = a.not(l); l.removeData(d); if (!a.length) { clearTimeout(i) } }, add: function (l) { if (!e[f] && this[k]) { return false } var n; function m(s, o, p) { var q = $(this), r = $.data(this, d); r.w = o !== c ? o : q.width(); r.h = p !== c ? p : q.height(); n.apply(this, arguments) } if ($.isFunction(l)) { n = l; return m } else { n = l.handler; l.handler = m } } }; function g() { i = h[k](function () { a.each(function () { var n = $(this), m = n.width(), l = n.height(), o = $.data(this, d); if (m !== o.w || l !== o.h) { n.trigger(j, [o.w = m, o.h = l]) } }); g() }, e[b]) } })(jQuery, this);

/**************************************************/
/**************************************************/
// StringFormat
/**************************************************/
/**************************************************/
function _StringFormatInline() {
    var txt = this;
    for (var i = 0; i < arguments.length; i++) {
        var exp = new RegExp('\\{' + (i) + '\\}', 'gm');
        txt = txt.replace(exp, arguments[i]);
    }
    return txt;
}

function _StringFormatStatic() {
    for (var i = 1; i < arguments.length; i++) {
        var exp = new RegExp('\\{' + (i - 1) + '\\}', 'gm');
        arguments[0] = arguments[0].replace(exp, arguments[i]);
    }
    return arguments[0];
}

if (!String.prototype.format) {
    String.prototype.format = _StringFormatInline;
}

if (!String.format) {
    String.format = _StringFormatStatic;
}

var xml_special_to_escaped_one_map = {
    '&': '&amp;',
    '"': '&quot;',
    '<': '&lt;',
    '>': '&gt;',
    "'" : '&#39;'
};

var escaped_one_to_xml_special_map = {
    '&amp;': '&',
    '&quot;': '"',
    '&lt;': '<',
    '&gt;': '>',
    '&#39;' : "'"
};

function encodeXml(string) {
    return string.replace(/([\&"<>'])/g, function (str, item) {
        return xml_special_to_escaped_one_map[item];
    });
};

function decodeXml(string) {
    return string.replace(/(&quot;|&lt;|&gt;|&amp;|&#39;)/g,
        function (str, item) {
            return escaped_one_to_xml_special_map[item];
        });
}
/**************************************************/
/**************************************************/
// jQuery elastic
/**************************************************/
/**************************************************/
/**
*	@name							Elastic
*	@descripton						Elastic is jQuery plugin that grow and shrink your textareas automatically
*	@version						1.6.11
*	@requires						jQuery 1.2.6+
*
*	@author							Jan Jarfalk
*	@author-email					jan.jarfalk@unwrongest.com
*	@author-website					http://www.unwrongest.com
*
*	@licence						MIT License - http://www.opensource.org/licenses/mit-license.php
*/

(function ($) {
    jQuery.fn.extend({
        elastic: function () {

            //	We will create a div clone of the textarea
            //	by copying these attributes from the textarea to the div.
            var mimics = [
				'paddingTop',
				'paddingRight',
				'paddingBottom',
				'paddingLeft',
				'fontSize',
				'lineHeight',
				'fontFamily',
				'width',
				'fontWeight',
				'border-top-width',
				'border-right-width',
				'border-bottom-width',
				'border-left-width',
				'borderTopStyle',
				'borderTopColor',
				'borderRightStyle',
				'borderRightColor',
				'borderBottomStyle',
				'borderBottomColor',
				'borderLeftStyle',
				'borderLeftColor'
				];

            return this.each(function () {

                // Elastic only works on textareas
                if (this.type !== 'textarea') {
                    return false;
                }

                var $textarea = jQuery(this),
				$twin = jQuery('<div />').css({
				    'position': 'absolute',
				    'display': 'none',
				    'word-wrap': 'break-word',
				    'white-space': 'pre-wrap'
				}),
				lineHeight = parseInt($textarea.css('line-height'), 10) || parseInt($textarea.css('font-size'), '10'),
				minheight = parseInt($textarea.css('height'), 10) || lineHeight * 3,
				maxheight = parseInt($textarea.css('max-height'), 10) || Number.MAX_VALUE,
				goalheight = 0;

                // Opera returns max-height of -1 if not set
                if (maxheight < 0) { maxheight = Number.MAX_VALUE; }

                // Append the twin to the DOM
                // We are going to meassure the height of this, not the textarea.
                $twin.appendTo($textarea.parent());

                // Copy the essential styles (mimics) from the textarea to the twin
                var i = mimics.length;
                while (i--) {
                    $twin.css(mimics[i].toString(), $textarea.css(mimics[i].toString()));
                }

                // Updates the width of the twin. (solution for textareas with widths in percent)
                function setTwinWidth() {
                    var curatedWidth = Math.floor(parseInt($textarea.width(), 10));
                    if ($twin.width() !== curatedWidth) {
                        $twin.css({ 'width': curatedWidth + 'px' });

                        // Update height of textarea
                        update(true);
                    }
                }

                // Sets a given height and overflow state on the textarea
                function setHeightAndOverflow(height, overflow) {

                    var curratedHeight = Math.floor(parseInt(height, 10));
                    if ($textarea.height() !== curratedHeight) {
                        $textarea.css({ 'height': curratedHeight + 'px', 'overflow': overflow });
                    }
                }

                // This function will update the height of the textarea if necessary 
                function update(forced) {

                    // Get curated content from the textarea.
                    var textareaContent = $textarea.val().replace(/&/g, '&amp;').replace(/ {2}/g, '&nbsp;').replace(/<|>/g, '&gt;').replace(/\n/g, '<br />');

                    // Compare curated content with curated twin.
                    var twinContent = $twin.html().replace(/<br>/ig, '<br />');

                    if (forced || textareaContent + '&nbsp;' !== twinContent) {

                        // Add an extra white space so new rows are added when you are at the end of a row.
                        $twin.html(textareaContent + '&nbsp;');

                        // Change textarea height if twin plus the height of one line differs more than 3 pixel from textarea height
                        if (Math.abs($twin.height() + lineHeight - $textarea.height()) > 3) {

                            var goalheight = $twin.height() + lineHeight;
                            if (goalheight >= maxheight) {
                                setHeightAndOverflow(maxheight, 'auto');
                            } else if (goalheight <= minheight) {
                                setHeightAndOverflow(minheight, 'hidden');
                            } else {
                                setHeightAndOverflow(goalheight, 'hidden');
                            }

                        }

                    }

                }

                // Hide scrollbars
                $textarea.css({ 'overflow': 'hidden' });

                // Update textarea size on keyup, change, cut and paste
                $textarea.bind('keyup change cut paste', function () {
                    update();
                });

                // Update width of twin if browser or textarea is resized (solution for textareas with widths in percent)
                $(window).bind('resize', setTwinWidth);
                $textarea.bind('resize', setTwinWidth);
                $textarea.bind('update', update);

                // Compact textarea on blur
                $textarea.bind('blur', function () {
                    if ($twin.height() < maxheight) {
                        if ($twin.height() > minheight) {
                            $textarea.height($twin.height());
                        } else {
                            $textarea.height(minheight);
                        }
                    }
                });

                // And this line is to catch the browser paste event
                $textarea.bind('input paste', function (e) { setTimeout(update, 250); });

                // Run update once when elastic is initialized
                update();

            });

        }
    });
})(jQuery);