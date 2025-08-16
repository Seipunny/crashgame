using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Валидатор RTP для тестирования математической модели краш игры
/// </summary>
public static class RTPValidator
{
    private const float TARGET_RTP = 0.96f;
    private const float ACCEPTABLE_DEVIATION = 0.01f; // 1%
    
    /// <summary>
    /// Результат валидации RTP
    /// </summary>
    [System.Serializable]
    public struct ValidationResult
    {
        public float totalBets;
        public float totalPayouts;
        public float actualRTP;
        public float successRate;
        public int successfulGames;
        public int failedGames;
        public Dictionary<string, int> crashDistribution;
        public float maxCrashPoint;
        public float maxWin;
        public float testDuration;
    }
    
    /// <summary>
    /// Проводит полную валидацию RTP через симуляцию
    /// </summary>
    /// <param name="numberOfRounds">Количество раундов для симуляции</param>
    /// <returns>Результат валидации</returns>
    public static ValidationResult ValidateRTP(int numberOfRounds = 100000)
    {
        var result = new ValidationResult
        {
            totalBets = 0f,
            totalPayouts = 0f,
            actualRTP = 0f,
            successRate = 0f,
            successfulGames = 0,
            failedGames = 0,
            crashDistribution = new Dictionary<string, int>
            {
                {"1.01-2.00", 0},
                {"2.01-5.00", 0}, 
                {"5.01-10.00", 0},
                {"10.01-20.00", 0},
                {"20.01-50.00", 0},
                {"50.01-100.00", 0},
                {"100.01+", 0}
            },
            maxCrashPoint = 0f,
            maxWin = 0f,
            testDuration = 0f
        };
        float startTime = Time.realtimeSinceStartup;
        
        Debug.Log($"Начинаем валидацию RTP на {numberOfRounds:N0} раундов...");
        
        for (int i = 0; i < numberOfRounds; i++)
        {
            float bet = 1.0f; // Фиксированная ставка 1.0
            float crashPoint = CrashPointGenerator.GenerateCrashPoint();
            
            // Симулируем поведение игрока
            float cashoutPoint = SimulatePlayerBehavior(crashPoint);
            
            result.totalBets += bet;
            
            if (cashoutPoint > 0f)
            {
                // Игрок успел забрать
                float winAmount = bet * cashoutPoint;
                result.totalPayouts += winAmount;
                result.successfulGames++;
                
                if (winAmount > result.maxWin)
                {
                    result.maxWin = winAmount;
                }
            }
            else
            {
                // Игрок проиграл (не успел кешаутиться)
                result.failedGames++;
            }
            
            // Анализируем распределение крашей
            AnalyzeCrashPoint(crashPoint, result);
            
            // Прогресс каждые 10% раундов
            if ((i + 1) % (numberOfRounds / 10) == 0)
            {
                float progress = (float)(i + 1) / numberOfRounds * 100f;
                float currentRTP = result.totalPayouts / result.totalBets;
                Debug.Log($"Прогресс: {progress:F1}% | Текущий RTP: {currentRTP:P4}");
            }
        }
        
        // Рассчитываем финальные результаты
        result.actualRTP = result.totalPayouts / result.totalBets;
        result.successRate = (float)result.successfulGames / numberOfRounds;
        result.testDuration = Time.realtimeSinceStartup - startTime;
        
        return result;
    }
    
    /// <summary>
    /// Симулирует поведение игрока (на каком мультипликаторе забирает)
    /// </summary>
    /// <param name="crashPoint">Точка краша</param>
    /// <returns>Мультипликатор кешаута</returns>
    private static float SimulatePlayerBehavior(float crashPoint)
    {
        // Реалистичная модель поведения игрока (как в HTML файле)
        // Игрок выбирает целевую точку кешаута и кешаутится, если успевает
        
        float target = SampleTargetStandard();
        
        // Игрок кешаутится, если краш происходит после его целевой точки
        return crashPoint >= target ? target : 0f;
    }
    
    /// <summary>
    /// Генерирует стандартную целевую точку кешаута игрока
    /// </summary>
    /// <returns>Целевой мультипликатор</returns>
    private static float SampleTargetStandard()
    {
        float r = UnityEngine.Random.Range(0f, 1f);
        
        if (r < 0.55f) return 1.10f + UnityEngine.Random.Range(0f, 1f) * 1.40f; // 1.10–2.50
        if (r < 0.85f) return 2.50f + UnityEngine.Random.Range(0f, 1f) * 2.50f; // 2.50–5.00
        if (r < 0.97f) return 5.00f + UnityEngine.Random.Range(0f, 1f) * 5.00f; // 5.00–10.00
        return 10.00f + UnityEngine.Random.Range(0f, 1f) * 40.00f;             // 10.00–50.00
    }
    
    /// <summary>
    /// Анализирует распределение точек краша
    /// </summary>
    /// <param name="crashPoint">Точка краша</param>
    /// <param name="result">Результат валидации</param>
    private static void AnalyzeCrashPoint(float crashPoint, ValidationResult result)
    {
        if (crashPoint > result.maxCrashPoint)
        {
            result.maxCrashPoint = crashPoint;
        }
        
        if (crashPoint <= 2.00f) result.crashDistribution["1.01-2.00"]++;
        else if (crashPoint <= 5.00f) result.crashDistribution["2.01-5.00"]++;
        else if (crashPoint <= 10.00f) result.crashDistribution["5.01-10.00"]++;
        else if (crashPoint <= 20.00f) result.crashDistribution["10.01-20.00"]++;
        else if (crashPoint <= 50.00f) result.crashDistribution["20.01-50.00"]++;
        else if (crashPoint <= 100.00f) result.crashDistribution["50.01-100.00"]++;
        else result.crashDistribution["100.01+"]++;
    }
    
    /// <summary>
    /// Выводит детальный отчет валидации
    /// </summary>
    /// <param name="result">Результат валидации</param>
    public static void PrintValidationReport(ValidationResult result)
    {
        Debug.Log("=".PadRight(50, '='));
        Debug.Log("ОТЧЕТ ВАЛИДАЦИИ RTP");
        Debug.Log("=".PadRight(50, '='));
        
        Debug.Log($"Целевой RTP: {TARGET_RTP:P2}");
        Debug.Log($"Фактический RTP: {result.actualRTP:P4}");
        Debug.Log($"Отклонение: {Mathf.Abs(result.actualRTP - TARGET_RTP):P4}");
        Debug.Log($"Винрейт: {result.successRate:P2}");
        
        Debug.Log($"\nОбщие показатели:");
        Debug.Log($"Всего игр: {(result.successfulGames + result.failedGames):N0}");
        Debug.Log($"Успешных: {result.successfulGames:N0}");
        Debug.Log($"Проигрышных: {result.failedGames:N0}");
        Debug.Log($"Общие ставки: {result.totalBets:N0}");
        Debug.Log($"Общие выплаты: {result.totalPayouts:N0}");
        
        Debug.Log($"\nМаксимальные значения:");
        Debug.Log($"Максимальный краш: x{result.maxCrashPoint:F2}");
        Debug.Log($"Максимальный выигрыш: {result.maxWin:F2}");
        Debug.Log($"Время тестирования: {result.testDuration:F2}с");
        
        Debug.Log($"\nРаспределение крашей:");
        int totalCrashes = 0;
        foreach (var kvp in result.crashDistribution)
        {
            totalCrashes += kvp.Value;
        }
        
        foreach (var kvp in result.crashDistribution)
        {
            float percentage = (float)kvp.Value / totalCrashes * 100f;
            Debug.Log($"{kvp.Key}: {kvp.Value:N0} ({percentage:F2}%)");
        }
        
        // Проверяем, соответствует ли RTP требованиям
        float rtpDifference = Mathf.Abs(result.actualRTP - TARGET_RTP);
        if (rtpDifference < ACCEPTABLE_DEVIATION)
        {
            Debug.Log($"\n✅ ВАЛИДАЦИЯ ПРОЙДЕНА! RTP в допустимых пределах");
            Debug.Log($"Отклонение {rtpDifference:P4} < {ACCEPTABLE_DEVIATION:P2}");
        }
        else
        {
            Debug.Log($"\n❌ ВАЛИДАЦИЯ НЕ ПРОЙДЕНА! RTP требует корректировки");
            Debug.Log($"Отклонение {rtpDifference:P4} > {ACCEPTABLE_DEVIATION:P2}");
        }
        
        Debug.Log("=".PadRight(50, '='));
    }
    
    /// <summary>
    /// Быстрый тест RTP (для отладки)
    /// </summary>
    public static void QuickRTPTest()
    {
        Debug.Log("🚀 Быстрый тест RTP (10,000 раундов)...");
        
        var result = ValidateRTP(10000);
        PrintValidationReport(result);
    }
    
    /// <summary>
    /// Полный тест RTP (для финальной проверки)
    /// </summary>
    public static void FullRTPTest()
    {
        Debug.Log("🧮 Полный тест RTP (1,000,000 раундов)...");
        
        var result = ValidateRTP(1000000);
        PrintValidationReport(result);
    }
} 