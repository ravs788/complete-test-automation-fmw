# Elasticsearch Logging Setup Guide

## Prerequisites
- Elasticsearch running at http://localhost:5601
- Credentials: Username `elastic`, Password `elastic123`

## Configuration Steps

### 1. Framework Configuration (Already Completed)
The `logging-config.json` has been updated with:
```json
{
  "Provider": "elastic",
  "Elastic": {
    "Url": "http://localhost:5601",
    "Username": "elastic",
    "Password": "elastic123"
  },
  "Console": {
    "Enabled": true
  }
}
```

### 2. Index Patterns
The framework will create indices with the following patterns:
- **Logs**: `logs-default` (default index for log entries)
- **Test Results**: `search-{projectname}` (for test metadata/results)

### 3. Running Tests with Elasticsearch Logging

When you run your tests, the framework will:
1. Automatically detect if Elasticsearch is reachable at http://localhost:5601
2. If reachable, enable logging and send all logs to Elasticsearch
3. If not reachable, disable logging and print a warning to console

#### Running API Tests
```bash
# Windows
bat\run_api_tests_gen_report.bat

# Linux/Mac
sh/run_api_tests_gen_report.sh
```

#### Running Web Tests
```bash
# Windows
bat\run_web_tests_gen_report.bat

# Linux/Mac
sh/run_web_tests_gen_report.sh
```

#### Running All Tests
```bash
# Windows
bat\run_all_tests_gen_report.bat

# Linux/Mac
sh/run_all_tests_gen_report.sh
```

### 4. Viewing Logs in Kibana

1. Open Kibana at http://localhost:5601
2. Go to **Management > Stack Management > Index Patterns**
3. Create index patterns for:
   - `logs-*` to view all log entries
   - `search-*` to view test results/metadata

### 5. Log Entry Structure

Each log entry contains:
```json
{
  "Timestamp": "2026-01-06T00:00:00Z",
  "Level": "INFO|ERROR|DEBUG",
  "Message": "Log message",
  "Metadata": {
    "ProjectName": "API.Tests",
    "TestName": "TestMethodName",
    "TestStatus": "Pass|Fail",
    "Duration": 1234,
    // Additional metadata fields
  }
}
```

### 6. Troubleshooting

#### Check Connectivity
The framework automatically tests connectivity. If you see this error:
```
[ElasticLoggingService] Elastic unreachable at 'http://localhost:5601'. Logging disabled.
```

Verify:
1. Elasticsearch is running at http://localhost:5601
2. Credentials are correct (username: elastic, password: elastic123)
3. No firewall blocking port 5601

#### Manual Connectivity Test
You can test connectivity using curl:
```bash
curl -u elastic:elastic123 http://localhost:5601
```

#### Environment Variables
You can override the Elasticsearch server choice:
```bash
# Use secure HTTPS connection
export ELASTIC_SERVER=ON_LOCALHOST_SECURE

# Use cloud instance
export ELASTIC_SERVER=ON_CLOUD

# Default (HTTP)
export ELASTIC_SERVER=ON_LOCALHOST_INSECURE
```

### 7. Custom Index Names

To use custom index names in your tests, configure the logging service:
```csharp
var loggingService = new ElasticLoggingService();
loggingService.Configure(
    indexFormat: "my-custom-index-{0:yyyy-MM-dd}",
    username: "elastic",
    password: "elastic123",
    elasticUrl: "http://localhost:5601"
);
```

### 8. Test Integration

The framework automatically integrates with your tests. When tests run:
1. **Before Each Test**: Logging service is initialized
2. **During Test**: All logs (Info, Debug, Error) are sent to Elasticsearch
3. **After Test**: Test metadata (pass/fail, duration) is indexed
4. **Test Completion**: Results can be viewed in Kibana

### 9. Creating Kibana Dashboards

After running some tests, you can create dashboards in Kibana:

1. Go to **Analytics > Dashboard**
2. Create a new dashboard
3. Add visualizations for:
   - Test pass/fail rates
   - Test execution times
   - Error frequency
   - Log levels distribution
   - Test trends over time

### 10. Example Query in Kibana

To view all ERROR logs:
```
Level: "ERROR"
```

To view failed tests:
```
Metadata.TestStatus: "Fail"
```

To view logs from specific test:
```
Metadata.TestName: "YourTestMethodName"
