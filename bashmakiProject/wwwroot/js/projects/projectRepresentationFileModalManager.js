$(".edit-file-btn").each(function(){
    $(this).click(function(e){
        $(e.currentTarget).parent().find(".modal-container").addClass("active");
    })
})

$(".project-representation__modal-close-btn").each(function(){
    $(this).click(function(e){
        $(e.currentTarget).closest(".modal-container").removeClass("active");
    })
})

$(".modal-container").each(function(){
    $(this).click(function(e){
        $(e.currentTarget).removeClass("active");
    })
})

$(".form-container").each(function(){
    $(this).click(function(e){
        e.stopPropagation();
    })
})