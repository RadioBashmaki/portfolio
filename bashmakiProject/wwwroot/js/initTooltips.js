$(document).ready(initTooltips)

function initTooltips(){
    const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]')
    tooltipTriggerList.forEach(trigger => {
        new bootstrap.Tooltip(trigger);
        trigger.removeEventListener("click", preventFocus);
        trigger.addEventListener("click", preventFocus)
    });
}

function preventFocus(event) {
    if (event.relatedTarget) {
        event.relatedTarget.focus();
    } else {
        this.blur();
    }
}