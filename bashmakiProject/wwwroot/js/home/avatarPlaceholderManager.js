const avatarViewer = document.querySelector(".avatar");

avatarViewer.addEventListener("error", e => {
    e.currentTarget.src = "../../img/svg/avatar-placeholder.svg"
})