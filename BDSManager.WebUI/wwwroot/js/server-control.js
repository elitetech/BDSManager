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
    console.log(controlOuput);
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
    console.log(`Server ${path} is ${online ? "online" : "offline"}`);
    $(`#server-status-${path}`).text(online ? "Online" : "Offline");
    $(`#server-start-${path}`).prop("disabled", online);
    $(`#server-stop-${path}`).prop("disabled", !online);
    $(`#server-restart-${path}`).prop("disabled", !online);
}

function updatePlayerCount(path, playerCount){
    console.log(`Server ${path} has ${playerCount} players`);
    $(`#server-player-count-${path}`).text(playerCount);
}

function updatePlayerList(path, playerJson){
    console.log(`Server ${path} has players ${playerJson}`);
    let players = JSON.parse(playerJson);
    let playerList = $(`#server-player-list-${path}`);
    console.log(playerList);
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


