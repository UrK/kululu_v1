var w, h;
var ajax;
$(document).ready(function () {
    ajax = new ajax();
    bg();
    DOMEvents();

    $('.btnCreate').click(function () {
        $('.welcome').hide();
        $('.loading').show();
        var pageData = $("#frmRegister").serialize();
        ajax.create(pageData, pageCreated);
    });
});

function pageCreated(playlistView) {
    console.log(playlistView);
    if (playlistView.Status == 0) {
        location.reload();
    }
    else { //error
        console.log('couldnt create new app');
    };
}

function DOMEvents() {
    var resizeTimer;
    $(window).resize(function () {
        clearTimeout(resizeTimer);
        resizeTimer = setTimeout(resizeWindow, 100);
    });
    var resizeWindow = function () {
        w = window.innerWidth;
        h = window.innerHeight;

        if (typeof window.innerWidth != 'undefined') {
            w = window.innerWidth,
            h = window.innerHeight
        }
        // IE6 in standards compliant mode (i.e. with a valid doctype as the first line in the document)
        else if (typeof document.documentElement != 'undefined'
            && typeof document.documentElement.clientWidth !=
            'undefined' && document.documentElement.clientWidth != 0) {
            w = document.documentElement.clientWidth,
            h = document.documentElement.clientHeight
        };
        dx = Math.round((w - Math.round(w / 127) * 127) / 2 + 1);
        dy = Math.round((h - Math.round(h / 127) * 127) / 2 + 1);
        $('#main').css('backgroundPosition', dx + 'px ' + dy + 'px');
        $('#main').css('height', h + 'px');
        $('.block').css('left', Math.round((w * 0.5 - 381 / 2) / 127) * 127 + dx + 'px');
        $('.block').css('top', Math.round((h * 0.5 - 508 / 2) / 127) * 127 + dy + 'px');
        $('canvas#bg').css('left', dx + 'px');
        $('canvas#bg').css('top', dy + 'px');
    };
    resizeWindow();
    //$('#main').show();
};
function bg() {
    var bg;
    var canvasSupported = "HTMLCanvasElement" in window;
    w = window.innerWidth;
    h = window.innerHeight;
    if (canvasSupported) {
        lights = new lights($('canvas#topLights'));
        bg = new background($('canvas#bg'), 1000);
    };
};
function background(el, speed) {
    var loaded = false;
    var bgImg = new Image();
    bgImg.src = hfAppPath + "Content/img/welcome.jpg";
    bgImg.onload = function () {
        loaded = true;
    };
    var tileWidth = 127;
    var tileHeight = 127;
    var timer; //animation
    var canvas = $(el)[0];
    var ctx = canvas.getContext("2d");
    var that = this;
    canvas.width = w;
    canvas.height = h;
    var numTileRows = h / tileHeight;
    var numTileCols = w / tileWidth;
    /***********************************************************/
    //private functions
    var draw = function (full) {//draw rectangles
        if (!loaded) {
            return
        };
        rs = Math.round(Math.random() * (bgImg.height / tileHeight - 1));
        cs = Math.round(Math.random() * (bgImg.width / tileWidth - 1));
        rd = Math.round((numTileRows - 1) * Math.random());
        cd = Math.round((numTileCols - 1) * Math.random());
        var timer;
        ctx.globalAlpha = 0.0;
        timer = setInterval(function () {
            ctx.globalAlpha = ctx.globalAlpha + 1 / 30;
            ctx.drawImage(bgImg, tileWidth * cs, tileHeight * rs, tileWidth - 1, tileHeight - 1, tileWidth * cd, tileHeight * rd, tileWidth - 1, tileHeight - 1);
            if (ctx.globalAlpha >= 0.9) {
                ctx.globalAlpha = 1;
                clearInterval(timer);
            };
        }, speed / 2 / 30);
    };
    (function () {//constructor
        setInterval(draw, speed);
    })();
};
function lights(el) {
    var canvas = $(el)[0];
    var ctx = canvas.getContext("2d");
    //create purple light on top
    var lingrad1 = ctx.createLinearGradient(0, canvas.height / 2, 0, canvas.height);
    var colors = new Array();
    colors.push([0, 0, 255]);
    colors.push([205, 0, 116]);
    colors.push([255, 100, 0]);
    colors.push([0, 0, 255]); //should be cyclic
    var c = [0, 0, 0];
    var colorindx = 0;
    var cs = colors[colorindx];
    var cd = colors[colorindx + 1];
    var duration = 8000; //5secs
    var t = 0;
    var dt = 1000 / 30;
    var timer = setInterval(function () {
        t = t + dt;
        if (t >= duration) {
            t = 0;
            colorindx = colorindx + 1;
            if (colorindx == colors.length - 1) {
                colorindx = 0;
            }
            cs = colors[colorindx];
            cd = colors[colorindx + 1];
        };
        c[0] = Math.round(cs[0] + t / duration * (cd[0] - cs[0]));
        c[1] = Math.round(cs[1] + t / duration * (cd[1] - cs[1]));
        c[2] = Math.round(cs[2] + t / duration * (cd[2] - cs[2]));
        var lingrad1 = ctx.createLinearGradient(0, canvas.height / 2, 0, canvas.height);
        lingrad1.addColorStop(0, 'rgba(' + c[0] + ',' + c[1] + ',' + c[2] + ',0)');
        lingrad1.addColorStop(1, 'rgba(' + c[0] + ',' + c[1] + ',' + c[2] + ',0.8)');
        ctx.fillStyle = lingrad1;
        ctx.clearRect(0, canvas.height / 2, w, canvas.height);
        ctx.fillRect(0, canvas.height / 2, w, canvas.height);
    }, dt);
    //create shading on top
    var lingrad2 = ctx.createLinearGradient(0, 0, 0, canvas.height / 2);
    lingrad2.addColorStop(0, 'rgba(0,0,0,0.7)');
    lingrad2.addColorStop(1, 'rgba(0,0,0,0)');
    ctx.fillStyle = lingrad2;
    ctx.fillRect(0, 0, w, canvas.height / 2);
};