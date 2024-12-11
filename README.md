***VENTAURA APPLICATION SETUP***

**Table of Contents**
- Overview
- Technologies Used
- System Requirements
- Project Structure
- Setting Up the Backend
- Environment Variables
- Database Setup
- Running the Backend
- Setting Up the Frontend
- Stripe Integration
- Additional Notes
- Common Troubleshooting

**Overview**

The Ventaura application consists of two main parts: a backend (ASP.NET Core) and a frontend (React). The backend provides a RESTful API for user management, event retrieval and merging, geocoding, and event ranking. It connects to external services (such as Ticketmaster and Yelp APIs) and a PostgreSQL database (Supabase) to store user data and host events. The frontend consumes the backend API to deliver a responsive, user-friendly interface for viewing and interacting with events.

**Technologies Used**

***Backend:***

ASP.NET Core / C#: For building APIs and implementing business logic.

Entity Framework Core: For database operations and migrations.

PostgreSQL: The relational database used to store users and host events (Managed by Supabase).

Stripe: For payment handling and checkout sessions.

'Home made' API using FastAPI for Ranking logic.

External APIs: Ticketmaster, Google Geocoding, Yelp Fusion, & Stripe.

***Frontend:***

React (JavaScript/TypeScript): For building a modern, interactive, and responsive web UI.

Axios: To communicate with the backend services.

Development Tools:

Swagger: For API documentation and testing.

CORS Policies: To allow the frontend to communicate with the backend during development.

**System Requirements**

.NET 7.0 SDK or compatible (for backend development)

Node.js (v14 or later) and npm or yarn (for frontend development)

PostgreSQL database (Supabase instance or local Postgres)

Git (for version control)

A modern browser (for running the frontend)

**TO RUN**

1. In a terminal, change into the backend/ventaura_backend directory.
   
   Run the following:
   
                      dotnet restore
   
                      dotnet build
   
                      dotnet run

3. In a new terminal, change into the backend/Ranking directory.
   
   Run the following:
   
                      pip install fastapi
   
                      pip install uvicorn

                      pip install httpx
   
                      python app.py

5. In a new terminal, change into the frontend directory.
   
   Run the following:

                      npm install

                      npm start

Access the website at: http://localhost:3000/

Enjoy!

FOR DEVELOPER: SUPABASE: https://supabase.com/dashboard/project/lzrnyahwsvygmcdqofkm

***Features that have been stubbed with approval and saved for future implementation:***

- Chat function with hosts.
- Saved events repository for users.
- Amadeus API was replaced with Yelp API.

***Other notes:***

Please read the backend README.md for more detailed information on the backend implementation. 

Snapshot of the database:

<img width="1440" alt="Screenshot 2024-12-10 at 20 31 19" src="https://github.com/user-attachments/assets/9a29b6ae-9b1c-4036-94ff-841f60bc7bcb">



