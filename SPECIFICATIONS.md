# Application Specification: RobRequest, a Blazor API Client (Simplified Postman Clone)

## Executive Summary
A lightweight, modern REST API development client built with .NET 10 Blazor Web Application using **Interactive Server** rendering. This browser-based tool provides developers with a fast, intuitive interface for testing and debugging RESTful APIs, serving as a streamlined alternative to heavyweight desktop clients like Postman. The server-side rendering model enables full access to .NET server capabilities — including EF Core with SQLite for persistent storage and unrestricted `HttpClient` usage — while delivering a rich, interactive UI over a SignalR connection.

---

## 1. Project Overview

### Vision
To create a responsive, feature-rich API testing tool that leverages modern web technologies while maintaining simplicity and performance.

### Core Value Propositions
- **Browser-native**: No installation required, works instantly in any modern browser
- **Modern Tech Stack**: Built on .NET 10 Blazor with MudBlazor for a polished UI
- **Performance Optimized**: Near-instant initial load (no large WASM payload); UI interactions processed server-side and pushed via SignalR
- **Server-Side Power**: Full access to .NET libraries, EF Core, and unrestricted `HttpClient` — no browser sandbox or CORS limitations
- **Developer Focused**: Designed by developers, for developers with essential features prioritized

### Success Criteria
- < 1 second initial page load on standard broadband (no WASM download)
- Support for 95% of common API testing workflows
- Intuitive UI requiring < 5 minutes for first-time users
- Responsive design working seamlessly on desktop and tablet devices

---

## 2. Target Personas

### Primary Users
- **Frontend Developers**: Testing API integrations during frontend development
- **Backend Developers**: Validating endpoint functionality and responses
- **QA Engineers**: Performing manual API verification and regression testing

### Secondary Users
- **DevOps Engineers**: Quick health checks and monitoring endpoint availability
- **Technical Writers**: Documenting API behavior and examples
- **Students/learners**: Understanding REST API concepts through hands-on testing

### Use Case Scenarios
- Rapid prototyping during development sprints
- API contract verification before integration
- Performance testing and response time analysis
- Debugging authentication and authorization flows
- Creating shareable API documentation examples

---

## 3. Core Features

### 3.1 Request Builder

#### HTTP Methods
- **Standard Methods**: GET, POST, PUT, PATCH, DELETE, HEAD, OPTIONS
- **Custom Methods**: Support for any custom HTTP method (e.g., PROPFIND, REPORT)
- **Method Persistence**: Remember last used method per session

#### URL Management
- **Smart Input**: Auto-complete for previously used URLs
- **Validation**: Real-time URL format validation with error indicators
- **Query Parameters**: 
  - Automatic parsing from URL into editable grid
  - Dynamic add/remove functionality
  - URL encoding/decoding handled automatically
  - Bulk parameter import from query string

#### Request Headers
- **Common Headers**: Quick-add dropdown for frequently used headers (Content-Type, Accept, Authorization)
- **Custom Headers**: Dynamic key-value pair management
- **Header Validation**: Warning for potentially problematic headers
- **Header Presets**: Save and reuse header sets

#### Request Body Types
- **JSON**: 
  - Syntax highlighting and validation
  - Auto-formatting on paste
  - JSON schema validation (optional)
- **Form Data**: 
  - Key-value pairs with file upload support
  - Multiple file selection
  - File type validation
- **Raw Text**: 
  - Plain text, XML, CSV, or custom formats
  - Line ending normalization
- **Binary**: File upload for binary data (future enhancement)
- **None**: Explicit no-body option for GET/HEAD requests

#### Authentication (Phase 4)
- **No Auth**: Default option
- **Bearer Token**: OAuth/JWT token input
- **Basic Auth**: Username/password with base64 encoding
- **API Key**: Custom header or query parameter placement
- **OAuth 2.0**: Full OAuth2 flow (future enhancement)

### 3.2 Response Viewer

#### Status Information
- **HTTP Status Code**: Color-coded status (2xx green, 3xx blue, 4xx orange, 5xx red)
- **Response Time**: Precise timing in milliseconds with performance indicators
- **Response Size**: Human-readable file size (KB, MB) for response body
- **Request Summary**: Brief overview of method, URL, and timestamp

#### Response Body Viewer
- **Format Detection**: Automatic content-type detection and appropriate formatting
- **JSON/XML**: Pretty-printed with collapsible/expandable nodes
- **HTML**: Rendered preview with safe iframe sandbox
- **Images**: Direct image display for common formats (PNG, JPG, GIF, SVG)
- **Binary**: Hex view with download option
- **Raw View**: Unformatted response text
- **Search Functionality**: 
  - Text search with highlighting
  - Regular expression support
  - Case-sensitive/insensitive options

#### Response Headers
- **Sortable Table**: Click headers to sort by name or value
- **Filter Options**: Search/filter specific headers
- **Header Analysis**: Security and performance insights (CORS, caching, etc.)
- **Export Options**: Copy headers as JSON or cURL command

#### Cookies (Phase 3)
- **Cookie Viewer**: List all response cookies with details
- **Cookie Management**: Edit, delete, or add cookies for subsequent requests
- **Session Persistence**: Maintain cookies across requests in same session

### 3.3 History & Collections

#### Request History
- **Automatic Logging**: Every request automatically saved to history
- **Rich Metadata**: Method, URL, status, response time, timestamp
- **Search & Filter**: Find requests by URL, method, or date range
- **History Management**: 
  - Clear individual items or bulk clear
  - Export history to JSON/CSV
  - Configurable history retention (default: 1000 items)

#### Collections
- **Folder Organization**: Create nested folder structure
- **Request Grouping**: Organize related requests by project, API, or feature
- **Collection Sharing**: 
  - Export collections to JSON
  - Import from Postman collections (v2 format)
  - Import from Insomnia collections (v5 format)
  - Share via URL or file
- **Collection Features**:
  - Bulk operations (run all requests in collection)
  - Collection-level variables
  - Description and documentation support

#### Persistence Strategy
- **Primary Storage**: SQLite via Entity Framework Core (server-side; fully available in InteractiveServer mode)
- **Backup Storage**: In-memory cache for transient session data
- **Data Export**: JSON file download/upload for backup and portability
- **Note**: Because all component logic executes on the server, full EF Core with SQLite is available — no IndexedDB or JS interop workarounds needed

### 3.4 Environment Variables

#### Variable Management
- **Environment Sets**: Create multiple environments (Dev, Staging, Production)
- **Variable Types**: 
  - String values
  - Secret values (masked in UI)
  - Dynamic values (timestamps, UUIDs)
- **Variable Scope**: 
  - Global variables (available everywhere)
  - Environment-specific variables
  - Collection-level variables

#### Variable Substitution
- **Syntax**: `{{variableName}}` format for substitution
- **Contexts**: Variables work in URLs, headers, request bodies
- **Real-time Preview**: Show substituted values before sending
- **Error Handling**: Clear indication of undefined variables

#### Advanced Features (Phase 4)
- **Variable Functions**: Built-in functions for common operations
- **Chained Variables**: Variables referencing other variables
- **Environment Switching**: Quick environment selector in UI
- **Variable Import**: Import from .env files or JSON

---

## 4. Non-Functional Requirements

### Performance Requirements
- **Load Time**: < 1 second initial load on standard broadband (no WASM payload to download)
- **Response Time**: < 100ms UI interactions (SignalR round-trip), < 500ms API request initiation
- **Memory Usage**: < 100MB server-side memory per active circuit
- **Concurrent Requests**: Support for 10+ simultaneous requests per user session

### Accessibility Requirements
- **WCAG 2.1 AA Compliance**: Full keyboard navigation and screen reader support
- **Keyboard Shortcuts**: Complete workflow accessible via keyboard
- **High Contrast Mode**: Support for system high contrast themes
- **Text Scaling**: Support for 200% text zoom without layout break

### Usability Requirements
- **Learning Curve**: < 5 minutes for basic functionality
- **Error Recovery**: Graceful handling of network errors and invalid inputs
- **Responsive Design**: Optimized for 1024x768 minimum resolution
- **Touch Support**: Full functionality on tablet devices

### Reliability Requirements
- **Uptime**: 99.9% availability for hosted version
- **Data Integrity**: No data loss during browser disconnects; SQLite provides durable server-side storage
- **Error Logging**: Comprehensive error tracking and reporting
- **Graceful Degradation**: Static SSR fallback for non-interactive content when circuit is unavailable

### InteractiveServer-Specific Requirements
- **Initial Load**: < 1 second on standard broadband (HTML + SignalR handshake only; no WASM payload)
- **SignalR Connection**: Automatic reconnection with configurable retry policy and user-visible reconnection UI
- **Circuit Management**: Configure `CircuitOptions` for idle timeout, max retained disconnected circuits, and max buffer size
- **Request Cancellation**: All HTTP requests support `CancellationToken`; cancellation propagated on circuit disconnect
- **Timeout Configuration**: Configurable request timeout (default: 30s)
- **Large Response Handling**: Stream responses >10MB or show warning; server memory must be monitored
- **State Persistence**: Use `PersistentComponentState` for prerendering; implement `RevalidatingServerAuthenticationStateProvider` if auth is added
- **Scalability**: Each connected user holds a server-side circuit consuming memory; plan capacity accordingly

### Security Requirements
- **Data Privacy**: API request data is processed server-side; responses are not stored beyond the user's session/history unless explicitly saved
- **XSS Protection**: Blazor's built-in Razor encoding handles output sanitization; additionally use `HtmlSanitizer` for user-supplied HTML content
- **CSRF Protection**: Required — Blazor's built-in antiforgery middleware (`app.UseAntiforgery()`) protects form posts and enhanced navigation; antiforgery tokens are automatically included via `<AntiforgeryToken />` in `App.razor`
- **Secure Storage**: Sensitive data (tokens, API keys) stored server-side in memory or encrypted SQLite columns; never sent to browser localStorage
- **Render Mode**: InteractiveServer — all component logic runs on the server; the browser receives only UI diffs over SignalR
- **Circuit Security**: Validate that SignalR circuit cannot be hijacked; enforce HTTPS and configure `HubOptions` (e.g., `MaximumReceiveMessageSize`)

---

## 5. Technical Architecture

### Technology Stack
- **Framework**: ASP.NET Core .NET 10.0 with **InteractiveServer** render mode
- **UI Framework**: MudBlazor 8.x for Material Design components
- **HTTP Client**: `System.Net.Http.HttpClient` via `IHttpClientFactory` (server-side; no browser CORS restrictions)
- **Persistence**: SQLite via Entity Framework Core (server-side database)
- **State Management**: Scoped services (one DI scope per SignalR circuit); use `@inject` with `Scoped` lifetime
- **Code Editor**: BlazorMonaco or MudExCodeEditor (MudText insufficient for JSON)
- **Testing**: xUnit, bUnit, FluentAssertions, Moq; EF Core in-memory/SQLite provider for integration tests
- **Build Tool**: .NET CLI with GitHub Actions for CI/CD

### Solution Structure
The solution uses a **three-project layout**. Because InteractiveServer runs all component logic on the server, there is no need for a separate WebAssembly client project. A dedicated test project provides comprehensive automated testing:

- **RobRequest.Server** (Web Application): ASP.NET Core host containing Razor components (`.razor` pages and layouts), static assets, and the application entry point (`Program.cs`). References `RobRequest.Shared`.
- **RobRequest.Shared** (Class Library): Contains all **models**, **services**, and **EF Core DbContext** used by the server project. Keeping these in a shared library preserves clean separation of concerns and makes it easy for the test project to exercise business logic without depending on the server host.
- **RobRequest.Tests** (xUnit Test Project): Contains all unit, integration, and component (bUnit) tests. References both `RobRequest.Shared` (for service/model tests) and `RobRequest.Server` (for bUnit component tests). Uses xUnit as the test framework with FluentAssertions, Moq, and bUnit.

> **Why not a separate WebAssembly client project?** InteractiveServer components execute entirely on the server within a single ASP.NET Core process. There is only one DI container, so the duplicate-registration problem of InteractiveWebAssembly does not exist. The former `RobRequest.Client` project has been merged into `RobRequest.Server`.

### Architecture Patterns
- **Clean Architecture**: Separation of concerns with layered approach
- **Shared Library Pattern**: Models, services, and data access in `RobRequest.Shared`, referenced by the server project
- **Repository Pattern**: Data access abstraction for SQLite/EF Core operations
- **Service Layer**: Business logic encapsulated in services within `RobRequest.Shared`
- **Component Architecture**: Reusable Blazor components in `RobRequest.Server`

### Key Components

#### Frontend Components (`RobRequest.Server/Components/`)
- **Layout Components**: 
  - `MainLayout.razor`: Overall application structure
  - `Sidebar.razor`: Navigation and history panel
  - `RequestPanel.razor`: Request building interface
  - `ResponsePanel.razor`: Response display interface

#### Business Services (`RobRequest.Shared/Services/`)
- **ApiService**: HTTP request execution and response handling (server-side `HttpClient`)
- **HistoryService**: Request history management and persistence (EF Core)
- **CollectionService**: Collection CRUD operations (EF Core)
- **EnvironmentService**: Variable management and substitution
- **SettingsService**: User preferences and configuration (EF Core)

> All services reside in `RobRequest.Shared` under the `RobRequest.Shared.Services` namespace and are registered in the server's DI container with **Scoped** lifetime (one instance per circuit).

#### Data Access (`RobRequest.Shared/Data/`)
- **AppDbContext**: EF Core `DbContext` with `DbSet` properties for all entities
- **Migrations**: EF Core code-first migrations for schema management

#### Data Models (`RobRequest.Shared/Models/`)
- **Request Models**: `HttpRequestModel`, `HeaderItem`, `QueryParamItem`
- **Response Models**: `HttpResponseModel`, `ResponseMetadata`
- **Domain Models**: `Collection`, `Environment`, `HistoryItem`
- **Configuration Models**: `UserSettings`, `AppConfiguration`

> All models reside in `RobRequest.Shared` under the `RobRequest.Shared.Models` namespace.

#### Test Project (`RobRequest.Tests/`)
- **Unit Tests** (`Tests/Unit/`): Service logic, model validation, utility functions
- **Integration Tests** (`Tests/Integration/`): EF Core database operations, HttpClient integration
- **Component Tests** (`Tests/Components/`): bUnit tests for Razor components (RequestPanel, ResponsePanel, Sidebar, Settings)

### Data Architecture
- **SQLite Schema**: EF Core code-first with tables for history, collections, environments, settings
- **Migration Strategy**: EF Core migrations applied automatically on startup (`DbContext.Database.MigrateAsync()`)
- **Indexing Strategy**: EF Core index attributes on URL, timestamp, and collection fields
- **Backup Strategy**: Export to JSON with user control; SQLite file can also be backed up directly

---

## 6. System Architecture

### Component Diagram
```
┌─────────────────────────────────────────────────────────────────┐
│  Browser                                                        │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │  Rendered HTML + SignalR Connection (blazor.web.js)        │  │
│  │  UI diffs pushed from server; user events sent to server   │  │
│  └───────────────────────────────────────────────────────────┘  │
└──────────────────────────┬──────────────────────────────────────┘
                           │ SignalR (WebSocket)
┌──────────────────────────▼──────────────────────────────────────┐
│  RobRequest.Server (ASP.NET Core + Blazor InteractiveServer)    │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │  UI Layer (MudBlazor Components)                          │  │
│  │  ┌────────────┬──────────────┬──────────┬──────────────┐  │  │
│  │  │RequestPanel│ResponsePanel │ Sidebar  │ Settings     │  │  │
│  │  └────────────┴──────────────┴──────────┴──────────────┘  │  │
│  ├───────────────────────────────────────────────────────────┤  │
│  │  Program.cs / Middleware / Static Assets (wwwroot/)        │  │
│  └───────────────────────────────────────────────────────────┘  │
├─────────────────────────────────────────────────────────────────┤
│  RobRequest.Shared (Class Library)                              │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │  Service Layer (Scoped — one instance per circuit)        │  │
│  │  ┌──────────┬─────────────┬─────────────┬─────────────┐  │  │
│  │  │ApiService│HistorySvc   │CollectionSvc│ EnvironSvc  │  │  │
│  │  └──────────┴─────────────┴─────────────┴─────────────┘  │  │
│  ├───────────────────────────────────────────────────────────┤  │
│  │  Data Access (EF Core)                                    │  │
│  │  ┌──────────────┬─────────────────────────────────────┐  │  │
│  │  │ AppDbContext  │  SQLite (robrequest.db)             │  │  │
│  │  └──────────────┴─────────────────────────────────────┘  │  │
│  ├───────────────────────────────────────────────────────────┤  │
│  │  Models (shared data types)                               │  │
│  │  ┌──────────────┬────────────────┬────────────────────┐  │  │
│  │  │HttpRequestModel│HttpResponseModel│HistoryItem etc │  │  │
│  │  └──────────────┴────────────────┴────────────────────┘  │  │
│  └───────────────────────────────────────────────────────────┘  │
├─────────────────────────────────────────────────────────────────┤
│  External APIs                                                  │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │  HttpClient requests execute server-side (no CORS limits) │  │
│  └───────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│  RobRequest.Tests (xUnit Test Project)                          │
│  References: RobRequest.Server, RobRequest.Shared               │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │  Unit Tests        │ Service logic, models, utilities      │  │
│  │  Integration Tests │ EF Core (SQLite), HttpClient          │  │
│  │  Component Tests   │ bUnit (RequestPanel, ResponsePanel…)  │  │
│  └───────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

> **Note**: With InteractiveServer, all component logic and HTTP requests execute on the server. The browser only receives pre-rendered HTML and subsequent UI diffs over SignalR. This eliminates CORS issues and enables full .NET library access including EF Core. Services are registered once in the server's DI container with Scoped lifetime. The `RobRequest.Tests` project references both `RobRequest.Server` and `RobRequest.Shared` to enable testing at every layer.

### Service Architecture

> All service classes below reside in the `RobRequest.Shared.Services` namespace.

#### ApiService
```csharp
public class ApiService
{
    // Uses IHttpClientFactory for server-side HTTP requests (no CORS restrictions)
    public async Task<HttpResponseModel> SendRequestAsync(HttpRequestModel request, CancellationToken ct = default);
    public void ConfigureHttpClient(Action<HttpClient> configure);
    public void SetTimeout(TimeSpan timeout);
    public event EventHandler<RequestProgressEventArgs> RequestProgress;
}
```

#### HistoryService
```csharp
public class HistoryService
{
    // Persists to SQLite via EF Core (AppDbContext injected)
    public async Task<IEnumerable<HistoryItem>> GetHistoryAsync(int limit = 100);
    public async Task AddToHistoryAsync(HttpRequestModel request, HttpResponseModel response);
    public async Task ClearHistoryAsync();
    public async Task ExportHistoryAsync(string format);
}
```

#### EnvironmentService
```csharp
public class EnvironmentService
{
    public async Task<string> SubstituteVariablesAsync(string input, string environmentId);
    public async Task<Environment> GetEnvironmentAsync(string id);
    public async Task SetVariableAsync(string environmentId, string key, string value);
}
```

### Data Flow
1. **User Input** → Browser (SignalR) → Server-side UI Components → Service Layer
2. **Service Processing** → Variable Substitution → Server-side `HttpClient` Request to External API
3. **HTTP Response** → Response Processing → UI Diff sent to Browser via SignalR
4. **Background Operations** → History Logging → SQLite via EF Core

---

## 7. User Interface Design

### Layout Structure
```
┌─────────────────────────────────────────────────────────────┐
│                    Header Bar                                │
│  [Logo] [Environment Selector]        [Theme] [Settings]    │
├─────────────────────────────────────────────────────────────┤
│ Sidebar │                Main Content Area                   │
│ ┌───────┬┤ ┌───────────────────────────────────────────────┐ │
│ │History││ │              Request Builder                   │ │
│ │       ││ │ [GET ▼] [https://api.example.com/endpoint] [Send]│ │
│ │       ││ ├───────────────────────────────────────────────┤ │
│ │Coll.  ││ │ Params | Headers | Body | Auth | Tests        │ │
│ │       ││ │ [Tab Content Area]                           │ │
│ │       ││ ├───────────────────────────────────────────────┤ │
│ │Env.   ││ │              Response Viewer                  │ │
│ │       ││ │ Status: 200 OK | Time: 245ms | Size: 1.2KB     │ │
│ │       ││ │ Body | Headers | Cookies | Test Results       │ │
│ │       ││ │ [Response Content Area]                      │ │
│ └───────┴┤ └───────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### Component Hierarchy
- **MainLayout**: Root layout following MudBlazor template pattern
  ```razor
  <MudLayout>
      <MudAppBar Elevation="1">
          <MudIconButton Icon="@Icons.Material.Filled.Menu" 
                          Color="Color.Inherit" 
                          Edge="Edge.Start" 
                          OnClick="@DrawerToggle" />
          <MudSpacer />
          <MudText Typo="Typo.h6">API Client</MudText>
          <MudSpacer />
          <EnvironmentSelector />
          <MudIconButton Icon="@Icons.Material.Filled.Settings" 
                          Color="Color.Inherit" 
                          Edge="Edge.End" />
      </MudAppBar>
      
      <MudDrawer @bind-Open="_drawerOpen" Elevation="2">
          <MudDrawerHeader>
              <MudText Typo="Typo.h5">API Client</MudText>
          </MudDrawerHeader>
          <NavMenu />
      </MudDrawer>
      
      <MudMainContent>
          @Body
      </MudMainContent>
  </MudLayout>
  ```

- **NavMenu**: Navigation drawer content
  - **HistoryView**: Request history with search/filter
  - **CollectionView**: Collection tree view with nested folders
  - **EnvironmentView**: Environment management and switching
  - **SettingsLink**: Quick access to application settings

- **RequestPanel**: Main request building interface
  - **RequestHeader**: Method selector, URL input, send button
  - **RequestTabs**: MudTabs container for request sections
    - **ParamsTab**: Query parameters management
    - **HeadersTab**: HTTP headers configuration
    - **BodyTab**: Request body editor (JSON, form, raw)
    - **AuthTab**: Authentication configuration
    - **TestsTab**: Response testing scripts

- **ResponsePanel**: Response display interface
  - **ResponseStatusBar**: Status code, time, size indicators with color coding
  - **ResponseTabs**: MudTabs for response sections
    - **BodyTab**: Formatted response content with syntax highlighting
    - **HeadersTab**: Response headers table with sorting/filtering
    - **CookiesTab**: Response cookies management
    - **TestResultsTab**: Test execution results

- **Shared Components**
  - **EnvironmentSelector**: Dropdown for environment switching
  - **MethodSelector**: HTTP method dropdown with custom methods support
  - **UrlInput**: URL input with validation and autocomplete
  - **CodeEditor**: BlazorMonaco or MudExCodeEditor with syntax highlighting for JSON/XML
  - **KeyValueEditor**: Dynamic key-value pair management
  - **StatusIndicator**: Color-coded status display

### Design Principles
- **Material Design 3**: Following latest Material Design guidelines
- **Responsive Layout**: Adaptive design for desktop and tablet
- **Dark/Light Themes**: System preference detection with manual override
- **Micro-interactions**: Subtle animations and transitions
- **Accessibility**: Full keyboard navigation and screen reader support

### Key UI Features
- **Keyboard Shortcuts**: Ctrl+Enter to send, Ctrl+S to save, etc.
- **Context Menus**: Right-click actions for quick access
- **Drag & Drop**: File upload and collection organization
- **Auto-save**: Automatic saving of drafts and responses
- **Tooltips**: Helpful hints for complex features

---

## 8. Implementation Roadmap

### Phase 1: Foundation (Weeks 1-2)
**Goal**: Basic working API client

#### Sprint 1.1: Core Infrastructure
- [ ] Project setup with .NET 10 Blazor Web App template using InteractiveServer render mode (three-project layout: `RobRequest.Server`, `RobRequest.Shared`, `RobRequest.Tests`)
- [ ] `RobRequest.Tests` xUnit project setup with bUnit, FluentAssertions, and Moq; references to `RobRequest.Server` and `RobRequest.Shared`
- [ ] MudBlazor integration and theme configuration
- [ ] Basic project structure and folder organization (models and services in `RobRequest.Shared`)
- [ ] SQLite + EF Core setup with `AppDbContext` in `RobRequest.Shared`; auto-migrate on startup
- [ ] Basic HTTP client wrapper service in `RobRequest.Shared` using `IHttpClientFactory`

#### Sprint 1.2: Basic Request/Response
- [ ] Request builder UI (method, URL, send button)
- [ ] Basic response viewer (status, body, headers)
- [ ] GET and POST request support
- [ ] JSON response formatting
- [ ] Error handling and user feedback

**Acceptance Criteria**:
- Can send GET requests and view responses
- Can send POST requests with JSON body
- Basic UI is functional and responsive
- No data persistence yet

### Phase 2: Core Features (Weeks 3-4)
**Goal**: Complete request building capabilities

#### Sprint 2.1: Request Enhancement
- [ ] All HTTP methods (PUT, PATCH, DELETE, HEAD, OPTIONS)
- [ ] Query parameters management
- [ ] Custom headers management
- [ ] Form data support
- [ ] Raw text body support

#### Sprint 2.2: Response Enhancement
- [ ] Multiple response formats (JSON, XML, HTML, images)
- [ ] Response search functionality
- [ ] Response headers table with sorting
- [ ] Response timing and size metrics
- [ ] Copy response functionality

**Acceptance Criteria**:
- Full CRUD operations supported
- Multiple body types supported
- Rich response viewing experience
- Basic request history (session only)

### Phase 3: Data Management (Weeks 5-6)
**Goal**: Persistent storage and organization

#### Sprint 3.1: History & Collections
- [ ] Request history persistence to SQLite via EF Core
- [ ] Collection creation and management
- [ ] Request organization into collections
- [ ] History search and filtering
- [ ] Import/export functionality (JSON file download/upload)

#### Sprint 3.2: UI Polish
- [ ] Sidebar implementation
- [ ] Keyboard shortcuts
- [ ] Settings page
- [ ] Theme switching (dark/light)
- [ ] Responsive design improvements

**Acceptance Criteria**:
- Data persists between sessions
- Collections can be created and organized
- Import/export from Postman collections
- Professional UI with smooth interactions

### Phase 4: Advanced Features (Weeks 7-8)
**Goal**: Professional-grade features

#### Sprint 4.1: Authentication & Environments
- [ ] Environment variables management
- [ ] Variable substitution in requests
- [ ] Basic authentication (Bearer, Basic)
- [ ] API key authentication
- [ ] Environment switching

#### Sprint 4.2: Testing & Automation
- [ ] Response testing framework
- [ ] Batch request execution
- [ ] Collection runner
- [ ] Test result reporting
- [ ] Performance monitoring

**Acceptance Criteria**:
- Environment variables work seamlessly
- Multiple authentication methods supported
- Can run automated tests on collections
- Performance metrics are tracked

### Phase 5: Collaboration & Deployment (Weeks 9-10)
**Goal**: Team features and production readiness

#### Sprint 5.1: Collaboration Features
- [ ] Collection sharing via URL
- [ ] Team workspace (basic)
- [ ] Comment and annotation system
- [ ] Version history for collections
- [ ] Real-time collaboration (optional)

#### Sprint 5.2: Production Deployment
- [ ] Docker containerization for self-hosting
- [ ] SignalR reconnection UI and circuit resilience testing
- [ ] Performance optimization (memory profiling, circuit limits)
- [ ] Security audit (CSRF, XSS, SignalR hub hardening)
- [ ] Documentation and help system

**Acceptance Criteria**:
- Application deployable via Docker or Azure App Service
- Teams can share collections
- Production-ready performance and security
- Comprehensive documentation

### Phase 6: Future Enhancements (Beyond MVP)
- **GraphQL Support**: Native GraphQL query builder
- **WebSocket Testing**: Real-time connection testing
- **API Documentation Generation**: Auto-generate docs from collections
- **Mock Server**: Built-in mock server for development
- **CI/CD Integration**: API testing in pipelines
- **Advanced Analytics**: Request pattern analysis and insights

---

## 9. Testing Strategy

All tests reside in the **`RobRequest.Tests`** project (xUnit). This project references both `RobRequest.Shared` and `RobRequest.Server` and is organized into subdirectories by test type:

```
RobRequest.Tests/
├── Unit/                  # Pure unit tests (no I/O, mocked dependencies)
│   ├── Services/          # ApiServiceTests, HistoryServiceTests, etc.
│   ├── Models/            # Model validation, serialization tests
│   └── Utilities/         # URL parsing, variable substitution, etc.
├── Integration/           # Tests with real dependencies (SQLite, HttpClient)
│   ├── Database/          # EF Core CRUD, migrations, data consistency
│   └── Http/              # HttpClient integration, error scenarios
├── Components/            # bUnit Razor component tests
│   ├── RequestPanelTests.razor
│   ├── ResponsePanelTests.razor
│   ├── SidebarTests.razor
│   └── SettingsTests.razor
├── Fixtures/              # Shared test fixtures, mock API server, sample data
└── RobRequest.Tests.csproj
```

### Testing Pyramid
```
    ┌─────────────────────┐
    │  E2E Tests (10%)    │ ← Playwright, full user workflows
    ├─────────────────────┤
    │ Integration (20%)   │ ← Service integration, database tests
    ├─────────────────────┤
    │ Unit Tests (70%)    │ ← xUnit, business logic, utilities
    └─────────────────────┘
```

> Unit, integration, and component tests all live in `RobRequest.Tests`. E2E (Playwright) tests may be run separately or included in the same project with a test filter.

### Unit Testing (xUnit)

> Location: `RobRequest.Tests/Unit/`

#### Core Logic Tests
- **Variable Substitution**: `{{variable}}` parsing and replacement
- **URL Parsing**: Query parameter extraction and validation
- **HTTP Header Management**: Header formatting and validation
- **JSON Processing**: Parsing, formatting, and validation
- **Time Calculations**: Response time and performance metrics

#### Service Layer Tests
- **ApiService**: Request building and response parsing (server-side `HttpClient`)
- **HistoryService**: History management and persistence (EF Core)
- **EnvironmentService**: Variable management and substitution
- **SettingsService**: User preferences CRUD and data integrity (EF Core)

#### Data Model Tests
- **Model Validation**: Request/response model validation rules
- **Entity Mapping**: EF Core entity mapping accuracy
- **Data Transformation**: Model conversion and serialization

### Integration Testing

> Location: `RobRequest.Tests/Integration/`

#### Database Integration
- **SQLite Operations**: CRUD operations with EF Core
- **Migration Testing**: Database schema changes
- **Connection Handling**: Connection pooling and error handling
- **Data Consistency**: Transaction integrity

#### HTTP Integration
- **HttpClient Integration**: Real API endpoint testing
- **Error Scenarios**: Network failures, timeouts, HTTP errors
- **Authentication**: Various auth method implementations
- **Response Handling**: Different content types and status codes

### Component Testing (bUnit)

> Location: `RobRequest.Tests/Components/`

#### UI Component Tests
- **RequestPanel**: User interactions and validation
- **ResponsePanel**: Data display and formatting
- **Sidebar**: Navigation and state management
- **SettingsPanel**: Configuration persistence

#### User Interaction Tests
- **Form Validation**: Input validation and error messages
- **State Management**: Component state and updates
- **Event Handling**: User events and callbacks
- **Rendering**: Component rendering with different data

### End-to-End Testing (Playwright)

#### User Workflow Tests
- **Complete Request Cycle**: From request to response
- **Collection Management**: Create, organize, share collections
- **Environment Switching**: Variable substitution across environments
- **Import/Export**: Data portability and migration

#### Cross-Browser Testing
- **Chrome**: Primary browser testing
- **Firefox**: Compatibility verification
- **Safari**: Apple ecosystem support
- **Edge**: Microsoft browser compatibility

#### Performance Testing
- **Load Time**: Application startup and initial load
- **Memory Usage**: Memory leak detection
- **Large Data Handling**: Large responses and history
- **Concurrent Operations**: Multiple simultaneous requests

### Test Data Strategy

#### Test Fixtures
- **Mock APIs**: Local API server for consistent testing
- **Sample Data**: Representative request/response examples
- **Edge Cases**: Invalid data, large payloads, special characters
- **Security Tests**: XSS, injection, authentication bypass

#### Test Environments
- **Development**: Local development with hot reload
- **Staging**: Pre-production environment
- **Production**: Smoke tests for deployment verification

### Quality Gates

#### Code Coverage
- **Minimum Coverage**: 80% for business logic
- **Critical Path Coverage**: 95% for core features
- **Integration Coverage**: 70% for service interactions

#### Performance Benchmarks
- **Response Time**: < 500ms for API requests
- **UI Responsiveness**: < 100ms for user interactions (SignalR round-trip)
- **Memory Usage**: < 100MB server-side baseline per circuit
- **Load Time**: < 1 second initial load (no WASM payload)

### Continuous Integration

#### Automated Testing Pipeline
```yaml
# GitHub Actions Workflow
- Build Application
- Run Unit Tests (xUnit)
- Run Integration Tests
- Run Component Tests (bUnit)
- Run E2E Tests (Playwright)
- Code Coverage Report
- Performance Benchmarks
- Security Scan
- Deploy to Staging
```

#### Quality Metrics
- **Test Results**: Pass/fail status and coverage reports
- **Performance Metrics**: Regression detection
- **Security Scans**: Vulnerability assessment
- **Code Quality**: Static analysis and linting

## 10. Deployment & DevOps

### Deployment Architecture

#### Hosting Options
- **Cloud Hosting**: Azure App Service, AWS Elastic Beanstalk, or any ASP.NET Core-compatible PaaS
- **Self-Hosting**: Docker containers with Kestrel (reverse-proxied by nginx/Caddy)
- **Local Development**: `dotnet run` with hot reload

> **Note**: InteractiveServer requires a persistent ASP.NET Core process; static hosting (GitHub Pages, Netlify) is **not** compatible with this render mode.

#### Build Pipeline
```yaml
# .github/workflows/deploy.yml
name: Build and Deploy
on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'
      - name: Restore dependencies
        run: dotnet restore
      - name: Build
        run: dotnet build --no-restore
      - name: Test
        run: dotnet test --no-build --verbosity normal
      
  deploy:
    needs: test
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'
      - name: Publish
        run: dotnet publish RobRequest.Server/RobRequest.Server.csproj -c Release -o publish
      - name: Build Docker Image
        run: docker build -t robrequest:latest .
      - name: Push to Container Registry
        run: docker push ${{ secrets.REGISTRY_URL }}/robrequest:latest
```

### Environment Configuration

#### Development Environment
- **Local Development**: `dotnet run` with hot reload
- **Database**: Local SQLite file
- **Configuration**: appsettings.Development.json
- **Logging**: Detailed debug logging

#### Staging Environment
- **Preview Deployment**: Automatic deployment on PR
- **Test Database**: Fresh SQLite database
- **Configuration**: Environment variables
- **Monitoring**: Basic error tracking

#### Production Environment
- **Server Hosting**: ASP.NET Core behind reverse proxy (nginx/Caddy) or Azure App Service
- **Database**: SQLite file on persistent volume (or upgrade to PostgreSQL for multi-instance)
- **Configuration**: Environment variables and Azure Key Vault / secrets manager
- **Monitoring**: Error tracking and analytics

### Performance Optimization

#### Build Optimizations
- **PublishTrimmed**: Enable trimming for smaller deployment size
- **Compression**: Response compression middleware (Brotli/gzip) for static assets
- **Static Asset Bundling**: Minimize CSS bundles; MudBlazor JS served from `_content/`
- **ReadyToRun**: Enable `PublishReadyToRun` for faster server startup

#### Runtime Optimizations
- **Response Caching**: Cache static assets with fingerprinted URLs
- **Circuit Configuration**: Tune `CircuitOptions.DisconnectedCircuitRetentionPeriod` and `MaxBufferedUnacknowledgedRenderBatches`
- **Connection Management**: Configure SignalR `HubOptions` for `KeepAliveInterval` and `ClientTimeoutInterval`
- **Memory Management**: Monitor per-circuit memory; set `MaximumReceiveMessageSize` limits

### Monitoring & Analytics

#### Application Monitoring
- **Error Tracking**: Sentry or similar error monitoring
- **Performance Monitoring**: Web Vitals and custom metrics
- **Usage Analytics**: Anonymous usage statistics
- **Health Checks**: Application health monitoring

#### Business Metrics
- **User Engagement**: Session duration and feature usage
- **Adoption Rates**: New user onboarding completion
- **Feature Usage**: Most used features and workflows
- **Performance Metrics**: Load times and error rates

### Security Considerations

#### Application Security
- **Content Security Policy**: Restrict resource loading; allow SignalR WebSocket connections
- **XSS Protection**: Blazor's built-in Razor encoding + `HtmlSanitizer` for user-supplied HTML
- **CSRF Protection**: Blazor antiforgery middleware (`app.UseAntiforgery()`) with `<AntiforgeryToken />` in `App.razor`
- **Authentication**: Secure token handling; sensitive values stored server-side only
- **Data Privacy**: API request/response data processed server-side; never persisted to browser storage

#### Infrastructure Security
- **HTTPS Only**: Enforce secure connections; required for SignalR WebSocket transport
- **SignalR Hardening**: Configure `MaximumReceiveMessageSize`, enable authentication on hub if needed
- **Secure Headers**: Implement `Strict-Transport-Security`, `X-Content-Type-Options`, `X-Frame-Options`
- **Dependency Scanning**: Regular vulnerability scans via `dotnet list package --vulnerable`

## 11. Project Governance

### Development Team Structure
- **Lead Developer**: Architecture and technical decisions
- **Frontend Developer**: UI/UX implementation
- **Backend Developer**: Service layer and data management
- **QA Engineer**: Testing strategy and implementation
- **DevOps Engineer**: Deployment and infrastructure

### Development Workflow
- **Git Flow**: Feature branches with main/develop branches
- **Code Review**: Required peer review for all changes
- **Sprint Planning**: 2-week sprints with defined goals
- **Retrospectives**: Continuous process improvement

### Documentation Strategy
- **API Documentation**: Comprehensive API reference
- **User Guide**: Step-by-step feature documentation
- **Developer Guide**: Contribution guidelines and setup
- **Architecture Documentation**: System design and decisions

---

## Conclusion

This specification provides a comprehensive foundation for developing a modern, feature-rich API testing client using .NET 10 Blazor. The project balances simplicity with powerful features, ensuring it meets the needs of developers while maintaining excellent performance and user experience.

### Project Summary
- **Technology Stack**: Modern .NET 10 Blazor InteractiveServer with MudBlazor UI and SQLite/EF Core persistence
- **Architecture**: Clean, layered three-project architecture (`RobRequest.Server` + `RobRequest.Shared` + `RobRequest.Tests`)
- **Features**: Complete API testing workflow with advanced capabilities
- **Timeline**: 10-week phased approach with clear milestones
- **Quality**: Comprehensive testing strategy with 80%+ code coverage

### Key Strengths
1. **Modern Technology**: Leverages latest .NET 10 and Blazor InteractiveServer capabilities
2. **Developer-Focused**: Built by developers, for developers
3. **Performance Optimized**: Near-instant initial load with server-side rendering; no WASM payload
4. **Full .NET Power**: Server-side execution enables EF Core, unrestricted HttpClient, and no CORS limitations
5. **Feature Complete**: Covers 95% of common API testing workflows
6. **Extensible**: Clean architecture allows for future enhancements

### Competitive Advantages
- **Browser-Native**: No installation required, instant access via any modern browser
- **Server-Side Security**: All data processed and stored server-side; nothing sensitive in browser storage
- **No CORS Issues**: HTTP requests execute server-side, bypassing browser CORS restrictions entirely
- **Open Source**: Community-driven development
- **Modern UI**: Material Design 3 with excellent UX
- **Cross-Platform**: Works on any modern browser with WebSocket support

### Next Steps
1. **Specification Review**: Stakeholder review and approval
2. **Environment Setup**: Development environment and tooling
3. **Phase 1 Implementation**: Core infrastructure and basic functionality
4. **Testing Framework**: Unit, integration, and E2E test setup
5. **CI/CD Pipeline**: Automated build and deployment pipeline

### Success Metrics
#### Technical Metrics
- **Performance**: < 1 second initial load, < 500ms request response
- **Quality**: 80%+ test coverage, < 1% error rate
- **Compatibility**: Support for 95%+ of modern browsers (WebSocket required)
- **Reliability**: 99.9% uptime for hosted version

#### Business Metrics
- **User Adoption**: 1000+ active users within 6 months
- **Engagement**: Average session duration > 10 minutes
- **Retention**: 70%+ monthly active user retention
- **Community**: 50+ contributors within first year

#### Development Metrics
- **Velocity**: Consistent 2-week sprint delivery
- **Quality**: Zero critical bugs in production
- **Documentation**: 100% API coverage with examples
- **Onboarding**: New developers productive within 1 week

### Risk Mitigation
#### Technical Risks
- **Blazor Maturity**: Mitigated by .NET 10 stability and mature InteractiveServer model
- **SignalR Latency**: Mitigated by keeping UI interactions lightweight; monitor round-trip times
- **Server Scalability**: Each circuit consumes server memory; mitigated by circuit limits and load testing
- **Browser Compatibility**: WebSocket support required; comprehensive cross-browser testing
- **Security**: Regular security audits, dependency scanning, and SignalR hub hardening

#### Project Risks
- **Scope Creep**: Managed through phased approach
- **Resource Constraints**: Clear team roles and responsibilities
- **Timeline Delays**: Buffer time in each phase
- **Quality Issues**: Automated testing and code reviews

### Future Roadmap
Beyond the initial 10-week development cycle, the project has clear paths for enhancement:

#### Short-term (6-12 months)
- GraphQL support
- WebSocket testing
- Advanced authentication methods
- Performance analytics dashboard

#### Long-term (12+ months)
- Team collaboration features
- CI/CD integration
- Mock server capabilities
- Advanced automation workflows

### Final Thoughts
This specification establishes a solid foundation for building a competitive, modern API testing client. The combination of cutting-edge technology, thoughtful architecture, and comprehensive planning positions the project for success in both technical execution and market adoption.

The phased approach ensures early value delivery while building toward a complete solution. The emphasis on performance, security, and user experience addresses the key pain points of existing tools in the market.

With proper execution of this specification, the resulting application will provide developers with a fast, intuitive, and powerful tool for API testing and development workflows.

---

**Document Version**: 2.0  
**Last Updated**: March 2026  
**Architecture**: InteractiveServer (migrated from InteractiveWebAssembly in v1.x)  
**Next Review**: April 2026 or as needed
 
