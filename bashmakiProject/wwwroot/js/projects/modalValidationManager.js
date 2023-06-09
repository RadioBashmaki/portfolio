const $form = $("form");
const $btn = $(".more-files-btn");
const $newFilesCont = $(".file-forms-container");


$(document).ready(function(){
    $(".add-file-container").each(function(){
        const $modalCont = $(this).find(".modal-container");
        const $editFileBtn = $(this).find(".edit-file-btn");
        $editFileBtn.find(".edit-file-btn__text").html($modalCont.find(".new-file-form__filename").val())
        $modalCont.find(".new-file-form__close-button").remove();
        $modalCont.find(".new-file-form__add-btn").html("Сохранить")
        $modalCont.find(".new-file-form__add-btn").click(e => manageAddBtnClick(e))

        $editFileBtn.click(function(){
            if (!$($modalCont)[0].classList.contains("active"))
                $($modalCont)[0].classList.add("active");
        })
        $editFileBtn.find(".edit-file-btn__delete-button").click(e => manageFileDeletion(e))
        $($editFileBtn)[0].classList.contains("d-none") && $($editFileBtn)[0].classList.remove("d-none");
    })
    $form.removeData("validator")
        .removeData("unobtrusiveValidation");
    $.validator.unobtrusive.parse($form);
})


$btn.click(function() {
    $.ajax({
        async: true,
        type: "GET",
        url: "/projects/addFileFormModal",
        data: { index: $newFilesCont.find(".add-file-container").length.toString() },
        success: function(partialView) {
            $newFilesCont.append(partialView);

            $form.removeData("validator")
                .removeData("unobtrusiveValidation");
            $.validator.unobtrusive.parse($form);
            
            const $addFileCont = $newFilesCont.find(".add-file-container:last-of-type");
            const $modalCont = $addFileCont.find(".modal-container");
            const $editFileBtn = $addFileCont.find(".edit-file-btn");
            const tooltip = new bootstrap.Tooltip($($editFileBtn)[0])
            tooltipList.push(tooltip);
            
            $modalCont.find(".new-file-form__close-button").click(function() {
                $addFileCont.remove();
            })
            
            $modalCont.find(".new-file-form__add-btn").click(e => manageAddBtnClick(e))
            $($modalCont)[0].classList.add("active");
        }
    })
})

function manageAddBtnClick(e){
    const $addFileCont = $(e.currentTarget).closest(".add-file-container");
    const $modalCont = $addFileCont.find(".modal-container");
    const $editFileBtn = $addFileCont.find(".edit-file-btn");
    let valid = true;
    $modalCont.find(".form-control").each(function(){
        if (!$form.validate().element($(this)))
            valid = false;
    })

    if (!valid)
        return;

    const modalCont = $($modalCont)[0]
    if (modalCont.classList.contains("active"))
        modalCont.classList.remove("active");

    $modalCont.find(".new-file-form__close-button").remove();
    $(e.currentTarget).html("Сохранить")


    $editFileBtn.find(".edit-file-btn__text").html($modalCont.find(".new-file-form__filename").val())
    $editFileBtn.click(function(){
        if (!modalCont.classList.contains("active"))
            modalCont.classList.add("active");
    })
    $editFileBtn.find(".edit-file-btn__delete-button").click(e => manageFileDeletion(e))

    $($editFileBtn)[0].classList.remove("d-none");
}

function manageFileDeletion(e){
    const $addFileCont = $(e.currentTarget).closest(".add-file-container");
    e.stopPropagation();
    bootstrap.Tooltip.getInstance($(e.currentTarget).closest(".edit-file-btn")).dispose();
    $addFileCont.remove();
}

