$(document).ready(initTooltips)

function initTooltips(){
    const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="popover"]')
    tooltipTriggerList.forEach(trigger => {
        new bootstrap.Popover(trigger);
    });
}