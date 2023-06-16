const $container = $("#notifications-container");
const $btn = $("#query-notifications-btn");

$btn.click(function () {
    if ($container.is(":visible")) {
        $container.html("<div class='d-flex justify-content-center mt-5 w-100'><span class='loader'></span></div>");
        $.ajax({
            async: true,
            type: "GET",
            url: $btn.data("notificationsUrl"),
            success: function (partialView) {
                $container.html(partialView);
                const $reprNotifications = $container.find(".notification.notification-representative");
                if ($reprNotifications.length !== 0){
                    $reprNotifications.each(function(){
                        const $notification = $(this);
                        const $buttons = $notification.find(".notification-button");
                        const $form = $notification.find(".send-notification-form");
                        const $editRequestForm = $notification.find(".edit-request-form");
                        $buttons.each(function(){
                            $(this).click(function(){
                                $form.find(".notification-type-input").val($(this).attr("data-notification-type"))
                                $editRequestForm.find(".request-status-input").val($(this).attr("data-request-status"))
                                $.ajax({
                                    async: true,
                                    method: "POST",
                                    url: "/notifications/delete",
                                    data: {
                                        "id": $form.attr("data-delete-notification-id")
                                    }
                                });
                                $.ajax({
                                    async: true,
                                    method: "POST",
                                    url: $editRequestForm.attr("action"),
                                    data: $editRequestForm.serialize()
                                })
                            })
                            bindSendButton($(this), $form, $notification);
                        })
                    })
                }
            }
        });
    }
})