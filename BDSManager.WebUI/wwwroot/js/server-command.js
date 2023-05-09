$(document).ready(function () {

    $('.server-command-give-item-search').on('input', function(){
        filterItems(this);
    });
});

function giveCommand(button){
    let path = $(button).attr('data-server-path');
    //aria-labelledby="server-command-give-item"
    let player = $(button).attr('data-player-name');
    let item = $(button).attr('data-item-id-name');
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

function tpCommand(button){
    let path = $(button).attr('data-server-path');
    //aria-labelledby="server-command-give-item"
    let target = $(button).attr('data-player-name');
    let destination = undefined;
    if ($(button).attr('class').includes('player-btn')) 
        destination = $(button).attr('data-destination-player-name');
    
    if (destination == undefined){
        let x = $(button).parent().find('input.server-command-tp-player-x').val();
        let y = $(button).parent().find('input.server-command-tp-player-y').val();
        let z = $(button).parent().find('input.server-command-tp-player-z').val();
        destination = `${x} ${y} ${z}`;
    }
    if (target == undefined && destination == undefined){
        alertToast('Error: Player and Target not defined!')
        return;
    }
    if (target == undefined){
        alertToast('Error: Player not defined!');
        return;
    }
    if (destination == undefined){
        alertToast('Error: Target not defined!');
        return;
    }
    $(`input#server-command-${path}`).val(`tp ${target} ${destination}`);
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

function enchantmentCommand(button){
    let path = $(button).attr('data-server-path');
    let enchant = $(button).attr('data-enchantment-id');
    let level = $(button).attr('data-enchantment-level');
    let player = $(button).attr('data-player-name');

    if (enchant == undefined){
        alertToast('Error: Enchant not defined!');
        return;
    }
    if (level == undefined){
        alertToast('Error: Level not defined!');
        return;
    }
    $(`input#server-command-${path}`).val(`enchant ${player} ${enchant} ${level}`);
    $('body').click();
    $(`input#server-command-${path}`).focus();
}

function effectCommand(button){
    let path = $(button).attr('data-server-path');
    let effect = $(button).attr('data-effect-id');
    let duration = $(button).parent().find('input.server-command-effect-player-duration').val();
    let amplifier = $(button).parent().find('input.server-command-effect-player-amplifier').val();
    let showParticles = $(button).parent().find('input.server-command-effect-player-show-particles').is(':checked');
    let player = $(button).attr('data-player-name');

    if (effect == undefined){
        alertToast('Error: Effect not defined!');
        return;
    }
    if (duration == undefined){
        duration = 30;
    }
    if (amplifier == undefined){
        amplifier = 1;
    }
    $(`input#server-command-${path}`).val(`effect ${player} ${effect} ${duration} ${amplifier} ${showParticles}`);
    $('body').click();
    $(`input#server-command-${path}`).focus();
}