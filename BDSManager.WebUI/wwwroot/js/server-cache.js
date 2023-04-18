const MAX_LOG_HISTORY = 1000;
const MAX_COMMAND_HISTORY = 100;

function ServerCache(path) {
    this._serverPath = path;
    this._console = [];
    this._command = [];
    this._player = [];

    this.AddLog = function (log){
        if (this._console.length > MAX_LOG_HISTORY) 
            this._console.shift();
        
        this._console.push(log);
    }

    this.AddCommand = function (command){
        if (this._command.length > MAX_COMMAND_HISTORY) 
            this._command.shift();
        
        this._command.push(command);
    }

    this.AddPlayer = function (player){
        this._player.push(player);
    }

    this.RemovePlayer = function (player){
        this._player = this._player.filter(p => p.xuid !== player.xuid);
    }

    this.ClearPlayers = function (){
        this._player = [];
    }
}