using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Прототип краш-игры с базовым UI
/// </summary>
public class CrashGamePrototype : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI multiplierText;
    public TextMeshProUGUI balanceText;
    public TextMeshProUGUI betText;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI historyText;
    public TextMeshProUGUI finalWinText;
    public TextMeshProUGUI roundText;
    public Slider bettingProgressBar;
    
    [Header("Buttons")]
    public Button placeBetButton;
    public Button cashoutButton;
    public Button increaseBetButton;
    public Button decreaseBetButton;
    
    [Header("Game Settings")]
    public float playerBalance = 1000f;
    public float currentBet = 10f;
    public float minBet = 10f;
    public float maxBet = 100f;
    public float betStep = 5f;
    
    [Header("Game State")]
    public bool isGameRunning = false;
    public bool isBettingPhase = false;
    public bool hasPlacedBet = false;
    public bool hasCashedOut = false;
    public float currentMultiplier = 1.0f;
    public float crashPoint = 1.0f;
    public float gameTime = 0f;
    public float maxGameTime = 60f;
    public float finalWinAmount = 0f;
    
    [Header("Round System")]
    public float bettingPhaseDuration = 5f;
    public float bettingPhaseTime = 0f;
    public int currentRound = 1;
    
    [Header("History")]
    public string[] gameHistory = new string[10];
    public int historyIndex = 0;
    
    [Header("UI Colors")]
    public Color normalColor = Color.white;
    public Color winColor = Color.green;
    public Color loseColor = Color.red;
    public Color missedColor = Color.yellow;
    
    private Coroutine gameCoroutine;
    
    private void Start()
    {
        // Инициализируем игру
        playerBalance = 1000f;
        currentBet = Mathf.Clamp(10f, minBet, maxBet); // Принудительно устанавливаем в допустимый диапазон
        
        Debug.Log("🎮 Прототип краш-игры запущен!");
        Debug.Log($"💰 Баланс: {playerBalance}, Ставка: {currentBet}");
        Debug.Log($"📊 Лимиты: мин {minBet}, макс {maxBet}, шаг {betStep}");
        
        // Добавляем тестовую запись в историю
        AddToHistory("ТЕСТ x1.00");
        
        // Запускаем систему раундов (UI создастся автоматически)
        StartNewRound();
    }
    
    public void SetupUI()
    {
        Debug.Log("🔧 Настройка UI обработчиков...");
        
        // Настройка кнопок
        if (placeBetButton != null)
        {
            placeBetButton.onClick.AddListener(PlaceBet);
            Debug.Log("✅ PlaceBetButton обработчик добавлен");
        }
        else
        {
            Debug.LogWarning("⚠️ PlaceBetButton не найден");
        }
        
        if (cashoutButton != null)
        {
            cashoutButton.onClick.AddListener(Cashout);
            Debug.Log("✅ CashoutButton обработчик добавлен");
        }
        else
        {
            Debug.LogWarning("⚠️ CashoutButton не найден");
        }
        
        if (increaseBetButton != null)
        {
            increaseBetButton.onClick.AddListener(IncreaseBet);
            Debug.Log("✅ IncreaseBetButton обработчик добавлен");
        }
        else
        {
            Debug.LogWarning("⚠️ IncreaseBetButton не найден");
        }
        
        if (decreaseBetButton != null)
        {
            decreaseBetButton.onClick.AddListener(DecreaseBet);
            Debug.Log("✅ DecreaseBetButton обработчик добавлен");
        }
        else
        {
            Debug.LogWarning("⚠️ DecreaseBetButton не найден");
        }
        
        // Начальное состояние кнопок
        SetButtonStates(false);
        Debug.Log("✅ UI обработчики настроены");
    }
    
    private void Update()
    {
        if (isGameRunning)
        {
            UpdateGame();
        }
    }
    
    private void UpdateGame()
    {
        gameTime += Time.deltaTime;
        currentMultiplier = MultiplierCalculator.CalculateMultiplier(gameTime);
        
        // Проверяем, не достигли ли точки краша
        if (currentMultiplier >= crashPoint)
        {
            Crash();
        }
        
        UpdateUI();
    }
    
    public void UpdateUI()
    {
        // Обновляем тексты
        if (multiplierText != null)
            multiplierText.text = $"x{currentMultiplier:F2}";
        
        if (balanceText != null)
            balanceText.text = $"💰 Баланс: {playerBalance:F0}";
        
        // Обновляем номер раунда
        if (roundText != null)
        {
            roundText.text = $"🎯 Раунд {currentRound}";
        }
        
        if (betText != null)
        {
            if (hasPlacedBet && isGameRunning)
            {
                float potentialWin = currentBet * currentMultiplier;
                betText.text = $"Ставка: {currentBet:F0} | Выигрыш: {potentialWin:F0}";
                betText.color = normalColor;
            }
            else if (hasCashedOut && isGameRunning)
            {
                betText.text = $"Ставка: {currentBet:F0} | Забрано: +{finalWinAmount:F0}";
                betText.color = winColor;
            }
            else
            {
                betText.text = $"Ставка: {currentBet:F0}";
                betText.color = normalColor;
            }
        }
        
        if (statusText != null)
        {
            if (isBettingPhase)
            {
                float timeLeft = bettingPhaseDuration - bettingPhaseTime;
                if (hasPlacedBet)
                {
                    statusText.text = $"✅ Ставка размещена! Осталось: {timeLeft:F1}с";
                    statusText.color = winColor;
                }
                else
                {
                    statusText.text = $"⏰ Фаза ставок! Осталось: {timeLeft:F1}с";
                    statusText.color = normalColor;
                }
            }
            else if (isGameRunning)
            {
                if (hasPlacedBet)
                {
                    statusText.text = "🎮 Игра идет...";
                    statusText.color = normalColor;
                }
                else if (hasCashedOut)
                {
                    statusText.text = $"Кешаут! +{finalWinAmount:F0}";
                    statusText.color = winColor;
                }
                else
                {
                    statusText.text = "👀 Игра продолжается...";
                    statusText.color = normalColor;
                }
            }
            else if (hasPlacedBet)
            {
                statusText.text = "Готов к игре";
                statusText.color = normalColor;
            }
            else
            {
                statusText.text = "Разместите ставку";
                statusText.color = normalColor;
            }
        }
        
        // Обновляем историю
        UpdateHistoryUI();
        
        // Обновляем итоговый выигрыш
        UpdateFinalWinText();
        
        // Обновляем прогресс-бар фазы ставок
        UpdateBettingProgressBar();
        
        // Обновляем состояние кнопок в фазе ставок
        if (isBettingPhase)
        {
            SetBettingPhaseButtonStates();
            UpdateBetButtonText();
        }
        else
        {
            UpdateBetButtonText();
        }
        
        // Отладка UI элементов
        Debug.Log($"🔍 UI Debug - historyText: {(historyText != null ? "найден" : "НЕ НАЙДЕН")}, " +
                 $"balanceText: {(balanceText != null ? "найден" : "НЕ НАЙДЕН")}, " +
                 $"betText: {(betText != null ? "найден" : "НЕ НАЙДЕН")}");
    }
    
    private void UpdateHistoryUI()
    {
        if (historyText != null)
        {
            string history = "История:\n";
            for (int i = 0; i < gameHistory.Length; i++)
            {
                int index = (historyIndex - i + gameHistory.Length) % gameHistory.Length;
                if (!string.IsNullOrEmpty(gameHistory[index]))
                {
                    history += gameHistory[index] + "\n";
                }
            }
            historyText.text = history;
            Debug.Log($"📜 Обновлена история: {history}");
        }
        else
        {
            Debug.LogWarning("⚠️ historyText не найден!");
        }
    }
    
    private void UpdateFinalWinText()
    {
        if (finalWinText != null)
        {
            if (hasCashedOut && isGameRunning)
            {
                finalWinText.text = $"🎉 ЗАБРАНО: +{finalWinAmount:F0}";
                finalWinText.color = winColor;
            }
            else
            {
                finalWinText.text = "";
            }
        }
    }
    
    private void UpdateBettingProgressBar()
    {
        if (bettingProgressBar != null)
        {
            if (isBettingPhase)
            {
                bettingProgressBar.gameObject.SetActive(true);
                bettingProgressBar.value = bettingPhaseTime / bettingPhaseDuration;
            }
            else
            {
                bettingProgressBar.gameObject.SetActive(false);
            }
        }
    }
    
    private void UpdateBetButtonText()
    {
        if (placeBetButton == null) return;
        
        if (hasCashedOut)
        {
            placeBetButton.GetComponentInChildren<TextMeshProUGUI>().text = "ЗАБЛОКИРОВАНО";
        }
        else if (isBettingPhase)
        {
            placeBetButton.GetComponentInChildren<TextMeshProUGUI>().text = hasPlacedBet ? "ОТМЕНИТЬ" : "СТАВКА";
        }
        else if (isGameRunning && !hasPlacedBet)
        {
            placeBetButton.GetComponentInChildren<TextMeshProUGUI>().text = "СТАВКА";
        }
        else
        {
            placeBetButton.GetComponentInChildren<TextMeshProUGUI>().text = "СТАВКА";
        }
    }
    
    public void PlaceBet()
    {
        Debug.Log($"🔘 PlaceBet вызван! isBettingPhase={isBettingPhase}, hasPlacedBet={hasPlacedBet}, isGameRunning={isGameRunning}");
        
        // Если игра идет и уже был кешаут - блокируем
        if (isGameRunning && hasCashedOut)
        {
            Debug.Log("❌ PlaceBet: уже был кешаут в этом раунде");
            return;
        }
        
        // Если в фазе ставок и ставка еще не размещена
        if (isBettingPhase && !hasPlacedBet)
        {
            hasPlacedBet = true;
            playerBalance -= currentBet;
            Debug.Log($"✅ Ставка размещена: {currentBet}");
            UpdateUI();
            return;
        }
        
        // Если в фазе ставок и ставка уже размещена - отменяем
        if (isBettingPhase && hasPlacedBet)
        {
            hasPlacedBet = false;
            playerBalance += currentBet;
            Debug.Log($"❌ Ставка отменена: {currentBet}");
            UpdateUI();
            return;
        }
        
        // Если игра идет и ставка не размещена - размещаем
        if (isGameRunning && !hasPlacedBet && !hasCashedOut)
        {
            hasPlacedBet = true;
            playerBalance -= currentBet;
            Debug.Log($"Ставка размещена: {currentBet}");
            UpdateUI();
            return;
        }
        
        Debug.Log("❌ PlaceBet: условие не выполнено");
    }
    

    
    public void Cashout()
    {
        Debug.Log($"🔘 Cashout вызван! isGameRunning={isGameRunning}, hasPlacedBet={hasPlacedBet}");
        if (!isGameRunning || !hasPlacedBet) 
        {
            Debug.Log("❌ Cashout: условие не выполнено");
            return;
        }
        
        float winAmount = currentBet * currentMultiplier;
        playerBalance += winAmount;
        
        // Добавляем в историю
        AddToHistory($"WIN x{currentMultiplier:F2}");
        
        Debug.Log($"Кешаут! Выигрыш: {winAmount:F2}");
        
        // Игра продолжается, но игрок уже забрал выигрыш
        hasPlacedBet = false;
        hasCashedOut = true;
        SetButtonStates(false);
        
        // Показываем, что игрок забрал выигрыш
        if (statusText != null)
        {
            statusText.text = $"Кешаут! +{winAmount:F0}";
            statusText.color = winColor;
        }
        
        // Сохраняем итоговый выигрыш для отображения
        finalWinAmount = winAmount;
        
        // Обновляем UI для показа итогового выигрыша
        UpdateUI();
    }
    
    public void IncreaseBet()
    {
        if (isGameRunning || hasPlacedBet) return;
        
        currentBet = Mathf.Min(currentBet + betStep, maxBet);
        Debug.Log($"📈 Ставка увеличена: {currentBet}");
        UpdateUI();
    }
    
    public void DecreaseBet()
    {
        if (isGameRunning || hasPlacedBet) return;
        
        currentBet = Mathf.Max(currentBet - betStep, minBet);
        // Дополнительная проверка для гарантии
        if (currentBet < minBet) currentBet = minBet;
        
        Debug.Log($"📉 Ставка уменьшена: {currentBet} (мин: {minBet})");
        UpdateUI();
    }
    
    private void StartNewRound()
    {
        // Генерируем новую точку краша
        crashPoint = CrashPointGenerator.GenerateCrashPoint();
        
        // Сбрасываем состояние игры
        isGameRunning = false;
        isBettingPhase = true;
        hasPlacedBet = false;
        hasCashedOut = false;
        currentMultiplier = 1.0f;
        gameTime = 0f;
        bettingPhaseTime = 0f;
        finalWinAmount = 0f;
        
        // Настраиваем кнопки для фазы ставок
        SetBettingPhaseButtonStates();
        UpdateUI();
        
        Debug.Log($"🔄 Раунд {currentRound}. Фаза ставок началась. Краш: x{crashPoint:F2}");
        
        // Запускаем фазу ставок
        StartCoroutine(BettingPhase());
    }
    

    
    private void StartGame()
    {
        // Настраиваем кнопки для игровой фазы
        SetButtonStates(true);
        
        // Запускаем игровой цикл
        gameCoroutine = StartCoroutine(GameLoop());
    }
    
    private IEnumerator GameLoop()
    {
        // Небольшая задержка перед началом
        yield return new WaitForSeconds(1f);
        
        // Запускаем игру
        isGameRunning = true;
        Debug.Log("🎮 Игра началась!");
        
        // Игра продолжается до краша или кешаута
        while (isGameRunning && gameTime < maxGameTime)
        {
            yield return null;
        }
        
        // Если время истекло, но игра не закончилась
        if (isGameRunning)
        {
            Crash();
        }
    }
    
    private void Crash()
    {
        isGameRunning = false;
        
        // Всегда добавляем краш в историю
        AddToHistory($"CRASH x{crashPoint:F2}");
        
        if (hasPlacedBet)
        {
            Debug.Log($"Краш! x{crashPoint:F2}");
            
            // Показываем, что игрок проиграл
            if (statusText != null)
            {
                statusText.text = $"КРАШ! -{currentBet:F0}";
                statusText.color = loseColor;
            }
        }
        else if (hasCashedOut)
        {
            // Игрок уже забрал выигрыш, показываем упущенную возможность
            if (statusText != null)
            {
                statusText.text = $"УПУЩЕНО! x{crashPoint:F2}";
                statusText.color = missedColor;
            }
        }
        else
        {
            // Игрок не участвовал в раунде
            Debug.Log($"Краш! x{crashPoint:F2} (игрок не участвовал)");
        }
        
        EndRound();
    }
    
    private void EndRound()
    {
        hasPlacedBet = false;
        hasCashedOut = false;
        finalWinAmount = 0f;
        SetButtonStates(false);
        
        // Останавливаем корутину игры
        if (gameCoroutine != null)
        {
            StopCoroutine(gameCoroutine);
            gameCoroutine = null;
        }
        
        // Небольшая задержка перед новым раундом
        StartCoroutine(DelayedNewRound());
    }
    
    private IEnumerator DelayedNewRound()
    {
        yield return new WaitForSeconds(2f);
        currentRound++;
        StartNewRound();
    }
    
    private IEnumerator BettingPhase()
    {
        Debug.Log("⏰ Фаза ставок началась!");
        
        while (bettingPhaseTime < bettingPhaseDuration && isBettingPhase)
        {
            bettingPhaseTime += Time.deltaTime;
            
            // Обновляем прогресс-бар
            if (bettingProgressBar != null)
            {
                bettingProgressBar.value = bettingPhaseTime / bettingPhaseDuration;
            }
            
            yield return null;
        }
        
        // Фаза ставок закончилась
        isBettingPhase = false;
        
        Debug.Log($"⏰ Фаза ставок закончилась. Ставка размещена: {hasPlacedBet}");
        
        // Начинаем игровую фазу независимо от того, поставил ли игрок ставку
        StartGame();
    }
    
    private void SetButtonStates(bool gameRunning)
    {
        if (placeBetButton != null)
        {
            // Блокируем кнопку ставки если уже был кешаут
            placeBetButton.interactable = !hasCashedOut;
        }
        
        if (cashoutButton != null)
        {
            cashoutButton.interactable = gameRunning && hasPlacedBet && !hasCashedOut;
        }
        
        if (increaseBetButton != null)
        {
            increaseBetButton.interactable = !gameRunning && !hasPlacedBet;
        }
        
        if (decreaseBetButton != null)
        {
            decreaseBetButton.interactable = !gameRunning && !hasPlacedBet;
        }
    }
    
    private void SetBettingPhaseButtonStates()
    {
        if (placeBetButton != null)
            placeBetButton.interactable = true; // Кнопка всегда активна в фазе ставок
        
        if (cashoutButton != null)
            cashoutButton.interactable = false;
        
        if (increaseBetButton != null)
            increaseBetButton.interactable = !hasPlacedBet; // Можно изменять ставку только если она не размещена
        
        if (decreaseBetButton != null)
            decreaseBetButton.interactable = !hasPlacedBet; // Можно изменять ставку только если она не размещена
    }
    
    private void AddToHistory(string result)
    {
        Debug.Log($"📜 Добавляем в историю: {result} (индекс: {historyIndex})");
        gameHistory[historyIndex] = result;
        historyIndex = (historyIndex + 1) % gameHistory.Length;
        Debug.Log($"📜 История обновлена, новый индекс: {historyIndex}");
    }
    
    // Методы для автоматического запуска игры (для тестирования)
    [ContextMenu("Авто-ставка и запуск")]
    public void AutoBetAndStart()
    {
        if (!hasPlacedBet)
        {
            PlaceBet();
        }
    }
    
    [ContextMenu("Авто-кешаут на x2")]
    public void AutoCashoutAt2x()
    {
        if (isGameRunning && hasPlacedBet && currentMultiplier >= 2.0f)
        {
            Cashout();
        }
    }
} 