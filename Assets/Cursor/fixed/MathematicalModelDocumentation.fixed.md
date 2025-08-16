# Mathematical Model Documentation (Fixed)

## RTP System (96%)
RTP is embedded in the distribution of the crash multiplier `M`:
```
S(m) = P(M ≥ m) = (1 − ε)/m,   ε = 0.04
```
No payout fees or time exponents are used to tune RTP.

## Sampling (Provably Fair)
1. Compute HMAC = HMAC_SHA256(serverSeed, clientSeed||nonce||roundIndex).
2. Extract 52 random bits: `u52 = int(hmac[0:13], 16)` (fits in 52 bits).
3. Convert to double uniform: `U = u52 / 2^52`.
4. Crash point: `M = (1 − ε) / (1 − U)`.
5. Clamp to `[MIN=1.01, MAX=1000]`.

## Player Choice & Outcome
- Player sets a target multiplier `m` **before** the round (manual or auto cashout).
- Payout rule: `payout = bet × m` if `M ≥ m`, else `0`.
- Expected return per bet is `1 − ε = 0.96` regardless of `m`.

## Client Growth vs Outcome
- The visual growth of the multiplier is a UI animation.
- Outcome is server-computed (`M`) and independent from the animation curve.

## API Sketch
```csharp
public record RoundSeeds(string ServerSeedHash, string ClientSeed, long Nonce);
public record RoundResult(double CrashPoint, string ServerSeed, string Hmac);

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

## Validation
- Monte Carlo 1e6 rounds → RTP ≈ 96% ± 0.3 p.p.
- Bucket probabilities derived from `f(m) = (1 − ε)/m²`.
