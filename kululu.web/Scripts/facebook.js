//Page must reference facebook's all.js script in order for it to work
function facebookUtils() {
    var userPermissions = null;
    var that = this;
    var requiredPermissions;
    var userRequiredPermissions = "publish_stream";
    var adminRequiredPermissions = "publish_stream,manage_pages,offline_access";
    if (Globals.User.IsPageAdmin) {
        requiredPermissions = adminRequiredPermissions;
    }
    else {
        requiredPermissions = userRequiredPermissions;
    };
    //******************************************************************
    var connect = function () {
        FB.getLoginStatus(function (response) {
            if (response.status === 'connected') {
                FB.api('/me/permissions', function (response) {
                    userPermissions = response.data[0];
                });
            };
        });
    };
    this.login = function (isAdmin) {
        Globals.blockmessage.load({ dialogName: 'connect' }, function (response) {
            if (response.success) {
                //show loading message
                Globals.blockmessage.block();
                //TODO: if user does not accept new permissions, response is also returned
                FB.login(function (response) {
                    if (response.authResponse) {
                        Globals.ajax.login(response.authResponse.accessToken, function (success) {
                            if (success) {
                                Globals.ajax.refresh();
                            };
                        });
                    }
                    else {
                        Globals.blockmessage.unblock();
                    }
                }, { scope: isAdmin || Globals.User.IsPageAdmin ? adminRequiredPermissions : userRequiredPermissions });
                return;
            }
        });
    };
    this.isConnected = function () {
        if (Globals.User.role == 'None') { //user is not connected
            return false;
        };
        if (userPermissions == null) {
            return false;
        };
        var pemissionNames = requiredPermissions.split(',');
        for (var index = 0; index < pemissionNames.length; index++) {
            var permission = pemissionNames[index];
            if (userPermissions[permission] == 0) {
                return false;
            };
        };
        return true; //user has all required permissions
    };
    this.getFriends = function (callback) {
        FB.api('/me/friends', function (response) {
            callback(response.data);
        });
    };
    this.isAdmin = function (userId, notAdminCallback) {
        var fqlQuery = 'SELECT uid from page_admin WHERE page_id={0} and uid={1}';
        fqlQuery = fqlQuery.format(Globals.localBusiness.fanPageId, userId);
        FB.api(
            {
                method: 'fql.query',
                query: fqlQuery
            },
            function (response) {
                if (response.length == 0 && notAdminCallback) {
                    notAdminCallback();
                }
            }
        );
    };
    this.init = function () {
        window.fbAsyncInit = function () {
            FB.init({ appId: Globals.appId, oauth: true, cookie: true, xfbml: true });
            $.ajaxSetup({ cache: false }); //why is it here?

            if (Globals.isInIFrame) {
                //FB.Canvas.setAutoGrow();
                $('#main').resize(function () {
                    //console.log('resize');
                    FB.Canvas.setSize({ height: $('#main').innerHeight() }); //resize FB IFrame
                });
            }
            else {
                $('#main').css('overflow', 'auto!important');
            }

            FB.Event.subscribe('auth.sessionChange', function (response) {
                //console.log('session changed: ' + response.status);
            });
            FB.Event.subscribe('auth.logout', function (response) {
                //console.log('logout');
            });
            connect(); //asynchronous connection to FB
            if (Globals.User.role != "None") {
                Globals.onLoadComplete.changed(true);
            }
            else {
                Globals.onLoadComplete.changed(false);
                //Globals.ajax.logout();
            };
        };

        //loading facebook sdk asynchronously
        (function () {
            var e = document.createElement('script'); e.async = true;
            e.src = document.location.protocol +
            '//connect.facebook.net/en_US/all.js';
            document.getElementById('fb-root').appendChild(e);
        } ());
    };
    //******************************************************************
    //******************************************************************
}