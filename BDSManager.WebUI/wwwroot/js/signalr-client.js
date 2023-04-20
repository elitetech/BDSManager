const consoleHub = new signalR.HubConnectionBuilder()
    .withUrl("/consoleHub")
    .configureLogging(signalR.LogLevel.Error)
    .build();

const commandHub = new signalR.HubConnectionBuilder()
    .withUrl("/commandHub")
    .configureLogging(signalR.LogLevel.Error)
    .build();


consoleHub.on("updateConsoleOutput", (server, output) => {
    if(output.includes("CONTROL"))
        processControlOutput(server, output);
    else
        appendConsoleOutput(server, output);
});

async function startConsoleHub() {
    try {
        await consoleHub.start();
    } catch (err) {
        console.log(err);
        setTimeout(() => start(), 5000);
    }
}

async function startCommandHub() {
    try {
        await commandHub.start();
    } catch (err) {
        console.log(err);
        setTimeout(() => start(), 5000);
    }
}

async function sendCommand(server, command) { // server is the server's directory name
    server = String(server).padStart(2, '0');
    if (commandHub.state === signalR.HubConnectionState.Connected) {
        commandHub.invoke("SendCommand", server, command)
            .catch((err) => {
                console.error(err.toString());
        });
    }
}

startConsoleHub();
startCommandHub();
