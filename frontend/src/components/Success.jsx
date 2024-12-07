import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

function CreateEventForm() {
  const [formData, setFormData] = useState({
    title: '',
    description: '',
    location: '',
    start: '',
    type: '',
    source: 'Host',
    currencyCode: 'Dollars',
    amount: '',
    url: '',
    hostUserId: null // Default to null, will set in useEffect
  });
  const navigate = useNavigate();
  

  // Retrieve userId from localStorage
  useEffect(() => {
    const storedUserId = localStorage.getItem('userId');
    if (storedUserId) {
      setFormData((prevData) => ({
        ...prevData,
        hostUserId: parseInt(storedUserId, 10) // Convert to number
      }));
    } else {
      // Redirect to login if userId is not in localStorage
      navigate("/login");
    }
  }, [navigate]);

  useEffect(() => {
    if (window.google && window.google.maps && window.google.maps.places) {
      const input = document.querySelector('input[name="eventLocation"]');
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
              eventLocation: place.formatted_address
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
      [name]: value
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault(); // Prevent default form submission

    try {
      // Prepare the payload
      const payload = {
        Title: formData.title,
        Description: formData.description,
        Location: formData.location,
        Start: new Date(formData.start), // Ensure this is in a valid date format
        Type: formData.type,
        CurrencyCode: formData.currencyCode,
        Amount: parseFloat(formData.amount), // Ensure amount is a number
        URL: formData.url,
        HostUserId: formData.hostUserId
      };

      // Send the POST request
      const response = await fetch('http://localhost:5152/api/create-host-event', {
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
  };

  const handleManualLogout = () => {
    localStorage.removeItem('userId');
    alert("Logged out successfully.");
    navigate("/login");
  };

  return (
    <div>
      <h2>Create Host Event</h2>
      <button onClick={handleManualLogout} style={{ marginBottom: '10px' }}>
        Logout
      </button>
      <form onSubmit={handleSubmit}>
        <div>
          <label>Title:</label>
          <input type="text" name="title" value={formData.title} onChange={handleChange} required />
        </div>
        <div>
          <label>Description:</label>
          <input type="text" name="description" value={formData.description} onChange={handleChange} />
        </div>
        <div>
          <label>Location:</label>
          <input type="text" name="location" value={formData.location} onChange={handleChange} />
        </div>
        <div>
          <label>Start:</label>
          <input type="datetime-local" name="start" value={formData.start} onChange={handleChange} required />
        </div>
        <div>
          <label>Type:</label>
          <input type="text" name="type" value={formData.type} onChange={handleChange} />
        </div>
        <div>
          <label>Amount:</label>
          <input type="number" name="amount" value={formData.amount} onChange={handleChange} required />
        </div>
        <div>
          <label>URL:</label>
          <input type="url" name="url" value={formData.url} onChange={handleChange} />
        </div>
        <button type="submit">Create Event</button>
      </form>
    </div>
  );
}

export default CreateEventForm;
