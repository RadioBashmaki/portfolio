const steps = document.querySelectorAll(".form-step");
const $form = $(steps[0]).closest("form");
const nextButtons = document.querySelectorAll(".btn-next");
const prevButtons = document.querySelectorAll(".btn-prev");

let currentStep = 0;

nextButtons.forEach(btn => btn.addEventListener("click",  e => {
    if (currentStep < steps.length) {
        if (!validateForm($form)) return;
        currentStep++;
        setActiveStep();
    }
}))

prevButtons.forEach(btn => btn.addEventListener("click",  e => {
    if (currentStep > 0) {
        currentStep--;
        setActiveStep();
    }
}))

function setActiveStep() {
    steps[currentStep].classList.add("form-step-active");
    steps.forEach((st, i) => i !== currentStep && st.classList.contains("form-step-active") 
        && st.classList.remove("form-step-active"));
}

function validateForm($form) {
    $.validator.unobtrusive.parse($form);
    $form.validate();

    return $form.valid();
}




