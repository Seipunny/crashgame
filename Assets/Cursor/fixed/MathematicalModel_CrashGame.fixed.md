# Mathematical Model — Crash Game (Fixed for RTP 96%)

## Core Principle
Target RTP is achieved **only** via the crash distribution. No payout commissions or time-based exponentials participate in the math of the outcome.

Let ε be the house edge. For RTP 96% we set **ε = 0.04**.

Define the survival function of the crash multiplier **M**:
```
S(m) = P(M ≥ m) = (1 − ε) / m,     m ≥ 1
```
Hence the pdf is `f(m) = (1 − ε) / m²` for `m ≥ 1`.

### Sampling (Inverse Transform)
Let `U ~ Uniform(0,1)`. Then
```
M = (1 − ε) / (1 − U)
```
Apply caps for design reasons: `MIN = 1.01`, `MAX = 1000` (configurable).

### Expected Value Proof (per-bet RTP)
For any player target `m ≥ 1.01` chosen **before** the round:
```
E[payout / stake | target m] = m · P(M ≥ m) = m · (1 − ε)/m = 1 − ε = 0.96
```
Therefore the expected return does not depend on the player’s chosen target.

---

## Server-Authoritative Outcome
- The server samples `U` (Provably Fair) and computes `M`.
- Client-side growth animation is purely visual; it must not alter the precomputed `M`.
- Manual cashout requests are accepted if and only if the **server timestamp** of the request ≤ the server’s crash moment implied by `M`.

---

## Pseudocode
```pseudo
function SampleCrash(epsilon, minM=1.01, maxM=1000):
    U ← Uniform(0,1)
    M ← (1 - epsilon) / (1 - U)
    return clamp(M, minM, maxM)
```

### C# (Unity / server)
```csharp
public static class CrashRng
{
    public static double SampleCrash(double epsilon, double u01, double minM = 1.01, double maxM = 1000.0)
    {
        double m = (1.0 - epsilon) / (1.0 - u01);
        if (m < minM) m = minM;
        if (m > maxM) m = maxM;
        return m;
    }
}
```

---

## Buckets & Probabilities
Given ε=0.04,
```
P(M ≥ a) = (1 − ε)/a
P(a ≤ M ≤ b) = (1 − ε)·(1/a − 1/b)
```
Example bucket probabilities (MIN=1.01, MAX=1000):
- 1.01–2.00: ≈ 47.05%
- 2.01–5.00: ≈ 28.56%
- 5.01–10.00: ≈ 9.56%
- 10.01–20.00: ≈ 4.79%
- 20.01–50.00: ≈ 2.88%
- 50.01–100.00: ≈ 0.96%
- 100.01+: ≈ 0.96%

---

## QA / Validation
- **Monte Carlo**: N=1e6 rounds, ε=0.04 ⇒ RTP ≈ 96% ± 0.3 p.p.
- **Unit tests**: For m in {1.5,2,3,5,10}: `|EV − 0.96| < 0.01`.
