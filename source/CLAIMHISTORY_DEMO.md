# ClaimHistoryService Implementation Demo

This demonstrates the ClaimHistoryService implementation that was created to fulfill issue #10.

## API Endpoint Usage

The new service provides a REST API endpoint:

```
GET /api/policy/{policyId}/claims
```

### Example Requests:

1. **Get claims for policy POL-123456789**:
   ```
   GET /api/policy/POL-123456789/claims
   ```
   Returns 2 claims for Toyota Camry

2. **Case-insensitive matching**:
   ```
   GET /api/policy/pol-555555555/claims
   ```
   Returns 3 claims for Ford F-150 (note lowercase policy ID)

3. **Single claim policy**:
   ```
   GET /api/policy/POL-333444555/claims
   ```
   Returns 1 claim for Chevrolet Malibu

4. **Non-existent policy**:
   ```
   GET /api/policy/POL-999999999/claims
   ```
   Returns empty array `[]`

## Sample Response Format

```json
[
  {
    "claimId": "CLM-20240101-001",
    "policyNumber": "POL-123456789",
    "dateOfAccident": "2024-01-15T14:30:00Z",
    "accidentDescription": "Rear-ended at a stoplight. Minor damage to rear bumper.",
    "vehicle": "Toyota Camry",
    "vehicleYear": 2022,
    "amountClaimed": 2500.00,
    "status": "Approved"
  }
]
```

## Available Test Policies

The service includes hardcoded data for these policies:
- **POL-123456789**: 2 claims (Toyota Camry)
- **POL-987654321**: 1 claim (Honda Civic)  
- **POL-555555555**: 3 claims (Ford F-150)
- **POL-333444555**: 1 claim (Chevrolet Malibu)

## Service Features

✅ **Case-insensitive policy matching**
✅ **Never returns null - always returns collection (empty if no matches)**
✅ **Claims ordered by accident date (newest first)**
✅ **Comprehensive input validation**
✅ **Full unit test coverage**
✅ **OpenAPI documentation support**

## Dependencies

- Registered in DI container as `IClaimHistoryService`
- Uses `ClaimDto` model for simplified claim representation
- Follows existing service patterns in the codebase