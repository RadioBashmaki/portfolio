const $topicsChoiceOpen = $(".topics-choice-open");
const $topicDropdownContainer = $(".dropdown-topics-list-container")
const $setFilterButton = $(".search-icon");
const $setFilterForm = $("#set-filter-form");
const $projectsCount = $(".projects-count")
const $projectsContainer = $(".my-projects-list-container__projects-list")

$topicsChoiceOpen.click(function() {
    if ($($topicDropdownContainer)[0].classList.contains("active"))
        $($topicDropdownContainer)[0].classList.remove("active")
    else
        $($topicDropdownContainer)[0].classList.add("active")
})

$setFilterButton.on("click", function(){
    $projectsContainer.empty();
    $projectsContainer.html("<div class='d-flex justify-content-center mt-5 w-100'><span class='loader'></span></div>")
    $.ajax({
        async: true,
        type: "POST",
        data: $setFilterForm.serialize(),
        url: $setFilterForm.attr("action"),
        success: function(partialView) {
            $projectsContainer.html(partialView);
            $projectsCount.html($projectsContainer.find(".my-projects-list-container__project").length)
        }
    })
})



