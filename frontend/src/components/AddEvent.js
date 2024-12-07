import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import axios from 'axios';
import '../styles.css'; // Ensure to import global CSS

const AddEvent = () => {
  const [isSidebarOpen, setIsSidebarOpen] = useState(false);
  const navigate = useNavigate();
  const [userId, setUserId] = useState(localStorage.getItem('userId'));

  // Check if the user is logged in when the component mounts
  useEffect(() => {
    if (!userId) {
      navigate('/login');
    }
  }, [userId, navigate]);

  const handleManualLogout = async () => {
    if (!userId) {
      alert("No user ID found in local storage.");
      return;
    }

    try {
      // Send logout request to the server
      const response = await axios.post(
        `http://localhost:5152/api/combined-events/logout?userId=${userId}`
      );

      // Remove userId from localStorage
      localStorage.removeItem('userId');
      alert(response.data.Message || "Logged out successfully.");
      navigate("/login");
    } catch (error) {
      if (error.response) {
        alert(error.response.data.Message || "Error logging out.");
      } else {
        alert("An error occurred while logging out.");
      }
    }
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
        <header className="header">
          <h2 className="page-title">Add Event Here:</h2>
        </header>

        <form className="event-form">
          {/* Submit Button - only enabled if the form is valid */}
          <form action="http://localhost:5152/api/create-checkout-session" method="POST">
            <button 
              type="submit" 
              role="link" 
              className="submit-button" 
            >
              Make Payment
            </button>
          </form>
        </form>
      </div>
    </div>
  );
};

export default AddEvent;
