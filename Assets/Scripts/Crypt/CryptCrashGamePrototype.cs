
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System;
using System.Security.Cryptography;
using System.Text;

#region Provably Fair + Server Mock

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

    public bool TryManualCashout(double visualMultiplier, double requestClientTime, double rttMillis) {
        if (!betAccepted) return false;
        double receiveTime = requestClientTime + (rttMillis * 0.5) / 1000.0;
        double mAtReceive = animCurve(receiveTime);
        if (receiveTime <= crashTime && mAtReceive >= betTarget) {
            return true;
        }
        return false;
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

public class CryptCrashGamePrototype : MonoBehaviour
{
    [Header("Economy/Math")]
    [Range(0.90f, 0.99f)] public float targetRTP = 0.96f;
    public float minCrash = 1.01f;
    public float maxCrash = 1000f;

    [Header("Bet Settings")]
    public float playerBalance = 1000f;
    public float currentBet = 10f;
    public float minBet = 1f;
    public float maxBet = 5000f;
    public float betStep = 1f;
    public float targetMultiplier = 2.00f;

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

    private RoundSeeds lastSeeds;
    private RoundReveal lastReveal;

    private IRoundService server;
    private ICrashRngProvider rng;

    [Header("UI")]
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

    [Header("Buttons")]
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
    public string[] gameHistory = new string[14];
    private int historyIndex = 0;

    private Coroutine gameLoop;

    private double AnimCurve(double t)
    {
        return (double)MultiplierAtTime((float)t);
    }

    #region Unity
    private void Start()
    {
        rng = new CrashRngProvider();
        // pass Func<double,double> wrapper
        server = new RoundServiceMock(rng, AnimCurve, 1f - targetRTP, minCrash, maxCrash);

        currentBet = Mathf.Clamp(currentBet, minBet, maxBet);
        targetMultiplier = Mathf.Max(1.01f, targetMultiplier);

        StartNewRound();
    }

    private void Update()
    {
        if (isGameRunning)
        {
            gameTime += Time.deltaTime;
            currentMultiplier = MultiplierAtTime(gameTime);
            if (currentMultiplier >= (float)crashPoint) {
                HandleCrash();
            }
            UpdateUI();
        }
    }
    #endregion

    #region Round Flow
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

        lastReveal = server.Reveal();
        crashPoint = lastReveal.CrashPoint;

        UpdateUI();
        if (gameLoop != null) StopCoroutine(gameLoop);
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
            if (statusText) { statusText.text = string.Format("Crash x{0:F2}. Проигрыш", currentMultiplier); statusText.color = loseColor; }
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
    #endregion

    #region Actions
    public void SetupUI()
    {
        if (placeBetButton) placeBetButton.onClick.AddListener(PlaceBet);
        if (cancelBetButton) cancelBetButton.onClick.AddListener(CancelBet);
        if (manualCashoutButton) manualCashoutButton.onClick.AddListener(ManualCashout);
        if (increaseBetButton) increaseBetButton.onClick.AddListener(() => { AdjustBet(+betStep); });
        if (decreaseBetButton) decreaseBetButton.onClick.AddListener(() => { AdjustBet(-betStep); });

        if (betInput)
        {
            betInput.onEndEdit.AddListener(s => {
                float v;
                if (float.TryParse(s.Replace(',', '.'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out v))
                    currentBet = Mathf.Clamp(v, minBet, maxBet);
                UpdateUI();
            });
            betInput.text = currentBet.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
        }
        if (targetInput)
        {
            targetInput.onEndEdit.AddListener(s => {
                float v;
                if (float.TryParse(s.Replace(',', '.'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out v))
                    targetMultiplier = Mathf.Max(1.01f, v);
                UpdateUI();
            });
            targetInput.text = targetMultiplier.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
        }
        if (latencyInput)
        {
            latencyInput.onEndEdit.AddListener(s => {
                float v;
                if (float.TryParse(s, out v)) simulatedRttMs = Mathf.Max(0f, v);
                UpdateUI();
            });
            latencyInput.text = simulatedRttMs.ToString("0");
        }

        UpdateCommitPanel();
        UpdateUI();
    }

    private void PlaceBet()
    {
        if (!isBettingPhase || betPlacedLocal) return;
        if (playerBalance < currentBet) { FlashStatus("Недостаточно средств", warnColor); return; }
        if (targetMultiplier < 1.01f) { FlashStatus("Цель должна быть ≥ 1.01", warnColor); return; }

        playerBalance -= currentBet;
        betPlacedLocal = true;
        server.SubmitBet((decimal)currentBet, targetMultiplier);
        FlashStatus(string.Format("Ставка {0:0.##} @ x{1:0.00}", currentBet, targetMultiplier), winColor);
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
            decimal payout = (decimal)targetMultiplier * (decimal)currentBet;
            playerBalance += (float)payout; // cast to float
            lastPayout = payout;
            settledThisRound = true;
            FlashStatus(string.Format("Выплата +{0:0.##} (x{1:0.00})", payout, targetMultiplier), winColor);
            AddToHistory(string.Format("WIN x{0:0.00} (+{1:0.##})", targetMultiplier, payout));
        }
        else
        {
            FlashStatus("Кешаут отклонён (поздно или цель не достигнута)", warnColor);
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
    #endregion

    #region Math/Animation
    private float MultiplierAtTime(float t)
    {
        if (t <= 0f) return 1f;
        if (t <= 8f)       return Mathf.Min(1f + t / 8f, maxCrash);
        else if (t <= 35f) { float nt = (t - 8f) / 27f; float g = nt * nt * 0.8f + 0.2f * nt; return Mathf.Min(2f + 18f * g, maxCrash); }
        else if (t <= 50f) { float nt = (t - 35f) / 15f; float g = nt * nt * nt; return Mathf.Min(20f + 80f * g, maxCrash); }
        else               { float ex = t - 50f; float g = 1f - Mathf.Exp(-ex / 30f); return Mathf.Min(100f + 900f * g, maxCrash); }
    }
    #endregion

    #region UI Helpers
    public void UpdateUI()
    {
        if (multiplierText) multiplierText.text = string.Format("x{0:F2}", currentMultiplier);
        if (balanceText) balanceText.text = string.Format("💰 Баланс: {0:0.##}", playerBalance);
        if (roundText) roundText.text = string.Format("Раунд {0}", currentRound + 1);
        if (betText)
        {
            if (betPlacedLocal && isGameRunning && !settledThisRound)
                betText.text = string.Format("Ставка: {0:0.##} @ x{1:0.00} | Текущ.: x{2:F2}", currentBet, targetMultiplier, currentMultiplier);
            else if (settledThisRound)
                betText.text = string.Format("Выплата: +{0:0.##}", lastPayout);
            else if (betPlacedLocal)
                betText.text = string.Format("Ставка: {0:0.##} @ x{1:0.00}", currentBet, targetMultiplier);
            else
                betText.text = string.Format("Ставка: {0:0.##} | Цель: x{1:0.00}", currentBet, targetMultiplier);
        }

        if (statusText)
        {
            if (isBettingPhase)
            {
                float left = Mathf.Max(0f, bettingPhaseDuration - bettingPhaseTime);
                statusText.text = betPlacedLocal ? string.Format("Ставка принята • Старт через {0:F1}с", left) : string.Format("Фаза ставок • Осталось {0:F1}с", left);
                statusText.color = betPlacedLocal ? winColor : normalColor;
            }
            else if (isGameRunning)
            {
                statusText.text = settledThisRound ? "Выплата зафиксирована" : "Игра идет...";
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

        UpdateCommitPanel();
    }

    private void UpdateCommitPanel()
    {
        if (commitHashText != null && lastSeeds != null)
            commitHashText.text = "Commit (hash): " + lastSeeds.ServerSeedHash;
    }

    private void UpdateRevealPanel()
    {
        if (revealText == null || lastReveal == null) return;
        revealText.text =
            "Reveal:\n" +
            "ServerSeed: " + lastReveal.ServerSeed + "\n" +
            "HMAC: " + lastReveal.HmacHex + "\n" +
            "u52: " + lastReveal.U52 + "\n" +
            "U: " + lastReveal.U01.ToString("F12") + "\n" +
            "Crash M: x" + lastReveal.CrashPoint.ToString("F2") + "\n";
    }

    private void AddToHistory(string s)
    {
        if (historyIndex >= gameHistory.Length) historyIndex = 0;
        gameHistory[historyIndex++] = s;
        if (historyText)
        {
            string t = "История:\n";
            for (int i = 0; i < gameHistory.Length; i++)
                if (!string.IsNullOrEmpty(gameHistory[i])) t += gameHistory[i] + "\n";
            historyText.text = t;
        }
    }

    private void FlashStatus(string msg, Color c)
    {
        if (statusText) { statusText.text = msg; statusText.color = c; }
    }
    #endregion
}
