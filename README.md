# TaskFlow

TaskFlow is a To-Do application inspired by Microsoft To-Do. The project was developed as a learning project to demonstrate backend architecture with ASP.NET Core Web API and frontend development using Angular.

## Features

* User registration and authentication (JWT)
* Login
* Search tasks
* Filter tasks by category
* Sort tasks
* Pagination

## Technologies

### Backend

* ASP.NET Core Web API
* Entity Framework Core
* MS SQL Server
* JWT Authentication
* xUnit
* Moq
* Swagger

### Frontend

* Angular
* Bootstrap
* TypeScript

## Architecture

Backend follows a classic 4-layer architecture:

* Controllers
* Services
* Interfaces
* Data Access (Repositories)

Entity Framework Core is used as ORM with SQL Server.

## Running the project

### Backend

1. Clone the repository.

2. Open the solution in Visual Studio.

3. Configure the database connection string inside:

```
TaskFlow.API/appsettings.json
```

```
Database=<your-own-local-db-name>
```


4. Apply migrations:

```
dotnet ef database update --project TaskFlow.Infrastructure --startup-project TaskFlow.API
```

5. Run the API.

Swagger will be available at:

```
https://localhost:xxxx/swagger
```

---

### Frontend

Navigate to the Angular project:

```
cd TaskFlow.Frontend
```

Install packages:

```
npm install
```

Run the application:

```
ng serve
```

Open:

```
http://localhost:4200
```

## Unit Tests

The service layer is covered with unit tests using xUnit and Moq.

Run tests using:

```
dotnet test
```

## Repository Structure

```
TaskFlow
│
├── TaskFlow.API
├── TaskFlow.Domain
├── TaskFlow.Infrastructure
├── TaskFlow.Tests
└── TaskFlow.Frontend
```

## Author

Developed by Yuriy as a personal learning project.
