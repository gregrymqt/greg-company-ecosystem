# Hubs Feature

SignalR real-time communication hubs and the shared connection mapping infrastructure used across all hubs.

---

## Folder Structure

```
Features/Hubs/
├── ConnectionMapping.cs        # Generic bidirectional key ↔ connectionId registry
├── PaymentProcessingHub.cs     # Hub for payment status notifications (auth required)
├── RefundProcessingHub.cs      # Hub for refund status notifications (auth required)
├── VideoProcessingHub.cs       # Hub for video processing progress (group-based)
└── README.md
```

---

## `ConnectionMapping<T>`

Thread-safe generic service that maps a key (e.g. `userId` or `storageIdentifier`) to one or more SignalR connection IDs, and vice versa.

| Method | Description |
|---|---|
| `Add(key, connectionId)` | Registers a connection under a key |
| `GetKey(connectionId)` | Resolves the key associated with a connection (used in `OnDisconnectedAsync`) |
| `GetConnections(key)` | Returns all active connection IDs for a given key (used to send targeted messages) |
| `Remove(connectionId)` | Removes a connection from both mappings; cleans up the key entry if no connections remain |

Registered as a singleton in DI so all hubs share the same instance.

---

## Hubs

### `PaymentProcessingHub`

- Requires authentication (`[Authorize]`)
- On connect: maps `userId` (from JWT claims) → `connectionId`
- On disconnect: removes the connection via `ConnectionMapping`
- Used by payment jobs to push status updates to the specific user

### `RefundProcessingHub`

- Same pattern as `PaymentProcessingHub`
- Requires authentication (`[Authorize]`)
- Used by refund jobs to notify the requesting user

### `VideoProcessingHub`

- No authentication required
- Client calls `SubscribeToJobProgress(storageIdentifier)` to join a SignalR group (`processing-{storageIdentifier}`) and register in `ConnectionMapping`
- On disconnect: resolves the `storageIdentifier` from the mapping, leaves the group, and removes the connection
- Used by video processing jobs to broadcast progress to all subscribers of a specific job

---

## Client Events

| Hub | Direction | Event | Payload |
|---|---|---|---|
| `PaymentProcessingHub` | Server → Client | `PaymentStatusUpdated` | Payment status object |
| `RefundProcessingHub` | Server → Client | `RefundStatusUpdated` | Refund status object |
| `VideoProcessingHub` | Server → Client | `ProgressUpdated` | Progress percentage / status |

---

## Sending Messages from Services

Use `IHubContext<THub>` to push from background jobs:

```csharp
// Target a specific user
var connections = _mapping.GetConnections(userId);
await _hubContext.Clients.Clients(connections).SendAsync("PaymentStatusUpdated", payload);

// Target a video processing group
await _hubContext.Clients.Group($"processing-{storageIdentifier}").SendAsync("ProgressUpdated", payload);
```
