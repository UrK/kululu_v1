// Globals extension
Globals.chart = new chart();
//set onLoadComplete
Globals.onLoadComplete.addObserver(function (success) {
    if (success) {
        //set username
        $('div.playlist').show();
    }
    else {
    }
    Globals.chart.populatePlaylist();
});
/**************************************************/
/**************************************************/
function playSong() {
    // Play the audio file at url
    var my_media = new Media('http://www.youtube.com/watch?feature=player_detailpage&v=bKQcuHnCvvM',
    // success callback
            function () {
                console.log("playAudio():Audio Success");
            },
    // error callback
            function (err) {
                console.log("playAudio():Audio Error: " + err);
            });
    // Play audio
    my_media.play();
};
$(document).ready(function () {
});