import React, { useState, useEffect } from "react";
import axios from "axios";
import "../styles.css"; // Import the global CSS
import { useNavigate } from "react-router-dom"; // Import useNavigate hook

const CreateAccount = () => {
  const [formData, setFormData] = useState({
    email: "",
    firstName: "",
    lastName: "",
    latitude: "",
    longitude: "",
    preferences: [],
    dislikes: [],
    priceRange: 50, // Default value for the slider
    password: "",
  });

  const [message, setMessage] = useState("");
  const navigate = useNavigate(); // Initialize navigate hook

  // Get user's live location using Geolocation API
  useEffect(() => {
    if (navigator.geolocation) {
      navigator.geolocation.getCurrentPosition(
        (position) => {
          setFormData((prevData) => ({
            ...prevData,
            latitude: position.coords.latitude,
            longitude: position.coords.longitude,
          }));
        },
        (error) => {
          console.error("Error fetching location: ", error);
          setMessage("Unable to retrieve your location. Please enter manually.");
        }
      );
    } else {
      console.error("Geolocation is not supported by this browser.");
      setMessage("Geolocation is not supported by your browser.");
    }
  }, []);

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handlePreferenceToggle = (preference) => {
    setFormData((prevData) => {
      const newPreferences = prevData.preferences.includes(preference)
        ? prevData.preferences.filter((item) => item !== preference)
        : [...prevData.preferences, preference];
      return { ...prevData, preferences: newPreferences };
    });
  };

  const handleDislikeToggle = (dislike) => {
    setFormData((prevData) => {
      const newDislikes = prevData.dislikes.includes(dislike)
        ? prevData.dislikes.filter((item) => item !== dislike)
        : [...prevData.dislikes, dislike];
      return { ...prevData, dislikes: newDislikes}
    });
  }

  const handleSliderChange = (e) => {
    setFormData((prevData) => ({
      ...prevData,
      priceRange: e.target.value,
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    console.log("Form data being sent:", {
      ...formData,
      preferences: formData.preferences.join(", "),
      passwordHash: formData.password,
    });

    try {
      const response = await axios.post(
        "http://localhost:5152/api/users/create-account",
        {
          ...formData,
          preferences: formData.preferences.join(", "), // Join preferences as a comma-separated string
          passwordHash: formData.password, // Send password as passwordHash
        }
      );
      setMessage(response.data.Message);
      setTimeout(() => {
        navigate("/login"); // Redirect to login screen after a brief delay
      }, 2000); // Adjust the delay if needed
      setFormData({
        email: "",
        firstName: "",
        lastName: "",
        latitude: "",
        longitude: "",
        preferences: [],
        dislikes: [],
        priceRange: 50,
        password: "",
      });
    } catch (error) {
      if (error.response) {
        setMessage(error.response.data.Message || "Error creating account.");
      } else {
        setMessage("An error occurred. Please try again.");
      }
    }
  };

  return (
    <div className="container">
      <h2>Create Account</h2>
      <form onSubmit={handleSubmit} className="form">
        <input
          type="email"
          name="email"
          placeholder="Email"
          value={formData.email}
          onChange={handleChange}
          className="form-input"
          required
        />
        <input
          type="text"
          name="firstName"
          placeholder="First Name"
          value={formData.firstName}
          onChange={handleChange}
          className="form-input"
          required
        />
        <input
          type="text"
          name="lastName"
          placeholder="Last Name"
          value={formData.lastName}
          onChange={handleChange}
          className="form-input"
          required
        />
        {/* <input
          type="number"
          name="latitude"
          placeholder="Latitude"
          value={formData.latitude}
          className="form-input"
          readOnly
        />
        <input
          type="number"
          name="longitude"
          placeholder="Longitude"
          value={formData.longitude}
          className="form-input"
          readOnly
        /> */}

        <div>
          <h3>Select Preferences:</h3>
          {["Festivals-Fairs", "Music", "Performing-Arts", "Visual-Arts", "Sports-active-life", "Nightlife", "Film", 
          "Charities", "Kids-Family", "Food-and-Drink", "Other"].map((preference) => (
            <button
              type="button"
              key={preference}
              onClick={() => handlePreferenceToggle(preference)}
              className={`preference-button ${
                formData.preferences.includes(preference) ? "selected" : ""
              }`}
            >
              {preference}
            </button>
          ))}
        </div>

        <div>
          <h3>Select Dislikes:</h3>
          {["Festivals-Fairs", "Music", "Performing-Arts", "Visual-Arts", "Sports-active-life", "Nightlife", "Film", "Charities", "Kids-Family", "Food-and-Drink", "Other"].map((dislike) => (
            <button
              type="button"
              key={dislike}
              onClick={() => handleDislikeToggle(dislike)}
              className={`dislike-button ${
                formData.dislikes.includes(dislike) ? "selected" : ""
              }`}
            >
              {dislike}
            </button>
          ))}
        </div>

        <div>
          <h3>Select Price Range:</h3>
          <label htmlFor="priceRange">
            Average Price: ${formData.priceRange}
          </label>
          <input
            type="range"
            id="priceRange"
            name="priceRange"
            min="0"
            max="100+"
            step="1"
            value={formData.priceRange}
            onChange={handleSliderChange}
            className="slider"
          />
        </div>

        <input
          type="password"
          name="password"
          placeholder="Password"
          value={formData.password}
          onChange={handleChange}
          className="form-input"
          required
        />
        <button type="submit" className="form-button">
          Create Account
        </button>
      </form>
      {message && <p className="message">{message}</p>}
    </div>
  );
};

export default CreateAccount;
