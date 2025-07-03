# Claims History API

This API provides access to customer insurance claims history for MCP-compatible plugins and Semantic Kernel integration.

## Endpoint

### GET /customers/{customerId}/claims/history

Returns historical insurance claims for a specific customer.

**Parameters:**
- `customerId` (path, required): The unique identifier for the customer

**Response:**
- `200 OK`: Array of claim objects
- `400 Bad Request`: Invalid or missing customer ID

## Response Format

Each claim in the response array contains:

```json
{
  "claimId": "CLM-20250703-001",
  "policyNumber": "POL-123456789",
  "claimantName": "Jane Doe", 
  "claimantContact": "jane.doe@email.com",
  "dateOfAccident": "2025-07-01T14:30:00Z",
  "accidentDescription": "Rear-ended at a stoplight. Minor damage to rear bumper.",
  "vehicleMake": "Toyota",
  "vehicleModel": "Camry",
  "vehicleYear": 2022,
  "licensePlate": "ABC1234",
  "amountClaimed": 2500.00,
  "status": "UnderReview"
}
```

## Mock Data

The API returns consistent mock data based on the customer ID:
- Each customer gets 1-4 historical claims
- Claims are generated deterministically for consistency
- Data includes realistic accident descriptions, vehicle information, and claim amounts
- Statuses include: Pending, UnderReview, Approved, Denied, Settled

## API Documentation

When running in development mode, Swagger documentation is available at:
- Swagger UI: `http://localhost:5000/swagger`
- OpenAPI JSON: `http://localhost:5000/swagger/v1/swagger.json`

## Example Usage

```bash
# Get claims history for customer 12345
curl http://localhost:5000/customers/12345/claims/history

# Response
[
  {
    "claimId": "CLM-20240830-001",
    "policyNumber": "POL-602812946",
    "claimantName": "Robert Brown",
    "claimantContact": "jane.miller@yahoo.com",
    "dateOfAccident": "2024-08-30T01:38:53.7528669Z",
    "accidentDescription": "Collision at intersection. Front-end damage.",
    "vehicleMake": "Mercedes-Benz",
    "vehicleModel": "A4",
    "vehicleYear": 2015,
    "licensePlate": "SCG5757",
    "amountClaimed": 12952.86,
    "status": "UnderReview"
  }
]
```

## Running the API

```bash
cd source/AgentFunction.ApiService
dotnet run
```

The API will start on `http://localhost:5000` by default.