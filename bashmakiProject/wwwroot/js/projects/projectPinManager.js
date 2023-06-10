const $toastContainer = $(".toast-container");
const $projectsCont = $(".my-projects-list-container__projects-list");

$(document).ready(function(){
    const $pinButtons = $(".project__pinned__pinned-mark");
    bindPinEvents($pinButtons);
})

$projectsCont.on("DOMNodeInserted", function(){
    const $pinButtons = $(".project__pinned__pinned-mark");
    bindPinEvents($pinButtons);
})

function bindPinEvents($pinButtons){
    $pinButtons.each(function () {
        $(this).off("click");
        $(this).click(function (e) {
            const cur = $(e.currentTarget);
            cur.addClass("disabled");
            $.ajax({
                url: e.currentTarget.dataset.url,
                type: "POST",
                data: {id: e.currentTarget.dataset.projectId},
                success: function (result) {
                    $pinButtons.removeClass("disabled")
                    result["pinned"] ?
                        !cur.hasClass("pinned") && cur.addClass("pinned")
                        :
                        cur.hasClass("pinned") && cur.removeClass("pinned");

                    const message = result["successful"] ?
                        result["pinned"] ?
                            `Проект "${result["projectTitle"]}" успешно закреплен на стене.`
                            :
                            `Проект "${result["projectTitle"]}" успешно убран со стены.`
                        : `При попытке изменить состояние проекта произошла ошибка`
                    const $toast = $(`<div class="toast primary-text" role="alert" aria-live="assertive" aria-atomic="true"><div class="toast-header justify-content-between">
        <span class="pin-${result["successful"] ? 'success' : 'failure'}"></span>
        <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
    </div>
    <div class="toast-body">${message}</div></div>`);
                    $toastContainer.append($toast);
                    const newToast = new bootstrap.Toast($($toast)[0]);
                    $toast.on("hidden.bs.toast", function(){
                        $toast.remove();
                    })
                    newToast.show();
                }
            })
        })
    })
}
