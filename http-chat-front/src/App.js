import "./App.css";
import { useEffect, useState } from "react";

function App() {
    const [messages, setMessages] = useState([]);
    const [messageInput, setMessageInput] = useState("");
    const [clientIdInput, setClientIdInput] = useState("");
    const [chatIdInput, setChatIdInput] = useState("");
    const [activeClientId, setActiveClientId] = useState(null);
    const [activeChatId, setActiveChatId] = useState(null);

    const startChat = async () => {
        const trimmedClientId = clientIdInput.trim();
        const trimmedChatId = chatIdInput.trim();

        if (!trimmedClientId || !trimmedChatId) {
            alert("Client ID and Chat ID cannot be empty!");
            return;
        }

        try {
            console.log("Registering user with payload:", {
                clientId: trimmedClientId,
                chatId: trimmedChatId,
            });

            const response = await fetch("http://localhost:5181/api/chat/register", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ clientId: trimmedClientId, chatId: trimmedChatId }),
            });

            if (!response.ok) {
                const errorData = await response.json();
                console.error("Failed to register user:", errorData);
                alert(`Error: ${errorData.Error || "Failed to register user."}`);
                return;
            }

            setActiveClientId(trimmedClientId);
            setActiveChatId(trimmedChatId);
            fetchMessageHistory(trimmedChatId);
        } catch (error) {
            console.error("Error registering user:", error);
            alert("An unexpected error occurred while registering the user.");
        }
    };

    const fetchMessageHistory = async (chatId) => {
        try {
            const response = await fetch(`http://localhost:5181/api/chat/history?chatId=${encodeURIComponent(chatId)}`);
            const data = await response.json();
            setMessages(data.messages || []);
        } catch (error) {
            console.error("Error fetching message history:", error);
        }
    };

    const sendMessage = async () => {
        if (!activeClientId || !activeChatId) {
            alert("You must enter a Client ID and Chat ID, and click 'Start Chat' first!");
            return;
        }

        const trimmedMessage = messageInput.trim();
        if (!trimmedMessage) {
            alert("Message cannot be empty!");
            return;
        }

        try {
            await fetch("http://localhost:5181/api/chat/send", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ clientId: activeClientId, chatId: activeChatId, content: trimmedMessage }),
            });
            setMessageInput("");
        } catch (error) {
            console.error("Error sending message:", error);
        }
    };

    const receiveMessages = async () => {
        if (!activeClientId || !activeChatId) {
            console.warn("Attempted to receive messages without a registered Client ID or Chat ID.");
            return;
        }

        try {
            const response = await fetch(
                `http://localhost:5181/api/chat/receive?clientId=${encodeURIComponent(activeClientId)}&chatId=${encodeURIComponent(activeChatId)}`
            );

            console.log("Response status:", response.status);

            const data = await response.json();
            console.log("Received data from API:", data);

            if (data && data.messages && data.messages.length > 0) {
                setMessages((prevMessages) => [...prevMessages, ...data.messages]);
                console.log("Updated messages state:", [...messages, ...data.messages]);
            }
        } catch (error) {
            console.error("Error receiving messages:", error);
        } finally {
            setTimeout(receiveMessages, 500);
        }
    };


    useEffect(() => {
        if (activeClientId && activeChatId) {
            console.log(`Starting to receive messages for chat ID: ${activeChatId}`);

            receiveMessages();
        }
    }, [activeClientId, activeChatId]);

    return (
        <div className="container">
            <h1>Group Chat</h1>

            {!activeClientId || !activeChatId ? (
                <div className="inputContainer">
                    <input
                        type="text"
                        placeholder="Enter your Client ID..."
                        value={clientIdInput}
                        onChange={(e) => setClientIdInput(e.target.value)}
                        className="input"
                    />
                    <input
                        type="text"
                        placeholder="Enter Chat ID..."
                        value={chatIdInput}
                        onChange={(e) => setChatIdInput(e.target.value)}
                        className="input"
                    />
                    <button onClick={startChat} className="button">
                        Start Chat
                    </button>
                </div>
            ) : (
                <div className="message">
                    Client ID: {activeClientId}, Chat ID: {activeChatId}
                </div>
            )}

            <div className="chatBox">
                {messages.length > 0 ? (
                    messages.map((msg, index) => (
                        <div key={index} className="message">
                            {msg}
                        </div>
                    ))
                ) : (
                    <div className="message">No messages yet...</div>
                )}
            </div>


            {activeClientId && activeChatId && (
                <div className="inputContainer">
                    <input
                        type="text"
                        placeholder="Type a message..."
                        value={messageInput}
                        onChange={(e) => setMessageInput(e.target.value)}
                        className="input"
                    />
                    <button onClick={sendMessage} className="button">
                        Send
                    </button>
                </div>
            )}
        </div>
    );
}

export default App;
