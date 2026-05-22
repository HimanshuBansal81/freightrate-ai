# Auth Contracts

Base local route through Nginx: `/auth`.

## Register

`POST /auth/api/auth/register`

Request:

```json
{
  "fullName": "Demo Shipper",
  "email": "demo.shipper@example.com",
  "password": "Passw0rd!",
  "role": "Shipper"
}
```

Roles:

- `Admin`
- `Shipper`

Response:

```json
{
  "token": "jwt-token",
  "email": "demo.shipper@example.com",
  "fullName": "Demo Shipper",
  "roles": ["Shipper"],
  "expiresAt": "2026-05-22T10:54:34Z"
}
```

## Login

`POST /auth/api/auth/login`

Request:

```json
{
  "email": "demo.shipper@example.com",
  "password": "Passw0rd!"
}
```

Response shape matches register.

## Current User

`GET /auth/api/auth/me`

Headers:

```text
Authorization: Bearer <token>
```

Response:

```json
{
  "userId": 1,
  "email": "demo.shipper@example.com",
  "fullName": "Demo Shipper",
  "roles": ["Shipper"]
}
```

## JWT Claims

Auth Service emits:

- `sub`: user id
- `ClaimTypes.NameIdentifier`: user id
- `email` and `ClaimTypes.Email`
- `ClaimTypes.Name`
- `full_name`
- `ClaimTypes.Role`
