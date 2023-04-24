$(document).ready(function () {
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl)
    });

    $('select').each(function () {
        console.log($(this).attr('value'));
        $(this).find(`option[value="${$(this).attr('value')}"]`).attr('selected', true);
    });
});