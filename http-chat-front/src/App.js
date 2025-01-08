import "./App.css";
import { useEffect, useState } from "react";
import axios from "axios";

function App() {
    const [messages, setMessages] = useState([]);
    const [messageInput, setMessageInput] = useState("");
    const [clientIdInput, setClientIdInput] = useState("");
    const [chatIdInput, setChatIdInput] = useState("");
    const [activeClientId, setActiveClientId] = useState(null);
    const [activeChatId, setActiveChatId] = useState(null);
    const [token, setToken] = useState(localStorage.getItem("authToken") || "");
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [isAuthenticated, setIsAuthenticated] = useState(!!token);

    const handleLogin = async (e) => {
        e.preventDefault();

        try {
            const response = await axios.post("http://localhost:5181/api/Auth/login", { email, password });
            const { token } = response.data;

            localStorage.setItem("authToken", token);
            setToken(token);
            setIsAuthenticated(true);
        } catch (error) {
            alert("Login failed! Please check your credentials.");
            console.error("Login error:", error);
        }
    };

    const handleLogout = () => {
        localStorage.removeItem("authToken");
        setToken("");
        setIsAuthenticated(false);
        setActiveClientId(null);
        setActiveChatId(null);
        setMessages([]);
    };

    const startChat = async () => {
        const trimmedClientId = clientIdInput.trim();
        const trimmedChatId = chatIdInput.trim();

        if (!trimmedClientId || !trimmedChatId) {
            alert("Client ID and Chat ID cannot be empty!");
            return;
        }

        try {
            const response = await axios.post(
                "http://localhost:5181/api/chat/register",
                { clientId: trimmedClientId, chatId: trimmedChatId },
                { headers: { Authorization: `Bearer ${token}` } }
            );

            setActiveClientId(trimmedClientId);
            setActiveChatId(trimmedChatId);
            fetchMessageHistory(trimmedChatId);
        } catch (error) {
            alert("Failed to start chat. Please try again.");
            console.error("Error registering user:", error);
        }
    };

    const fetchMessageHistory = async (chatId) => {
        try {
            const response = await axios.get(
                `http://localhost:5181/api/chat/history?chatId=${encodeURIComponent(chatId)}`,
                { headers: { Authorization: `Bearer ${token}` } }
            );
            setMessages(response.data.messages || []);
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
            await axios.post(
                "http://localhost:5181/api/chat/send",
                { clientId: activeClientId, chatId: activeChatId, content: trimmedMessage },
                { headers: { Authorization: `Bearer ${token}` } }
            );
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
            const response = await axios.get(
                `http://localhost:5181/api/chat/receive?clientId=${encodeURIComponent(activeClientId)}&chatId=${encodeURIComponent(activeChatId)}`,
                { headers: { Authorization: `Bearer ${token}` } }
            );

            if (response.data && response.data.messages && response.data.messages.length > 0) {
                setMessages((prevMessages) => [...prevMessages, ...response.data.messages]);
            }
        } catch (error) {
            console.error("Error receiving messages:", error);
        } finally {
            setTimeout(receiveMessages, 500);
        }
    };

    useEffect(() => {
        if (activeClientId && activeChatId) {
            receiveMessages();
        }
    }, [activeClientId, activeChatId]);

    return (
        <div className="container">
            <h1>Group Chat</h1>

            {!isAuthenticated ? (
                <div className="inputContainer">
                    <h2>Login</h2>
                    <form onSubmit={handleLogin}>
                        <input
                            type="email"
                            placeholder="Enter your email"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            required
                            className="input"
                        />
                        <input
                            type="password"
                            placeholder="Enter your password"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            required
                            className="input"
                        />
                        <button type="submit" className="button">
                            Login
                        </button>
                    </form>
                </div>
            ) : (
                <>
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
                            <button onClick={handleLogout} className="button">
                                Logout
                            </button>
                        </div>
                    ) : (
                        <>
                            <div className="message">
                                Client ID: {activeClientId}, Chat ID: {activeChatId}
                            </div>
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
                                <button onClick={handleLogout} className="button">
                                    Logout
                                </button>
                            </div>
                        </>
                    )}
                </>
            )}
        </div>
    );
}

export default App;
