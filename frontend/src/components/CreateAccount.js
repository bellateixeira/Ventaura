// CreateAccount.js
import React, { useState, useEffect } from "react";
import axios from "axios";
import { useNavigate } from "react-router-dom";

import layoutStyles from '../styles/layout.module.css';
import formsStyles from '../styles/modules/forms.module.css';
import buttonStyles from '../styles/modules/buttons.module.css';

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
    maxDistance: 10, // Default preferred distance (e.g., 10 miles)
    password: "",
  });

  const [message, setMessage] = useState("");
  const navigate = useNavigate();

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
      return { ...prevData, dislikes: newDislikes };
    });
  };

  const handleSliderChange = (e) => {
    setFormData((prevData) => ({
      ...prevData,
      priceRange: e.target.value,
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    // Convert fields to the types expected by the backend
    const requestData = {
      ...formData,
      latitude: Number(formData.latitude),
      longitude: Number(formData.longitude),
      priceRange: formData.priceRange.toString(), // Ensure priceRange is a string if backend expects that
      maxDistance: Number(formData.maxDistance),
      preferences: formData.preferences.join(", "),
      dislikes: formData.dislikes.join(", "),
      passwordHash: formData.password,
      isLoggedIn: false,
    };

    console.log("Form data being sent:", requestData);

    try {
      const response = await axios.post(
        "http://localhost:5152/api/users/create-account",
        requestData
      );
      setMessage(response.data.Message);
      setTimeout(() => {
        navigate("/login");
      }, 2000);
      setFormData({
        email: "",
        firstName: "",
        lastName: "",
        latitude: "",
        longitude: "",
        preferences: [],
        dislikes: [],
        priceRange: 50,
        maxDistance: 10,
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
    <div className={layoutStyles.container}>
      <h2 className={layoutStyles.heading}>Create Account</h2>
      <form onSubmit={handleSubmit} className={formsStyles.form}>
        <input
          type="email"
          name="email"
          placeholder="Email"
          value={formData.email}
          onChange={handleChange}
          className={formsStyles.formInput}
          required
        />
        <input
          type="text"
          name="firstName"
          placeholder="First Name"
          value={formData.firstName}
          onChange={handleChange}
          className={formsStyles.formInput}
          required
        />
        <input
          type="text"
          name="lastName"
          placeholder="Last Name"
          value={formData.lastName}
          onChange={handleChange}
          className={formsStyles.formInput}
          required
        />

        <div className={formsStyles.preferencesSection}>
          <h3 className={formsStyles.subheading}>Select Preferences:</h3>
          {["Music", "Festivals", "Hockey", "Outdoors", "Workshops", "Conferences", 
            "Exhibitions", "Community", "Theater", "Family", "Nightlife", "Wellness", 
            "Holiday", "Networking", "Gaming", "Film", "Pets", "Virtual", "Charity", 
            "Science", "Basketball", "Pottery", "Tennis", "Soccer", "Football", 
            "Fishing", "Hiking"].map((preference) => (
            <button
              type="button"
              key={preference}
              onClick={() => handlePreferenceToggle(preference)}
              className={`${formsStyles.preferenceButton} ${
                formData.preferences.includes(preference) ? formsStyles.selected : ""
              }`}
            >
              {preference}
            </button>
          ))}
        </div>

        <div className={formsStyles.preferencesSection}>
          <h3 className={formsStyles.subheading}>Select Dislikes:</h3>
          {["Music", "Festivals", "Hockey", "Outdoors", "Workshops", "Conferences", 
            "Exhibitions", "Community", "Theater", "Family", "Nightlife", "Wellness", 
            "Holiday", "Networking", "Gaming", "Film", "Pets", "Virtual", "Charity", 
            "Science", "Basketball", "Pottery", "Tennis", "Soccer", "Football", 
            "Fishing", "Hiking"].map((dislike) => (
            <button
              type="button"
              key={dislike}
              onClick={() => handleDislikeToggle(dislike)}
              className={`${formsStyles.dislikeButton} ${
                formData.dislikes.includes(dislike) ? formsStyles.selected : ""
              }`}
            >
              {dislike}
            </button>
          ))}
        </div>

        <div className={formsStyles.priceRangeSection}>
          <h3 className={formsStyles.subheading}>Select Price Range:</h3>
          <label htmlFor="priceRange" className={formsStyles.rangeLabel}>
            Average Price: ${formData.priceRange}
          </label>
          <input
            type="range"
            id="priceRange"
            name="priceRange"
            min="0"
            max="100"
            step="1"
            value={formData.priceRange}
            onChange={handleSliderChange}
            className={formsStyles.slider}
          />
        </div>

        <div className={formsStyles.priceRangeSection}>
          <h3 className={formsStyles.subheading}>Select Preferred Distance:</h3>
          <label htmlFor="maxDistance" className={formsStyles.rangeLabel}>
            Max Distance: {formData.maxDistance} km
          </label>
          <input
            type="range"
            id="maxDistance"
            name="maxDistance"
            min="0"
            max="100"
            step="1"
            value={formData.maxDistance}
            onChange={handleChange}
            className={formsStyles.slider}
          />
        </div>

        <input
          type="password"
          name="password"
          placeholder="Password"
          value={formData.password}
          onChange={handleChange}
          className={formsStyles.formInput}
          required
        />
        <button type="submit" className={buttonStyles.formButton}>
          Create Account
        </button>
      </form>
      {message && <p className={formsStyles.message}>{message}</p>}
    </div>
  );
};

export default CreateAccount;
