# TrainChecker Tests

This directory contains comprehensive unit and integration tests for the TrainChecker application.

## Test Structure

### Unit Tests

#### Controllers
- **TrainControllerTests.cs** - Tests for the TrainController API endpoint
  - Tests successful responses
  - Tests null responses
  - Tests parameter validation
  - Tests exception handling

#### Services
- **TrainServiceTests.cs** - Tests for the main TrainService business logic
  - Tests message formatting for different train statuses (on time, delayed, cancelled)
  - Tests handling of null/empty responses
  - Tests delay calculation logic
  - Tests Telegram message sending integration

- **NationalRailServiceTests.cs** - Tests for the National Rail API integration
  - Tests HTTP request building
  - Tests response deserialization
  - Tests error handling and logging
  - Tests API endpoint construction

- **TelegramServiceTests.cs** - Tests for the Telegram bot integration
  - Tests message sending
  - Tests request formatting
  - Tests error handling
  - Tests special character handling

#### Jobs
- **TrainCheckJobTests.cs** - Tests for the Quartz scheduled job
  - Tests job execution with valid parameters
  - Tests error handling for missing parameters
  - Tests exception logging
  - Tests job data validation

#### Models
- **TrainServicesTests.cs** - Tests for data models and JSON serialization
  - Tests JSON serialization/deserialization
  - Tests null value handling
  - Tests property mapping
  - Tests API response parsing

#### Options
- **OptionsTests.cs** - Tests for configuration option classes
  - Tests default values
  - Tests property setting
  - Tests section name constants

### Integration Tests

#### Integration
- **TrainControllerIntegrationTests.cs** - End-to-end tests
  - Tests full application flow with mocked external services
  - Tests configuration handling
  - Tests application startup

### Test Helpers

#### Helpers
- **TestHelpers.cs** - Utility methods for creating test data
  - Factory methods for creating sample responses
  - Helper methods for different train statuses
  - Message formatting utilities

## Running Tests

### Prerequisites
- .NET 9.0 SDK
- All test dependencies are included in the project file

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Categories
```bash
# Run only unit tests
dotnet test --filter "FullyQualifiedName!~Integration"

# Run only integration tests
dotnet test --filter "FullyQualifiedName~Integration"

# Run tests for a specific class
dotnet test --filter "ClassName=TrainServiceTests"
```

### Generate Test Coverage Report
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Test Configuration

The tests use the following configuration:
- **TestSettings.json** - Test-specific configuration values
- In-memory configuration for integration tests
- Mocked HTTP clients for external API calls

## Test Patterns Used

### Mocking
- **Moq** framework for creating mock objects
- HTTP message handler mocking for external API calls
- Logger mocking for testing log output

### Test Data
- Factory pattern for creating test objects
- Builder pattern for complex test scenarios
- Parameterized tests for testing multiple scenarios

### Assertions
- **xUnit** assertions for test validation
- Custom assertion helpers for complex objects
- Fluent assertions for readable test code

## Coverage Areas

The test suite covers:

✅ **Controllers** - API endpoint behavior  
✅ **Services** - Business logic and external integrations  
✅ **Jobs** - Scheduled task execution  
✅ **Models** - Data serialization and validation  
✅ **Options** - Configuration handling  
✅ **Integration** - End-to-end application flow  

### Key Test Scenarios

1. **Happy Path Testing**
   - Normal train status retrieval and messaging
   - Successful API responses
   - Proper message formatting

2. **Error Handling**
   - API failures and timeouts
   - Invalid configuration
   - Missing or malformed data

3. **Edge Cases**
   - Null/empty responses
   - Special characters in messages
   - Timezone handling
   - Delay calculations

4. **Integration Testing**
   - Full application workflow
   - Service dependency injection
   - Configuration binding

## Continuous Integration

These tests are designed to run in CI/CD pipelines:
- No external dependencies required
- Fast execution times
- Deterministic results
- Clear failure reporting

## Adding New Tests

When adding new functionality:

1. **Unit Tests** - Test individual components in isolation
2. **Integration Tests** - Test component interactions
3. **Test Data** - Add factory methods to TestHelpers
4. **Documentation** - Update this README with new test categories

### Test Naming Convention

- Test class: `{ClassUnderTest}Tests`
- Test method: `{MethodUnderTest}_{Scenario}_{ExpectedResult}`
- Example: `GetTrainStatus_WithValidResponse_ReturnsOkResult`