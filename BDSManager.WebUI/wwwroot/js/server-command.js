$(document).ready(function () {

    $('.server-command-give-item-search').on('input', function(){
        filterItems(this);
    });

    $(".tp-coordinate").on('input', function(){
        let x = $(this).parent().find('input.server-command-tp-player-x').val().length == 0 
            ? 0 
            : $(this).parent().find('input.server-command-tp-player-x').val();
        let y = $(this).parent().find('input.server-command-tp-player-y').val().length == 0
            ? 0
            : $(this).parent().find('input.server-command-tp-player-y').val();
        let z = $(this).parent().find('input.server-command-tp-player-z').val().length == 0
            ? 0
            : $(this).parent().find('input.server-command-tp-player-z').val();

        let tpBtn = $(this).parent().find('button.tp-btn');
        let path = $(tpBtn).attr('data-server-path');
        let player = $(tpBtn).attr('data-player-name');
        $(tpBtn).attr('onclick', `renderCommand('${path}', 'tp', ['${player}', '${x}', '${y}', '${z}'])`);
    });

    $('.effect-input').on('input', function(){
        updateEffectFunctionCall($(this));
    });

    $('.server-command-effect-player-hide-particles').on('click', function(){
        updateEffectFunctionCall($(this).parent());
    });

    $('.server-command-time-set').on('input', function(){
        let time = $(this).val().length == 0
            ? 0
            : $(this).val();
        let timeBtn = $(this).parent().find('button.time-btn');
        let path = $(timeBtn).attr('data-server-path');
        $(timeBtn).attr('onclick', `renderCommand('${path}', 'time set', ['${time}'])`);
    });

    $('.server-command-time-add').on('input', function(){
        let time = $(this).val().length == 0
            ? 0
            : $(this).val();
        let timeBtn = $(this).parent().find('button.time-btn');
        let path = $(timeBtn).attr('data-server-path');
        $(timeBtn).attr('onclick', `renderCommand('${path}', 'time add', ['${time}'])`);
    });
});

function updateEffectFunctionCall(input){
    let duration = $(input).parent().find('input.server-command-effect-player-duration').val().length == 0
        ? 10
        : $(input).parent().find('input.server-command-effect-player-duration').val();
    let amplifier = $(input).parent().find('input.server-command-effect-player-amplifier').val().length == 0
        ? 1
        : $(input).parent().find('input.server-command-effect-player-amplifier').val();
    let hideParticles = !$(input).parent().find('input.server-command-effect-player-hide-particles').prop('checked');

    let effectBtn = $(input).parent().find('button.effect-btn');
    let path = $(effectBtn).attr('data-server-path');
    let player = $(effectBtn).attr('data-player-name');
    let effect = $(effectBtn).attr('data-effect-id');
    $(effectBtn).attr('onclick', `renderCommand('${path}', 'effect', ['${player}', '${effect}', '${duration}', '${amplifier}', '${hideParticles}'])`);
}

function filterItems(input){
    let filter = $(input).val();
    let items = $('li.give-item');
    console.log(filter);
    console.log(items);
    items.each(function(i, item){
        let itemName = $(item).attr('data-item-id-name');
        if(itemName.includes(filter)){
            $(item).show();
        }else{
            $(item).hide();
        }
    });
}

function renderCommand(path, command, args){
    let commandInput = $(`input#server-command-${path}`);
    let argsString = args.join(' ');
    commandInput.val(`${command} ${argsString}`);
    $('body').click();
    commandInput.focus();
}

function clearCommand(path){
    let commandInput = $(`input#server-command-${path}`);
    commandInput.val('').focus();
}