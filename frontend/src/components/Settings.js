// Settings.js
import React, { useState, useEffect } from "react";
import axios from "axios";
import { useNavigate } from "react-router-dom";

// Import specific CSS modules
import layoutStyles from '../styles/layout.module.css';
import formsStyles from '../styles/modules/forms.module.css';
import buttonStyles from '../styles/modules/buttons.module.css';
import navigationStyles from '../styles/modules/navigation.module.css';
import logoFull from '../assets/ventaura-logo-full-small-dark.png'; 

const Settings = () => {
  const [userData, setUserData] = useState({
    email: "",
    firstName: "",
    lastName: "",
  });
  const [message, setMessage] = useState("");
  const navigate = useNavigate(); // Initialize navigate hook
  const [userId, setUserId] = useState(localStorage.getItem("userId")); // Retrieve userId from localStorage

  useEffect(() => {
    // Fetch the user's existing data when the component mounts
    const fetchUserData = async () => {
    };

    fetchUserData();
  }, [navigate, userId]);

  const handleChange = (e) => {
    setUserData({ ...userData, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      const userId = localStorage.getItem("userId"); // Retrieve userId from localStorage

      // Prepare the request data
      const updateData = {
        userId: userId,
        email: userData.email, // Include the updated email
        firstName: userData.firstName, // Include the updated first name
        lastName: userData.lastName, // Include the updated last name
      };

      // Make the PUT request to the server
      const response = await axios.put(
        `http://localhost:5152/api/users/updatePreferences`,
        updateData
      );

      setMessage(response.data.Message || "User data updated successfully.");
    } catch (error) {
      if (error.response) {
        setMessage(error.response.data.Message || "Error updating user data.");
      } else {
        setMessage("An error occurred while updating data. Please try again.");
      }
    }
  };

  return (
    <div className={layoutStyles['page-container']}>
      <header className={layoutStyles['header-side']}>
        <div className={layoutStyles['logo-container-side']}>
          <img src={logoFull} alt="Logo" className={navigationStyles['logo-header']} />
        </div>
        <button
          className={buttonStyles.button}
          onClick={() => navigate("/for-you")}
        >
          Back
        </button>
      </header>

      <div className={formsStyles['settings-container']}>
        <h2 className={formsStyles.heading}>Settings</h2>
        <form onSubmit={handleSubmit} className={formsStyles.form}>
          <input
            type="email"
            name="email"
            placeholder="Email"
            value={userData.email}
            onChange={handleChange}
            className={formsStyles.formInput}
            required
          />
          <input
            type="text"
            name="firstName"
            placeholder="First Name"
            value={userData.firstName}
            onChange={handleChange}
            className={formsStyles.formInput}
            required
          />
          <input
            type="text"
            name="lastName"
            placeholder="Last Name"
            value={userData.lastName}
            onChange={handleChange}
            className={formsStyles.formInput}
            required
          />
          <button type="submit" className={buttonStyles.formButton}>
            Update Settings
          </button>
        </form>
        {message && <p className={formsStyles.message}>{message}</p>}
      </div>
    </div>
  );
};

export default Settings;
