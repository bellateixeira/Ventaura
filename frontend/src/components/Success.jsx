import React, { useState, useEffect } from "react";
import "../styles.css"; // Import the global CSS
import { useNavigate } from "react-router-dom"; // Import useNavigate hook

const Success = () => {
  const [userId] = useState(localStorage.getItem("userId"));

  const [formData, setFormData] = useState({
    "title": "",
    "description": "",
    "location": "",
    "start": "",
    "type": "",
    "currencyCode": "USD",
    "amount": "",
    "url": "",
    "hostUserId": userId,
    "eventDate": "",
    "eventTime": "" 
  });
  const [message, setMessage] = useState("");
  const navigate = useNavigate(); // Initialize navigate hook

  // Get the session ID from the URL query parameters
  const urlParams = new URLSearchParams(window.location.search);
  const sessionId = urlParams.get('session_id');

  useEffect(() => {
    if (sessionId) {
      setMessage("Payment Successful! Please fill out the event details below.");
    } else {
      setMessage("Payment Unsuccessful.");
    }
  }, [sessionId]);

  useEffect(() => {
    if (window.google && window.google.maps && window.google.maps.places) {
      const input = document.querySelector('input[name="location"]');
      if (input) {
        // Create the autocomplete object
        const autocomplete = new window.google.maps.places.Autocomplete(input, {
          types: ['geocode'] // Restrict the suggestions to addresses only
        });
  
        // Add a listener for when a place is selected
        autocomplete.addListener('place_changed', () => {
          const place = autocomplete.getPlace();
          if (place && place.formatted_address) {
            // Update the formData with the selected address
            setFormData((prevData) => ({
              ...prevData,
              location: place.formatted_address
            }));
          }
        });
      }
    }
  }, []);  

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData({
      ...formData,
      [name]: value,
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault(); // Prevent default form submission

    // Extract date and time from form data
    const date = formData.eventDate; // "2024-12-08"
    const time = formData.eventTime; // "00:49"

    // Combine into an ISO 8601 string
    // Append 'T' to separate date and time, and 'Z' for UTC if necessary
    formData.start = `${date}T${time}:00.000Z`;

    try {
      // Prepare the payload
      const payload = {
        Title: formData.title,
        Description: formData.description,
        Location: formData.location,
        Start: new Date(formData.start).toISOString(), // Ensure this is in a valid date format
        Source: "Host",
        Type: formData.type,
        CurrencyCode: formData.currencyCode,
        Amount: parseFloat(formData.amount), // Ensure amount is a number
        URL: formData.url,
        HostUserId: parseInt(formData.hostUserId)
      };

      // Send the POST request
      const response = await fetch('http://localhost:5152/api/combined-events/create-host-event', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(payload)
      });

      // Check for response status
      if (!response.ok) {
        if (response.status === 409) {
          console.error("Conflict error: An event with this title already exists for this host.");
          alert("An event with this title already exists for this host.");
        } else if (response.status === 400) {
          console.error("Bad request: Validation failed.");
          alert("Validation failed. Please check your input.");
        } else {
          console.error("Unexpected error occurred.");
          alert("An unexpected error occurred. Please try again later.");
        }
        return;
      }

      const result = await response.json();
      console.log("Host event created successfully:", result);
      alert("Host event created successfully!");
    } catch (error) {
      console.error("An error occurred while creating the event:", error);
      alert("An error occurred while creating the event.");
    }
    navigate("/for-you"); // Redirect to the /for-you page
  };

  return (
    <div className="container">
      {sessionId ? (
        <>
          <header className="header">
            <h2 className="page-title">Payment Successful! 
              Add Event Details:</h2>
          </header>

          <form onSubmit={handleSubmit} className="event-form">
            <div className="form-group">
              <label>Event Title:</label>
              <input
                type="text"
                name="title"
                value={formData.title}
                onChange={handleChange}
                className="form-input"
                required
              />
            </div>

            <div className="form-group">
              <label>Event Description:</label>
              <textarea
                name="description"
                value={formData.description}
                onChange={handleChange}
                className="form-input"
                required
              />
            </div>

            <div className="form-group">
              <label>Location:</label>
              <input
                type="text"
                name="location"
                value={formData.location}
                onChange={handleChange}
                className="form-input"
                required
              />
            </div>

            <div className="form-group">
              <label>Date:</label>
              <input
                type="date"
                name="eventDate"
                value={formData.eventDate}
                onChange={handleChange}
                className="form-input"
                required
              />
            </div>

            <div className="form-group">
              <label>Time:</label>
              <input
                type="time"
                name="eventTime"
                value={formData.eventTime}
                onChange={handleChange}
                className="form-input"
                required
              />
            </div>

            <div className="form-group">
              <label>Event Type:</label>
              <select
                name="type"
                value={formData.type}
                onChange={handleChange}
                className="form-input"
                required
              >
                <option value="">Select event type</option>
                <option value="Festival-Fairs">Festival/Fair</option>
                <option value="Music">Music</option>
                <option value="Performing-Arts">Performing Arts</option>
                <option value="Sports-Active-Life">Sports/Active Life</option>
                <option value="Nightlife">Nightlife</option>
                <option value="Film">Film</option>
                <option value="Kids-Family">Kids/Family</option>
                <option value="Food-And-Drink">Food And Drink</option>
                <option value="Other">Other</option>
              </select>
            </div>

            <div className="form-group">
              <label>Price (USD):</label>
              <input
                type="number"
                name="amount"
                value={formData.amount}
                onChange={handleChange}
                className="form-input"
                required
              />
            </div>

            <div className="form-group">
              <label>Contact Info (Phone number):</label>
              <input
                type="text"
                name="url"
                value={formData.url}
                onChange={handleChange}
                className="form-input"
                required
              />
            </div>

            <button type="submit" className="form-button">
              Submit Event
            </button>
          </form>
        </>
      ) : (
        <p className="message">{message}</p>
      )}
    </div>
  );
};

export default Success;
