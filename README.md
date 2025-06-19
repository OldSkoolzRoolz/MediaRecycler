# MediaRecycler

MediaRecycler is a user-friendly application designed to help recover and reuse videos or images from websites or intranets. It is built with modern .NET technologies and leverages robust libraries for web automation, downloading, and configuration.

## Features

- Concurrent, robust downloading with retry and queue persistence
- Automated extraction of video links from web pages
- Headless browser automation for scraping dynamic content
- Configurable via JSON and environment variables
- Logging to file, console, and UI

## Technologies Used

### .NET 9 & Windows Forms
- **Target Framework:** .NET 9.0 (`net9.0-windows10.0.17763.0`)
- **UI:** Windows Forms for a familiar desktop experience

### Dependency Injection & Logging
- **Microsoft.Extensions.DependencyInjection**: For service registration and dependency management
- **Microsoft.Extensions.Logging**: For structured logging (console, debug, file, and UI)

### Configuration
- **Microsoft.Extensions.Configuration**: For flexible configuration via `appsettings.json` and environment variables

### Asynchronous Programming
- **Microsoft.Bcl.AsyncInterfaces**: For advanced async/await support

### Resilient Networking
- **Polly**: For retry policies and fault handling during downloads

### Web Automation & Scraping
- **PuppeteerSharp**: For headless browser automation and scraping of dynamic web content
- **PuppeteerSharp.Dom**: For advanced DOM manipulation and querying

### Custom Libraries
- **MiniFrontier**: Used for managing a queue/frontier of URLs to process

### Testing
- **xUnit**: For unit testing
- **Moq**: For mocking dependencies in tests
- **coverlet.collector**: For code coverage

## Getting Started

1. **Requirements**
   - Windows 10 (build 17763) or later
   - .NET 9 SDK

2. **Configuration**
   - Edit `appsettings.json` for custom settings (download paths, scraping options, etc.)

3. **Running**
   - Build and run the solution in Visual Studio 2022 or via `dotnet run`

## License

Open Source. See file headers for author and license information.

---

*This project uses open source libraries. All code can be reused; do not remove author tags.*
