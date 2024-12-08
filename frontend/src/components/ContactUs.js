// ContactUs.js
import React, { useState } from "react";
import axios from "axios";
import { Link, useNavigate } from "react-router-dom";

// Import specific CSS modules
import layoutStyles from '../styles/layout.module.css';
import buttonStyles from '../styles/modules/buttons.module.css';
import navigationStyles from '../styles/modules/navigation.module.css';
import contactStyles from '../styles/modules/contactus.module.css'; // Import contact-specific styles
import logo from '../assets/ventaura-logo-white.png'; 


const ContactUs = () => {
  const [isSidebarOpen, setIsSidebarOpen] = useState(false);
  const navigate = useNavigate();
  const [userId, setUserId] = useState(localStorage.getItem("userId"));

  const handleManualLogout = async () => {
    if (!userId) {
      alert("No user ID found in local storage.");
      return;
    }
  
    try {
      const response = await axios.post(
        `http://localhost:5152/api/combined-events/logout?userId=${userId}`
      );
  
      // Check if response.data is defined and has a Message property
      if (response.data && response.data.Message) {
        alert(response.data.Message);
      } else {
        alert("Logout successful");
      }
  
      // Remove userId from localStorage
      localStorage.removeItem("userId");
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
    <div className={layoutStyles['page-container']}>
      {/* Header */}
      <header className={layoutStyles['header-side']}>
        <button 
          className={`${buttonStyles['sidebar-handle']} ${isSidebarOpen ? buttonStyles.open : ''}`} 
          onClick={() => setIsSidebarOpen(!isSidebarOpen)} 
          aria-label="Toggle Sidebar"
        >
          
        </button>
        <h1 className={layoutStyles['header-title']}>Contact Us</h1>
        <div className={layoutStyles['header-right']}>
          <button className={buttonStyles['settings-button']} onClick={() => navigate("/settings")}>
            ⚙️
          </button>
        </div>
      </header>

      {/* Sidebar */}
      <div className={`${layoutStyles.sidebar} ${isSidebarOpen ? layoutStyles.open : ''}`}>
        <div className={navigationStyles['top-section']}>
          <div className={navigationStyles['logo-container']}>
            <img src={logo} alt="Logo" className={navigationStyles['logo']} />
          </div>
          <button 
            className={buttonStyles['close-sidebar']} 
            onClick={() => setIsSidebarOpen(false)}
            aria-label="Close Sidebar"
          />
          <div className={navigationStyles['links-container']}>
            <Link to="/for-you" className={navigationStyles['sidebar-link']}>For You</Link>
            <Link to="/about-us" className={navigationStyles['sidebar-link']}>About Us</Link>
            <Link to="/contact-us" className={navigationStyles['sidebar-link']}>Contact Us</Link>
          </div>
        </div>
        <button 
          onClick={handleManualLogout} 
          className={buttonStyles['logout-button']}
        >
          Logout
        </button>
      </div>


      {/* Main Content */}
      <main className={contactStyles['contact-container']}>

        <p className={contactStyles['contact-text']}>
          For support or inquiries, email us at 
          <a href="mailto:support@ventaura.com" className={contactStyles['email-link']}> support@ventaura.com</a>.
        </p>

        <p className={contactStyles['contact-text']}>
          You can also follow us on social media for updates:
        </p>

        <div className={contactStyles['social-buttons-container']}>
          <a href="#" className={contactStyles['social-button']}>Facebook</a>
          <a href="#" className={contactStyles['social-button']}>Twitter</a>
          <a href="#" className={contactStyles['social-button']}>Instagram</a>
        </div>

        <p className={contactStyles['address']}>
          Or visit us at:<br />
          <span className={contactStyles['strong-text']}>123 Main Street, Suite 400</span><br />
          Boston, MA
        </p>
      </main>
    </div>
  );
};

export default ContactUs;
