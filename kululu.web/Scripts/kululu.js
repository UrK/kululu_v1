/*****************************************
Kululu.js
Main Script
*****************************************/
$(document).ready(function () {
    $.extend(Globals, {
        ajax: new ajax(),
        blockmessage: new blockmessage(),
        facebookUtils: new facebookUtils()
    });
    Globals.facebookUtils.init();
})