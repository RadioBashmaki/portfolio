const avatarViewer = document.querySelector(".profile-form__avatar-viewer");
const avatarInput = document.querySelector(".profile-form__avatar-input");

avatarViewer.addEventListener("error", e => {
    e.currentTarget.src = "../../img/svg/avatar-placeholder.svg"
})

avatarViewer.addEventListener("click", e => {
    avatarInput.click()
})

avatarInput.addEventListener("change", e => {
    const reader = new FileReader();
    reader.onload = function(){
        avatarViewer.src = reader.result;
    };
    reader.readAsDataURL(e.currentTarget.files[0]);
})