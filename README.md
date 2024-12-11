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

Custom API using FastAPI for Ranking logic.

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

Python 3.12

PostgreSQL database (Supabase instance or local Postgres) (make sure it is listening on port 5432, which is the default)

Git (for version control)

A modern browser (for running the frontend)

Once in the app, run:
   dotnet add package Microsoft.EntityFrameworkCore
   dotnet add package Microsoft.EntityFrameworkCore.Tools
   dotnet add package Microsoft.EntityFrameworkCore.Npqsql
   dotnet add package Stripe
   dotnet add package DotNetEnv
   dotnet add package Swashbuckle.AspNetCore
   dotnet add package Microsoft.AspNetCore.Http.Abstractions

**TO RUN**

Access our code from GitHub:
Main Repo:

          git clone https://github.com/your-username/Ventaura.git

          cd Ventaura

          git checkout final-submission

**ENSURE YOU ARE IN OUR FINAL SUBMISSION BRANCH FOR RUNNING OUR APP AND SEEING THE FINAL CODE FILES!**

Or direct to the branch: 

          git clone --branch final-submission https://github.com/your-username/Ventaura.git

1. In a terminal, change into the backend/ventaura_backend directory.
   
   Run the following:
   
                      dotnet restore
   
                      dotnet build
   
                      dotnet run

3. In a new terminal, change into the backend/Ranking directory.
   
   Run the following:
   
                      pip install -r requirements.txt
   
                      python app.py

5. In a new terminal, change into the frontend directory.
   
   Run the following:

                      npm install react-scripts

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



