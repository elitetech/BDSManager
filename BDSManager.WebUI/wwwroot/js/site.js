$(document).ready(function () {

    // show server-details on tr click
    $('tr.server-row').click(function () {
        let path = $(this).attr('id');

        let serverDetails = $(`#server-details-${path}`);
        if (serverDetails.is(':visible')) {
            serverDetails.hide();
        } else {
            hideAllServerDetails();
            serverDetails.show();
        }
    });
});

function hideAllServerDetails() {
    $('.server-details').hide();
}