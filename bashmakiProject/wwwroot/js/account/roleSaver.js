const roleRadioButtons = document.querySelectorAll(".register-form__custom-radio-cont");
const companyCont = document.querySelector(".register-form__company-cont");

roleRadioButtons.forEach(btn => btn.addEventListener("click", e => {
    const input =  e.currentTarget.querySelector(".custom-radio");
    if (input.value === "Representative")
        companyCont.classList.remove("d-none");
    else
        !companyCont.classList.contains("d-none") && companyCont.classList.add("d-none")
}))
