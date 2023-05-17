$(document).ready(function () {
    let tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl)
    });

    $('select').each(function () {
        $(this).find(`option[value="${$(this).attr('value')}"]`).attr('selected', true);
    });
});