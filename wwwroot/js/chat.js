﻿const rootElement = document.querySelector('#app');

const app = Vue.createApp({
    data() {
        return {
            connection: null,
            connected: false,
            currentUser: null,
            messages: [],
            newMessage: '',
            isTyping: false,
            typingTimeout: null,
            usersCurrentlyTyping: [],
            timeoutDuration: 2500,
            api: 'https://tinderapp.azurewebsites.net/Users',
            interests: ['sports', 'music', 'travel', 'literature', 'film',
                'art', 'cooking', 'photography', 'gardening', 'programming'],
            prompt: '',
            profiles: []
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
            this.connection.invoke('SendMessage', loggedInUser, this.newMessage).then(() => {
                this.newMessage = '';
            }).catch((err) => {
                console.error(err);
            });
        },
        deleteMessage(id) {
            this.connection.invoke('DeleteMessage', id).catch((err) => {
                console.error(err);
            });
        },
        currentlyTyping() {
            clearTimeout(this.typingTimeout);

            if (!this.isTyping) {
                this.connection.send('UserTyping', loggedInUser, true)
                    .then(() => {
                        this.isTyping = true;
                    })
                    .catch(err => console.error(err));
            }

            // Set a timeout to detect when the user stops typing
            this.typingTimeout = setTimeout(() => {
                // Send typing indicator to the server indicating the user stopped typing
                this.connection.send('UserTyping', loggedInUser, false)
                    .then(() => {
                        this.isTyping = false;
                    })
                    .catch(err => console.error(err));
            }, this.timeoutDuration); // Adjust the timeout duration as needed
        },
        loadProfiles() {
            fetch(this.matchProfile())
                .then(response => response.json())
                .then(data => this.profiles.push(data))
        },
        matchProfile() {
            for (const interest of this.interests) {
                if (this.prompt.includes(interest)) {
                    return this.api + '/' + interest;
                }
            }
            return '';
        }
    }
});

app.mount('#app');