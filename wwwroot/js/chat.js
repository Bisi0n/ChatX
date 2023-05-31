const rootElement = document.querySelector('#app');

const app = Vue.createApp({
    data() {
        return {
            connection: null,
            connected: false,
            currentUser: null,
            newChatRoom: '',
            joinedRoomId: null,
            roomIsRemoved: false,
            chatRooms: [],
            messages: [],
            dateBotMessages: [{
                sender: {
                    name: 'DateBot'
                },
                content: 'Hello, I\'m DateBot! Tell me something you\'re interested in and I\'ll try to match you with someone!'
            }],
            newMessage: '',
            isTyping: false,
            typingTimeout: null,
            usersCurrentlyTyping: [],
            timeoutDuration: 2500,
            dateFinderInput: ''
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

            this.connection.on('ReceiveChatRooms', (chatRooms) => {
                this.chatRooms = chatRooms;
            });

            this.connection.on('CreateChatRoom', (chatRoom) => {
                this.chatRooms.push(chatRoom);
            });

            this.connection.on('DeleteChatRoom', (id) => {
                this.chatRooms = this.chatRooms.filter(room => room.id !== id);
            });

            this.connection.on('ReceiveMessage', (message) => {
                this.messages.push(message);
            });

            this.connection.on('AnnounceDeletedChatRoom', (message) => {
                this.messages.push(message);
                this.roomIsRemoved = true;
            });

            this.connection.on('DeleteMessage', (id) => {
                this.messages = this.messages.filter(message => message.id !== id);
            });

            this.connection.on('CurrentlyTyping', (usersTyping) => {
                this.usersCurrentlyTyping = usersTyping;
            });

            this.connection.on('ReceiveDateMatch', (match) => {
                if (match !== null) {
                    var interests = '';

                    if (match.userInterests.length === 1) {
                        interests = array[0];
                    }
                    else if (match.userInterests.length === 2) {
                        interests = array.join(" and ");
                    }
                    else {
                        const lastElement = match.userInterests.pop();
                        interests = `${match.userInterests.join(", ")} and ${lastElement}`;
                    }

                    var outputStrings = [
                        'I found a match! Here is some information about them:',
                        `Name: ${match.firstName} ${match.lastName}`,
                        `Age: ${match.age} years old`,
                        `Personality: ${match.personalityType}`,
                        `Description: ${match.description}`,
                        `They're interested in ${interests}!`,
                        `Here's a link to their profile <a href="${match.profileUrl}" target="_blank">${match.profileUrl}</a>`
                    ];

                    outputStrings.forEach((message) => {
                        if (message.includes('<a')) {
                            this.dateBotMessages.push({
                                sender: {
                                    name: 'DateBot'
                                },
                                content: message,
                                isHtml: true
                            });
                        } else {
                            this.dateBotMessages.push({
                                sender: {
                                    name: 'DateBot'
                                },
                                content: message
                            });
                        }
                    });
                }
                else {
                    this.messages.push({
                        sender: {
                            name: 'DateBot'
                        },
                        content: 'Sorry, I didn\'t find a match... Try again with another interest that you have!'
                    });
                }
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
            if (!this.isEmptyOrWhitespace(this.newMessage)) {
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
            if (!this.isEmptyOrWhitespace(this.newChatRoom)) {
                this.connection.invoke('CreateChatRoom', loggedInUser, this.newChatRoom)
                    .then(() => {
                        this.newChatRoom = '';
                    })
                    .catch((err) => {
                        console.log(err);
                    });
            }
        },
        deleteChatRoom(roomId) {
            this.connection.invoke('DeleteChatRoom', loggedInUser, roomId)
                .then(() => {
                    this.announceDeletedChatRoom(roomId);
                })
                .catch((err) => {
                    console.log(err);
                });
        },
        announceDeletedChatRoom(roomId) {
            this.connection.invoke('AnnounceDeletedChatRoom', loggedInUser, roomId)
                .catch((err) => {
                    console.log(err);
                });
        },
        joinChatRoom(roomId) {
            if (this.joinedRoomId !== null) {
                this.leaveChatRoom();
            }

            this.connection.invoke('JoinChatRoom', roomId)
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
                    this.roomIsRemoved = false;
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
        },
        getDateMatch() {
            this.dateBotMessages.push({
                sender: loggedInUser,
                content: this.dateFinderInput
            });

            this.connection.invoke('GetDateMatch', this.dateFinderInput)
                .then(() => {
                    this.dateFinderInput = '';
                })
                .catch((err) => {
                    console.log(err)
                });
        }
    }
});

app.mount('#app');