/**************************************************/
/**************************************************/
$(document).ready(function () {
    $('a.tooltip').tipsy();
    var cbIncludeDates = $("input#includeMarchDates");
    if (!cbIncludeDates.is(':checked')) {
        hideDates();
    }

    $(".datePicker").datetimepicker(
    {
        altFormat: 'dd/mm/yy',
        dateFormat: 'dd/mm/yy',
        timeFormat: 'hh:mm',
        minDate: 0
    });
    $('a#connect').click(function () {
        Globals.facebookUtils.login(true); //ask admin permissions
    });
    $("input#includeMarchDates").click(function () {
        if ($(this).is(':checked')) { //show dates
            $('#dates').show();
        }
        else {
            hideDates();
        }
    });
});


function hideDates() {
    $('input#Playlist_StartPlayDate').val('');
    $('input#Playlist_NextPlayDate').val('');
    $('#dates').hide();
}