# Game Rules — Crash (Fixed)

## Bets and Targets
- Minimum target: `m ≥ 1.01`. Player must choose target **before** a round starts (or enable auto-cashout).
- Bet is accepted when the server acknowledges it for the current round.

## Outcome & Payout
- Server draws the crash multiplier M using `S(m) = (1 − ε)/m` with ε=0.04.
- If `M ≥ target` → `payout = bet × target`, else `0`.
- No extra fees; RTP per bet equals `1 − ε = 96%` by construction.

## Manual Cashout
- Manual cashout is a **request**. It succeeds only if the **server** receives it before the crash moment derived from `M`.
- Latency rules and timeouts are defined by server time; client clocks are not authoritative.

## Provably Fair
- Round seeds are committed at start (hash). The reveal provides the full seed to recompute `M` on the client.
