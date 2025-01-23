# Pantry Tracker

Pantry Tracker is a web application designed for families and shared households to manage pantry items efficiently. The app helps prevent duplicate purchases and notifies users when items are low on stock. It simplifies household inventory management and encourages collaboration among members.

## Table of Contents

- [Features](#features)
- [Technologies Used](#technologies-used)
- [Getting Started](#getting-started)
- [Setup](#setup)
  - [Backend Setup](#backend-setup)
  - [Frontend Setup](#frontend-setup)
  - [Database Migration](#database-migration)
  - [Alternative Configuration](#alternative-configuration)
- [Usage](#usage)
- [External API](#external-api)
- [Contributing](#contributing)
- [License](#license)
- [Acknowledgments](#acknowledgments)

---

## Features

- **Household Management**:
  - Create or join a household using unique join codes.
  - Admins can remove members and manage household settings.
- **Pantry Item Tracking**:

  - Add, update, and remove items.
  - Track stock levels and receive alerts for low-stock items.

- **Roles**:

  - **Household Admin**: Manages the household, including join codes and member removal.
  - **Member**: Participates in managing pantry items.

- **Authentication**:
  - Secure user accounts using ASP.NET Identity.

---

## Technologies Used

- **Backend**:

  - ASP.NET Core with Entity Framework Core
  - PostgreSQL with pgAdmin
  - ChompAPI integration for product data

- **Frontend**:

  - React with Vite
  - Reactstrap for styling

- **Authentication**:
  - ASP.NET Identity

---

## Getting Started

To run this project locally, follow these steps:

### Prerequisites

1. Install the following tools:

   - [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
   - [Node.js](https://nodejs.org/)
   - [PostgreSQL](https://www.postgresql.org/)
   - [pgAdmin](https://www.pgadmin.org/)

2. Set up an API key for [ChompAPI](https://chompthis.com/api/).

---

## Setup

### Backend Setup

1. Clone the repository:

   ```bash
   git clone https://github.com/yourusername/pantry-tracker.git
   cd pantry-tracker/backend
   ```

2. Set up your environment variables:

   Create an `appsettings.json` file in the `PantryTracker` project directory with the following:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=pantrytracker;Username=yourusername;Password=yourpassword"
     },
     "ChompApiKey": "your-chomp-api-key"
   }
   ```

3. Apply migrations and start the backend:
   ```bash
   dotnet ef database update
   dotnet run
   ```

### Frontend Setup

1. Navigate to the frontend directory:

   ```bash
   cd ../frontend
   ```

2. Install dependencies:

   ```bash
   npm install
   ```

3. Start the development server:
   ```bash
   npm run dev
   ```

### Database Migration

1. Ensure the database server is running and accessible.
2. Run the following command to apply migrations to the database:
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```
3. Verify the tables were created in the PostgreSQL database using pgAdmin or your preferred database tool.
4. If changes are made to the database schema in the future, create new migrations with:
   ```bash
   dotnet ef migrations add MigrationName
   ```
5. Apply the new migration with:
   ```bash
   dotnet ef database update
   ```

### Alternative Configuration

If you prefer to store sensitive data like the ChompAPI key or connection string in environment variables:

1. **Set up environment variables**:

   - Add the following to your `.env` file (create one if it doesn't exist):
     ```env
     CHOMP_API_KEY=your-chomp-api-key
     CONNECTION_STRING=Host=localhost;Database=pantrytracker;Username=yourusername;Password=yourpassword
     ```

2. **Use `UserSecrets` for local development**:

   - Run the following command in the backend project directory to enable user secrets:
     ```bash
     dotnet user-secrets init
     ```
   - Add secrets with:
     ```bash
     dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=pantrytracker;Username=yourusername;Password=yourpassword"
     dotnet user-secrets set "ChompApiKey" "your-chomp-api-key"
     ```

3. **Modify the code to load environment variables or secrets**:

   - Update the `Program.cs` file to read from environment variables or user secrets:

     ```csharp
     var builder = WebApplication.CreateBuilder(args);

     builder.Configuration.AddEnvironmentVariables();
     builder.Configuration.AddUserSecrets<Program>();

     var connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"];
     var chompApiKey = builder.Configuration["ChompApiKey"];
     ```

4. Restart the application to apply the changes.

---

## Usage

### Admin Role

- **Create a Household**: Admins can create a household and generate a join code.
- **Manage Members**: Admins can remove members from the household.

### Member Role

- **Join a Household**: Use the join code to become a part of the household.
- **Track Pantry Items**: Add, update, and remove pantry items.

---

## External API

Pantry Tracker integrates with ChompAPI to fetch product data. To use this feature:

1. Obtain an API key from ChompAPI.
2. Add the API key to the `appsettings.json` file or use environment variables as described above.

## Acknowledgments

- Reactstrap for UI components.
- ASP.NET Core for backend development.
- ChompAPI for external product data.
