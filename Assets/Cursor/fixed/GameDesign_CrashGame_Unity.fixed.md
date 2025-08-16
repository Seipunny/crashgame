# Game Design — Crash (Unity) (Fixed)

## Server-Authoritative Round Flow
1) **Commit**: Server announces round start, provides `serverSeedHash`, accepts bets & targets.
2) **Run**: Server samples `U`, computes `M = (1 − ε)/(1 − U)`, drives the UI animation.
3) **Cashouts**: Auto/Manual cashout requests are judged by **server time** vs the implied crash moment from `M`.
4) **Reveal**: Server publishes seeds/HMAC so clients recompute `M` and verify.
5) **Settle**: Payouts: `bet × target` if `M ≥ target`, else `0`.

## RNG / Provably Fair
- Extract 52 random bits from HMAC; `U = u52 / 2^52`.
- `M = (1 − ε)/(1 − U)`; clamp to `[1.01, 1000]`.

## Client Animation
- The growth curve and any acceleration/easing are **visual only** and must not change outcome.
- Keep the animation continuous and deterministic from the announced `M`.

## APIs
```csharp
public interface ICrashRngProvider {
    double SampleCrash(double epsilon, ulong u52);
}
public interface IRoundService {
    Task<RoundSeeds> StartRoundAsync();
    Task SubmitBetAsync(decimal amount, double target);
    Task<bool> TryManualCashoutAsync(Guid betId, double target);
    Task<RoundResult> RevealAsync();
}
```
