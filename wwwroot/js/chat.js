<<<<<<< Updated upstream
﻿const app = Vue.createApp({
    data() {
        return {
            connection: null,
            connected: false,
            currentUser: null,
            messages: [],
            newMessage: ''
        };
    },
    mounted() {
        this.connect();
    },
    methods: {
        connect() {
            const url = '/chatHub';
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl(url)
                .withAutomaticReconnect()
                .build();

            this.connection.on('ReceiveMessageHistory', (messages) => {
                this.messages = messages;
            });

            this.connection.on('ReceiveMessage', (message) => {
                this.messages.push(message);
            });
=======
﻿"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();
var now = new Date();
var year = now.getFullYear();
var month = now.getMonth();
var day = now.getDay();
var hour = now.getHours();
var minutes = now.getMinutes();
var fullDate = `${year}-${month}-${day} ${hour}:${minutes}`;
var counter = 0;

>>>>>>> Stashed changes

            this.connection.on('DeleteMessage', (id) => {
                this.messages = this.messages.filter(message => message.id !== id);
            });

<<<<<<< Updated upstream
            this.connection.start().then(() => {
                this.connected = true;
                this.currentUser = this.connection.connectionId;
                this.loadPreviousMessages();
            }).catch((err) => {
                console.error(err);
            });
        },
        loadPreviousMessages() {
            this.connection.invoke('LoadPreviousMessages').catch((err) => {
                console.error(err);
            });
        },
        sendMessage() {
            this.connection.invoke('SendMessage', loggedInUserName, loggedInUser, this.newMessage).then(() => {
                this.newMessage = '';
            }).catch((err) => {
                console.error(err);
            });
        },
        deleteMessage(id) {
            this.connection.invoke('DeleteMessage', id).catch((err) => {
                console.error(err);
            });
        }
    },
});

app.mount('#app');
=======
connection.on("ReceiveMessage", function (user, message, messageId) {
    var li = document.createElement("li");
    document.getElementById("messagesList").appendChild(li);
    // We can assign user-supplied strings to an element's textContent because it
    // is not interpreted as markup. If you're assigning in any other way, you 
    // should be aware of possible script injection concerns.
    li.textContent = `${user} (${fullDate}): ${message}`;
    var currentUser = document.getElementById("user").value;
    li.id = messageId;

    if (user == currentUser) {
        messageId = 'userMsg';
        li.id = messageId;
        var deleteButton = document.createElement('button');
        deleteButton.className = 'delete-mark';
        deleteButton.addEventListener('click', function () {
            connection.invoke("DeleteMessage", li.id).catch(function (err) {
                console.error(err.toString());
            });
        });
        li.appendChild(deleteButton);
    }   
});

connection.on("DeleteMessage", function (messageId) {
    var li = document.getElementById(messageId);
    if (li) {
        li.remove();
    }
});

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    var user = document.getElementById("user").value;
    var message = document.getElementById("messageInput").value;
    var messageId = 'msg';
    connection.invoke("SendMessage", user, message, messageId).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});
>>>>>>> Stashed changes
