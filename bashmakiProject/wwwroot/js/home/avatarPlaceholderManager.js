const avatarViewer = document.querySelector(".student-wall-info-container__avatar");

avatarViewer.addEventListener("error", e => {
    e.currentTarget.src = "../../img/svg/avatar-placeholder.svg"
})