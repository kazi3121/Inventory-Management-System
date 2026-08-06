# 📦 Inventory Management System (IMS) API

An enterprise-grade **Inventory Management System (IMS)** Web API built with **.NET 8**, adhering strictly to **Clean Architecture** principles, **CQRS (Command Query Responsibility Segregation)**, and **SOLID** software design patterns.

---

## 🌟 Key Features

- **🔐 Authentication & Role-Based Authorization (JWT):** Secure user registration and login flow with JSON Web Tokens and role mapping (`User` $\rightarrow$ `AuthResponseDto`).

- **📦 Product Management (CRUD):** Complete lifecycle management for inventory items, including business validation for SKU uniqueness and category linking.

- **🏷️ Category Management (CRUD):** Dynamic product categorization with strict business guardrails (e.g., prevention of duplicate category names and deletion protection for categories containing linked products).

- **⚡ MediatR & CQRS Architecture:** Complete segregation of read (Queries) and write (Commands) operations dispatched via `MediatR` pipelines.

- **🔍 Advanced Pagination & Filtering:** Server-side pagination (`Skip`/`Take`) and multi-parameter filtering (`searchTerm`, `categoryId`, `minPrice`, `maxPrice`) implemented in EF Core repository methods (`GetPagedAsync`).

- **🛡️ Centralized Exception & Error Handling:** Custom global middleware (`ExceptionHandlingMiddleware`) mapping domain exceptions (`ValidationException`, `KeyNotFoundException`, `InvalidOperationException`) to standardized HTTP status codes (`400`, `401`, `404`, `500`).

- **📊 Logging & Observability:** Integrated `ILogger<T>` structured logging throughout pipeline execution and command handlers.

- **💾 EF Core & SQL Persistence:** High-performance database operations using Entity Framework Core, LINQ optimization (`AsNoTracking`, `Include`), and the Repository Pattern.

- **🗺️ AutoMapper Integration:** Clean DTO-to-Domain mapping profiles handling complex data flattening (e.g., mapping `Category.Name` directly to `ProductDto.CategoryName`).

- **🧪 Comprehensive Unit Testing:** Robust unit test suite built with **xUnit**, **Moq**, and **FluentAssertions** covering handler logic, validation rules, and error scenarios.

---

## 🏛️ Architecture & Clean Code Principles

The project follows **Clean Architecture** divided into four distinct layers:

1. **Domain Layer:** Enterprise entities (`Product`, `Category`, `User`), Enums, and base abstractions (`BaseEntity`). Free of external dependencies.

2. **Application Layer:** Core business logic, DTOs, Mapping Profiles (`AutoMapper`), Interfaces, and MediatR Commands/Queries & Handlers.

3. **Infrastructure Layer:** Database context (`ApplicationDbContext`), Repository implementations (`ProductRepository`, `CategoryRepository`), and external service integrations.

4. **API Layer:** ASP.NET Core Web API Controllers, Custom Middlewares, Swagger configuration, and Application Entry Point.

### Applied Design Patterns & Principles

- **CQRS via MediatR:** Decouples API endpoints from business logic processing.

- **Repository Pattern:** Abstracts EF Core data access operations behind clean interface contracts (`IProductRepository`, `ICategoryRepository`).

- **SOLID Principles:** Single Responsibility Handlers, Open/Closed extensibility, Interface Segregation, and Dependency Inversion.

---

## 📁 Project Structure

```text
InventoryManagementSystem/                             
├── .gitignore                   
├── README.md
├── InventoryManagementSystem.slnx
├── src/
│   ├── IMS.API/
│   ├── IMS.Application/
│   ├── IMS.Domain/
│   └── IMS.Infrastructure/
└── tests/
    └── IMS.UnitTests/