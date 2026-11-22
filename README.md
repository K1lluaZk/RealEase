# RealEase
RealEase is a comprehensive real estate management platform that simplifies the listing of properties, scheduling of visits, rental contract administration, and rent payment processing.

## Features

### Property Listings: Allows real estate agents to list properties with detailed descriptions and images.

### Visit Scheduling: Efficiently manages property visit appointments.

### Contract Management: Generates and stores rental contracts securely.

### Rent Payments: Manages rental payments and keeps track of all transactions.

### filters: The filter allows users to search and retrieve specific user data based on provided criteria, such as name or role, helping to quickly find relevant records.

## Tech Stack

### Language: C#

### Frontend: HTML, CSS, JavaScript

### Backend: ASP.NET Core

### Database: SQL Server

## Installation and Usage

1- Clone the repository:
  git clone https://github.com/1MarioSR/RealEase.git
  
2- Open the solution in Visual Studio.  

3 - Restore the NuGet packages.

4 - Configure the database connection string in appsettings.json.

5 - dotnet ef database update

6 - dotnet run

## API Documentation (Preview)

The API follows RESTful principles and uses DTOs (Data Transfer Objects) for communication.

## Authentication and roles

Sign-in uses the existing Supabase Auth account. The Web app keeps the session in a protected, HttpOnly cookie and forwards the access token server-to-server; the API validates the token with Supabase and checks the matching active user profile before authorizing a request.

The application maps the existing stored roles to its access roles without changing the database enum:

- `guest` → `client`: browse properties, schedule visits, and view their own visits, contracts, and payments.
- `host` → `agent`: manage visits for properties they own and contracts assigned to them; agents cannot delete contracts or change contract approval status.
- `administrator` → `admin`: manage all records and assign supported roles.

Accounts must already exist in Supabase Auth and have a matching active user profile. Provision the first administrator through a trusted administrative process; the public login does not create accounts or grant roles. Contract approval remains restricted until a trusted payment-verification flow is implemented.

Both `RealEase.Web` and `RealEase.API` require `ConnectionStrings:DefaultConnection`. Configure it as the `ConnectionStrings__DefaultConnection` environment variable with a PostgreSQL key/value connection string for Supabase, for example `Host=<host>;Port=5432;Database=<database>;Username=<user>;Password=<password>;SSL Mode=Require`. Do not use a `postgres://` URL or commit credentials to `appsettings.json`.

`RealEase.Web` uses `http://localhost:5035/` for the API when running in Development, matching the API's local launch profiles. In other environments, set `Api__BaseUrl` to the HTTPS base URL of `RealEase.API`.

### Public property browsing

The customer pages separate properties for sale from rentals. Apply the additive marketplace migration in `sql/20261114_customer_marketplace.sql` (or the matching EF Core migration) before deploying the updated API and Web app. Existing properties default to rentals to preserve their current behavior. Set `listing_type` to `sale` and provide `sale_price` for sale listings; add each listing's city. For location-based seller inquiries, set each agent's `service_area` to one or more comma-separated city/area names that match the location entered by customers. Rental inquiries are routed to the agent assigned as the property's owner when that user has the agent role.

## Endpoints

GET /api/users
Returns a list of all user.

GET /api/users/{id}
Returns the details of a specific user.

POST /api/users
Creates a new user.

PUT /api/users/{id}
Updates a user listing.

DELETE /api/users/{id}
Deletes a user.

GET /api/users/filter
filters for fields

## Contributing

Contributions are welcome! If you’d like to help improve RealEase, please:

1 - Fork the repo.

2 - Create a new branch.

3 - Make your changes and test them.

4 - Open a pull request with a clear explanation.

## Screenshot



## License
This project is licensed under the MIT License. See the LICENSE file for details.

## Updates may be made in the future.
