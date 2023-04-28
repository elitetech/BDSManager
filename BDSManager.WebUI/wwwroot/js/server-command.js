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

    $(`input#server-command-${path}`).val(`tp ${player} ${target}`);
    $('body').click();
    $(`input#server-command-${path}`).focus();
}