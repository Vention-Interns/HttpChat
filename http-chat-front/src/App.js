import "./App.css";
import { useEffect, useState } from "react";

function App() {
    const [messages, setMessages] = useState([]);
    const [messageInput, setMessageInput] = useState("");
    const [clientIdInput, setClientIdInput] = useState("");
    const [activeClientId, setActiveClientId] = useState(null);

    const startChat = async () => {
        const trimmedClientId = clientIdInput.trim();

        if (!trimmedClientId) {
            alert("Client ID cannot be empty!");
            return;
        }

        try {
            console.log("Registering user with payload:", { clientId: trimmedClientId });

            const response = await fetch("http://localhost:5181/api/chat/register", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ clientId: trimmedClientId }),
            });

            if (!response.ok) {
                const errorData = await response.json();
                console.error("Failed to register user:", errorData);
                alert(`Error: ${errorData.Error || "Failed to register user."}`);
                return;
            }

            setActiveClientId(trimmedClientId);
            fetchMessageHistory();
        } catch (error) {
            console.error("Error registering user:", error);
            alert("An unexpected error occurred while registering the user.");
        }
    };

    const fetchMessageHistory = async () => {
        try {
            const response = await fetch("http://localhost:5181/api/chat/history");
            const data = await response.json();
            setMessages(data.Messages || []);
        } catch (error) {
            console.error("Error fetching message history:", error);
        }
    };

    const sendMessage = async () => {
        if (!activeClientId) {
            alert("You must enter a Client ID and click 'Start Chat' first!");
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
                body: JSON.stringify({ clientId: activeClientId, content: trimmedMessage }),
            });
            setMessageInput("");
        } catch (error) {
            console.error("Error sending message:", error);
        }
    };

    const receiveMessages = async () => {
        if (!activeClientId) {
            console.warn("Attempted to receive messages without a registered Client ID.");
            return;
        }

        try {
            const response = await fetch(`http://localhost:5181/api/chat/receive?clientId=${encodeURIComponent(activeClientId)}`);
            const data = await response.json();

            console.log("Received data:", data);

            if (data && data.messages && data.messages.length > 0) {
                setMessages((prevMessages) => {
                    console.log("Previous Messages:", prevMessages);
                    console.log("New Messages:", data.messages);
                    return [...prevMessages, ...data.messages];
                });
            }

        } catch (error) {
            console.error("Error receiving messages:", error);
        } finally {
            setTimeout(receiveMessages, 500);
        }
    };

    useEffect(() => {
        if (activeClientId) {
            console.log(`Starting to receive messages for client ID: ${activeClientId}`);
            receiveMessages();
        }
    }, [activeClientId]);

    return (
        <div className="container">
            <h1>Group Chat</h1>

            {!activeClientId ? (
                <div className="inputContainer">
                    <input
                        type="text"
                        placeholder="Enter your Client ID..."
                        value={clientIdInput}
                        onChange={(e) => setClientIdInput(e.target.value)}
                        className="input"
                    />
                    <button onClick={startChat} className="button">
                        Start Chat
                    </button>
                </div>
            ) : (
                <div className="message">Client ID: {activeClientId}</div>
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




            {activeClientId && (
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
