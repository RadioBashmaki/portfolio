const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]')
const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl))
tooltipTriggerList.forEach(trigger => {
    trigger.addEventListener("click", preventFocus)
});

function preventFocus(event) {
    console.log("fired")
    if (event.relatedTarget) {
        event.relatedTarget.focus();
    } else {
        this.blur();
    }
}