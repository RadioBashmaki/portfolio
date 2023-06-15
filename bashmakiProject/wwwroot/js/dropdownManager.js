const $dropdowns = $(".custom-dropdown");
const $visible = $(".custom-dropdown__visible");
const $invisible = $(".custom-dropdown__invisible");

$visible.each(function(){
    $(this).on("click", function(){
        const $invisible = $(this).next(".custom-dropdown__invisible");
        if (!$invisible.hasClass("active")) {
            $invisible.addClass("active");
        }
    })
})

$dropdowns.each(function(){
    $(this).click(function(e){
        e.stopPropagation();
    })
})

$("html").click(function(){
    $invisible.each(function(){
        $(this).hasClass("active") && $(this).removeClass("active");
    })
})