# Acceleration Analysis — Notes (Fixed)

## Scope
Acceleration/easing affects **UX only**. It must not influence the game math or RTP.

## Constraints
- Outcome `M` is precomputed server-side from Provably Fair RNG.
- The animation curve (including any acceleration phases) is a rendering concern.
- Control points (e.g., x2@8s, x20@35s, x100@50s) are UI targets and may be adjusted without changing RTP.

## QA
- Ensure that the revealed `M` matches the committed seeds.
- Verify that large `M` values are displayed fully before settle.
