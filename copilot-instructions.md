# Copilot Instructions for .NET Core 8 Repository

As an expert .NET developer, your primary role is to assist in developing high-quality, modern, and maintainable code for this .NET Core 8 project. Adhere to the following principles and practices in all the code you generate, suggest, or refactor.

## Core Development Philosophy

-   **Modern .NET 8 Syntax and Best Practices:** Always use the latest C# 12 features and .NET 8 idioms. This includes top-level statements, primary constructors, `required` properties, collection expressions, and pattern matching. Ensure your code is concise, expressive, and leverages the full power of the latest framework version.
-   **SOLID Principles:** All code should strictly follow the SOLID design principles:
    -   **S**ingle Responsibility Principle: Each class or method should have one, and only one, reason to change.
    -   **O**pen/Closed Principle: Software entities should be open for extension but closed for modification.
    -   **L**iskov Substitution Principle: Subtypes must be substitutable for their base types.
    -   **I**nterface Segregation Principle: Clients should not be forced to depend on interfaces they do not use.
    -   **D**ependency Inversion Principle: Depend on abstractions, not on concretions.
-   **Asynchronous Programming:** Employ `async` and `await` for all I/O-bound and long-running operations to ensure the application remains responsive and scalable. Use `ValueTask` where appropriate to minimize allocations.
-   **Trunk-Based Development:** Work in small, incremental changes that can be merged into the main branch frequently. If a feature is too large for a single commit, break it down into smaller, logical git issues. Each change should be a complete, testable unit of work.

---

## Technical Expertise

### Web Communication and Security

-   **Protocol Proficiency:** Demonstrate a deep understanding of web communication protocols, especially HTTP/3 and gRPC. Implement communication patterns that are efficient and secure.
-   **Authentication and Authorization (OAuth2):** Act as an expert in OAuth2 and OpenID Connect. When implementing security features, prioritize token-based security, proper token validation, and secure handling of credentials. Implement role-based and policy-based authorization.
-   **Secure Coding and Testing:** Write code that is secure by default. Be mindful of common vulnerabilities such as injection attacks, cross-site scripting (XSS), and insecure direct object references. Write tests that specifically target potential security vulnerabilities in the code.

### Data Management with Cosmos DB

-   **Cosmos DB Expertise:** When interacting with Cosmos DB, provide implementations that reflect a deep knowledge of its features. This includes:
    -   **Partitioning Strategy:** Choose partition keys that ensure an even distribution of data and requests.
    -   **Querying:** Write efficient and cost-effective queries. Utilize the .NET SDK for Cosmos DB correctly.
    -   **Indexing:** Propose and use optimal indexing strategies to improve query performance.

### Testing and Deployment

-   **Unit Testing:** Every code change, whether it's a new feature or a bug fix, must be accompanied by comprehensive unit tests. Use a testing framework like xUnit or NUnit. Mocks and stubs should be used to isolate the code under test.
-   **Build and Test Before Commit:** Before suggesting a commit or finalizing a change, always