import React, { useState, useEffect } from "react";
import { Link, useNavigate } from "react-router-dom";
import "../styles.css"; // Import the global CSS

const ContactUs = () => {
  const [isSidebarOpen, setIsSidebarOpen] = useState(false);
  const navigate = useNavigate();
  const [userId, setUserId] = useState(localStorage.getItem("userId"));

  useEffect(() => {
    // Redirect to login if the user is not logged in
    if (!userId) {
      alert("You are not logged in. Redirecting to login page.");
      navigate("/login");
    }
  }, [userId, navigate]);

  const handleManualLogout = () => {
    // Clear userId from localStorage and redirect to login
    localStorage.removeItem("userId");
    alert("Logged out successfully.");
    navigate("/login");
  };

  return (
    <div>
      {/* Header */}
      <header className="header">
        <button
          className="sidebar-button"
          onClick={() => setIsSidebarOpen(!isSidebarOpen)}
        >
          ☰
        </button>
        {/* Settings Icon Button */}
        <button
          className="settings-button"
          onClick={() => navigate("/settings")}
        >
          ⚙️
        </button>
      </header>

      {/* Sidebar */}
      <div className={`sidebar ${isSidebarOpen ? "open" : ""}`}>
        <button className="close-sidebar" onClick={() => setIsSidebarOpen(false)}>
          X
        </button>
        <Link to="/for-you" className="sidebar-link">
          For You
        </Link>
        <Link to="/about-us" className="sidebar-link">
          About Us
        </Link>
        <Link to="/contact-us" className="sidebar-link">
          Contact Us
        </Link>
        <Link to="/post-event-page" className="sidebar-link">
          Post An Event
        </Link>
        <Link className="sidebar-link" onClick={handleManualLogout}>
          Logout
        </Link>
      </div>

      {/* Main Content */}
      <div className="container">
        <h2>Contact Us</h2>
        <p>For support or inquiries, email us at support@ventaura.com.</p>
      </div>
    </div>
  );
};

export default ContactUs;
