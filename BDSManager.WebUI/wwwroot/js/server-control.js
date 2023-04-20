$(document).ready(function () {
    
    $('tr.server-row').click(function () {
        let path = $(this).attr('data-server-path');
        toggleServerDetails(path);
    });
    $("tr.server-row").on("contextmenu", function (e) {
        let mousepos = { x: e.pageX, y: e.pageY };
        let path = $(this).attr('data-server-path');
        let online = $(`#server-status-${path}`).text() === "Online" ? true : false;
        setupContextMenu(path, online);
        $("#server-context-menu").css({
            display: "fixed",
            top: mousepos.y,
            left: mousepos.x
        }).addClass("show");
        return false;
    });

    $(document).click(function () {
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

    // on change
    $("#Server_Options_AllowList").change(function () {
        if($(this).val() === "true") {
            $("#allow-player-list").show();
        } else {        
            $("#allow-player-list").hide();
        }
    });
});

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
}

function setupContextMenu(path, online){
    $("#server-context-menu").attr("data-server-path", path);
    let startServerLink = $('#context-server-start');
    let stopServerLink = $('#context-server-stop');
    let restartServerLink = $('#context-server-restart');
    let configureServerLink = $('#context-server-configure');
    let removeServerLink = $('#context-server-remove');

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
    configureServerLink.off("click").click(function(e){
        e.preventDefault();
        window.location.href = `/ManageServer?path=${path}`;
    });
    removeServerLink.off("click").click(function(e){
        e.preventDefault();
        window.location.href = `/RemoveServer?path=${path}`;
    });
}

function startServer(path){
    sendCommand(path, "start");
}

function stopServer(path){
    sendCommand(path, "stop");
}

function restartServer(path){
    sendCommand(path, "restart");
}

function sendCommandToHub(path){
    let command = $(`#server-command-${path}`).val();
    sendCommand(path, command);
}

function appendConsoleOutput(path, output){
    let consoleOutput = $(`#console-window-${path}`);
    consoleOutput.append(`<li class="console-line list-group-item"><span class="console-line-content">${output}</span></li>`);
    consoleOutput.scrollTop(consoleOutput.scrollHeight);
}

function processControlOutput(path, output){
    let controlOuput = output.replace("CONTROL:", "");
    let control = controlOuput.split('|')[0];
    let data = controlOuput.split('|')[1]
    switch (control) {
        case "start-success":
            setOnlineStatus(path, true);
            break;
        case "start-failed":
            setOnlineStatus(path, false);
            break;
        case "stop-success":
            setOnlineStatus(path, false);
            break;
        case "stop-failed":
            setOnlineStatus(path, true);
            break;
        case "player-count-update":
            updatePlayerCount(path,data);
            break;
        case "player-list-update":
            updatePlayerList(path,data);
            break;
    }
}

function setOnlineStatus(path, online){
    $(`#server-status-${path}`).text(online ? "Online" : "Offline");
}

function updatePlayerCount(path, playerCount){
    $(`#server-player-count-${path}`).text(playerCount);
}

function updatePlayerList(path, playerJson){
    let players = JSON.parse(playerJson);
    let playerList = $(`#server-player-list-${path}`);
    if(playerList.length > 0)
        playerList.empty();

    players.forEach(player => {
        playerList.append(`
            <tr>
                <td>${player.Name}</td>
                <td>${player.XUID}</td>
                <td>${new Date(player.LastSeen).toDateString()}</td>
                <td>${player.Online}</td>
            </tr>`);
    });
    
}

function addAddon(){
    // make new modal with file picker
    let token = $('input[name="__RequestVerificationToken"]').val();
    let modal = $(`
        <div class="modal fade" id="addon-modal" tabindex="-1" role="dialog" aria-labelledby="addon-modal-label" aria-hidden="true">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="addon-modal-label">Upload Addon</h5>
                        <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div class="modal-body">
                        <form id="addon-upload-form" method="post" action="/ManageServer?handler=UploadAddon" enctype="multipart/form-data">
                            <div class="form-group">
                                <label for="addon-file">Addon File</label>
                                <input type="file" class="form-control-file" id="addon-file" name="UploadedFile">
                                <input type="hidden" name="__RequestVerificationToken" value="${token}">
                            </div>
                        </form>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-dismiss="modal">Close</button>
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


