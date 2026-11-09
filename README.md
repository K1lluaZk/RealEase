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

![image](https://github.com/user-attachments/assets/b8fedc59-cffd-47bd-b28e-b1a69bf0dc12)

![image](https://github.com/user-attachments/assets/1c558658-4700-4c72-9f7c-bc225c8a1cbe)

![image](https://github.com/user-attachments/assets/009621ca-894c-4414-b1dd-a30285acafa0)

![image](https://github.com/user-attachments/assets/151a192d-d7c2-4bba-9ee6-dc6b100a6958)

![image](https://github.com/user-attachments/assets/ac52ace9-8561-459c-bb7a-2939aaf3e730)




## License
This project is licensed under the MIT License. See the LICENSE file for details.

## Updates may be made in the future.
