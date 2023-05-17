let _commandCache = {};

$(document).ready(function () {
    
    $('tr.server-row').click(function () {
        let path = $(this).attr('data-server-path');
        toggleServerDetails(path);
        let alreadyActive = $(this).hasClass('table-secondary');
        $('tr.server-row').removeClass('table-secondary');
        if(!alreadyActive) $(this).addClass('table-secondary');
    });
    $("tr.server-row").on("contextmenu", function (e) {
        e.preventDefault();
        let mousepos = { x: e.pageX, y: e.pageY };
        let path = $(this).attr('data-server-path');
        let online = $(`#server-status-${path}`).text() === "Offline" ? false : true;
        setupServerContextMenu(path, online);
        let css = {
            display: "fixed",
            top: mousepos.y,
            left: mousepos.x
        }
        $("#server-context-menu").css(css).addClass("show");
        return false;
    });

    $('tr.player-row').on("contextmenu", function (e) {
        e.preventDefault();
        setupPlayerContextMenu(this, e);
        return false;
    });

    $('#player-context-menu').on('mouseleave', function () {
        $('#player-context-whitelist').click();
        $('#player-context-permissions').click();
    });

    $(document).click(function (e) {
        // check if e.target is a child of the context menu
        if (!$(e.target).closest("#player-context-menu").length) 
            $("#player-context-menu").removeClass("show");
        $("#server-context-menu").removeClass("show");
    });

    $("#create-button").click(function() {
        $(this).text("Creating...").attr("disabled", "disabled");
        $('form#server-form').submit();
    });

    $("#save-button").click(function() {
        $(this).text("Saving...").attr("disabled", "disabled");
        $('form#server-form').submit();
    });

    setUptime();
    setInterval(function () {
        setUptime();
    }, 1000);

    $('.server-command-send').click(function (e) {
        e.preventDefault();
        let path = $(this).attr('data-server-path');
        let command = $(`#server-command-${path}`).val();
        if (command) {
            sendCommandToHub(path, command);
            $(`#server-command-${path}`).val('');
        }
    });
    
    $('input[id^="server-command-"]').on('keydown', function (e) {
        if (e.code == "Enter" || e.charCode == 13) {
            $(this).parent().find('.server-command-send').click();
            return false;
        }
        if (e.code == "ArrowUp" || e.code == "ArrowDown" || e.charCode == 38 || e.charCode == 40) {
            let path = $(this).attr('data-server-path');
            let command = $(this).val();
            let newCommand = '';
            if (e.code == "ArrowUp" || e.charCode == 38)
                newCommand = getCommandFromCache(path, command, "up");
            else 
                newCommand = getCommandFromCache(path, command, "down");
            
            $(this).val(newCommand);
            return false;
        }
    });
});

function getCommandFromCache(path, command, direction) {
    let commandCache = _commandCache[path];
    if (commandCache) {
        let index = commandCache.indexOf(command);
        if (index === -1)
            index = commandCache.length;
        if (direction === "up") {
            // up arrow 
            if (index > 0) {
                index--;
            }
            return commandCache[index];
        } else {
            // down arrow
            if (index < commandCache.length - 1) {
                index++;
                return commandCache[index];
            }
        }
    }
    return '';
}

function setUptime() {
    let serverRows = $('tr.server-row');
    serverRows.each(function () {
        let status = $(this).find('.server-status');
        if(!status || status.text() == 'Offline') return;

        let uptime = new Date(status.attr('data-server-uptime'));
        let now = new Date();
        let diff = now - uptime;

        const seconds = Math.floor((diff / 1000) % 60);
        if(seconds == undefined || seconds == null || isNaN(seconds)) return;
        const minutes = Math.floor((diff / 1000 / 60) % 60);
        const hours = Math.floor((diff / (1000 * 60 * 60)) % 24);
        const days = Math.floor(diff / (1000 * 60 * 60 * 24));

        let dayString = days > 0 ? `${days}d ` : '';
        let hourString = hours > 0 ? `${hours}h ` : '';
        let minuteString = minutes > 0 ? `${minutes}m ` : '';
        let secondString = `${seconds}s`;

        let uptimeString = `${dayString}${hourString}${minuteString}${secondString}`;
        uptimeString = uptimeString.length > 0 ? `Online (${uptimeString})` : 'Online';

        status.text(uptimeString);
    });
}

function cacheCommand(path, command) {
    if (!_commandCache[path]) {
        _commandCache[path] = [];
    }
    _commandCache[path].push(command);
}

function addAllowPlayerInputGroup() {
    let lastInputGroup = $('.allow-player').last();
    let newInputGroup = lastInputGroup.clone();
    let index = newInputGroup.attr('data-group-index');
    index++;
    newInputGroup.attr('data-group-index', index);
    newInputGroup.find('input').val('');
    newInputGroup.find(`input[name="Server.AllowList[${index - 1}].name"]`).attr('name', `Server.AllowList[${index}].name`);
    newInputGroup.find(`input[name="Server.AllowList[${index - 1}].xuid"]`).attr('name', `Server.AllowList[${index}].xuid`);
    newInputGroup.find(`select[name="Server.AllowList[${index - 1}].ignoresPlayerLimit"]`).attr('name', `Server.AllowList[${index}].ignoresPlayerLimit`).val('false').find('option').removeAttr('selected').last().attr('selected', 'selected');
    if(index > 0) {
        newInputGroup.find('.remove-allow-player').show();
    }
    lastInputGroup.find('.add-allow-player').hide();
    lastInputGroup.after(newInputGroup);
    lastInputGroup.after($("<hr />"));
}

function removeAllowPlayerInputGroup(target) {
    let inputGroup = $(target).closest('.allow-player');
    let index = inputGroup.attr('data-group-index');
    if (index > 0) {
        $(`.allow-player[data-group-index="${index - 1}"]`).find('add-allow-player').show()
        inputGroup.remove();
    }
}

function addPlayerPermissionInputGroup() {
    let lastInputGroup = $('.player-permission').last();
    let newInputGroup = lastInputGroup.clone();
    let index = newInputGroup.attr('data-group-index');
    index++;
    newInputGroup.attr('data-group-index', index);
    newInputGroup.find('input').val('');
    newInputGroup
        .find(`input[name="Server.PlayerPermissions[${index - 1}].xuid"]`)
        .attr('name', `Server.PlayerPermissions[${index}].xuid`);
    newInputGroup
        .find(`select[name="Server.PlayerPermissions[${index - 1}].permission"]`)
        .attr('name', `Server.PlayerPermissions[${index}].permission`)
        .val('visitor')
        .find('option')
        .removeAttr('selected')
        .first()
        .next()
        .attr('selected', 'selected');
    if(index > 0) {
        newInputGroup.find('.remove-player-permission').show();
    }
    lastInputGroup.find('.add-player-permission').hide();
    lastInputGroup.after(newInputGroup);
    lastInputGroup.after($("<hr />"));
}

function removePlayerPermissionInputGroup(target) {
    let inputGroup = $(target).closest('.player-permission');
    let index = inputGroup.attr('data-group-index');
    if (index > 0) {
        $(`.player-permission[data-group-index="${index - 1}"]`).find('.add-player-permission').show();
        inputGroup.remove();
    }
}

function toggleServerDetails(path) {
    let serverDetails = $(`#server-details-${path}`);
    if (serverDetails.is(':visible')) {
        serverDetails.hide();
    } else {
        $('.server-details').hide();
        serverDetails.show();
    }
    
    let consoleOutput = $(`#console-window-${path}`);
    consoleOutput.scrollTop(consoleOutput[0].scrollHeight);
}

function setupServerContextMenu(path, online){
    $("#server-context-menu").attr("data-server-path", path);
    let startServerLink = $('#context-server-start');
    let stopServerLink = $('#context-server-stop');
    let restartServerLink = $('#context-server-restart');
    let backupServerLink = $('#context-server-backup');
    let restoreServerLink = $('#context-server-restore');
    let configureServerLink = $('#context-server-configure');
    let removeServerLink = $('#context-server-remove');
    let updateServerLink = $('#context-server-update');

    startServerLink.prop("disabled", online);
    stopServerLink.prop("disabled", !online);
    restartServerLink.prop("disabled", !online);

    startServerLink.off("click").click(function(e){
        e.preventDefault();
        startServer(path);
    });
    stopServerLink.off("click").click(function(e){
        e.preventDefault();
        stopServer(path);
    });
    restartServerLink.off("click").click(function(e){
        e.preventDefault();
        restartServer(path);
    });
    backupServerLink.off("click").click(function(e){
        e.preventDefault();
        sendCommandToHub(path, "backup");
    });
    restoreServerLink.off("click").click(function(e){
        e.preventDefault();
        getBackupList(path);
    });
    configureServerLink.off("click").click(function(e){
        e.preventDefault();
        window.location.href = `/ManageServer?path=${path}`;
    });
    removeServerLink.off("click").click(function(e){
        e.preventDefault();
        removeServer(path);
    });
    updateServerLink.off("click").click(function(e){
        e.preventDefault();
        checkForUpdates(path);
    });
}

function setupPlayerContextMenu(element, event){
    let mousepos = { x: event.pageX, y: event.pageY };
    let path = $(element).attr('data-server-path');
    let xuid = $(element).attr('data-player-xuid');
    let name = $(element).attr('data-player-name');
    $("#player-context-menu").attr("data-server-path", path);
    $("#player-context-menu").attr("data-player-xuid", xuid);
    $("#player-context-menu").attr("data-player-name", name);
    let kickPlayerLink = $('#context-player-kick');
    let whitelistPlayerLink = $('#context-player-whitelist-add');
    let unwhitelistPlayerLink = $('#context-player-whitelist-remove');
    let permissionsMember = $('#context-player-permissions-member');
    let permissionsOperator = $('#context-player-permissions-operator');

    kickPlayerLink.off("click").click(function(e){
        e.preventDefault();
        sendCommandToHub(path, `kick ${name}`);
    });
    whitelistPlayerLink.off("click").click(function(e){
        e.preventDefault();
        sendCommandToHub(path, `whitelist add ${name}`);
        sendCommandToHub(path, `whitelist reload`);
    });
    unwhitelistPlayerLink.off("click").click(function(e){
        e.preventDefault();
        sendCommandToHub(path, `whitelist remove ${name}`);
        sendCommandToHub(path, `whitelist reload`);
    });
    permissionsMember.off("click").click(function(e){
        e.preventDefault();
        sendCommandToHub(path, `deop ${name}`);
    });
    permissionsOperator.off("click").click(function(e){
        e.preventDefault();
        sendCommandToHub(path, `op ${name}`);
    });
    
    $("#player-context-menu").css({
        display: "fixed",
        top: mousepos.y,
        left: mousepos.x
    }).addClass("show");
}

function startServer(path){
    sendCommandToHub(path, "start");
}

function stopServer(path){
    sendCommandToHub(path, "stop");
}

function restartServer(path){
    sendCommandToHub(path, "restart");
}

function checkForUpdates(path){
    alertToast("Checking for updates...");
    sendCommandToHub(path, "update");
}

function sendCommandToHub(path, command){
    let input = $(`#server-command-${path}`);
    if (!command || command.length === 0) 
        return;
    cacheCommand(path, command);
    sendCommand(path, command);
    input.val("");
}

function appendConsoleOutput(path, output){
    // separate the timestamp from the output
    let timestamp = '';
    if(output.startsWith("[") && output.indexOf("]") > 0){
        timestamp = output.split(']')[0] + "]";
        output = output.replace(timestamp, "").trim();
    }
    let consoleOutput = $(`#console-window-${path}`);
    if(consoleOutput.length === 0)
        return;

    let logElement = $(`
        <li class="console-line list-group-item" data-bs-toggle="tooltip" data-bs-placement="top" title="${timestamp}">
            <span class="console-line-content">${output}</span>
        </li>`);
    consoleOutput.append(logElement);
    if (timestamp.length > 0)
        logElement.tooltip();
    consoleOutput.scrollTop(consoleOutput[0].scrollHeight);
}

function processControlOutput(path, output){
    let controlOuput = output.replace("CONTROL:", "");
    let control = controlOuput.split('|')[0];
    let data = controlOuput.split('|')[1]
    switch (control) {
        case "start-success":
            setOnlineStatus(path, true);
            alertToast("Server started.");
            break;
        case "start-failed":
            setOnlineStatus(path, false);
            alertToast("Server failed to start.");
            break;
        case "stop-success":
            setOnlineStatus(path, false);
            alertToast("Server stopped.");
            break;
        case "stop-failed":
            setOnlineStatus(path, true);
            alertToast("Server failed to stop.");
            break;
        case "player-count-update":
            updatePlayerCount(path,data);
            break;
        case "player-list-update":
            updatePlayerList(path,data);
            break;
        case "update-available":
            alertToast("Performing update...");
            break;
        case "update-not-available":
            alertToast("No update available.");
            break;
        case "update-failed":
            alertToast("Update failed.");
            break;
        case "update-complete":
            alertToast("Update complete.");
            break;
    }
}

function setOnlineStatus(path, online){
    if($(`#server-status-${path}`) === undefined || $(`#server-details-${path}`) === undefined)
        return;
    $(`#server-status-${path}`).text(online ? "Online" : "Offline").attr("data-server-uptime", online ? new Date() : 0);
    $(`#server-details-${path}`).find('#server-command-menu-btn').prop('disabled', !online);
}

function updatePlayerCount(path, playerCount){
    if($(`#server-player-count-${path}`) === undefined)
        return;
    $(`#server-player-count-${path}`).text(playerCount);
}

function updatePlayerList(path, playerJson){
    if($(`#server-player-list-${path}`) === undefined)
        return;
    let players = JSON.parse(playerJson);
    let playerList = $(`#server-player-list-${path}`);
    if(playerList.length > 0)
        playerList.empty();
    players.forEach(player => {
        let lastSeenDate = new Date(player.LastSeen).getDate() == new Date().getDate() ? new Date(player.LastSeen).toLocaleTimeString() : new Date(player.LastSeen).toLocaleDateString();
        let lastSeen = player.Online ? "Now" : lastSeenDate;
        let playerRow = $(`
        <tr class="player-row" data-player-xuid="${player.XUID}" data-player-name="${player.Name}" data-server-path="${path}">
            <td>${player.Name}</td>
            <td>${lastSeen}</td>
        </tr>`);
        if(player.Online)
            $(`li[data-player-name="${player.Name}"]`).removeClass("disabled");
        else
            $(`li[data-player-name="${player.Name}"]`).addClass("disabled");
        playerList.append(playerRow);
        $(playerRow).on("contextmenu", function(e){
            e.preventDefault();
            setupPlayerContextMenu(this, e);
            return false;
        });
    });
    
}

function addAddon(path){
    // make new modal with file picker
    let token = $('input[name="__RequestVerificationToken"]').val();
    let modal = $(`
        <div class="modal fade" id="addon-modal" tabindex="-1" role="dialog" aria-labelledby="addon-modal-label" aria-hidden="true">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="addon-modal-label">Upload Addon</h5>
                    </div>
                    <div class="modal-body">
                        <form id="addon-upload-form" method="post" action="/ManageServer?handler=UploadAddon" enctype="multipart/form-data">
                            <div class="form-group">
                                <label for="addon-file">Addon File</label>
                                <input type="file" class="form-control-file" id="addon-file" name="UploadedFile">
                                <input type="hidden" name="path" value="${path}">
                                <input type="hidden" name="__RequestVerificationToken" value="${token}">
                            </div>
                        </form>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-primary" id="addon-upload-button">Upload</button>
                    </div>
                </div>
            </div>
        </div>`);
    $('body').append(modal);
    $('#addon-modal').modal('show');
    $('#addon-modal').on('hidden.bs.modal', function () {
        // Remove the modal from the DOM when it is hidden
        $(this).remove();
    });
    $('#addon-upload-button').click(function(e){
        e.preventDefault();
        $('#addon-upload-form').submit();
    });
}

function addWorld(path){
    let token = $('input[name="__RequestVerificationToken"]').val();
    let modal = $(`
        <div class="modal fade" id="world-modal" tabindex="-1" role="dialog" aria-labelledby="world-modal-label" aria-hidden="true">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="world-modal-label">Upload World</h5>
                    </div>
                    <div class="modal-body">
                        <form id="world-upload-form" method="post" action="/ManageServer?handler=UploadWorld" enctype="multipart/form-data">
                            <div class="form-group">
                                <label for="world-name">Level Name</label>
                                <input type="text" class="form-control" id="world-name" name="levelName">
                                <label for="world-file">World File</label>
                                <input type="file" class="form-control-file" id="world-file" name="UploadedFile">
                                <input type="hidden" name="path" value="${path}">
                                <input type="hidden" name="__RequestVerificationToken" value="${token}">
                            </div>
                        </form>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-primary" id="world-upload-button">Upload</button>
                    </div>
                </div>
            </div>
        </div>`);
    $('body').append(modal);
    $('#world-modal').modal('show');
    $('#world-modal').on('hidden.bs.modal', function () {
        // Remove the modal from the DOM when it is hidden
        $(this).remove();
    });
    $('#world-upload-button').click(function(e){
        e.preventDefault();
        $('#world-upload-form').submit();
    });
}

function removeWorld(path, worldName){
    let token = $('input[name="__RequestVerificationToken"]').val();
    let modal = $(`
        <div class="modal fade" id="world-modal" tabindex="-1" role="dialog" aria-labelledby="world-modal-label" aria-hidden="true">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="world-modal-label">Remove World</h5>
                    </div>
                    <div class="modal-body">
                        <form id="world-remove-form" method="post" action="/ManageServer?handler=RemoveWorld" enctype="multipart/form-data">
                            <div class="form-group">
                                <input type="hidden" name="levelName" value="${worldName}" readonly>
                                <input type="hidden" name="path" value="${path}">
                                <input type="hidden" name="__RequestVerificationToken" value="${token}">
                            </div>
                            <span class="alert alert-warning" role="alert">Are you sure you want to remove the world "${worldName}"?</span>
                        </form>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-danger" id="world-remove-button">Remove</button>
                    </div>
                </div>
            </div>
        </div>`);
    $('body').append(modal);
    $('#world-modal').modal('show');
    $('#world-modal').on('hidden.bs.modal', function () {
        // Remove the modal from the DOM when it is hidden
        $(this).remove();
    });
    $('#world-remove-button').click(function(e){
        e.preventDefault();
        $('#world-remove-form').submit();
    });
}

function removeServer(path){
    var serverName = $(`tr.server-row[data-server-path="${path}"] td:first-child`).text().trim();
    var token = $('input[name="__RequestVerificationToken"]').val();
    let modal = $(`
        <div class="modal fade" id="server-modal" tabindex="-1" role="dialog" aria-labelledby="server-modal-label" aria-hidden="true">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="server-modal-label">Remove Server</h5>
                    </div>
                    <div class="modal-body text-center">
                        <form id="server-remove-form" method="post" action="/ManageServer?handler=RemoveServer" enctype="multipart/form-data">
                            <div class="form-group">
                                <input type="hidden" name="path" value="${path}">
                                <input type="hidden" name="__RequestVerificationToken" value="${token}">
                            </div>
                            <div class="alert alert-warning" role="alert">Are you sure you want to remove this server?</div>
                            <br />
                            <h3>${serverName} (${path})<h3>
                        </form>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-danger" id="server-remove-button">Remove</button>
                    </div>
                </div>
            </div>
        </div>`);
    $('body').append(modal);
    $('#server-modal').modal('show');
    $('#server-modal').on('hidden.bs.modal', function () {
        // Remove the modal from the DOM when it is hidden
        $(this).remove();
    });
    $('#server-remove-button').click(function(e){
        e.preventDefault();
        $('#server-remove-form').submit();
    });
}

function restoreFromBackup(path, backList){
    let backupOptions = '';
    for(var i = 0; i < backList.length; i++){
        backList[i] = backList[i].split('\\').pop();
        backupOptions += `<option value="${backList[i]}">${backList[i]}</option>`;
    }
    let token = $('input[name="__RequestVerificationToken"]').val();
    let modal = $(`
        <div class="modal fade" id="backup-modal" tabindex="-1" role="dialog" aria-labelledby="backup-modal-label" aria-hidden="true">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="backup-modal-label">Restore From Backup</h5>
                    </div>
                    <div class="modal-body">
                        <form id="backup-restore-form" method="post" action="/ManageServer?handler=RestoreFromBackup" enctype="multipart/form-data">
                            <div class="form-group">
                                <input type="hidden" name="path" value="${path}">
                                <input type="hidden" name="__RequestVerificationToken" value="${token}">
                                <label for="backup-file">Backup File</label>
                                <select class="form-control" id="backup-file" name="backupFileName">
                                    <option value="">Select a backup file</option>
                                    ${backupOptions}
                                </select>
                            </div>
                        </form>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-primary" id="backup-restore-button">Restore</button>
                    </div>
                </div>
            </div>
        </div>`);
    $('body').append(modal);
    $('#backup-modal').modal('show');
    $('#backup-modal').on('hidden.bs.modal', function () {
        // Remove the modal from the DOM when it is hidden
        $(this).remove();
    });
    $('#backup-restore-button').click(function(e){
        e.preventDefault();
        // sumbit form via ajax
        $.ajax({
            url: '/ManageServer?handler=RestoreFromBackup',
            type: 'POST',
            data: {
                path: path,
                backupFileName: $('#backup-file').val(),
                __RequestVerificationToken: token
            },
            success: function(data){
                alertToast(data.message);
            },
            error: function(xhr, status, err){
                console.error(this.props.url, status, err.toString());
                alertToast(err.toString());
            },
            complete: function(){
                $('#backup-modal').modal('hide');
            }            
        });
            
    });
}

function getBackupList(path){
    $.ajax({
        url: '/ManageServer?handler=BackupList',
        type: 'POST',
        data: {
            path: path,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(data){
            restoreFromBackup(path, data);
        }.bind(this),
        error: function(xhr, status, err){
            console.error(this.props.url, status, err.toString());
            restoreFromBackup(path, []);
        }.bind(this)
    });
}

function alertToast(message){
    let toast = $(`<div class="toast" role="alert" aria-live="assertive" aria-atomic="true">
    <div class="toast-header">
        <strong class="mr-auto">Server Controler</strong>
    </div>
    <div class="toast-body">${message}</div>
    </div>`);
    $('#toast-container').append(toast);
    $(toast).toast({delay: 5000}).toast('show');
    $(toast).on('hidden.bs.toast', function () {
        // Remove the toast from the DOM when it is hidden
        $(toast).remove();
    });
}


