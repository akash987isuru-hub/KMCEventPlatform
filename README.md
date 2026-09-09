# KMC Event Platform

A service-oriented event management platform developed for the **Kandy Municipal Council (KMC)** to provide a centralized solution for publishing, discovering, managing, booking, and monitoring events.

The system allows public users to explore events, registered participants to book events and receive tickets, KMC organizers to manage events and revenue, and external organizers to synchronize partner events with the KMC platform through APIs.

---

## 📌 Project Overview

The **KMC Event Platform** was developed using Service-Oriented Computing principles.

Instead of implementing all functionality inside a single application, the system separates major responsibilities into reusable services exposed through REST APIs.

The platform consists of:

- KMC Web Application
- KMC Web API
- KMC Organizer Desktop Application
- External Organizer Web Application
- External Organizer API
- KMC Database
- External Organizer Database

Communication between applications and services is performed using **HTTP and JSON**.

---

## 🎯 Project Objectives

The main objectives of the project are to:

- Provide a centralized event management platform for Kandy Municipal Council.
- Allow the public to search and view available events.
- Allow registered participants to book events.
- Provide secure online payment and ticket generation.
- Allow KMC organizers to create and manage events.
- Support external event organizers.
- Synchronize partner events between independent systems.
- Demonstrate reusable and maintainable service-oriented architecture.
- Improve scalability and separation of responsibilities.

---

## 👥 System Users

The system supports the following main users.

### Public Visitor

Public visitors can:

- Browse available events
- Search events
- Filter events
- View event details
- View ticket availability
- View event prices
- Register or sign in to continue with booking

### Participant

Registered participants can:

- Sign in securely
- Browse events
- Search for events
- View event details
- Book available events
- Make payments
- Receive tickets
- Print tickets
- View booking information
- Cancel registrations where applicable

### KMC Organizer

KMC organizers can:

- Sign in to the organizer workspace
- Create events
- Edit events
- Publish events
- Delete events
- Manage event information
- Monitor participant registrations
- View booking information
- Monitor ticket availability
- View total revenue
- Manage partner events

### External Organizer

External organizers can:

- Create partner events
- Publish events through their own platform
- Synchronize events with KMC
- Receive booking updates
- Manage event availability
- Manage partner bookings

---

## ✨ Main Features

### Authentication and Authorization

- User registration
- Secure login
- JWT authentication
- Password verification
- Role-based authorization
- Protected API endpoints
- Organizer and participant access control

### Event Management

- Create events
- Edit events
- Publish events
- Delete events
- View event information
- Maintain event date and time
- Manage location and venue details
- Manage ticket prices
- Manage event capacity
- Manage event status

### Event Search

Users can search and filter events using information such as:

- Event name
- Date
- Event type
- Venue
- Location
- Keywords

The displayed matching event count is updated according to the actual search results.

### Participant Registration

- Register for events
- Maintain participant registration records
- Validate event availability
- Prevent invalid bookings
- Handle registration cancellation

### Payment Management

- Process event payment information
- Store payment records
- Validate payment details
- Associate payments with bookings
- Display payment status

Card numbers are validated as **16 digits** using the standard:

```text
1234 5678 9012 3456
```

### Ticket Management

- Generate tickets after successful payment
- Maintain ticket information
- Display ticket details
- Print tickets
- Track ticket availability
- Update remaining ticket quantities dynamically

### Organizer Revenue

The organizer workspace displays total revenue based on successful event bookings and payments.

### Partner Event Integration

The system supports events published by external organizers.

The External Organizer API communicates with the KMC API to:

- Synchronize partner events
- Share event information
- Update bookings
- Update remaining ticket quantities
- Maintain availability information

---

## 🏗️ System Architecture

The system follows a **Service-Oriented Architecture (SOA)**.

```text
                    ┌─────────────────────────┐
                    │       KMC Website       │
                    │ Public / Participant /  │
                    │       Organizer         │
                    └────────────┬────────────┘
                                 │
                                 │ HTTP / JSON
                                 ▼
                    ┌─────────────────────────┐
                    │         KMC API         │
                    │                         │
                    │ Authentication          │
                    │ Event Management        │
                    │ Event Search            │
                    │ Registration            │
                    │ Payment                 │
                    │ Ticketing               │
                    │ Partner Events          │
                    └────────────┬────────────┘
                                 │
                                 │ Entity Framework Core
                                 ▼
                    ┌─────────────────────────┐
                    │      KMC Database       │
                    │       SQL Server        │
                    └─────────────────────────┘


        External Organizer Platform

┌─────────────────────────┐
│ External Organizer Web  │
└────────────┬────────────┘
             │
             │ HTTP / JSON
             ▼
┌─────────────────────────┐
│     Organizer API       │
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│   Organizer Database    │
│      SQL Server         │
└─────────────────────────┘


        Organizer API  <──────────────>  KMC API

               Event Synchronization
               Booking Synchronization
```

---

## 🔧 Implemented Services

### 1. Authentication Service

Responsible for:

- User registration
- User login
- Password validation
- JWT token generation
- User identity management
- Role-based authorization

---

### 2. Event Management Service

Responsible for:

- Creating events
- Updating events
- Publishing events
- Retrieving event information
- Deleting events
- Maintaining event availability

---

### 3. Event Search Service

Responsible for:

- Searching available events
- Filtering events
- Searching by date
- Searching by event type
- Searching by venue
- Searching by location
- Keyword-based search

---

### 4. Participant Registration Service

Responsible for:

- Registering participants
- Maintaining booking information
- Checking ticket availability
- Preventing invalid registrations
- Processing registration cancellation

---

### 5. Payment Service

Responsible for:

- Creating payment records
- Validating payment information
- Linking payments to bookings
- Maintaining payment status
- Supporting revenue calculations

---

### 6. Ticket Service

Responsible for:

- Generating tickets
- Maintaining ticket records
- Displaying ticket information
- Printing tickets
- Updating available ticket quantities

---

### 7. Partner Event Service

Responsible for:

- Receiving external organizer events
- Synchronizing partner events
- Maintaining external event information
- Updating booking information between systems
- Updating ticket availability between systems

---

## 💻 Technologies Used

### Backend

- ASP.NET Core
- .NET 10
- ASP.NET Core Web API
- C#
- REST API
- JSON

### Frontend

- ASP.NET Core MVC
- Razor Views
- HTML5
- CSS3
- JavaScript

### Database

- Microsoft SQL Server
- SQL Server LocalDB
- Entity Framework Core

### Authentication

- JSON Web Token (JWT)
- ASP.NET Core Authentication
- Role-Based Authorization

### Development Tools

- Microsoft Visual Studio
- Visual Studio Code
- SQL Server / LocalDB
- Git
- GitHub

---

## 📦 Main NuGet Packages

The project uses packages including:

- `AutoMapper`
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `Microsoft.AspNetCore.OpenApi`
- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Design`
- `Microsoft.EntityFrameworkCore.Tools`
- `Microsoft.OpenApi`
- `Scalar.AspNetCore`

The application was developed using **ASP.NET Core on .NET 10**.

---

## 📁 Project Structure

```text
KMCEventPlatform/
│
├── KMC/
│   │
│   ├── API/
│   │   │
│   │   ├── KMC.Api/
│   │   │   ├── Controllers/
│   │   │   ├── Data/
│   │   │   ├── DTOs/
│   │   │   ├── Entities/
│   │   │   ├── Infrastructure/
│   │   │   ├── Interfaces/
│   │   │   ├── Mapping/
│   │   │   ├── Middleware/
│   │   │   ├── Migrations/
│   │   │   ├── OpenApi/
│   │   │   ├── Properties/
│   │   │   ├── Services/
│   │   │   ├── Program.cs
│   │   │   ├── appsettings.json
│   │   │   └── KMC.Api.csproj
│   │   │
│   │   └── KMCEventPlatform.Api.slnx
│   │
│   ├── Desktop/
│   │   └── KMC.OrganizerDesktop/
│   │
│   └── Website/
│       └── KMC.Web/
│
├── OrganizerWebsite/
│   │
│   ├── API/
│   │   └── Organizer.Api/
│   │       ├── Controllers/
│   │       ├── Data/
│   │       ├── DTOs/
│   │       ├── Entities/
│   │       ├── Interfaces/
│   │       ├── Migrations/
│   │       ├── Properties/
│   │       ├── Services/
│   │       ├── Program.cs
│   │       ├── appsettings.json
│   │       └── Organizer.Api.csproj
│   │
│   └── Organizer.Web/
│       ├── Infrastructure/
│       ├── Models/
│       ├── Pages/
│       ├── Properties/
│       ├── Services/
│       ├── wwwroot/
│       ├── Program.cs
│       └── Organizer.Web.csproj
│
├── DEMO_LOGIN_DETAILS.txt
│
└── README.md
```

---

## 🗄️ Database

The platform uses separate databases for the KMC platform and external organizer platform.

### KMC Database

Stores information related to:

- Users
- Events
- Participant registrations
- Tickets
- Payments
- Partner events
- Event availability
- Booking information

### External Organizer Database

Stores information related to:

- External organizer events
- Partner event details
- Bookings
- Participants
- Ticket availability
- Payment-related information

Entity Framework Core is used as the Object Relational Mapper between the ASP.NET Core applications and SQL Server.

---

## 🔄 Partner Event Workflow

```text
External Organizer
        │
        ▼
Create Partner Event
        │
        ▼
Organizer API
        │
        │ Event Synchronization
        ▼
KMC API
        │
        ▼
KMC Platform
        │
        ▼
Public User Views Event
        │
        ▼
User Signs In
        │
        ▼
Book Event
        │
        ▼
Payment
        │
        ▼
Ticket Generated
        │
        ▼
Booking Update
        │
        ▼
KMC API
        │
        ▼
Organizer API
        │
        ▼
External Organizer System Updated
```

---

## 🔄 KMC Event Booking Workflow

```text
Public Visitor
      │
      ▼
Explore Events
      │
      ▼
View Event Details
      │
      ▼
Sign In / Register
      │
      ▼
Select Event
      │
      ▼
Book Event
      │
      ▼
Check Availability
      │
      ▼
Make Payment
      │
      ▼
Payment Successful
      │
      ▼
Generate Ticket
      │
      ▼
View / Print Ticket
```

---

## 🚦 Event Status

Events may use statuses such as:

- Draft
- Published
- Approved
- Available
- Completed
- Cancelled

Only appropriate published or available events are displayed to public users.

---

## 🎟️ Ticket Availability

Ticket availability is dynamically calculated based on:

```text
Remaining Tickets = Total Tickets - Confirmed Bookings
```

The system updates the available ticket quantity as bookings are completed.

This prevents users from purchasing tickets when an event has reached its maximum capacity.

---

## 💰 Revenue Calculation

Organizer revenue is calculated using successful payments associated with event bookings.

```text
Total Revenue = Sum of Successful Event Payments
```

The organizer dashboard displays the calculated revenue to help organizers monitor event performance.

---

## 🎨 User Interface

The system uses a modern dark-themed user interface.

The main interface colors are used consistently for different actions:

- 🟣 Purple – Primary actions and active navigation
- 🔵 Cyan – Links, information and synchronization
- 🟡 Yellow – Ticket prices and featured information
- 🟢 Green – Published, approved and confirmed status
- 🔴 Red – Delete, cancel, failed and logout actions

---

## ⚙️ Prerequisites

Before running the application, install:

- .NET 10 SDK
- Microsoft Visual Studio 2022 or later with ASP.NET development tools
- Microsoft SQL Server or SQL Server LocalDB
- Git

Check the installed .NET version using:

```bash
dotnet --version
```

---

## 📥 Clone the Repository

Clone this repository using:

```bash
git clone https://github.com/akash987isuru-hub/KMCEventPlatform.git
```

Move into the project directory:

```bash
cd KMCEventPlatform
```

---

## 📦 Restore Dependencies

Restore NuGet packages using:

```bash
dotnet restore
```

Alternatively, Visual Studio can automatically restore the required NuGet packages when the solution is opened.

---

## 🗃️ Database Configuration

Configure the database connection strings inside the relevant `appsettings.json` files.

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=KMCEventPlatformDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

Use your own local database configuration where required.

---

## 🛠️ Entity Framework Core Migrations

If database migrations need to be applied, navigate to the appropriate API project and run:

```bash
dotnet ef database update
```

If the Entity Framework CLI tools are not installed:

```bash
dotnet tool install --global dotnet-ef
```

---

## ▶️ Running the Application

The complete system contains multiple applications.

For the full platform, run the required projects including:

1. KMC API
2. KMC Website
3. Organizer API
4. External Organizer Website

The organizer desktop application can also be started separately where required.

### Run using Visual Studio

1. Clone or download the repository.
2. Open the appropriate solution/project in Visual Studio.
3. Restore NuGet packages.
4. Configure database connection strings.
5. Apply database migrations.
6. Set the required startup projects.
7. Run the application.

### Run using .NET CLI

Navigate to the required project directory.

Example:

```bash
cd KMC/API/KMC.Api
```

Then run:

```bash
dotnet run
```

Repeat the process for the other required applications.

---

## 🔐 Authentication

The KMC API uses JWT-based authentication.

The authentication flow is:

```text
User Login
    │
    ▼
Validate Credentials
    │
    ▼
Generate JWT Token
    │
    ▼
Return Token
    │
    ▼
Client Stores Token
    │
    ▼
Token Sent With Protected Requests
    │
    ▼
API Validates Token and Role
```

Role-based authorization ensures that users can only access functions permitted for their role.

---

## 🔒 Security

Sensitive information should never be committed to a public GitHub repository.

Do not commit:

```text
Database passwords
JWT secret keys
API keys
Production credentials
Private user information
```

Development-specific configuration can be placed inside:

```text
appsettings.Development.json
```

and excluded from Git using `.gitignore`.

---

## 🚫 Files Excluded from Git

Generated Visual Studio and .NET build files should not be committed.

Examples:

```text
.vs/
bin/
obj/
*.user
*.suo
TestResults/
```

These files are generated automatically during development and application builds.

---

## 🧪 API Testing

API endpoints can be tested using:

- Scalar API Reference
- HTTP files
- Browser-based API tools
- Other REST API testing tools

The API communicates using JSON request and response objects.

Example response:

```json
{
  "success": true,
  "message": "Operation completed successfully"
}
```

---

## 🌐 API Communication

Applications communicate using REST-style HTTP requests.

Common HTTP methods include:

| HTTP Method | Purpose |
|-------------|---------|
| GET | Retrieve information |
| POST | Create new information |
| PUT | Update existing information |
| DELETE | Delete information |

The APIs exchange data primarily using JSON.

---

## 🔗 Service Communication

The platform supports communication between independent applications.

```text
KMC Web
   │
   └──── HTTP/JSON ────► KMC API

Organizer Desktop
   │
   └──── HTTP/JSON ────► KMC API

External Organizer Web
   │
   └──── HTTP/JSON ────► Organizer API

Organizer API
   │
   └──── HTTP/JSON ────► KMC API
```

This architecture allows individual components to be maintained and extended with reduced coupling.

---

## ✅ Benefits of the Architecture

The Service-Oriented Architecture provides several benefits:

- Separation of concerns
- Reusable services
- Easier maintenance
- Independent system components
- Improved scalability
- Better external system integration
- Reduced application coupling
- Easier future expansion
- Centralized business logic through APIs

---

## 📚 Academic Context

This project was developed as an academic implementation of **Service-Oriented Computing (SOC)** concepts.

It demonstrates how independent applications can communicate through reusable web services while maintaining separate responsibilities and databases.

The implementation demonstrates concepts including:

- Service-Oriented Architecture
- REST API development
- API consumption
- Authentication and authorization
- Database integration
- External system integration
- Service reusability
- Separation of concerns
- Maintainability
- Interoperability

---

## 🚀 Future Improvements

Possible future improvements include:

- Email notifications
- SMS notifications
- QR-code ticket validation
- Mobile application support
- Advanced organizer analytics
- Interactive reporting
- Refund processing
- Advanced payment gateway integration
- Event recommendation system
- Automated event notifications
- Cloud deployment
- Real-time notifications
- Advanced admin dashboard
- Event attendance tracking

---

## 👨‍💻 Author

**Isuru Akash**

GitHub:  
[akash987isuru-hub](https://github.com/akash987isuru-hub)

Repository:  
[KMCEventPlatform](https://github.com/akash987isuru-hub/KMCEventPlatform)

---

## 📄 License

This project was created primarily for educational and academic purposes.

Unless otherwise specified, the source code should not be redistributed or used commercially without permission from the author.

---

## ⭐ Repository

If you are reviewing this project, the repository contains the complete implementation of the KMC Event Platform including the KMC services, client applications and external organizer integration.

**KMC Event Platform – Service-Oriented Event Management System**
