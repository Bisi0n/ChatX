const app = Vue.createApp({
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

            this.connection.on('ReceiveMessage', (message) => {
                this.messages.push(message);
            });

            this.connection.on('DeleteMessage', (id) => {
                this.messages = this.messages.filter(message => message.id !== id);
            });

            this.connection.start().then(() => {
                this.connected = true;
                this.currentUser = this.connection.connectionId;
            }).catch((err) => {
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