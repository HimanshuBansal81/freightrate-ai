# Quote Contracts

Base local route through Nginx: `/quotes`.

## Public Reference Endpoints

`GET /quotes/api/carriers`

Returns active carriers.

`GET /quotes/api/zones`

Returns active pincode-zone mappings.

## Admin Rate Rules

`GET /quotes/api/admin/rate-rules`

Requires `Admin` role.

## Compare Quotes

`POST /quotes/api/quotes/compare`

Requires `Admin` or `Shipper` role.

Request:

```json
{
  "originPincode": "110001",
  "destinationPincode": "560001",
  "actualWeightKg": 8,
  "lengthCm": 40,
  "widthCm": 30,
  "heightCm": 25,
  "preference": "Balanced"
}
```

Allowed preferences:

- `Cheapest`
- `Fastest`
- `Balanced`

Response:

```json
{
  "quoteId": 1,
  "originZone": "North",
  "destinationZone": "South",
  "actualWeightKg": 8,
  "volumetricWeightKg": 6,
  "chargeableWeightKg": 8,
  "preference": "Balanced",
  "recommendedCarrier": "Xpressbees",
  "recommendedAmount": 166.14,
  "aiExplanation": "Xpressbees is recommended because it best matches your Balanced preference based on the calculated price and delivery time.",
  "options": [
    {
      "carrier": "Xpressbees",
      "serviceType": "Surface",
      "baseFreight": 128.0,
      "fuelSurcharge": 12.8,
      "gst": 25.34,
      "totalAmount": 166.14,
      "estimatedDeliveryDays": 5
    }
  ]
}
```

## Quote History

`GET /quotes/api/quotes/history`

Requires `Admin` or `Shipper`.

- Shipper sees only their quotes.
- Admin sees all quotes.

## Quote By ID

`GET /quotes/api/quotes/{id}`

Requires `Admin` or `Shipper`.

- Shipper can fetch only their own quote.
- Admin can fetch any quote.
