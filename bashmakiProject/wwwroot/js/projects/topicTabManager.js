const openTopicDropdown = document.querySelector(".open-topic-dropdown");
const topicDropdownContainer = document.querySelector(".dropdown-topics-list-container");
const $topicCheckboxes = $(document).find(".topic-checkbox");
const $topicTabs = $(document).find(".topic-tab");
const form = document.querySelector(".create-project-form")

openTopicDropdown.addEventListener("click", e => {
    if (topicDropdownContainer.classList.contains("active"))
        topicDropdownContainer.classList.remove("active")
    else
        topicDropdownContainer.classList.add("active")
})

$(document).ready(function () {
    $topicCheckboxes.each(function (i) {
        const topicTab = $($topicTabs[i])[0];
        if (this.checked)
            topicTab.classList.contains("d-none") && topicTab.classList.remove("d-none")
        $(this).click(function (e) {
            e.currentTarget.checked && topicTab.classList.contains("d-none") ?
                topicTab.classList.remove("d-none") :
                topicTab.classList.add("d-none");
        })
    })
})

