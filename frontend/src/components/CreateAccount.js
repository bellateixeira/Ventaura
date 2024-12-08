import React, { useState, useEffect } from "react";
import axios from "axios";
import { useNavigate } from "react-router-dom";

// Import specific CSS modules
import layoutStyles from '../styles/layout.module.css';
import createAccountStyles from '../styles/modules/createaccount.module.css';
import buttonStyles from '../styles/modules/buttons.module.css';

// **Preference & Dislike Mapping**
const preferenceMapping = {
  "Festivals": "Festivals-Fairs",
  "Outdoors": "Sports-active-life",
  "Exhibitions": "Visual-Arts",
  "Community": "Charities",
  "Theater": "Performing-Arts",
  "Family": "Kids-Family",
  "Film": "Film",
  "Charity": "Charities",
};

const uniqueOptions = [
  "Music", "Festivals", "Hockey", "Outdoors", "Workshops", "Conferences", 
  "Exhibitions", "Community", "Theater", "Family", "Nightlife", "Wellness", 
  "Holiday", "Networking", "Gaming", "Film", "Pets", "Virtual", "Charity", 
  "Science", "Basketball", "Pottery", "Tennis", "Soccer", "Football", 
  "Fishing", "Hiking", "Other"
];

const priceOptions = ["$", "$$", "$$$", "Irrelevant"]; // **Defined priceOptions**

const CreateAccount = () => {
  const [formData, setFormData] = useState({
    email: "",
    firstName: "",
    lastName: "",
    latitude: "",
    longitude: "",
    preferences: [],
    dislikes: [],
    priceRange: 50,
    maxDistance: "",
    password: "",
    confirmPassword: "", // New confirm password field
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

  const handlePriceRangeSelect = (price) => {
    setFormData((prevData) => ({
      ...prevData,
      priceRange: price, // Set only one option at a time
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (formData.password !== formData.confirmPassword) {
      setMessage("Passwords do not match.");
      return;
    }

    const mappedPreferences = formData.preferences.map(preference => 
      preferenceMapping[preference] || preference
    );

    const mappedDislikes = formData.dislikes.map(dislike => 
      preferenceMapping[dislike] || dislike
    );

    const requestData = {
      ...formData,
      latitude: Number(formData.latitude),
      longitude: Number(formData.longitude),
      priceRange: formData.priceRange.toString(),
      maxDistance: Number(formData.maxDistance),
      preferences: mappedPreferences.join(", "),
      dislikes: mappedDislikes.join(", "),
      passwordHash: formData.password,
      isLoggedIn: false,
    };

    try {
      const response = await axios.post(
        "http://localhost:5152/api/users/create-account",
        requestData
      );
      setMessage(response.data.Message);
      setTimeout(() => {
        navigate("/login");
      }, 2000);
    } catch (error) {
      if (error.response) {
        setMessage(error.response.data.Message || "Error creating account.");
      } else {
        setMessage("An error occurred. Please try again.");
      }
    }
  };

  return (
    <div className={createAccountStyles['create-account-container']}>
      <h2 className={createAccountStyles['create-account-heading']}>Create Account</h2>

      <form onSubmit={handleSubmit} className={createAccountStyles['create-account-form']}>
        <input
          type="email"
          name="email"
          placeholder="Email"
          value={formData.email}
          onChange={handleChange}
          className={createAccountStyles['form-input']}
          required
        />
        <input
          type="text"
          name="firstName"
          placeholder="First Name"
          value={formData.firstName}
          onChange={handleChange}
          className={createAccountStyles['form-input']}
          required
        />
        <input
          type="text"
          name="lastName"
          placeholder="Last Name"
          value={formData.lastName}
          onChange={handleChange}
          className={createAccountStyles['form-input']}
          required
        />

        <div className={createAccountStyles['grid-container']}>
          <h3 className={createAccountStyles['subheading']}>Select Preferences:</h3> 
          <div className={createAccountStyles['button-grid']}>
            {uniqueOptions.map((preference) => (
              <button
                type="button"
                key={preference}
                onClick={() => handlePreferenceToggle(preference)}
                className={`${createAccountStyles['preference-button']} ${formData.preferences.includes(preference) ? createAccountStyles['selected'] : ''}`}
              >
                {preference}
              </button>
            ))}
          </div>
        </div>

        <div className={createAccountStyles['grid-container']}>
          <h3 className={createAccountStyles['subheading']}>Select Dislikes:</h3>
          <div className={createAccountStyles['button-grid']}>
            {uniqueOptions.map((dislike) => (
              <button
                type="button"
                key={dislike}
                onClick={() => handleDislikeToggle(dislike)}
                className={`${createAccountStyles['dislike-button']} ${formData.dislikes.includes(dislike) ? createAccountStyles['selected'] : ''}`}
              >
                {dislike}
              </button>
            ))}
          </div>
        </div>

        <div className={createAccountStyles['price-range-section']}>
          <h3>Select Price Range:</h3>
          <div className={createAccountStyles['price-buttons-container']}>
            {priceOptions.map((price) => (
              <button
                type="button"
                key={price}
                onClick={() => handlePriceRangeSelect(price)}
                className={`${createAccountStyles['price-button']} ${formData.priceRange === price ? createAccountStyles['selected'] : ''}`}
              >
                {price}
              </button>
            ))}
          </div>
        </div>

        <input
          type="number"  // <-- Use type="number" instead of type="integer"
          name="maxDistance"  // <-- Corrected the typo (was MaxDistanct)
          placeholder="Max Distance (km)"  // Optional: Placeholder to guide user input
          value={formData.maxDistance}
          onChange={handleChange}
          className={createAccountStyles['form-input']}
          min="1"  // <-- Set a minimum distance
          max="100" // <-- Set a maximum distance
          required
        />

        <input
          type="password"
          name="password"
          placeholder="Password"
          value={formData.password}
          onChange={handleChange}
          className={createAccountStyles['form-input']}
          required
        />

        <input
          type="password"
          name="confirmPassword"
          placeholder="Confirm Password"
          value={formData.confirmPassword}
          onChange={handleChange}
          className={createAccountStyles['form-input']}
          required
        />

        <button type="submit" className={createAccountStyles['create-account-button']}>
          Create Account
        </button>
      </form>

      {message && <p className={createAccountStyles['message']}>{message}</p>}
    </div>
  );
};

export default CreateAccount;
