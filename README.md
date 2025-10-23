Brewery API  A scalable, enterprise-level RESTful API for brewery information with caching, authentication, search, and sorting
 capabilities.

Features:
 RESTful API endpoints for brewery data

 In-memory caching with 10-minute expiration
 
 SQLite database with Entity Framework Core

 Classes and Interfaces with Dependency Injection

 Sorting by brewery name, distance, and city
 
 Search functionality

 Data mapping/transformation from OpenBreweryDB

 SOLID principles implementation

 Comprehensive error handling
 Autocomplete search functionality
 
 API versioning (V1 and V2)

 Structured logging with Serilog

 SQLite database with EF Core
 
 JWT-based authentication/security
 
 Architecture & Design Principles
 SOLID Principles Implementation
 1. Single Responsibility Principle (SRP)
 Each class has one responsibility
 BreweryRepository: Data access only
 BreweryService: Business logic only
 BreweryDataService: External API integration only
 ErrorHandlingMiddleware: Global error handling only
 2. Open/Closed Principle (OCP)
Services are open for extension through interfaces
 New data sources can be added without modifying existing code
 New sorting/filtering strategies can be added easily
 3. Liskov Substitution Principle (LSP)
 All implementations properly substitute their interfaces
 IBreweryRepository implementations are interchangeable
 4. Interface Segregation Principle (ISP)
 Small, focused interfaces
 Clients depend only on methods they use
 5. Dependency Inversion Principle (DIP)
 High-level modules depend on abstractions (interfaces)
 Dependency injection throughout
 Project Structure
 BreweryApi/
 ├── Controllers/
 │   ├── V1/BreweriesController.cs    # API Version 1
 │   ├── V2/BreweriesController.cs    # API Version 2
 │   └── AuthController.cs            # Authentication
 ├── Data/
 │   └── BreweryDbContext.cs          # EF Core DbContext
 ├── Interfaces/
 │   ├── IBreweryService.cs           # Business logic contract
 │   ├── IBreweryRepository.cs        # Data access contract
 │   └── IBreweryDataService.cs       # External API contract
 ├── Middleware/
 │   └── ErrorHandlingMiddleware.cs   # Global exception handling
 ├── Models/
 │   └── Brewery.cs                   # Domain models & DTOs
 ├── Services/
 │   ├── BreweryService.cs            # Business logic implementation
 │   ├── BreweryRepository.cs         # Data access implementation
 │   ├── BreweryDataService.cs        # External API client
 │   └── BreweryDataSyncService.cs    # Background sync service
 ├── Program.cs                       # Application entry point
 ├── appsettings.json                 # Configuration
 └── BreweryApi.csproj               # Project file


 Technology Stack
 .NET 8.0 - Latest LTS framework
 ASP.NET Core Web API - RESTful API framework
 Entity Framework Core 8.0 - ORM
 SQLite - Lightweight database
Serilog - Structured logging
 JWT Bearer Authentication - Security
 Swagger/OpenAPI - API documentation
 Microsoft.Extensions.Caching.Memory - In-memory caching


 Getting Started


 The API will be available at:
HTTPS: https://localhost:7001
HTTP: http://localhost:5001
Swagger UI: https://localhost:7001/swagger
 First-Time Setup
 1. Generate an authentication token
 bash
 curl -X POST https://localhost:7001/api/auth/token \
  -H "Content-Type: application/json" \
  -d '{"username":"demo","password":"demo123"}'
 Response:
 json
 {
 }
 "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
 "expiresIn": 3600,
 "tokenType": "Bearer"
 2. Use the token in subsequent requests
 bash
 curl -X GET https://localhost:7001/api/v1/breweries \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"

 API Documentation
 Authentication
 POST /api/auth/token
 Generate a JWT token for API access.
 Request:
json
 {
 }
 "username": "demo",
 "password": "demo123"
 Response:
 json
 {
 }
 "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
 "expiresIn": 3600,
 "tokenType": "Bearer"
 Brewery Endpoints (V1)
 GET /api/v1/breweries
 Get paginated list of breweries with filtering and sorting.
 Query Parameters:
 searchTerm (string, optional): Search in name, city, or state
 city (string, optional): Filter by city
 state (string, optional): Filter by state
 sortBy (string, default: "name"): Sort by "name", "city", or "distance"
 ascending (bool, default: true): Sort order
 page (int, default: 1): Page number
 pageSize (int, default: 50): Items per page
 userLatitude (double, optional): User latitude for distance calculation
 userLongitude (double, optional): User longitude for distance calculation
 Example Request:
 bash
 GET /api/v1/breweries?searchTerm=IPA&sortBy=name&page=1&pageSize=20
 Response:
 json
{
 "data": [
 {
 "id": "5128df48-79fc-4f0f-8b52-d06be54d0cec",
 "name": "(405) Brewing Co",
 "city": "Norman",
 "phone": "4058160490",
 "state": "Oklahoma",
 "country": "United States",
 "websiteUrl": "http://www.405brewing.com",
 "breweryType": "micro",
 "distance": null
 }
 ],
 "page": 1,
 "pageSize": 20,
 "totalCount": 150,
 "totalPages": 8,
 "hasPreviousPage": false,
 "hasNextPage": true
 }
 GET /api/v1/breweries/{id}
 Get a specific brewery by ID.
 Response:
 json
 {
 }
 "id": "5128df48-79fc-4f0f-8b52-d06be54d0cec",
 "name": "(405) Brewing Co",
 "city": "Norman",
 "phone": "4058160490",
 "state": "Oklahoma",
 "country": "United States",
 "websiteUrl": "http://www.405brewing.com",
 "breweryType": "micro"
 GET /api/v1/breweries/autocomplete
Autocomplete search for brewery names.
 Query Parameters:
 term (string, required): Search term
 limit (int, default: 10): Maximum results
 Example Request:
 bash
 GET /api/v1/breweries/autocomplete?term=stone&limit=5
 Response:
 json
 [
 ]
 {
 }
 "id": "stone-brewing-san-diego",
 "name": "Stone Brewing",
 "city": "San Diego",
 "displayText": "Stone Brewing - San Diego, California"
 Brewery Endpoints (V2)
 V2 endpoints have the same functionality as V1 but include additional metadata in response headers:
 X-Total-Count: Total number of items
 X-Page-Count: Total number of pages
 X-Current-Page: Current page number
 Base URL: /api/v2/breweries


 Design Decisions
 1. Caching Strategy
 Decision: Two-tier caching with in-memory cache and database
 Reasoning:
 In-memory cache provides fast response times (microseconds)
 Database cache persists across application restarts
 10-minute cache expiration balances data freshness and API calls
 Background service automatically refreshes data

Implementation:
 csharp
 // Cache check in repository
 if (_cache.TryGetValue(CacheKey, out List<Brewery>? cachedBreweries))
 {
 return cachedBreweries;
 }
 // Fallback to database
 var breweries = await _context.Breweries.ToListAsync();
 _cache.Set(CacheKey, breweries, TimeSpan.FromMinutes(10));
 2. Data Mapping/Transformation
 Decision: Explicit mapping from source API to generic domain model
 Reasoning:
 Decouples internal model from external API changes
 Allows for data cleansing and normalization
 Enables easy switching of data sources
 Follows DTOs pattern for API responses
 Implementation:
 csharp
 private Brewery MapToBrewery(OpenBreweryDbResponse source)
 {
 return new Brewery
 {
        Id 
= source.Id,
        Name = source.Name ?? string.Empty,
        Phone = CleanPhoneNumber(source.Phone),
 // ... transformation logic
 };
 }
 3. Distance Calculation
 Decision: Haversine formula for geographic distance
 Reasoning:
 Industry-standard formula for calculating distances
Accurate for short to