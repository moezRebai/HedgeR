# HedgeR — Functional Documentation
_Generated 2026-07-19._

## 1. Purpose & scope
HedgeR is described as ".Net application that prices financial products based on a microservices architecture" (`README.md:2`). In its current state, the only working end-to-end capability is generating and broadcasting synthetic live currency-pair spot prices for one internal consumer to receive. There is **no working pricing capability yet** — no fee calculation, no spread calculation, no aggregation of these into a final price, and no ingestion of real external market data. This document describes what the system *actually does today*, not the pricing platform its name and README imply it will become; the gap is called out explicitly rather than assumed away.

**Explicitly not (yet) in scope / not implemented:**
- Computing a price, fee, or spread for any instrument.
- Consuming real market/FX data from an external provider.
- Any capability in the `Aggregator`, `Fees`, or `Spread` services (each is an empty placeholder — `src/HedgeR.Aggregator/Program.cs`, `src/HedgeR.Feeds/Fees/Program.cs`, `src/HedgeR.Feeds/Spread/Program.cs`).
- Authentication/authorization on any endpoint (none found in any service).

## 2. Capability inventory

| Capability | What it does (domain terms) | Implemented in (cited) | Notes / gaps |
|---|---|---|---|
| Start the spot price feed | Begins generating a live stream of synthetic currency-pair prices at a caller-chosen update frequency, one currency pair at a time in round-robin order | `src/HedgeR.Feeds/Spot/Program.cs:19-25`, `CurrencyPairSpotProviderService.cs`, `SpotProviders/CurrencyPairSpotProvider.cs` | Starting twice while already running is a no-op (logged warning), not an error |
| Stop the spot price feed | Halts the running price generation loop | `Program.cs:27-33`, `CurrencyPairSpotProviderService.cs:35-46` | Stopping twice while already stopped is a no-op (logged warning) |
| Broadcast spot prices | Publishes each generated price tick to a shared real-time channel so other services can receive it live | `CurrencyPairSpotProviderService.cs:58-61`, `RedisStreamingPublisher.cs` | Fire-and-forget: a consumer that isn't listening at publish time never receives that tick |
| Receive spot prices (Pricer) | Listens to the broadcast channel and logs each incoming price tick | `src/HedgeR.Pricer/StreamProviders/SpotStreamProvider.cs` | Logging only — no price, fee, or spread is computed from the received tick |
| Currency-pair catalog | Holds the list of tradeable currency pairs and a starting ("default") value for each, used to seed the price generator | `src/HedgeR.Feeds/Spot/Entities/CurrencyPair.cs`, `MongoRepository.cs` | Read once at service startup; catalog changes require restarting the Spot service to take effect |
| API self-documentation | Each service that has real endpoints exposes an interactive Swagger UI describing them | `SwaggerExtensions.cs`, wired in `HedgeR.Spot` and `HedgeR.Pricer` only | `Aggregator`, `Fees`, `Spread` don't expose Swagger (they also have no endpoints beyond a greeting) |

## 3. Use cases / user journeys

### UC-1: Start streaming live spot prices
- **Actor:** Operator (any HTTP caller — no auth is enforced).
- **Trigger:** `POST /spot/start/{frequency}` against the Spot service.
- **Preconditions:** Spot service is running and has successfully loaded its currency-pair catalog from the database at startup.
- **Main flow:**
  1. Operator calls `POST /spot/start/{frequency}`, where `frequency` is the desired seconds between updates.
  2. The service accepts the request immediately (`200 OK`) and queues it internally.
  3. If a frequency of `0` or less is supplied, the system falls back to its last-used frequency (initially 1 second) rather than rejecting the request.
  4. The system begins cycling through every known currency pair, computing a new price for each and broadcasting it.
  5. Each broadcast price is a small random step (up or down, up to about ±0.05) away from that pair's previous value — a random walk, not a real market feed.
- **Alternate / error flows:** If streaming is already running, the new start request is accepted but has no additional effect (logged as "already started").
- **Outcome:** A continuous stream of synthetic price ticks is broadcast until stopped.
- **Source:** `Program.cs:19-25`, `CurrencyPairSpotProviderService.cs:48-62`, `CurrencyPairSpotProvider.cs:30-63,71-76`.

### UC-2: Stop streaming live spot prices
- **Actor:** Operator.
- **Trigger:** `POST /spot/stop`.
- **Preconditions:** none (safe to call even if not currently streaming).
- **Main flow:**
  1. Operator calls `POST /spot/stop`.
  2. The service accepts the request immediately (`200 OK`) and queues it internally.
  3. The running generation loop observes the stop signal and exits after finishing its current step.
- **Alternate / error flows:** If streaming is already stopped, the request is accepted but has no additional effect (logged as "already stopped").
- **Outcome:** No further price ticks are broadcast until a new start request is made.
- **Source:** `Program.cs:27-33`, `CurrencyPairSpotProviderService.cs:35-46`.

### UC-3: Receive spot price updates (system-to-system)
- **Actor:** The Pricer service (not a human actor).
- **Trigger:** Any price tick broadcast per UC-1.
- **Preconditions:** Pricer service is running and has an active connection to the broadcast channel.
- **Main flow:**
  1. Pricer receives a broadcast price tick (currency pair, value, timestamp).
  2. Pricer logs that it received the tick.
- **Alternate / error flows:** If Pricer is offline, restarting, or its connection has dropped when a tick is broadcast, that tick is lost — there is no queueing or replay.
- **Outcome:** The tick is recorded in the Pricer service's logs. No price, fee, or spread is calculated as a result.
- **Source:** `SpotStreamProvider.cs:16-22`.

## 4. Business rules & invariants

- **Rule:** Requesting a start frequency of 0 or a negative number does not fail the request; the system silently keeps using whatever frequency it last had (1 second on first run).
  **Source:** `CurrencyPairSpotProvider.cs:34` — confidence: High.
- **Rule:** Each price update moves at most ~0.05 away from the previous value for that currency pair, in a random direction — prices are a bounded random walk, not sourced from a real market.
  **Source:** `CurrencyPairSpotProvider.cs:71-76` — confidence: High.
- **Rule:** Starting the feed while it is already running has no effect beyond a warning log — it is not rejected and does not restart the loop.
  **Source:** `CurrencyPairSpotProviderService.cs:52-56` — confidence: High.
- **Rule:** Stopping the feed while it is already stopped has no effect beyond a warning log.
  **Source:** `CurrencyPairSpotProviderService.cs:39-43` — confidence: High.
- **Rule:** The currency-pair catalog (which pairs exist, and their starting price) is loaded once when the Spot service starts; changes made in the database afterward do not take effect until the service is restarted.
  **Source:** `CurrencyPairSpotProvider.cs:14-28` — confidence: High.
- **Rule:** A broadcast price tick that is published while no consumer is actively subscribed is lost — there is no queue, buffer, or replay of missed ticks.
  **Source:** `RedisStreamingPublisher.cs`/`RedisStreamingSubscriber.cs` (Redis pub/sub semantics as used — no persistence layer involved) — confidence: High.

## 5. Domain glossary

| Term | Definition |
|---|---|
| **Currency pair** | A tradeable instrument identified by name (e.g. an FX pair), with a configured starting/default value. Stored in the catalog. |
| **Spot (spot price)** | The current synthetic price value for a currency pair at a point in time; the payload broadcast on each tick. |
| **Frequency** | The number of seconds between successive price updates for the spot feed, chosen by the caller when starting the feed. |
| **Spot feed / feeder** | The running process (in `HedgeR.Spot`) that cycles through all currency pairs and broadcasts a new price for each on a timer. |
| **Aggregator** | A service intended (by name/README) to combine pricing inputs; currently an empty placeholder with no logic. |
| **Fees** | A service intended (by name) to represent fee calculation; currently an empty placeholder with no logic. |
| **Spread** | A service intended (by name) to represent bid/ask spread calculation; currently an empty placeholder with no logic. |
| **Pricer** | The service intended (by name) to compute prices from inputs like the spot feed; currently only logs received spot ticks and performs no calculation. |
| **SpotFeed (channel)** | The name of the real-time broadcast channel that carries spot price ticks from Spot to any subscriber (currently only Pricer subscribes). |

## 6. Data & integrations (functional view)

- **Stores:** A catalog of currency pairs (name + starting price) is the only persisted business data in the system today, held in a database owned by the Spot service.
- **Produces:** A live stream of synthetic spot price ticks (currency pair, value, timestamp), broadcast whenever the feed is running.
- **Consumes:** Nothing from outside the system — the "spot price" is generated internally rather than sourced from a real market-data provider (see open questions).
- **Exchanges with:** Only internal services exchange data today (Spot → Pricer, over the broadcast channel). No external parties are integrated.

## 7. Assumptions & open questions

- It is unverified whether `Aggregator`, `Fees`, and `Spread` are meant to each publish their own broadcast channel (mirroring Spot) that would feed into `Pricer`, or whether they'll take some other shape — no code exists yet to confirm either way. **Low confidence.**
- It is unverified whether real external market data will eventually replace the synthetic random-walk generator, or whether the random walk is intended as the permanent behavior (e.g., for a demo/paper-trading system). **Low confidence.**
- The deployment config for `Spread` declares a startup dependency on `Aggregator`, hinting at an intended (but not yet built) interaction between those two services. **Medium confidence** this reflects real planned behavior rather than incidental ordering.
- No access-control model exists on any endpoint — every capability described above is callable by any network caller that can reach the service. Whether this is acceptable depends on the deployment environment, which is out of scope for this document.
- No automated tests exist in the repository, so the business rules above are derived entirely from reading the implementation, not from an executable specification. Treat rule details (exact tick bound, default frequency) as accurate to the code as read, but subject to change without a test suite to guard them.
