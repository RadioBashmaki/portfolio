const $topicDropdownContainer = $("#topic-dropdown-container");
const $topicsChoiceOpen = $("#open-topic-dropdown");

$topicsChoiceOpen.click(function() {
    if ($topicDropdownContainer.hasClass("d-none"))
        $topicDropdownContainer.removeClass("d-none");
    else
        $topicDropdownContainer.addClass("d-none");
})