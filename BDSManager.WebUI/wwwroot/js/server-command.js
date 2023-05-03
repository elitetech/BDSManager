$(document).ready(function () {

    $('.server-command-give-item-search').on('input', function(){
        filterItems(this);
    });
});

function giveCommand(button){
    let path = $(button).attr('data-server-path');
    //aria-labelledby="server-command-give-item"
    let player = $('button[aria-labelledby="server-command-give-item"][aria-expanded="true"]').attr('data-player-name');
    let item = $('button[aria-labelledby="server-command-give-amount"][aria-expanded="true"]').attr('data-item-id-name');
    let amount = $(button).attr('data-item-amount');
    if (player == undefined && item == undefined){
        alertToast('Error: Player and Item not defined!')
        return;
    }
    if (player == undefined){
        alertToast('Error: Player not defined!');
        return;
    }
    if (item == undefined){
        alertToast('Error: Item not defined!');
        return;
    }
    $(`input#server-command-${path}`).val(`give ${player} ${item} ${amount}`);
    $('body').click();
    $(`input#server-command-${path}`).focus();
}

function filterItems(input){
    let filter = $(input).val();
    let items = $('button[aria-labelledby="server-command-give-amount"]');
    items.each(function(i, item){
        let itemName = $(item).attr('data-item-id-name');
        if(itemName.includes(filter)){
            $(item).show();
        }else{
            $(item).hide();
        }
    });
}

function tpCommand(button){
    let path = $(button).attr('data-server-path');
    //aria-labelledby="server-command-give-item"
    let player = $('button[aria-labelledby="server-command-tp-player"][aria-expanded="true"]').attr('data-player-name');
    let target = undefined;
    if ($(button).attr('class').includes('player-btn')) 
        target = $(button).attr('data-player-name');
    
    if (target == undefined){
        let x = $(button).parent().find('input.server-command-tp-player-x').val();
        let y = $(button).parent().find('input.server-command-tp-player-y').val();
        let z = $(button).parent().find('input.server-command-tp-player-z').val();
        target = `${x} ${y} ${z}`;
    }
    if (player == undefined && target == undefined){
        alertToast('Error: Player and Target not defined!')
        return;
    }
    if (player == undefined){
        alertToast('Error: Player not defined!');
        return;
    }
    if (target == undefined){
        alertToast('Error: Target not defined!');
        return;
    }
    $(`input#server-command-${path}`).val(`tp ${player} ${target}`);
    $('body').click();
    $(`input#server-command-${path}`).focus();
}

function timeSetCommand(button){
    let path = $(button).attr('data-server-path');
    let time = $(button).attr('data-time');

    if (time == undefined){
        time = $(button).parent().find('input.server-command-time-set').val();
    }

    if (time == undefined){
        alertToast('Error: Ticks/time not defined!');
        return;
    }
    $(`input#server-command-${path}`).val(`time set ${time}`);
    $('body').click();
    $(`input#server-command-${path}`).focus();
}

function timeAddCommand(button){
    let path = $(button).attr('data-server-path');
    let time = $(button).parent().find('input.server-command-time-add').val();
    

    if (time == undefined){
        alertToast('Error: Ticks not defined!');
        return;
    }
    $(`input#server-command-${path}`).val(`time add ${time}`);
    $('body').click();
    $(`input#server-command-${path}`).focus();
}

function timeQueryCommand(button){
    let path = $(button).attr('data-server-path');
    let time = $(button).attr('data-query');

    if (time == undefined){
        alertToast('Error: Query not defined!');
        return;
    }
    $(`input#server-command-${path}`).val(`time query ${time}`);
    $('body').click();
    $(`input#server-command-${path}`).focus();
}

function weatherSetCommand(button){
    let path = $(button).attr('data-server-path');
    let weather = $(button).attr('data-weather');

    if (weather == undefined){
        alertToast('Error: Weather not defined!');
        return;
    }
    $(`input#server-command-${path}`).val(`weather ${weather}`);
    $('body').click();
    $(`input#server-command-${path}`).focus();
}
