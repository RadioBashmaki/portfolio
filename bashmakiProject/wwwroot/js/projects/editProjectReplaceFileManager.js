const $replaceFileButton = $(".replace-file-btn");

$replaceFileButton.each(function(){
    $(this).click(function(e){
        const $replacement = $(e.currentTarget).closest(".new-file-form").find(".new-file-form__file-replacement-input");
        $replacement.removeClass("d-none");
        $(e.currentTarget).closest(".new-file-form__file-download").remove();
    })
})