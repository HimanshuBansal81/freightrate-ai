# Database Design

FreightRate AI uses PostgreSQL as the source of truth.

## Auth Service Tables

- `Users`: active users, email, full name, password hash, created date.
- `Roles`: supported roles such as `Admin` and `Shipper`.
- `UserRoles`: many-to-many relationship between users and roles.

## Quote Service Tables

- `Carriers`: carrier identity, code, service type, active flag.
- `ZoneMappings`: pincode to zone, city, and state.
- `CarrierRateRules`: carrier pricing rules by origin and destination zone.
- `QuoteRequests`: persisted quote request, selected recommendation, and explanation.
- `QuoteOptions`: individual carrier options calculated for one quote request.

## Relationships

- `CarrierRateRule` belongs to `Carrier`.
- `QuoteOption` belongs to `QuoteRequest`.

## Why QuoteRequest and QuoteOption Are Separate

A quote request represents the user input, selected recommendation, and final explanation. Quote options represent the carrier-by-carrier calculations produced for that request.

Keeping them separate allows:

- One quote request to store multiple carrier options.
- Historical audit of each carrier amount and ETA.
- Easy quote detail retrieval without recalculating old results.

## Why CarrierRateRule Is Separate

Carrier pricing changes over time and varies by lane. `CarrierRateRule` isolates rate-card data from carrier identity and quote history, making it easier to manage active rules, add lanes, and preserve historical quote outputs.

## Current Seed Data

Carriers:

- Delhivery
- BlueDart
- Xpressbees

Zones:

- `110001`: North, New Delhi, Delhi
- `122001`: North, Gurugram, Haryana
- `400001`: West, Mumbai, Maharashtra
- `560001`: South, Bengaluru, Karnataka
- `700001`: East, Kolkata, West Bengal

Rate rules:

- North to South
- South to North
