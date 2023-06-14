const $setFilterButton = $("#search-btn");
const $setFilterForm = $("#filter-form");
const $itemsCount = $("#filtered-items-count");
const $filteredItemsContainer = $("#filtered-items-container");

$setFilterButton.on("click", function(){
    $filteredItemsContainer.empty();
    $filteredItemsContainer.html("<div class='d-flex justify-content-center mt-5 w-100'><span class='loader'></span></div>")
    $.ajax({
        async: true,
        type: "POST",
        data: $setFilterForm.serialize(),
        url: $setFilterForm.attr("action"),
        success: function(partialView) {
            $filteredItemsContainer.html(partialView);
            $itemsCount.html($filteredItemsContainer.find(".filtered-item").length);
            initTooltips();
        }
    })
})

