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

$setFilterForm.on("submit", function(e){
    e.preventDefault();
    $projectsContainer.empty();
    $projectsContainer.html("<div class='d-flex justify-content-center mt-5 w-100'><span class='loader'></span></div>")
    $.ajax({
        async: true,
        type: "POST",
        data: $setFilterForm.serialize(),
        url: $setFilterForm.attr("action"),
        success: function(partialView) {
            $projectsContainer.html(partialView);
            const $innerContainers = $projectsContainer.find(".my-projects-list-container__project");
            $innerContainers.each(function(){
                new bootstrap.Tooltip($(this).find(".my-projects-list-container__project__info")[0]);
            })
            $projectsCount.html($innerContainers.length);
        }
    })
})



