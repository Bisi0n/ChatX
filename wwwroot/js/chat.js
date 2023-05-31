const rootElement = document.querySelector('#app');

const app = Vue.createApp({
    data() {
        return {
            connection: null,
            connected: false,
            currentUser: null,
            newChatRoom: '',
            joinedRoomId: null,
            messages: [],
            newMessage: '',
            isTyping: false,
            typingTimeout: null,
            usersCurrentlyTyping: [],
            timeoutDuration: 2500
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

            this.connection.on('DeleteMessage', (id) => {
                this.messages = this.messages.filter(message => message.id !== id);
            });

            this.connection.on('CurrentlyTyping', (usersTyping) => {
                this.usersCurrentlyTyping = usersTyping;
            });

            this.connection.start()
                .then(() => {
                    this.connected = true;
                    this.currentUser = this.connection.connectionId;
                    this.loadChatRooms();
                }).catch((err) => {
                    console.error(err);
                });
        },
        loadPreviousMessages() {
            this.messages = [];
            this.connection.invoke('LoadPreviousMessages', this.joinedRoomId)
                .catch((err) => {
                    console.error(err);
                });
        },
        loadChatRooms() {
            this.connection.invoke('LoadChatRooms')
                .catch((err) => {
                    console.error(err);
                });
        },
        sendMessage() {
            if (!isEmptyOrWhitespace(this.newMessage)) {
                this.connection.invoke('SendMessage', loggedInUser, this.newMessage, this.joinedRoomId)
                    .then(() => {
                        this.newMessage = '';
                    }).catch((err) => {
                        console.error(err);
                    });
            }
        },
        deleteMessage(id) {
            this.connection.invoke('DeleteMessage', id)
                .catch((err) => {
                    console.error(err);
                });
        },
        currentlyTyping() {
            clearTimeout(this.typingTimeout);

            if (!this.isTyping) {
                this.connection.send('UserTyping', loggedInUser, true, this.joinedRoomId)
                    .then(() => {
                        this.isTyping = true;
                    })
                    .catch(err => console.error(err));
            }

            // Set a timeout to detect when the user stops typing
            this.typingTimeout = setTimeout(() => {
                // Send typing indicator to the server indicating the user stopped typing
                this.connection.send('UserTyping', loggedInUser, false, this.joinedRoomId)
                    .then(() => {
                        this.isTyping = false;
                    })
                    .catch(err => console.error(err));
            }, this.timeoutDuration); // Adjust the timeout duration as needed
        },
        createChatRoom() {
            this.connection.invoke('CreateChatRoom', loggedInUser, this.newChatRoom)
                .then(() => {
                    this.newChatRoom = '';
                })
                .catch((err) => {
                    console.log(err);
                });
        },
        joinChatRoom(roomId) {
            if (this.joinedRoomId !== null) {
                this.leaveChatRoom();
            }

            this.connection.invoke('JoinChatRoom', loggedInUser, roomId)
                .then(() => {
                    this.joinedRoomId = roomId;
                    this.loadPreviousMessages();
                    this.announceActivity(true);
                })
                .catch((err) => {
                    console.log(err);
                });
        },
        leaveChatRoom() {
            this.connection.invoke('LeaveChatRoom', this.joinedRoomId)
                .then(() => {
                    this.announceActivity(false);
                    this.joinedRoomId = null;
                    this.messages = [];
                })
                .catch((err) => {
                    console.log(err);
                })
        },
        announceActivity(joined) {
            this.connection.invoke('AnnounceActivity', loggedInUser, this.joinedRoomId, joined)
                .catch((err) => {
                    console.log(err);
                });
        },
        isEmptyOrWhitespace(input) {
            return !input || input.trim().length === 0;
        }
    }
});

app.mount('#app');