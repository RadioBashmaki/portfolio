const $sendNotificationButtons = $(".send-notification-btn");

$sendNotificationButtons.each(function(){
    bindSendButton($(this));
})

function bindSendButton($btn, $form, $deleteCont){
    $btn.click(function()
    {
        $.ajax({
            async: true,
            type: "POST",
            data: $form.serialize(),
            url: $form.attr("action"),
            success: function() {
                if ($deleteCont){
                    $deleteCont.remove();
                }
            }
        })
    })
}
