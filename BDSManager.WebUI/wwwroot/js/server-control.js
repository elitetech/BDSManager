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