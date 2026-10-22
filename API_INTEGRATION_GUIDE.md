# Test Service - API Integration Guide

**Version:** 1.0.0  
**Base URL:** `http://localhost:5000` (Development) | `https://your-domain.com` (Production)  
**Authentication:** JWT Bearer Token  
**Date:** December 16, 2025

---

## Table of Contents

1. [Authentication](#1-authentication)
2. [Entity Schemas](#2-entity-schemas)
3. [Dynamic Entities](#3-dynamic-entities)
4. [Environments](#4-environments)
5. [Settings & API Keys](#5-settings--api-keys)
6. [Users](#6-users)
7. [Mocks (Import, Duplicate, Callbacks & Praxis Emulator)](#7-mocks-import-duplicate-callbacks--praxis-emulator)
8. [Data Models](#8-data-models)
9. [Error Handling](#9-error-handling)
10. [Integration Examples](#10-integration-examples)

---

## 1. Authentication

### 1.1 Login
**Endpoint:** `POST /api/auth/login`  
**Authorization:** None (Public)  
**Description:** Authenticate user and receive JWT token

#### Request Body:
```json
{
  "username": "admin",
  "password": "Admin@123"
}
```

#### Response (200 OK):
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "507f1f77bcf86cd799439011",
    "username": "admin",
    "email": "admin@example.com",
    "role": "Admin",
    "createdAt": "2025-12-16T10:00:00Z"
  }
}
```

#### Default Credentials:
- Username: `admin`
- Password: `Admin@123`

### 1.2 Get Current User
**Endpoint:** `GET /api/auth/me`  
**Authorization:** Bearer Token Required  
**Description:** Get authenticated user information

#### Response (200 OK):
```json
{
  "id": "507f1f77bcf86cd799439011",
  "username": "admin",
  "email": "admin@example.com",
  "role": "Admin",
  "createdAt": "2025-12-16T10:00:00Z"
}
```

---

## 2. Entity Schemas

Entity schemas define the structure of dynamic entity types. Think of them as "tables" in a traditional database.

### 2.1 Get All Schemas
**Endpoint:** `GET /api/schemas`  
**Authorization:** Bearer Token Required

#### Response (200 OK):
```json
[
  {
    "id": "507f1f77bcf86cd799439011",
    "entityName": "test-agent",
    "fields": [
      {
        "name": "username",
        "type": "string",
        "required": true,
        "isUnique": true,
        "description": "User's login name"
      },
      {
        "name": "userId",
        "type": "number",
        "required": true,
        "isUnique": true,
        "description": null
      },
      {
        "name": "brandId",
        "type": "number",
        "required": false,
        "isUnique": false,
        "description": null
      }
    ],
    "filterableFields": ["username", "brandId"],
    "uniqueFields": [],
    "useCompoundUnique": false,
    "excludeOnFetch": false,
    "createdAt": "2025-12-16T10:00:00Z",
    "updatedAt": "2025-12-16T10:00:00Z"
  }
]
```

### 2.2 Get Schema by Name
**Endpoint:** `GET /api/schemas/{entityName}`  
**Authorization:** Bearer Token Required

**Example:** `GET /api/schemas/test-agent`

#### Response (200 OK):
Same structure as single schema above.

### 2.3 Create Schema
**Endpoint:** `POST /api/schemas`  
**Authorization:** Bearer Token Required  
**Description:** Create a new entity schema (defines entity structure)

#### Request Body:
```json
{
  "entityName": "test-agent",
  "fields": [
    {
      "name": "username",
      "type": "string",
      "required": true,
      "isUnique": true,
      "description": "User's login name"
    },
    {
      "name": "userId",
      "type": "number",
      "required": true,
      "isUnique": true
    },
    {
      "name": "brandId",
      "type": "number",
      "required": false,
      "isUnique": false
    },
    {
      "name": "password",
      "type": "string",
      "required": true,
      "isUnique": false
    }
  ],
  "filterableFields": ["username", "brandId"],
  "excludeOnFetch": false
}
```

#### Field Types:
- `string` - Text values
- `number` - Numeric values (integer or decimal)
- `boolean` - True/False values
- `date` - Date/DateTime values
- `array` - List of values
- `object` - Nested JSON objects

#### Field Properties:
- `name` (string, required): Field name (alphanumeric + underscore)
- `type` (string, required): Data type
- `required` (boolean): If true, field must be provided when creating entities
- `isUnique` (boolean): If true, no two entities can have the same value for this field
- `description` (string, optional): Field description

#### Schema Properties:
- `entityName` (string, required): Name of the entity type (e.g., "test-agent")
- `fields` (array, required): List of field definitions
- `filterableFields` (array, optional): Fields that can be used in filter queries
- `uniqueFields` (array, optional): For compound unique constraints
- `useCompoundUnique` (boolean): If true, uniqueFields are checked together as a compound key
- `excludeOnFetch` (boolean): If true, entities are marked as "consumed" when fetched (useful for parallel test execution)

#### Response (201 Created):
Returns the created schema with ID and timestamps.

### 2.4 Update Schema
**Endpoint:** `PUT /api/schemas/{entityName}`  
**Authorization:** Bearer Token Required

#### Request Body:
Same as create, but for updating existing schema.

### 2.5 Delete Schema
**Endpoint:** `DELETE /api/schemas/{entityName}`  
**Authorization:** Bearer Token Required

#### Response (204 No Content):
Schema deleted successfully.

---

## 3. Dynamic Entities

Dynamic entities are instances of the schemas you've defined. Each entity conforms to its schema's structure.

### 3.1 Get All Entities by Type
**Endpoint:** `GET /api/entities/{entityType}?environment={env}`  
**Authorization:** Bearer Token Required  
**Description:** Get all entities of a specific type

**Example:** `GET /api/entities/test-agent?environment=qa`

#### Query Parameters:
- `environment` (optional): Filter by environment (dev, staging, qa, prod)

#### Response (200 OK):
```json
[
  {
    "id": "507f1f77bcf86cd799439011",
    "entityType": "test-agent",
    "environment": "qa",
    "fields": {
      "username": "AutomationMgr_213",
      "userId": 12283351,
      "brandId": 14,
      "password": "welcome123"
    },
    "isConsumed": false,
    "createdAt": "2025-12-16T10:00:00Z",
    "updatedAt": "2025-12-16T10:00:00Z"
  }
]
```

### 3.2 Get Entity by ID
**Endpoint:** `GET /api/entities/{entityType}/{id}`  
**Authorization:** Bearer Token Required

**Example:** `GET /api/entities/test-agent/507f1f77bcf86cd799439011`

#### Response (200 OK):
Returns single entity object.

**Note:** If schema has `excludeOnFetch: true`, this endpoint will mark the entity as consumed.

### 3.3 Get Next Available Entity (Atomic)
**Endpoint:** `GET /api/entities/{entityType}/next?environment={env}`  
**Authorization:** Bearer Token Required  
**Description:** Atomically fetch and mark an entity as consumed (for parallel test execution)

**Example:** `GET /api/entities/test-agent/next?environment=qa`

#### Use Case:
Perfect for parallel test execution where multiple tests need unique test data simultaneously. This endpoint:
1. Finds a non-consumed entity
2. Marks it as consumed
3. Returns it
All in a single atomic operation to prevent race conditions.

#### Requirements:
- Schema must have `excludeOnFetch: true`

#### Response (200 OK):
Returns single entity object.

#### Response (404 Not Found):
```json
{
  "error": "No available entities found"
}
```

### 3.4 Filter Entities by Field Value
**Endpoint:** `GET /api/entities/{entityType}/filter/{fieldName}/{value}?environment={env}`  
**Authorization:** Bearer Token Required

**Example:** `GET /api/entities/test-agent/filter/brandId/14?environment=qa`

#### Requirements:
- Field must be in schema's `filterableFields` array

#### Response (200 OK):
Returns array of matching entities.

### 3.5 Create Entity
**Endpoint:** `POST /api/entities/{entityType}`  
**Authorization:** Bearer Token Required  
**Description:** Create a new entity instance

**Example:** `POST /api/entities/test-agent`

#### Request Body:
```json
{
  "fields": {
    "username": "AutomationMgr_213",
    "userId": 12283351,
    "brandId": 14,
    "password": "welcome123"
  },
  "environment": "qa"
}
```

#### Validation Rules:
- All `required` fields must be provided
- Fields marked as `isUnique` cannot have duplicate values
- Field values must match their defined types
- **Environment must exist** if provided (validates against Environments collection)
- Unknown fields (not in schema) are ignored

#### Response (201 Created):
```json
{
  "id": "507f1f77bcf86cd799439011",
  "entityType": "test-agent",
  "environment": "qa",
  "fields": {
    "username": "AutomationMgr_213",
    "userId": 12283351,
    "brandId": 14,
    "password": "welcome123"
  },
  "isConsumed": false,
  "createdAt": "2025-12-16T10:00:00Z",
  "updatedAt": "2025-12-16T10:00:00Z"
}
```

#### Error (400 Bad Request):
```json
{
  "error": "Entity does not match schema for type: test-agent"
}
```

#### Error (400 Bad Request - Invalid Environment):
```json
{
  "error": "Environment 'qa' does not exist. Please create it first."
}
```

### 3.6 Update Entity
**Endpoint:** `PUT /api/entities/{entityType}/{id}`  
**Authorization:** Bearer Token Required

**Example:** `PUT /api/entities/test-agent/507f1f77bcf86cd799439011`

#### Request Body:
```json
{
  "fields": {
    "username": "UpdatedUsername",
    "userId": 12283351,
    "brandId": 15,
    "password": "newpassword123"
  },
  "environment": "qa"
}
```

#### Response (204 No Content):
Entity updated successfully.

### 3.7 Delete Entity
**Endpoint:** `DELETE /api/entities/{entityType}/{id}`  
**Authorization:** Bearer Token Required

**Example:** `DELETE /api/entities/test-agent/507f1f77bcf86cd799439011`

#### Response (204 No Content):
Entity deleted successfully.

### 3.8 Reset Consumed Status
**Endpoint:** `POST /api/entities/{entityType}/{id}/reset`  
**Authorization:** Bearer Token Required  
**Description:** Reset entity's consumed status (marks it as available again)

**Example:** `POST /api/entities/test-agent/507f1f77bcf86cd799439011/reset`

#### Response (200 OK):
```json
{
  "message": "Entity reset successfully"
}
```

### 3.9 Reset All Consumed Entities
**Endpoint:** `POST /api/entities/{entityType}/reset-all?environment={env}`  
**Authorization:** Bearer Token Required  
**Description:** Reset all consumed entities of a type (optionally filtered by environment)

**Example:** `POST /api/entities/test-agent/reset-all?environment=qa`

#### Response (200 OK):
```json
{
  "message": "Reset 25 entities"
}
```

### 3.10 Export Entities (Bulk)
**Endpoint:** `GET /api/entities/{entityType}/export?environment={env}&format={csv|json}`  
**Authorization:** Bearer Token Required (Entities Read)  
**Description:** Export all entities of a type as JSON array or CSV. CSV header row is derived from schema field names; optional `environment` column when entities have environment set.

**Example:** `GET /api/entities/test-agent/export?format=csv&environment=qa`

#### Response (200 OK):
- `format=json`: application/json — array of entity objects (same structure as GET entities).
- `format=csv`: text/csv — header row plus one row per entity. File name: `{entityType}-export.{csv|json}`.

### 3.11 Import Entities (Bulk)
**Endpoint:** `POST /api/entities/{entityType}/import?environment={env}&mode={append|upsert}`  
**Authorization:** Bearer Token Required (Entities Write)  
**Description:** Bulk import entities from a CSV or JSON file (multipart form, field name `file`).

- **environment** (optional): Default environment for imported rows (can be overridden per row in data).
- **mode**: `append` — always create new entities; `upsert` — update existing if unique key matches (schema’s unique/compound unique fields), otherwise create.

**JSON format:** Array of objects. Each object can have `fields` (object with field names and values) and optional `environment`. Alternatively, flat objects with field names at root and optional `environment`.

**CSV format:** First row = headers (field names; optional `environment` column). Subsequent rows = values. Headers must match schema field names.

**Limits:** Max file size 5 MB; max 5000 rows.

#### Response (200 OK):
```json
{
  "created": 10,
  "updated": 2,
  "skipped": 1,
  "errors": [
    { "row": 5, "message": "Required field 'name' is missing." }
  ]
}
```

---

## 4. Environments

Environments allow you to segment your test data (dev, staging, qa, production, etc.).

### 4.1 Get All Environments
**Endpoint:** `GET /api/environments?includeInactive={bool}&includeStatistics={bool}`  
**Authorization:** Bearer Token Required

#### Query Parameters:
- `includeInactive` (boolean, default: false): Include inactive environments
- `includeStatistics` (boolean, default: false): Include entity counts per environment

#### Response (200 OK):
```json
[
  {
    "id": "507f1f77bcf86cd799439011",
    "name": "dev",
    "displayName": "Development",
    "description": "Development environment",
    "url": "https://dev.example.com",
    "color": "#00ff00",
    "order": 1,
    "isActive": true,
    "configuration": {
      "apiTimeout": "30",
      "maxRetries": "3"
    },
    "tags": ["development", "local"],
    "createdAt": "2025-12-16T10:00:00Z",
    "updatedAt": "2025-12-16T10:00:00Z",
    "entityCounts": {
      "test-agent": 45,
      "test-user": 120
    }
  }
]
```

### 4.2 Get Environment by ID
**Endpoint:** `GET /api/environments/{id}?includeStatistics={bool}`  
**Authorization:** Bearer Token Required

### 4.3 Get Environment by Name
**Endpoint:** `GET /api/environments/name/{name}?includeStatistics={bool}`  
**Authorization:** Bearer Token Required

**Example:** `GET /api/environments/name/qa`

### 4.4 Get Environment Statistics
**Endpoint:** `GET /api/environments/{name}/statistics`  
**Authorization:** Bearer Token Required

**Example:** `GET /api/environments/qa/statistics`

#### Response (200 OK):
```json
{
  "environmentName": "qa",
  "totalEntities": 165,
  "entityCounts": {
    "test-agent": 45,
    "test-user": 120
  },
  "consumedCounts": {
    "test-agent": 12,
    "test-user": 8
  }
}
```

### 4.5 Create Environment
**Endpoint:** `POST /api/environments`  
**Authorization:** Bearer Token Required (Admin Role)

#### Request Body:
```json
{
  "name": "qa",
  "displayName": "QA Environment",
  "description": "Quality Assurance testing environment",
  "url": "https://qa.example.com",
  "color": "#0000ff",
  "order": 4,
  "configuration": {
    "apiKey": "qa-api-key",
    "timeout": "30"
  },
  "tags": ["qa", "testing"]
}
```

#### Response (201 Created):
Returns created environment with ID and timestamps.

### 4.6 Update Environment
**Endpoint:** `PUT /api/environments/{id}`  
**Authorization:** Bearer Token Required (Admin Role)

### 4.7 Delete Environment
**Endpoint:** `DELETE /api/environments/{id}`  
**Authorization:** Bearer Token Required (Admin Role)

---

## 5. Settings & API Keys

### 5.1 Get Application Settings
**Endpoint:** `GET /api/settings`  
**Authorization:** Bearer Token Required (Admin Role)

#### Response (200 OK):
```json
{
  "id": "507f1f77bcf86cd799439011",
  "dataRetentionDays": 30,
  "enableNotifications": true,
  "maxEntitiesPerType": 10000,
  "allowParallelExecution": true,
  "updatedAt": "2025-12-16T10:00:00Z",
  "updatedBy": "admin"
}
```

### 5.2 Update Application Settings
**Endpoint:** `PUT /api/settings`  
**Authorization:** Bearer Token Required (Admin Role)

#### Request Body:
Same structure as GET response.

### 5.3 Get API Keys
**Endpoint:** `GET /api/settings/api-keys`  
**Authorization:** Bearer Token Required (Admin Role)

#### Response (200 OK):
```json
[
  {
    "id": "507f1f77bcf86cd799439011",
    "name": "Integration Key 1",
    "key": "sk_test_abc123...",
    "expiresAt": "2026-12-16T10:00:00Z",
    "createdAt": "2025-12-16T10:00:00Z",
    "createdBy": "admin",
    "lastUsed": "2025-12-16T15:30:00Z",
    "isActive": true
  }
]
```

### 5.4 Create API Key
**Endpoint:** `POST /api/settings/api-keys`  
**Authorization:** Bearer Token Required (Admin Role)

#### Request Body:
```json
{
  "name": "Integration Key 1",
  "expiresInDays": 365
}
```

#### Response (201 Created):
Returns created API key with generated key value.

### 5.5 Delete API Key
**Endpoint:** `DELETE /api/settings/api-keys/{id}`  
**Authorization:** Bearer Token Required (Admin Role)

---

## 6. Users

### 6.1 Get All Users
**Endpoint:** `GET /api/users`  
**Authorization:** Bearer Token Required (Admin Role)

#### Response (200 OK):
```json
[
  {
    "id": "507f1f77bcf86cd799439011",
    "username": "admin",
    "email": "admin@example.com",
    "role": "Admin",
    "isActive": true,
    "createdAt": "2025-12-16T10:00:00Z",
    "lastLogin": "2025-12-16T15:30:00Z"
  }
]
```

### 6.2 Get User by ID
**Endpoint:** `GET /api/users/{id}`  
**Authorization:** Bearer Token Required (Admin Role)

### 6.3 Get User by Username
**Endpoint:** `GET /api/users/username/{username}`  
**Authorization:** Bearer Token Required (Admin Role)

### 6.4 Create User
**Endpoint:** `POST /api/users`  
**Authorization:** Bearer Token Required (Admin Role)

#### Request Body:
```json
{
  "username": "testuser",
  "email": "test@example.com",
  "password": "SecurePass@123",
  "role": "User"
}
```

#### Roles:
- `Admin` - Full access
- `User` - Read/Write access to entities
- `ReadOnly` - Read-only access

### 6.5 Update User
**Endpoint:** `PUT /api/users/{id}`  
**Authorization:** Bearer Token Required (Admin Role)

### 6.6 Delete User
**Endpoint:** `DELETE /api/users/{id}`  
**Authorization:** Bearer Token Required (Admin Role)

### 6.7 Change Password
**Endpoint:** `POST /api/users/{id}/change-password`  
**Authorization:** Bearer Token Required

#### Request Body:
```json
{
  "currentPassword": "OldPass@123",
  "newPassword": "NewPass@123"
}
```

---

## 7. Mocks (Import, Duplicate, Callbacks & Praxis Emulator)

### 7.1 Import from Postman Collection
**Endpoint:** `POST /api/mocks/expectations/import/postman?targetEnvironment={env}&pathPrefix={prefix}`  
**Authorization:** Bearer Token Required (Mocks Write)  
**Description:** Import mock expectations from a Postman Collection v2.1 JSON file. Request method, URL path, headers, body, and the first response (status, body, headers) are mapped to expectations for the given target environment.

- **targetEnvironment** (required): Environment name for all created expectations (e.g. `qa2`, `st`). Runtime URL will be `/mock/{targetEnvironment}/{path}`.
- **pathPrefix** (optional): Prefix to prepend to each request path (e.g. `/api`).
- **Body:** multipart form with field `file` (Postman Collection JSON).

#### Response (200 OK):
```json
{
  "created": 5,
  "errors": []
}
```

### 7.2 Duplicate Expectation to Another Environment
**Endpoint:** `POST /api/mocks/expectations/{id}/duplicate`  
**Authorization:** Bearer Token Required (Mocks Write)  
**Description:** Clone an existing expectation into another environment. The path is unchanged; the new expectation is available at `/mock/{targetEnvironment}/{path}`.

#### Request Body:
```json
{
  "targetEnvironment": "st"
}
```

#### Response (200 OK):
Returns the newly created mock expectation (same structure as a single expectation). The name is suffixed with ` ({targetEnvironment})`.

### 7.3 Outbound Callbacks and the Callback Log
Callback delivery is the component provider emulators use to send delayed outbound requests, such as signed payment notifications, to a merchant's callback URL. Its producer is the Praxis emulator (§7.5), which sends its signed notifications through it. A producer builds each request completely and a background dispatcher sends it unchanged at its due time. There are no retries, and requests still pending when the service stops are not resent.

**Allowed hosts.** A callback is sent only if its URL host is listed in `Mocking:AllowedCallbackHosts` (exact, case-insensitive host match; the port is ignored). An empty list means nothing is sent. Defaults: `localhost` in Development and Docker, nothing in `appsettings.json` and Production. Set it with environment variables, one per index:

```bash
Mocking__AllowedCallbackHosts__0=billing.qa2.local
Mocking__AllowedCallbackHosts__1=localhost
```

An environment variable replaces the appsettings entry at the same index; it does not append.

**Queue.** At most 1000 callbacks can be pending (enqueued and not yet logged); outbound requests time out after 30 seconds and only the first 64 KB of the reply body is kept. Redirects are not followed: a 3xx reply is logged as the target's response.

Every attempt is one entry in the callback log (MongoDB collection `MockCallbackLogs`), including refused, failed, dropped and lost ones:

| `error` | Meaning |
|---|---|
| `null` | Sent; `responseStatusCode`, `responseHeaders` and `responseBody` hold the target's reply |
| `host not allowed` | The URL host is not in `Mocking:AllowedCallbackHosts`; nothing was sent |
| `invalid url` | The URL is not an absolute http(s) URL; nothing was sent |
| `dropped: queue full` | 1000 callbacks were already pending |
| `lost: shutdown` | The service stopped before the callback was sent, or while it was in flight (the status code is kept if the reply headers had arrived) |
| any other text | Connection error or 30 s timeout; no status code |

#### 7.3.1 List Callback Log
**Endpoint:** `GET /api/mocks/callbacks?environment={env}&source={source}&correlationId={id}&limit={n}`  
**Authorization:** Bearer Token Required (`mocks.logs.read`)  
**Description:** Newest first (by `sentAt`). All filters are optional; `limit` is 1..1000, default 100.

#### Response (200 OK):
```json
[
  {
    "id": "6717a3f0c2b1e4a5d6f70812",
    "source": "praxis",
    "environment": "qa2",
    "correlationId": "1234567",
    "url": "http://localhost:5000/api/praxis/notification",
    "method": "POST",
    "requestHeaders": { "Content-Type": "application/json", "GT-Authentication": "9f1c..." },
    "requestBody": "{\"status\":0,...}",
    "responseStatusCode": 200,
    "responseHeaders": { "Content-Type": "application/json" },
    "responseBody": "{\"status\":\"ok\"}",
    "error": null,
    "dueAt": "2026-10-22T10:15:00.000Z",
    "sentAt": "2026-10-22T10:15:00.004Z",
    "durationMs": 37
  }
]
```

| Field | Description |
|---|---|
| `source` | Producer of the callback, for example `praxis` |
| `correlationId` | Producer's id, for example the emulated transaction id |
| `requestHeaders`, `requestBody` | The request exactly as built by the producer |
| `responseStatusCode` | Target's HTTP status; `null` when nothing was received |
| `responseBody` | First 64 KB of the target's reply |
| `dueAt` | When the producer asked for the callback to be sent |
| `sentAt` | When the attempt was recorded: send start for real sends, log time for refused, dropped and lost entries |
| `durationMs` | Duration of the HTTP exchange |

#### 7.3.2 Delete Callback Log
**Endpoint:** `DELETE /api/mocks/callbacks?environment={env}`  
**Authorization:** Bearer Token Required (`mocks.logs.delete`)  
**Description:** Deletes the callback log entries of one environment (all entries when `environment` is omitted).

#### Response (200 OK):
```json
{ "deletedCount": 3 }
```

### 7.4 Regex Patterns, Verification and Log Retention
**Regex patterns.** When `requestMatcher.pathMatchType` is `Regex` (path) or `requestMatcher.bodyMatchType` is `Regex` (body), the pattern is compiled when the expectation is created, updated or duplicated. An invalid pattern is rejected and nothing is stored:

```json
HTTP 400
{ "message": "requestMatcher.path is not a valid regular expression: Invalid pattern '/(' at offset 2. Not enough )'s." }
```

At match time each regex evaluation is limited to 100 ms; a pattern that runs longer counts as no match for that expectation (and for verification) and a warning naming the expectation is logged.

**Verification.** `POST /api/mocks/verify` counts every logged request of the environment that matches, however many there are, with the same rules as live `/mock/*` requests: `query` is matched against the logged query string and header names are case-insensitive.

**Log retention.** Setting `dataRetention.mockLogRetentionDays` (`GET`/`PUT /api/settings`, default `7`, `null` keeps logs forever) applies to both mock request logs (by `timestamp`) and callback logs (by `sentAt`). The hourly cleanup deletes entries older than that while `dataRetention.autoCleanupEnabled` is `true`. Settings stored before this field existed read as `7`. Mock expectations and logs are never touched by entity retention.

### 7.5 Praxis Emulator
A self-contained Praxis emulator lives at `/emulators/praxis/{environment}/<Praxis path>`, so BillingServer only changes its Praxis base URLs. It behaves like the Praxis sandbox "Test Card Processor" for every documented test case of https://docs.praxis.tech/docs/testing that the API can express (corrected by redacted sandbox captures where they differ): signed requests and responses, encrypted card data, state across calls (find-transaction, capture, refund of an earlier sale), a 3DS simulation page and delayed signed notifications sent through callback delivery (§7.3). test-service never calls a Praxis host. E-wallet flows are not emulated yet.

The emulator is anonymous like `/mock/*`, and it answers only for environments that have at least one registered merchant. Any other path answers HTTP 404 with a JSON `message` and stores nothing (also for an unknown provider, for example `/emulators/other/...`).

#### 7.5.1 Merchants
| Endpoint | Authorization | Result |
|---|---|---|
| `GET /api/emulators/praxis/merchants?environment={env}` | `mocks.read` | Merchants (all environments when `environment` is omitted) |
| `POST /api/emulators/praxis/merchants` | `mocks.write` | 201 with the merchant; 400 `{ message }` when invalid; 409 when `environment` + `merchantId` exists |
| `PUT /api/emulators/praxis/merchants/{id}` | `mocks.write` | 200 with the merchant; 404 unknown id; 400; 409. Without `secret`, the stored secret is kept |
| `DELETE /api/emulators/praxis/merchants/{id}` | `mocks.write` | 204; 404 unknown id |

```json
{
  "environment": "qa2",
  "merchantId": "<BillingServer merchant id>",
  "applicationKey": "<BillingServer application key>",
  "secret": "<BillingServer gateway password>",
  "gateways": [
    { "id": "040e154f306f145b84208512d00ef8d9", "kind": "card", "paymentProcessor": "Test Card Processor", "paymentMethod": "Credit Card" }
  ],
  "dropNotifications": false,
  "timeScale": 1.0
}
```

| Field | Rules |
|---|---|
| `environment` | Required; the `{environment}` path segment |
| `merchantId` | Required; unique per environment; must equal the request's `merchant_id` |
| `applicationKey` | Required; must equal the request's `application_key` |
| `secret` | Required on create, at most 32 characters (it is the AES-256 key, left-padded with `0`); write-only: responses carry `secretLength` instead |
| `gateways` | At least one `{ id, kind: card\|ewallet, paymentProcessor, paymentMethod }`. A request's `gateway` that is not registered is accepted; processor and method then default to `Test Card Processor` / `Credit Card` |
| `dropNotifications` | Default `false`; `true` sends no notifications for this merchant |
| `timeScale` | Number >= 0, default `1.0`; multiplies every emulator delay (notification delays, 3DS and pending timeouts). Tests use `0` (everything immediate) or a small positive value when they need the 3DS page |

#### 7.5.2 Endpoints, Signatures and Rejections
| Praxis endpoint | Path under `/emulators/praxis/{env}/` |
|---|---|
| Card sale or payout | `POST api/direct-process` (`transaction_type` `sale` or `payout`; a payout may send `card_token` + `customer_token` of an earlier sale instead of `card_data`) |
| Card refund | `POST direct/refund` (`reference_id` = tid of the sale) |
| Find transaction | `POST agent/find-transaction` |
| Capture / cancel an authorized sale | `POST agent/manage-transaction` with `intent` `capture-authorization` or `cancel-authorization` |
| 3DS page (the sale's `redirect_url`) | `GET pages/3ds/{tid}`; its buttons POST `error_code` to `pages/3ds/{tid}/submit?status=approved\|declined\|pending\|verification`, which redirects (302) to the sale's `return_url` |

**Request signature.** Header `GT-Authentication` = lowercase hex SHA-384 of the concatenated fields followed by the secret (a null or missing field is empty, a number is its JSON text):

| Endpoint | Fields |
|---|---|
| `api/direct-process` | `merchant_id, application_key, timestamp, transaction_type, cid, order_id, currency, amount, gateway, notification_url, return_url`, then `card_token` or else `card_data.card_number` (the cipher text) |
| `direct/refund` | `merchant_id, application_key, timestamp, transaction_type, cid, order_id, currency, amount, reference_id, withdrawal_request_id` |
| `agent/find-transaction` | `merchant_id, application_key, timestamp, tid` |
| `agent/manage-transaction` | `merchant_id, application_key, timestamp, intent, tid` |

**Card data.** `card_number`, `card_exp` (`MM/YYYY`) and `cvv` are AES-256-CBC, PKCS7, Base64; key = secret left-padded with `0` to 32 bytes, IV = the request `timestamp` left-padded with `0` to 16 bytes.

**Response and notification signatures.** Every successful (`status 0`) response carries `GT-Authentication` = SHA-384 of `status, timestamp, customer.customer_token, transaction.tid, transaction.transaction_status, transaction.processed_currency, transaction.processed_amount` + secret. Notifications are POSTed as JSON to the request's `notification_url` with `GT-Authentication` = SHA-384 of `merchant_id, application_key, timestamp, customer.customer_token, session.order_id, transaction.tid, transaction.currency, transaction.amount, transaction.conversion_rate, transaction.processed_currency, transaction.processed_amount` + secret. The merchant's reply is only logged: read it with `GET /api/mocks/callbacks?source=praxis&correlationId={tid}` (§7.3). The notification host must be listed in `Mocking:AllowedCallbackHosts`.

Bodies have the key sets of the captured Praxis responses (`transaction.amount` is the final amount, `processed_amount` the requested one); find-transaction carries `is_3d`, the other bodies `is_async`; manage-transaction answers only `status, description, transaction, version, timestamp`.

**Rejections** are HTTP 200 without a signature header, checked in this order:

| Check | Body |
|---|---|
| Unknown merchant or application key (in this environment), empty or invalid body | `{"status":401,"description":"User not found"}` |
| Wrong signature | `{"status":1,"description":"Validation Error","errors":{"merchant_id":["Invalid signature"]}}` |
| Timestamp older than 60 s on `agent/*` | `{"status":1,"description":"Validation Error","errors":{"timestamp":["Timestamp expired: 130s old, should be at most 60s old (required with merchant_id)"]}}` |
| Blank fields, decrypt failures and, last, an expired timestamp on `api/direct-process` / `direct/refund` | `{"status":400,"description":"City cannot be blank.,Address cannot be blank.,..."}` (a sale needs `customer_data.city`, `customer_data.address` and `device_data.user_agent, accept_header, language, ip_address, color_depth, pixel_depth, pixel_ratio`; `Failed to decrypt card_number`, `card_exp`, `cvv`; `Reference Id cannot be blank.`) |
| Missing `timestamp` on `agent/*` | `{"status":1,"description":"Validation Error","errors":{"timestamp":["Timestamp cannot be blank."]}}` (on `api/direct-process` / `direct/refund`, `Timestamp cannot be blank.` is the last item of the `status 400` list) |
| Non-integer `amount` on `api/direct-process` / `direct/refund` | `Amount must be an integer.` in the `status 400` list (after the blank fields) |
| Missing `tid` (find, manage) | `{"status":400,"description":"Tid cannot be blank."}` |
| Missing `intent` (manage) | `{"status":400,"description":"Intent cannot be blank."}` |
| `transaction_type` other than `sale`/`payout` on `api/direct-process` | `{"status":1,"description":"Gateway does not support the transaction type (authorization)"}` |
| Unknown tid, or a tid / `reference_id` of another merchant | `{"status":401,"description":"Wrong credentials"}` |
| Capture or cancel of a transaction that is not authorized, unknown intent | `{"status":1,"description":"Generic Error"}` |

The 3DS page answers with real HTTP statuses and a JSON `message`: `GET`/submit of an unknown tid (or one of another environment) 404; a transaction no longer awaiting 3DS (already decided, timed out, or not a 3DS sale) 409, including a second submit; submit with an unknown `status` 400.

#### 7.5.3 Transactions (for test assertions)
**Endpoint:** `GET /api/emulators/praxis/transactions?environment={env}&orderId={orderId}&tid={tid}`  
**Authorization:** Bearer Token Required (`mocks.read`)  
**Description:** Emulated transactions (MongoDB collection `PraxisTransactions`), newest first, at most 1000; every filter is optional. Each item has `tid`, `environment`, `merchantId`, `transactionType`, `orderId`, `currency`, `requestedAmount`, `amount`, `processedAmount`, `status` (wire name, for example `partial_approved`), `statusCode`, `statusDetails`, `sessionStatus`, `caseName`, `history` (one entry per transition), the pending `deadline` and the Praxis `providerData` (customer and card fields).

#### 7.5.4 Pointing BillingServer at the Emulator
1. Register the merchant (§7.5.1) with BillingServer's merchant id, application key, gateway password (`secret`) and card gateway hash.
2. Card gateway URLs: `<test-service>/emulators/praxis/<env>/api/direct-process` (sale, payout), `<test-service>/emulators/praxis/<env>/direct/refund`, and `<test-service>/emulators/praxis/<env>/` as the base BillingServer appends `agent/find-transaction` and `agent/manage-transaction` to.
3. E-wallet `SiteUrl` (once e-wallet flows are emulated): `<test-service>/emulators/praxis/<env>/`.
4. Allow BillingServer's notification host: `Mocking__AllowedCallbackHosts__0=<billing host>`.
5. Lower `timeScale` to shorten the 15 s notification delay and the 540 s timeouts.

#### 7.5.5 Test Cases (card, Test Card Processor)
Amounts are in minor units (`4900` = 49.00). CVV other than the listed ones declines.

| Trigger | First response | Then |
|---|---|---|
| CVV 568 | `approved`, `[TEST] Transaction status: approved` | approved notification |
| CVV 000 / other | `rejected`, `[TEST] Transaction status: declined` | rejected notification |
| CVV 353 | `partial_approved`, `[TEST] Transaction status: partial_approved. Reason: Notification with partial approved` | notification |
| CVV 444 | `error`, `[TEST] Transaction status: error. Reason: Internal server error` | - |
| CVV 555 | `authorized` | `capture-authorization` (amount 47 rejected, 48 error, other approved; `status_details` stays `[TEST] Transaction status: authorized`) or `cancel-authorization` (rejected, `Transaction status: Cancelled`) |
| CVV 801 | `rejected`, `Insufficient funds`, `GE-006` | - |
| CVV 900 / 901 / 902 / 903 / 904 / 905 | `rejected`: `Card has been lost` / `Card has been stolen` / `Declined due to fraud` / `Do not honor` / `Transaction declined` / `Transaction failed. Please contact our support team` (`GE-007`) | - |
| CVV 333 or 300 | `initialized`, `[TEST] Transaction status: pending_async. Reason: 3DS authentication required`, code `530`, `redirect_url` to the 3DS page | Approve: `approved`, `Approved`. Pending: `pending`, `Transaction in progress.`, then `rejected`, `Transaction timed out. Old status pending. New status declined` after 540 s. Verification: `verification`, `Transaction set to verification status`. Decline: `rejected` with the chosen code (903 `Do not honor`, 906 `. Reason: Insufficient funds`, 904 `No status details`, 900 `Card has been lost`, 901 `Card has been  stolen`, 902 `Declined due to fraud`, 912 `Declined due to suspected-fraud`). Page never submitted: `rejected`, `Transaction timed out. Old status pending_async. New status declined` after 540 s. Each result notifies 15 s later |
| CVV 333 + amount 334 | as 3DS | after Approve: `verification`, `[TEST] Transaction status: declined`; notifications `approved`, then `verification` 5 s later |
| CVV 333 + amount 335 | as 3DS | after Approve: `verification`, amount 35000, `[TEST] Transaction status: approved`; notifications `approved` (33500), then `verification` (35000) 5 s later |
| CVV 454 + amount 498 | as 3DS | after Approve: `approved`, amount 40000, `TEST amount 498, 3D partial approved with amount 400` |
| CVV 588 + amount 713 | `initialized`, code `0`, no `redirect_url` | after 540 s `rejected`, `Transaction timed out. Old status pending_async. New status declined`, notification |
| Amount 11 / 19 (CVV 568) | `approved`, amount 1000 / 2000 (processed 1100 / 1900), `[TEST CASE - amount 11] Changing final amount to 10` / `... 19] Changing final amount to 20` | - |
| Amount 12 / 13 (CVV 568) | `approved`, `[TEST CASE - amount 12] Changing final currency` / `[TEST CASE - amount 13] Changing final currency and amount` (amount 2600); despite the text the currency stays the requested one, as the sandbox shows | - |
| Amount 14 / 46 (CVV 568) | `approved`, amount x100 / x2 | - |
| Amount 339 (CVV 568) | `approved` | - |
| Amount 20.20 + CVV 801 | `approved` | - |
| Amount 30 + CVV 801 | `pending` | `approved` on the next find-transaction, `[TEST CASE - amount 30.00] transaction status changed from pending to approved` |
| Payout amount 14 / 46 | `approved`, x100 / x2 | - |
| Payout amount 47 / 48 | `rejected` / `error` | - |
| Payout amount 332 / 333 | `pending`, `[TEST] Transaction status: pending. Reason: Awaiting authorization` | stays pending (as the sandbox does) |
| Payout other | `approved` | - |
| Refund amount 47 / 48 | `rejected` / `error` | - |
| Refund amount 332 / 333 | `pending`, `Awaiting authorization` | `rejected` / `approved` on the next find-transaction, `[TEST CASE - amount 332] transaction status changed from pending to rejected` (333: `... to approved`) |
| Refund other | `approved` | - |

Not emulated: amounts 0.011, 0.012 and 0.013 (the API takes integer minor units) and `transaction_type: authorization` (rejected, as the sandbox gateway does).

#### 7.5.6 Emulator-Defined Behaviour
These points have no Praxis source; the emulator fixes them:
- **Trigger precedence:** CVV + amount rows, then amount rows (which apply with CVV 568), then CVV rows, then the default decline. A sale sent with `card_token` has no CVV and takes the default decline.
- **Notifications:** 15 s after a state change that notifies (approved, declined and partial sales, every 3DS result and timeout; the 334/335 pairs 5 s apart). Payouts, refunds, captures, amount cases and status-check follow-ups send none. A notification's `timestamp` is its send time.
- **Timeouts:** 540 s for an abandoned 3DS page, for 3DS Pending and for 588 + 713. A deadline is due when `now >= dueAt`; it is applied before every read or action on the transaction and by a background service every second, whichever saves first (optimistic versioning), so it is applied once.
- **Ids and tokens:** tids are unique across environments, start at 1000000 and stay within int32; `customer_token` and `card_token` are derived from the merchant and the customer `cid` / card number; card fields are masked (`411111******1111`), typed by the first digit (`VISA`, `MASTERCARD`, `AMEX`) and issued by `EMULATOR TEST BANK`.
- **Session status** follows the sandbox: `successful`, `failed`, `open` (pending, initialized, partial, 3DS verification) or `action required` (authorized, captured, cancelled).

---

## 8. Data Models

### 8.1 DynamicEntity
```typescript
{
  id: string;                          // MongoDB ObjectId
  entityType: string;                  // Schema name (e.g., "test-agent")
  environment: string | null;          // Environment name or null
  fields: Record<string, any>;         // Dynamic fields based on schema
  isConsumed: boolean;                 // Consumed status
  createdAt: string;                   // ISO 8601 datetime
  updatedAt: string;                   // ISO 8601 datetime
}
```

### 8.2 EntitySchema
```typescript
{
  id: string;
  entityName: string;                  // Unique entity type name
  fields: FieldDefinition[];           // Field definitions
  filterableFields: string[];          // Fields available for filtering
  uniqueFields: string[];              // Compound unique constraint fields
  useCompoundUnique: boolean;          // Treat uniqueFields as compound key
  excludeOnFetch: boolean;             // Auto-consume on fetch
  createdAt: string;
  updatedAt: string;
}
```

### 8.3 FieldDefinition
```typescript
{
  name: string;                        // Field name
  type: "string" | "number" | "boolean" | "date" | "array" | "object";
  required: boolean;                   // Field is required
  isUnique: boolean;                   // Field must be unique
  description: string | null;          // Optional description
}
```

### 7.4 Environment
```typescript
{
  id: string;
  name: string;                        // Unique name (lowercase, alphanumeric)
  displayName: string;                 // Display name
  description: string;                 // Description
  url: string | null;                  // Environment URL
  color: string;                       // Hex color code
  order: number;                       // Display order
  isActive: boolean;                   // Active status
  configuration: Record<string, string>; // Key-value config
  tags: string[];                      // Tags
  createdAt: string;
  updatedAt: string;
  entityCounts?: Record<string, number>; // Optional: entity counts per type
}
```

### 7.5 LoginRequest
```typescript
{
  username: string;
  password: string;
}
```

### 7.6 LoginResponse
```typescript
{
  token: string;                       // JWT token
  user: {
    id: string;
    username: string;
    email: string;
    role: string;
    createdAt: string;
  };
}
```

---

## 9. Error Handling

### HTTP Status Codes:
- `200 OK` - Request succeeded
- `201 Created` - Resource created successfully
- `204 No Content` - Request succeeded (no response body)
- `400 Bad Request` - Invalid request (validation error)
- `401 Unauthorized` - Missing or invalid authentication
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource not found
- `409 Conflict` - Resource already exists or conflict
- `500 Internal Server Error` - Server error

### Error Response Format:
```json
{
  "error": "Error message describing what went wrong"
}
```

### Common Errors:

#### Validation Errors (400):
```json
{
  "error": "Entity does not match schema for type: test-agent"
}
```

#### Unique Constraint Violation (400):
```json
{
  "error": "Entity does not match schema for type: test-agent"
}
```
*Check logs for specific field that violated uniqueness*

#### Authentication Error (401):
```json
{
  "error": "Invalid username or password"
}
```

#### Not Found (404):
```json
{
  "error": "Entity type 'unknown-type' not found. Create a schema first."
}
```

---

## 10. Integration Examples

### 9.1 C# Client Example

```csharp
using System.Net.Http.Headers;
using System.Net.Http.Json;

public class TestServiceClient
{
    private readonly HttpClient _httpClient;
    private string? _token;

    public TestServiceClient(string baseUrl)
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    // Login and get token
    public async Task<bool> LoginAsync(string username, string password)
    {
        var request = new { username, password };
        var response = await _httpClient.PostAsJsonAsync("/api/auth/login", request);
        
        if (!response.IsSuccessStatusCode)
            return false;

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        _token = result?.Token;
        
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _token);
        
        return true;
    }

    // Create schema
    public async Task<EntitySchema?> CreateSchemaAsync(EntitySchema schema)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/schemas", schema);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<EntitySchema>();
    }

    // Create entity
    public async Task<DynamicEntity?> CreateEntityAsync(
        string entityType, 
        Dictionary<string, object> fields, 
        string? environment = null)
    {
        var entity = new 
        { 
            fields, 
            environment 
        };
        
        var response = await _httpClient.PostAsJsonAsync(
            $"/api/entities/{entityType}", 
            entity
        );
        
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DynamicEntity>();
    }

    // Get next available entity (for parallel tests)
    public async Task<DynamicEntity?> GetNextEntityAsync(
        string entityType, 
        string? environment = null)
    {
        var url = $"/api/entities/{entityType}/next";
        if (environment != null)
            url += $"?environment={environment}";
        
        var response = await _httpClient.GetAsync(url);
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DynamicEntity>();
    }

    // Get all entities
    public async Task<List<DynamicEntity>> GetEntitiesAsync(
        string entityType, 
        string? environment = null)
    {
        var url = $"/api/entities/{entityType}";
        if (environment != null)
            url += $"?environment={environment}";
        
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<List<DynamicEntity>>() 
            ?? new List<DynamicEntity>();
    }

    // Delete entity
    public async Task<bool> DeleteEntityAsync(string entityType, string id)
    {
        var response = await _httpClient.DeleteAsync(
            $"/api/entities/{entityType}/{id}"
        );
        return response.IsSuccessStatusCode;
    }

    // Reset all consumed entities
    public async Task<int> ResetAllEntitiesAsync(
        string entityType, 
        string? environment = null)
    {
        var url = $"/api/entities/{entityType}/reset-all";
        if (environment != null)
            url += $"?environment={environment}";
        
        var response = await _httpClient.PostAsync(url, null);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<ResetResponse>();
        return result?.ResetCount ?? 0;
    }
}

// Models
public class LoginResponse
{
    public string Token { get; set; } = "";
    public UserInfo User { get; set; } = new();
}

public class UserInfo
{
    public string Id { get; set; } = "";
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
}

public class EntitySchema
{
    public string? Id { get; set; }
    public string EntityName { get; set; } = "";
    public List<FieldDefinition> Fields { get; set; } = new();
    public List<string> FilterableFields { get; set; } = new();
    public bool ExcludeOnFetch { get; set; }
}

public class FieldDefinition
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "string";
    public bool Required { get; set; }
    public bool IsUnique { get; set; }
}

public class DynamicEntity
{
    public string? Id { get; set; }
    public string EntityType { get; set; } = "";
    public string? Environment { get; set; }
    public Dictionary<string, object> Fields { get; set; } = new();
    public bool IsConsumed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ResetResponse
{
    public string Message { get; set; } = "";
    public int ResetCount { get; set; }
}
```

### 9.2 Usage Example

```csharp
// Initialize client
var client = new TestServiceClient("http://localhost:5000");

// Login
await client.LoginAsync("admin", "Admin@123");

// Create schema
var schema = new EntitySchema
{
    EntityName = "test-user",
    Fields = new List<FieldDefinition>
    {
        new() { Name = "username", Type = "string", Required = true, IsUnique = true },
        new() { Name = "email", Type = "string", Required = true, IsUnique = true },
        new() { Name = "age", Type = "number", Required = false }
    },
    FilterableFields = new List<string> { "username", "email" },
    ExcludeOnFetch = true // Enable for parallel test execution
};

await client.CreateSchemaAsync(schema);

// Create entities
var entity1 = await client.CreateEntityAsync(
    "test-user",
    new Dictionary<string, object>
    {
        ["username"] = "user1",
        ["email"] = "user1@example.com",
        ["age"] = 25
    },
    environment: "qa"
);

// Get next available entity (parallel-safe)
var testUser = await client.GetNextEntityAsync("test-user", "qa");
if (testUser != null)
{
    // Use the entity in your test
    Console.WriteLine($"Using entity: {testUser.Fields["username"]}");
    // Entity is automatically marked as consumed
}

// Get all entities
var allUsers = await client.GetEntitiesAsync("test-user", "qa");

// Reset consumed entities after test run
var resetCount = await client.ResetAllEntitiesAsync("test-user", "qa");
Console.WriteLine($"Reset {resetCount} entities");
```

### 9.3 Parallel Test Execution Pattern

```csharp
[Test]
[Parallelizable(ParallelScope.All)]
public async Task Test_UserLogin_WithUniqueAccount()
{
    var client = new TestServiceClient("http://localhost:5000");
    await client.LoginAsync("admin", "Admin@123");
    
    // Get unique test user (thread-safe)
    var testUser = await client.GetNextEntityAsync("test-user", "qa");
    
    if (testUser == null)
    {
        Assert.Fail("No available test users");
        return;
    }
    
    // Use the test user
    var username = testUser.Fields["username"].ToString();
    var password = testUser.Fields["password"].ToString();
    
    // Run your test with unique credentials
    var loginResult = await YourApp.LoginAsync(username, password);
    Assert.That(loginResult.IsSuccess, Is.True);
    
    // Entity remains marked as consumed - won't be used by parallel tests
}

[OneTimeTearDown]
public async Task ResetTestData()
{
    var client = new TestServiceClient("http://localhost:5000");
    await client.LoginAsync("admin", "Admin@123");
    
    // Reset all consumed entities for next test run
    await client.ResetAllEntitiesAsync("test-user", "qa");
}
```

---

## 10. Best Practices

### 10.1 Schema Design
- ✅ Use descriptive entity names (e.g., "test-agent", "user-account")
- ✅ Mark fields as `required` if they must always be provided
- ✅ Mark fields as `isUnique` for fields like username, email, userId
- ✅ Add fields to `filterableFields` if you'll search by them
- ✅ Set `excludeOnFetch: true` for parallel test execution scenarios

### 10.2 Uniqueness
- Individual fields with `isUnique: true` are validated independently
- Each unique field must have a distinct value across all entities
- Uniqueness is scoped to the environment
- Use compound unique keys via `uniqueFields` array when needed

### 10.3 Parallel Testing
- Use schemas with `excludeOnFetch: true`
- Use `GET /api/entities/{type}/next` to atomically fetch entities
- Reset consumed entities with `/reset-all` after test runs
- Each test gets a unique entity without conflicts

### 10.4 Environment Strategy
- Use separate environments for different test stages
- `dev` - Development/local testing
- `qa` - QA team testing
- `staging` - Pre-production testing
- `prod` - Production (if applicable)

### 10.5 Security
- Always use HTTPS in production
- Store JWT tokens securely
- Rotate API keys regularly
- Use role-based access control

---

## 11. Troubleshooting

### Issue: "Entity does not match schema"
**Cause:** Validation failure  
**Solution:** 
- Check all required fields are provided
- Verify unique fields don't have duplicates
- Ensure field types match schema definition

### Issue: "Environment does not exist"
**Cause:** Referenced environment not found  
**Solution:**
- Create the environment first using POST /api/environments
- Verify environment name spelling
- Check environment exists in the Environments section

### Issue: "No available entities found"
**Cause:** All entities are consumed  
**Solution:**
- Call `/reset-all` endpoint to reset consumed status
- Create more entities
- Check environment filter

### Issue: 401 Unauthorized
**Cause:** Missing or invalid token  
**Solution:**
- Login again to get a fresh token
- Ensure token is included in Authorization header
- Check token hasn't expired

### Issue: 404 Entity type not found
**Cause:** Schema doesn't exist  
**Solution:**
- Create the schema first using POST /api/schemas
- Verify schema name spelling

---

## 12. Support & Contact

**Documentation Version:** 1.0.0  
**API Version:** 1.0.0  
**Last Updated:** December 16, 2025

For issues or questions:
- Check logs: `docker compose logs api`
- Review this documentation
- Verify authentication and permissions

---

## Appendix A: Complete cURL Examples

### Login
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin@123"}'
```

### Create Schema
```bash
curl -X POST http://localhost:5000/api/schemas \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "entityName": "test-agent",
    "fields": [
      {"name": "username", "type": "string", "required": true, "isUnique": true},
      {"name": "userId", "type": "number", "required": true, "isUnique": true}
    ],
    "excludeOnFetch": true
  }'
```

### Create Entity
```bash
curl -X POST http://localhost:5000/api/entities/test-agent \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "fields": {
      "username": "user123",
      "userId": 12345
    },
    "environment": "qa"
  }'
```

### Get Next Available
```bash
curl -X GET "http://localhost:5000/api/entities/test-agent/next?environment=qa" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Reset All
```bash
curl -X POST "http://localhost:5000/api/entities/test-agent/reset-all?environment=qa" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

**End of Documentation**
