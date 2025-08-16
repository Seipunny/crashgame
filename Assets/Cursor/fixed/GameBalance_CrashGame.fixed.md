# Game Balance — Crash (Fixed)

## Target RTP and Edge
- RTP = 96% (ε=0.04). Achieved via crash distribution, not payout commissions.

## Distribution
`f(m) = (1 − ε)/m²`, `S(m) = (1 − ε)/m` for `m ≥ 1`.

### Bucket Probabilities (ε=0.04, MIN=1.01, MAX=1000)
- 1.01–2.00: ≈ 47.05%
- 2.01–5.00: ≈ 28.56%
- 5.01–10.00: ≈ 9.56%
- 10.01–20.00: ≈ 4.79%
- 20.01–50.00: ≈ 2.88%
- 50.01–100.00: ≈ 0.96%
- 100.01+: ≈ 0.96%

Use the exact formulas to regenerate any bucket scheme:
```
P(a ≤ M ≤ b) = (1 − ε)·(1/a − 1/b)
P(M ≥ a)     = (1 − ε)/a
```

## Winrate vs Targets
For a single target `m`: `Winrate(m) = (1 − ε)/m`.
Aggregate winrate with a discrete target mix `{(w_i, m_i)}`:
```
Winrate_total = Σ_i w_i · (1 − ε)/m_i
EV_per_bet    = Σ_i w_i · m_i · (1 − ε)/m_i = 1 − ε = 0.96
```

## Sanity Checks
- Monte Carlo over 1e6 rounds with the projected player mix should yield RTP ≈ 96%.
- Max crash cap affects tail metrics only (hit rates of large multipliers), not RTP.
