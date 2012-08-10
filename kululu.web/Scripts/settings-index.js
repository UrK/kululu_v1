$(document).ready(function () {
    Globals.flashmessage = new flashmessage($('div#flash'));

    $('a.tooltip').tipsy({ gravity: 's', fade: true });

    var myTokenInput = $("#friendSelect").tokenInput(null, {
        theme: 'facebook'
    });

    var fbContentInited = false;
    $('.settings-menu a').click(function (e) {
        e.preventDefault();
        if (!fbContentInited) {
            highlightNonAdmins();
            setAutoComplete();
            fbContentInited = true;
        }

        var showItem = $(this).attr('href').replace('#', '');
        $('.infoContainer').hide();
        $('.' + showItem).fadeIn(250);

        $('.settings-menu span').removeClass('selected');
        $(this).parent().addClass('selected');
    });

    $('div.action-options').click(function (e) {
        var toggledMenu;
        if ($(this).next('.action-options-content').is(':visible')) {
            toggledMenu = true;
        }

        hideElement('div.infoContainer ul.info-list div.action-options-content');

        var optionContent = $(this).next('.action-options-content');
        if (toggledMenu) {
            optionContent.hide();
        }
        else {
            optionContent.show();
        }

        //registering a disposable - one time event for closing the options menu
        $(document).one('click', { options: optionContent }, function (event) {
            event.data.options.hide();
        });

        e.preventDefault();
        e.stopPropagation();
    });

    $('.btnAddManager').click(function () {
        var businessId = $('#businessId').val();
        var ownerId = $('#friendSelect').val();
        Globals.ajax.addOwner(businessId, ownerId, function (response) {
            if (response.Status == 0) {
                window.location.reload();
            }
            else {
                Globals.flashmessage.show(Globals.Strings.settingIndex_errorWhenAddingUser);
            }
        });
    });

    $('.managers .remove').click(function () {
        var businessId = $('#businessId').val();
        var ownerId = $(this).attr('id');
        Globals.ajax.removeOwner(businessId, ownerId, function (response) {
            console.log(response);
            if (response.Status == 0) {
                window.location.reload();
            }
            else {
                Globals.flashmessage.show(Globals.Strings.settingIndex_cannotRemoveYourself);
            }
        });
    });

    $('.btnImport').click(function () {
        if ($('#ImportPlaylist_Id').val().length == 0) {
            $('#importPlaylistNotSelectedWarning').show();
            return;
        }
        var shouldImport = true; //$('#cbImportNow').is(":checked");
        if (shouldImport) {
            vars = {
                dialogName: 'harvestPreDialog'
            };
            Globals.blockmessage.load(vars, function (response) {
                if (response.success) {
                    showHarvestDialog(Globals.ajax.importFBStream, response.data['importType']);
                }
            });
        }
    });


    $('.btnSynchronize').click(function () {
        Globals.blockmessage.load({ dialogName: 'synchronize' }, function (response) {
            if (response.success) {
                showHarvestDialog(Globals.ajax.synchronizeFBStream, response.data['importType']);
            }
        });
    });

    $('.btnSaveSettings').click(function () {
        Globals.blockmessage.block();
        $('#loaderSocialSettings').show(); //show loader

        var checkedRadiobtn = $('.socialSettings input:radio:checked');
        var selectValues = $('.socialSettings select');

        var properties = {};

        checkedRadiobtn.each(function () {
            properties[$(this).attr('id')] = $(this).val() == "True";
        });

        selectValues.each(function () {
            properties[$(this).attr('id')] = $(this).val();
        });
        Globals.ajax.setSocialSettings(
                parseInt($('#businessId').val()),
                properties,
                highlightSocialUpdateResponse
            );
    });
});

//************************************************************************************************
//** Extract to some plugin

var getStatusIntervalId;
var getNumOfLikesIntervalId;
var lastImportActionTitle;
var totalNumOfLikes;
var operationComplete;

//*************************


function showHarvestDialog(fbHarvestOperation, importType) {
    operationComplete = false;
    setTimeout(function () {
        //var importType = $('#importType').val();
        fbHarvestOperation(Globals.localBusiness.Id, probeForUpdates, importType);
    }, 1000);

    //initing default values
    getStatusIntervalId = 0;
    getNumOfLikesIntervalId = 0;
    lastImportActionTitle = '';
    totalNumOfLikes = 0;
    operationComplete;

    var vars =
    {
        dialogName: 'harvestFBContent',
        data:
        {
            importTitle: Globals.Strings.settingIndex_harvestInit,
            importLikesComplete: 0,
            importLikesTotal: 0,
            complete: false,
            likeImportPerformed: false
        }
    };

    Globals.blockmessage.load(vars);
}

function updateBlockMesssage(importTitle, importLikesComplete, importLikesTotal, likeImportPerformed, complete) {
    if (operationComplete)
        return;

    var vars =
    {
        dialogName: 'harvestFBContent',
        data:
        {
            importTitle: importTitle,
            importLikesComplete: importLikesComplete,
            importLikesTotal: importLikesTotal,
            complete: complete,
            likeImportPerformed: likeImportPerformed
        }
    };

    Globals.blockmessage.update(vars, function () {
    });
}

function probeForUpdates() {
    getStatusIntervalId = setInterval(
            function () {
                Globals.ajax.getImportStatus(getStatusUpdate)
            }, 2000);
}

function getStatusUpdate(response) {
    if (operationComplete) {
        return;
    }

    if (response.Data.length != '') {
        lastImportActionTitle = response.Data.statusName;
        updateBlockMesssage(lastImportActionTitle, 0, 0, false, false)
    }
    if (response.Data.statusKey == 5) {
        if (getNumOfLikesIntervalId != 0) {
            return;
        }
        getNumOfLikesIntervalId = setInterval(
                function () {
                    Globals.ajax.getImportLikesComplete(getNumOfLikes)
                }, 2000);
    }

    //if harvest finished or failed, exit
    if (response.Data.statusKey == 7 || response.Data.statusKey == -1) {
         lastImportActionTitle = response.Data.statusName;

        clearInterval(getStatusIntervalId);
        clearInterval(getNumOfLikesIntervalId);
        updateBlockMesssage(lastImportActionTitle, totalNumOfLikes, totalNumOfLikes, false, true);
        operationComplete = true;
        return;
    }
}

function getNumOfLikes(response) {
    if (operationComplete) {
        return;
    }

    if (response.Status == 1) {
        totalNumOfLikes = response.Data.TotalNumOfLikes;
        updateBlockMesssage(lastImportActionTitle, response.Data.CompleteLikes, response.Data.TotalNumOfLikes, true, false)
    }
}

//************************************************************************************************


function highlightSocialUpdateResponse(isSuccess) {
    var message = '';
    if (isSuccess) {
        message = Globals.Strings.settingIndex_settingsSaved;
    }
    else {
        message = Globals.Strings.settingIndex_settingsNotSaved;
    }
    Globals.blockmessage.unblock();
    Globals.blockmessage.show(message);
}

function highlightNonAdmins() {
    //apparently, this cannot be achieved
    return;
    var userIds = new Array();
    var rows = new Array();

    $('.infoContainer.managers .info-list input[type=hidden]').each(function () {
        var hfUserId = $(this);
        var row = hfUserId.parent();
        rows.push(row);
        userIds.push(hfUserId.val());
    });

    Globals.ajax.areUsersPageAdmins(Globals.localBusiness.Id,
                                    JSON.stringify(userIds),
                                    rows,
                                    highlightNonAdmin);
}

function highlightNonAdmin(users, rows) {
    for (var index = 0; index < rows.length; index++) {
        var row = rows[index];
        var usrId = row.find('input[type=hidden]').val()
        //if user is not admin
        if (!users[usrId]) {
            var notAdminSpan = $('<div title="' + Globals.Strings.settingIndex_userNotAdminTitle + '">' +
                                Globals.Strings.settingIndex_userNotAdminContent + 
                             '</div>');

            notAdminSpan.addClass('notAdmin');
            row.find('.action-options').removeClass('hidden');
            row.find('.column2').append(notAdminSpan);
        }
    }
}

function setAutoComplete() {
    Globals.facebookUtils.getFriends(
            function (friends) {
                //exlude friends that are already admins
                var currentManagers = new Array();
                $('.userId').each(function () {
                    var id = parseInt($(this).val());
                    currentManagers.push(id);
                });

                friends = $(friends).filter(function () {
                    var inArray = currentManagers.indexOf(parseInt(this.id));
                    return inArray == -1;
                });

                //remove previously set autocomplete with old values
                $('.token-input-list-facebook').remove();

                //create new autocomplete with latest friends data
                $("#friendSelect").tokenInput(friends, {
                    theme: 'facebook'
                });
            });
}

function hideElement(selector) {
    $(selector).each(function () {
        $(this).addClass('hidden');
        $(this).hide();
    })
}