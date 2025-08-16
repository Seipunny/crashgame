using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System;
using System.Security.Cryptography;
using System.Text;

#region Provably Fair + Server Mock (no UI spawning here)

public class RoundSeeds
{
    public string ServerSeedHash { get; private set; }
    public string ClientSeed { get; private set; }
    public long Nonce { get; private set; }
    public int RoundIndex { get; private set; }

    public RoundSeeds(string serverSeedHash, string clientSeed, long nonce, int roundIndex)
    {
        ServerSeedHash = serverSeedHash;
        ClientSeed = clientSeed;
        Nonce = nonce;
        RoundIndex = roundIndex;
    }
}

public class RoundReveal
{
    public double CrashPoint { get; private set; }
    public string ServerSeed { get; private set; }
    public string HmacHex { get; private set; }
    public ulong U52 { get; private set; }
    public double U01 { get; private set; }

    public RoundReveal(double crashPoint, string serverSeed, string hmacHex, ulong u52, double u01)
    {
        CrashPoint = crashPoint;
        ServerSeed = serverSeed;
        HmacHex = hmacHex;
        U52 = u52;
        U01 = u01;
    }
}

public interface ICrashRngProvider {
    double SampleCrash(double epsilon, ulong u52, double minM = 1.01, double maxM = 1000.0);
    ulong ExtractU52FromHmac(byte[] hmac);
    double ToUnit01(ulong u52);
}

public class CrashRngProvider : ICrashRngProvider {
    public double SampleCrash(double epsilon, ulong u52, double minM = 1.01, double maxM = 1000.0) {
        double u = ToUnit01(u52);
        double m = (1.0 - epsilon) / (1.0 - u);
        if (m < minM) m = minM;
        if (m > maxM) m = maxM;
        return m;
    }
    public ulong ExtractU52FromHmac(byte[] hmac) {
        ulong v = BitConverter.ToUInt64(hmac, 0);
        v >>= (64 - 52);
        return v;
    }
    public double ToUnit01(ulong u52) {
        const double denom = 4503599627370496.0; // 2^52
        return u52 / denom;
    }
}

public interface IRoundService {
    RoundSeeds StartRound();
    void SubmitBet(decimal amount, double target);
    bool TryManualCashout(double visualMultiplier, double requestClientTime, double rttMillis);
    RoundReveal Reveal();
    double GetCrashPoint();
    double GetCrashTime();
}

public class RoundServiceMock : IRoundService {
    private readonly ICrashRngProvider rng;
    private readonly Func<double, double> animCurve;
    private readonly double epsilon;
    private readonly double minM, maxM;

    private int roundIndex = 0;
    private long nonce = 0;
    private string serverSeed;
    private string clientSeed;
    private string serverSeedHash;
    private string lastHmacHex;
    private ulong lastU52;
    private double lastU01;
    private double crashM;
    private double crashTime;

    private decimal betAmount;
    private double betTarget;
    private bool betAccepted;

    public RoundServiceMock(ICrashRngProvider rngProvider, Func<double,double> visualCurve,
                            double epsilon=0.04, double minM=1.01, double maxM=1000.0) {
        rng = rngProvider;
        animCurve = visualCurve;
        this.epsilon = epsilon;
        this.minM = minM; this.maxM = maxM;
    }

    public RoundSeeds StartRound() {
        roundIndex++;
        nonce++;
        serverSeed = Guid.NewGuid().ToString("N");
        clientSeed = Guid.NewGuid().ToString("N");
        serverSeedHash = Sha256Hex(serverSeed);
        betAmount = 0m;
        betTarget = 0.0;
        betAccepted = false;
        lastHmacHex = "";
        lastU52 = 0;
        lastU01 = 0;
        crashM = 1.01;
        crashTime = 0;
        return new RoundSeeds(serverSeedHash, clientSeed, nonce, roundIndex);
    }

    public void SubmitBet(decimal amount, double target) {
        betAmount = amount;
        betTarget = target;
        betAccepted = true;
    }

    // Cashout accepted if the server receives the request BEFORE crash.
    // Payout multiplier equals visual multiplier at the server receive time.
    public bool TryManualCashout(double visualMultiplier, double requestClientTime, double rttMillis) {
        if (!betAccepted) return false;
        double receiveTime = requestClientTime + (rttMillis * 0.5) / 1000.0;
        return receiveTime <= crashTime;
    }

    public RoundReveal Reveal() {
        string message = string.Format("{0}|{1}|{2}", clientSeed, nonce, roundIndex);
        byte[] hmacBytes = HmacSha256(serverSeed, message);
        lastHmacHex = ToHex(hmacBytes);
        lastU52 = rng.ExtractU52FromHmac(hmacBytes);
        lastU01 = rng.ToUnit01(lastU52);
        crashM = rng.SampleCrash(epsilon, lastU52, minM, maxM);
        crashTime = SolveCrashTime(crashM);
        return new RoundReveal(crashM, serverSeed, lastHmacHex, lastU52, lastU01);
    }

    public double GetCrashPoint() { return crashM; }
    public double GetCrashTime() { return crashTime; }

    private double SolveCrashTime(double M) {
        double t = 0.0;
        const double dt = 0.01;
        const double tMax = 120.0;
        while (t < tMax) {
            double m = animCurve(t);
            if (m >= M) return t;
            t += dt;
        }
        return tMax;
    }

    private static string Sha256Hex(string s) {
        using (var sha = SHA256.Create())
        {
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(s));
            return ToHex(bytes);
        }
    }
    private static byte[] HmacSha256(string key, string msg) {
        using (var h = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
        {
            return h.ComputeHash(Encoding.UTF8.GetBytes(msg));
        }
    }
    private static string ToHex(byte[] bytes) {
        var sb = new StringBuilder(bytes.Length * 2);
        for (int i = 0; i < bytes.Length; i++)
            sb.Append(bytes[i].ToString("x2"));
        return sb.ToString();
    }
}

#endregion

/// <summary>
/// Assumes ALL UI is already placed in the scene and wired in the Inspector.
/// Does NOT create or destroy any UI or helpers.
/// </summary>
public class CryptCrashGamePrototype : MonoBehaviour
{
    [Header("Economy/Math")]
    [Range(0.90f, 0.99f)] public float targetRTP = 0.96f; // ε = 1 - RTP
    public float minCrash = 1.01f;
    public float maxCrash = 1000f;

    [Header("Bet Settings")]
    public float playerBalance = 1000f;
    public float currentBet = 10f;
    public float minBet = 10f;
    public float maxBet = 100f;
    public float betStep = 10f;
    public float targetMultiplier = 2.00f; // ≥ 1.01

    [Header("Phases")]
    public float bettingPhaseDuration = 10f;
    public float interRoundDelay = 4f;

    [Header("Latency Simulation (ms RTT)")]
    public float simulatedRttMs = 80f;

    [Header("Game State (runtime)")]
    public bool isBettingPhase;
    public bool isGameRunning;
    public int currentRound = 0;
    public float currentMultiplier = 1f;
    public float gameTime = 0f;
    public double crashPoint = 1.01;
    private float bettingPhaseTime = 0f;
    private bool betPlacedLocal = false;
    private bool settledThisRound = false;
    private decimal lastPayout = 0m;

    // Provably fair / reveal
    private RoundSeeds lastSeeds;
    private RoundReveal lastReveal;

    // Server mock + RNG
    private IRoundService server;
    private ICrashRngProvider rng;

    [Header("UI (assign in Inspector)")]
    public TextMeshProUGUI multiplierText;
    public TextMeshProUGUI balanceText;
    public TextMeshProUGUI betText;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI historyText;
    public TextMeshProUGUI finalWinText;
    public TextMeshProUGUI roundText;
    public Slider bettingProgressBar;
    public TMP_InputField betInput;
    public TMP_InputField targetInput;
    public TMP_InputField latencyInput;
    public TextMeshProUGUI commitHashText;
    public TextMeshProUGUI revealText;

    [Header("Buttons (assign in Inspector)")]
    public Button placeBetButton;
    public Button cancelBetButton;
    public Button manualCashoutButton;
    public Button increaseBetButton;
    public Button decreaseBetButton;

    [Header("UI Colors")]
    public Color normalColor = Color.white;
    public Color winColor = Color.green;
    public Color loseColor = Color.red;
    public Color warnColor = new Color(1f, 0.85f, 0.2f);

    [Header("History")]
    public string[] gameHistory = new string[7];
    private int historyCount = 0;

    private Func<double,double> animCurveFunc;

    private double AnimCurve(double t) { return (double)MultiplierAtTime((float)t); }

    private void OnValidate()
    {
        if (minBet < 10f) minBet = 10f;
        if (maxBet < minBet) maxBet = minBet;
        if (betStep != 10f) betStep = 10f;
        currentBet = Mathf.Clamp(currentBet, minBet, maxBet);
    }

    private void OnEnable() { SetupUI(); }

    private void Start()
    {
        rng = new CrashRngProvider();
        animCurveFunc = AnimCurve;
        server = new RoundServiceMock(rng, animCurveFunc, 1f - targetRTP, minCrash, maxCrash);

        // Enforce required betting config at runtime
        minBet = 10f; maxBet = 100f; betStep = 10f;
        currentBet = Mathf.Clamp(currentBet < minBet ? minBet : currentBet, minBet, maxBet);

        targetMultiplier = Mathf.Max(1.01f, targetMultiplier);

        StartNewRound();
    }

    private void Update()
    {
        if (!isGameRunning) return;

        gameTime += Time.deltaTime;
        currentMultiplier = MultiplierAtTime(gameTime);
        if (currentMultiplier >= (float)crashPoint) {
            HandleCrash();
        }
        UpdateUI();
    }

    private void StartNewRound()
    {
        lastSeeds = server.StartRound();
        crashPoint = 1.01;
        settledThisRound = false;
        lastPayout = 0m;

        isBettingPhase = true;
        isGameRunning = false;
        gameTime = 0f;
        currentMultiplier = 1f;
        bettingPhaseTime = 0f;
        betPlacedLocal = false;

        lastReveal = server.Reveal(); // precomputed outcome (provably fair)
        crashPoint = lastReveal.CrashPoint;

        UpdateUI();
        StartCoroutine(BettingPhase());
    }

    private IEnumerator BettingPhase()
    {
        while (bettingPhaseTime < bettingPhaseDuration)
        {
            bettingPhaseTime += Time.deltaTime;
            UpdateUI();
            yield return null;
        }
        isBettingPhase = false;
        StartRun();
    }

    private void StartRun()
    {
        isGameRunning = true;
        gameTime = 0f;
        currentMultiplier = 1f;
        UpdateUI();
    }

    private void HandleCrash()
    {
        if (!isGameRunning) return;
        isGameRunning = false;

        if (betPlacedLocal && !settledThisRound)
        {
            AddToHistory(string.Format("CRASH x{0:F2} (−{1:F2})", currentMultiplier, currentBet));
            if (statusText) { statusText.text = string.Format(" Краш x{0:F2}. Проигрыш", currentMultiplier); statusText.color = loseColor; }
        }
        else if (settledThisRound)
        {
            AddToHistory(string.Format("CRASH x{0:F2} (победа уже зафиксирована)", currentMultiplier));
        }
        else
        {
            AddToHistory(string.Format("CRASH x{0:F2} (наблюдение)", currentMultiplier));
        }

        UpdateRevealPanel();
        StartCoroutine(EndRoundDelay());
    }

    private IEnumerator EndRoundDelay()
    {
        yield return new WaitForSeconds(interRoundDelay);
        currentRound++;
        StartNewRound();
    }

    public void SetupUI()
    {
        if (placeBetButton != null) { placeBetButton.onClick.RemoveAllListeners(); placeBetButton.onClick.AddListener(PlaceBet); }
        if (cancelBetButton != null) { cancelBetButton.onClick.RemoveAllListeners(); cancelBetButton.onClick.AddListener(CancelBet); }
        if (manualCashoutButton != null) { manualCashoutButton.onClick.RemoveAllListeners(); manualCashoutButton.onClick.AddListener(ManualCashout); }
        if (increaseBetButton != null) { increaseBetButton.onClick.RemoveAllListeners(); increaseBetButton.onClick.AddListener(() => AdjustBet(+betStep)); }
        if (decreaseBetButton != null) { decreaseBetButton.onClick.RemoveAllListeners(); decreaseBetButton.onClick.AddListener(() => AdjustBet(-betStep)); }

        if (betInput != null)
        {
            betInput.onEndEdit.RemoveAllListeners();
            betInput.onEndEdit.AddListener(s => {
                float v;
                if (float.TryParse(s.Replace(',', '.'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out v))
                    currentBet = Mathf.Clamp(v, minBet, maxBet);
                UpdateUI();
            });
            betInput.text = currentBet.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
        }
        if (targetInput != null)
        {
            targetInput.onEndEdit.RemoveAllListeners();
            targetInput.onEndEdit.AddListener(s => {
                float v;
                if (float.TryParse(s.Replace(',', '.'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out v))
                    targetMultiplier = Mathf.Max(1.01f, v);
                UpdateUI();
            });
            targetInput.text = targetMultiplier.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
        }
        if (latencyInput != null)
        {
            latencyInput.onEndEdit.RemoveAllListeners();
            latencyInput.onEndEdit.AddListener(s => {
                float v;
                if (float.TryParse(s, out v)) simulatedRttMs = Mathf.Max(0f, v);
                UpdateUI();
            });
            latencyInput.text = simulatedRttMs.ToString("0");
        }
        UpdateUI();
    }

    // If you prefer wiring via Inspector, use these methods instead of passing numbers:
    public void IncreaseBet() { AdjustBet(+betStep); }
    public void DecreaseBet() { AdjustBet(-betStep); }

    private void PlaceBet()
    {
        if (!isBettingPhase || betPlacedLocal) return;
        if (playerBalance < currentBet) { FlashStatus("Недостаточно средств", warnColor); return; }
        if (targetMultiplier < 1.01f) { FlashStatus("Цель должна быть ≥ 1.01", warnColor); return; }

        playerBalance -= currentBet;
        betPlacedLocal = true;
        server.SubmitBet((decimal)currentBet, targetMultiplier);
        FlashStatus(string.Format(" Ставка {0:0.##} ", currentBet, targetMultiplier), winColor);
        UpdateUI();
    }

    private void CancelBet()
    {
        if (!isBettingPhase || !betPlacedLocal) return;
        playerBalance += currentBet;
        betPlacedLocal = false;
        FlashStatus("Ставка отменена", normalColor);
        UpdateUI();
    }

    private void ManualCashout()
    {
        if (!isGameRunning || settledThisRound || !betPlacedLocal) return;

        double requestTime = gameTime;
        bool accepted = server.TryManualCashout(currentMultiplier, requestTime, simulatedRttMs);

        if (accepted)
        {
            // Server receives at t + RTT/2; payout = multiplier at server receive time.
            double receiveTime = requestTime + (simulatedRttMs * 0.5) / 1000.0;
            float payoutM = Mathf.Max(1.01f, MultiplierAtTime((float)receiveTime));
            decimal payout = (decimal)payoutM * (decimal)currentBet;
            playerBalance += (float)payout;
            lastPayout = payout;
            settledThisRound = true;
            FlashStatus(string.Format(" Выплата +{0:0.##} (x{1:0.00})", payout, payoutM), winColor);
            AddToHistory(string.Format("WIN x{0:0.00} (+{1:0.##})", payoutM, payout));
        }
        else
        {
            FlashStatus(" Кешаут отклонён (поздно)", warnColor);
        }
        UpdateUI();
    }

    private void AdjustBet(float delta)
    {
        if (isGameRunning || betPlacedLocal) return;
        currentBet = Mathf.Clamp(currentBet + delta, minBet, maxBet);
        if (betInput) betInput.text = currentBet.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
        UpdateUI();
    }

    private float MultiplierAtTime(float t)
    {
        if (t <= 0f) return 1f;
        if (t <= 8f)       return Mathf.Min(1f + t / 8f, maxCrash);
        else if (t <= 35f) { float nt = (t - 8f) / 27f; float g = nt * nt * 0.8f + 0.2f * nt; return Mathf.Min(2f + 18f * g, maxCrash); }
        else if (t <= 50f) { float nt = (t - 35f) / 15f; float g = nt * nt * nt; return Mathf.Min(20f + 80f * g, maxCrash); }
        else               { float ex = t - 50f; float g = 1f - Mathf.Exp(-ex / 30f); return Mathf.Min(100f + 900f * g, maxCrash); }
    }

    public void UpdateUI()
    {
        if (multiplierText) multiplierText.text = string.Format("x{0:F2}", currentMultiplier);
        if (balanceText) balanceText.text = string.Format(" Баланс: {0:0.##}", playerBalance);
        if (roundText) roundText.text = string.Format(" Раунд {0}", currentRound + 1);
        if (betText)
        {
            if (betPlacedLocal && isGameRunning && !settledThisRound)
                betText.text = string.Format("Ставка: {0:0.##} | Текущ.: x{2:F2}", currentBet, targetMultiplier, currentMultiplier);
            else if (settledThisRound)
                betText.text = string.Format("Выплата: +{0:0.##}", lastPayout);
            else if (betPlacedLocal)
                betText.text = string.Format("Ставка: {0:0.##} ", currentBet, targetMultiplier);
            else
                betText.text = string.Format("Ставка: {0:0.##} ", currentBet, targetMultiplier);
        }

        if (statusText)
        {
            if (isBettingPhase)
            {
                float left = Mathf.Max(0f, bettingPhaseDuration - bettingPhaseTime);
                statusText.text = betPlacedLocal ? string.Format(" Ставка принята • Старт через {0:F1}с", left) : string.Format(" Фаза ставок • Осталось {0:F1}с", left);
                statusText.color = betPlacedLocal ? winColor : normalColor;
            }
            else if (isGameRunning)
            {
                statusText.text = settledThisRound ? " Выплата зафиксирована" : " Игра идет...";
                statusText.color = settledThisRound ? winColor : normalColor;
            }
            else
            {
                statusText.text = "Готов к новому раунду";
                statusText.color = normalColor;
            }
        }

        if (bettingProgressBar)
        {
            if (isBettingPhase)
            {
                bettingProgressBar.gameObject.SetActive(true);
                bettingProgressBar.value = bettingPhaseTime / bettingPhaseDuration;
            }
            else bettingProgressBar.gameObject.SetActive(false);
        }

        if (placeBetButton) placeBetButton.interactable = isBettingPhase && !betPlacedLocal && playerBalance >= minBet;
        if (cancelBetButton) cancelBetButton.interactable = isBettingPhase && betPlacedLocal;
        if (manualCashoutButton) manualCashoutButton.interactable = isGameRunning && betPlacedLocal && !settledThisRound;
        if (increaseBetButton) increaseBetButton.interactable = isBettingPhase && !betPlacedLocal;
        if (decreaseBetButton) decreaseBetButton.interactable = isBettingPhase && !betPlacedLocal;

        if (commitHashText != null && lastSeeds != null)
            commitHashText.text = "Commit (hash): " + lastSeeds.ServerSeedHash;
    }

    private void UpdateRevealPanel()
    {
        if (revealText == null || lastReveal == null) return;
        var sb = new StringBuilder();
        sb.AppendLine("Reveal:");
        sb.AppendLine("ServerSeed: " + lastReveal.ServerSeed);
        sb.AppendLine("HMAC: " + lastReveal.HmacHex);
        sb.AppendLine("u52: " + lastReveal.U52);
        sb.AppendLine("U: " + lastReveal.U01.ToString("F12"));
        sb.AppendLine("Crash M: x" + lastReveal.CrashPoint.ToString("F2"));
        revealText.text = sb.ToString();
    }

    private void AddToHistory(string s)
    {
        // Ensure fixed capacity = 7
        if (gameHistory == null || gameHistory.Length != 7)
            gameHistory = new string[7];

        if (historyCount < gameHistory.Length)
        {
            // Append to the end while not full
            gameHistory[historyCount] = s;
            historyCount++;
        }
        else
        {
            // Full: drop index 0, shift others down, put new at index 6
            for (int i = 1; i < gameHistory.Length; i++)
                gameHistory[i - 1] = gameHistory[i];
            gameHistory[gameHistory.Length - 1] = s;
        }

        if (historyText)
        {
            var sb = new StringBuilder();
            sb.AppendLine("История:");
            int count = Mathf.Min(historyCount, gameHistory.Length);
            for (int i = 0; i < count; i++)
            {
                if (!string.IsNullOrEmpty(gameHistory[i])) sb.AppendLine(gameHistory[i]);
            }
            historyText.text = sb.ToString();
        }
    }

    private void FlashStatus(string msg, Color c)
    {
        if (statusText) { statusText.text = msg; statusText.color = c; }
    }
}
