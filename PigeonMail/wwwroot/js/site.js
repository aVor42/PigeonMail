// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

let avatarOnMessageScala = 0.075;

function setMessage(text, sender, imageSource) {

    let imageSize = $("#chat-container").outerHeight() * avatarOnMessageScala;

    let imageContainer = $("<div>").
        addClass("col-md-1").
        append(
            $("<img>").
                css("height", imageSize).
                css("width", imageSize).
                css("border-radius", "100%").
                css("margin-left", "10px").
                css("margin-top", "3px").
                attr("src", imageSource));

    let textContainer = $("<div>").
        addClass("col-md-10").
        append(
            $("<div>").
                addClass("message-container-sender-name").
                text(sender)).
        append(
            $("<div>").
                addClass("message-container-text").
                text(text));

    let messageContainer = $("<div>").
        addClass("message-container").
        append(
            $("<div>").
                addClass("row").
                css("height", "100px").
                append(imageContainer).
                append(textContainer)
    );

    $(".messages-container").append(messageContainer);
}

function goToChat(id) {
    window.location.href = "/Home/Chat?id=" + id;
}

function goToChats() {
    window.location.href = "/Home/Index";
}

function getUserContainer(fullName, id, hasChat) {

    let imageSize = $(window).height() * 0.06;

    let imageContainer = $("<div>").
        addClass("col-md-1").
        append(
            $("<img>").
                css("height", imageSize).
                css("width", imageSize).
                css("border-radius", "100%").
                css("margin-left", "10px").
                css("margin-top", "3px").
                attr("src", "/Users/GetPhoto?id=" + id));

    let button = $("<button>").
        css("height", imageSize / 2.5).
        css("width", imageSize * 4).
        css("font-size", "12px").
        css("padding", "0px").
        addClass("btn").
        addClass("btn-primary").
        click(function () {
            clickAddChat(id);
        }).
        text("Добавить собеседника");

    let textContainer = $("<div>").
        addClass("col-md-10").
        append(
            $("<div>").
                addClass("message-container-sender-name").
                text(fullName))

    if (!hasChat)
        textContainer.append(button);

    let hr = $("<hr>").css("margin", "0px");

    let container = $("<div>").
        addClass("chat-row").
        append(
            $("<div>").
                addClass("row").
                css("height", "100px").
                append(imageContainer).
                append(textContainer)
        ).
        append(hr);

    return container;
}

function search(searchValue) {

    $.get("/Home/SearchUsers",
        {
            searchValue: searchValue
        },
        function (data) {
            for (let i = 0; i < data.length; i++)
                $("#search-results").append(getUserContainer(data[i].FullName, data[i].Id, data[i].HasChat));
        }
    );

    
}


function clickAddChat(id) {
    $.post("/Home/AddChatWithUser",
        {
            id: id
        },
        function (data) {
            alert("Собеседник добавлен");
        }
    ).fail(function (data) {
        alert("Собеседник уже был добавлен ранее");
    });
}

function readAllMessagesFromChat(chatId) {
    $.ajax({
        url: '/Home/SetReadChatMessages',
        type: 'PUT',
        data: "chatId=" + chatId,
        success: function (data) {
        }
    });
}

function setUnreadMessagesCount(chatId, count) {

    let container = $(".unread-messages-count-container").find(`[data-chat-id="${chatId}"]`);
    container.empty();
    let span = $("<span>").
        addClass("unread-messages-count").
        text("Непрочитанных сообщений: " + count);
    container.append(span).attr("data-count", count);

}

function increaseUnreadMessagesCount(chatId) {

    let container = $(".unread-messages-count-container").filter(`[data-chat-id="${chatId}"]`);
    container.empty();
    let count = container.attr("data-count");
    let span = $("<span>").
        addClass("unread-messages-count").
        text("Непрочитанных сообщений: " + (+count + 1));
    container.append(span).attr("data-count", (+count + 1));
}
