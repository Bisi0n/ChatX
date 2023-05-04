"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();
var now = new Date();
var time = `${now.getHours()}:${now.getMinutes()}`

//var time = `@(DateTime.Now.ToString("h:mm tt"))`;


//Disable the send button until connection is established.
document.getElementById("sendButton").disabled = true;

connection.on("ReceiveMessage", function (user, message) {
    var li = document.createElement("li");
    document.getElementById("messagesList").appendChild(li);
    // We can assign user-supplied strings to an element's textContent because it
    // is not interpreted as markup. If you're assigning in any other way, you 
    // should be aware of possible script injection concerns.
    li.textContent = `${user} ${time}  ${message}`;

    twemoji.parse(li);

    //Emoji addition
    var emojiContainer = document.createElement("div");
    emojiContainer.classList.add("emoji-container");
    var emojis = ["😍", "👍", "😂", "😢"];
    emojis.forEach(emoji => {
        var button = document.createElement("button");
        button.classList.add("emoji-button");
        button.textContent = emoji;
        button.addEventListener("click", function () {
            connection.invoke("AddEmoji", user, message, emoji);
        });
        emojiContainer.appendChild(button);
    });
    li.appendChild(emojiContainer);
});

//connection.on("ReceiveReaction", function (user, message, emoji) {
//    // Find the message element based on the user and message content
//    var messageElements = document.querySelectorAll(`li:contains("${user} ${time} ${message}")`);
//    messageElements.forEach(function (messageElement) {
//        // Create a span element for the emoji
//        var emojiElement = document.createElement("span");
//        emojiElement.classList.add("emoji");
//        emojiElement.innerHTML = emoji;

//        // Insert the emoji element before the emoji-container
//        var emojiContainer = messageElement.querySelector(".emoji-container");
//        emojiContainer.parentNode.insertBefore(emojiElement, emojiContainer);
//    });
//});


connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    var message = document.getElementById("messageInput").value;
    connection.invoke("SendMessage", loggedInUser, message).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});