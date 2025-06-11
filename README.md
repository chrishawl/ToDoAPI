# ToDoAPI

A .NET 8 Web API for managing Todo items with support for multiple database providers.

## Features

- RESTful API for Todo items (Create, Read, Update, Delete)
- Configurable database providers:
  - **InMemory Database** (default) - for development and testing
  - **Azure Cosmos DB** - for production scenarios
- Clean architecture with repository pattern
- Dependency injection support

## Configuration

### Using InMemory Database (Default)

No additional configuration required. The API will use an in-memory database by default.

```json
{
  "DatabaseProvider": "InMemory"
}
```

### Using Azure Cosmos DB

Update your `appsettings.json` or `appsettings.Development.json`:

```json
{
  "DatabaseProvider": "CosmosDb",
  "CosmosDb": {
    "ConnectionString": "your-cosmos-db-connection-string",
    "DatabaseName": "TodoDB",
    "ContainerName": "Todos"
  }
}
```

## API Endpoints

- `GET /todoitems` - Get all todo items
- `GET /todoitems/complete` - Get completed todo items
- `GET /todoitems/{id}` - Get a specific todo item
- `POST /todoitems` - Create a new todo item
- `PUT /todoitems/{id}` - Update a todo item
- `DELETE /todoitems/{id}` - Delete a todo item

## Running the Application

```bash
dotnet run
```

The API will be available at `http://localhost:5236` (or the port shown in the console output).