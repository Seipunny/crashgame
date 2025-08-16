using UnityEngine;

/// <summary>
/// Тестовый скрипт для проверки математической модели в Unity Editor
/// Добавьте этот скрипт на любой GameObject в сцене
/// </summary>
public class MathTestRunner : MonoBehaviour
{
    [Header("Тестовые настройки")]
    public bool runTestsOnStart = true;
    public int rtpTestRounds = 10000;
    public bool showDetailedLogs = true;
    
    private void Start()
    {
        if (runTestsOnStart)
        {
            RunAllTests();
        }
    }
    
    /// <summary>
    /// Запускает все тесты математической модели
    /// </summary>
    [ContextMenu("Запустить все тесты")]
    public void RunAllTests()
    {
        Debug.Log("🚀 ЗАПУСК ТЕСТОВ МАТЕМАТИЧЕСКОЙ МОДЕЛИ КРЕШ ИГРЫ");
        Debug.Log("=".PadRight(70, '='));
        
        // Тест 1: Формула мультипликатора
        TestMultiplierFormula();
        
        // Тест 2: Распределение крашей
        TestCrashDistribution();
        
        // Тест 3: RTP
        TestRTP();
        
        // Тест 4: Ожидаемые значения
        TestExpectedValues();
        
        // Тест 5: Обратное вычисление
        TestReverseCalculation();
        
        Debug.Log("=".PadRight(70, '='));
        Debug.Log("✅ ВСЕ ТЕСТЫ ЗАВЕРШЕНЫ!");
    }
    
    /// <summary>
    /// Тест 1: Формула роста мультипликатора
    /// </summary>
    [ContextMenu("Тест формулы мультипликатора")]
    public void TestMultiplierFormula()
    {
        Debug.Log("\n🧮 ТЕСТ 1: ФОРМУЛА РОСТА МУЛЬТИПЛИКАТОРА");
        Debug.Log("-".PadRight(50, '-'));
        
        float[] testTimes = { 0f, 8f, 35f, 50f, 80f, 120f };
        float[] expectedValues = { 1.00f, 2.00f, 20.00f, 100.00f, 500.00f, 1000.00f };
        
        bool allPassed = true;
        int passedTests = 0;
        
        for (int i = 0; i < testTimes.Length; i++)
        {
            float calculated = MultiplierCalculator.CalculateMultiplier(testTimes[i]);
            float expected = expectedValues[i];
            float difference = Mathf.Abs(calculated - expected);
            float accuracy = (1f - difference / expected) * 100f;
            
            string status = accuracy > 95f ? "✅" : "❌";
            Debug.Log($"{status} t={testTimes[i]:F1}s: Ожидается={expected:F2}, Вычислено={calculated:F2}, Точность={accuracy:F1}%");
            
            if (accuracy > 95f) passedTests++;
            else allPassed = false;
        }
        
        Debug.Log($"\nРезультат: {passedTests}/{testTimes.Length} тестов пройдено");
        Debug.Log(allPassed ? "✅ ТЕСТ 1 ПРОЙДЕН" : "❌ ТЕСТ 1 НЕ ПРОЙДЕН");
    }
    
    /// <summary>
    /// Тест 2: Распределение точек краша
    /// </summary>
    [ContextMenu("Тест распределения крашей")]
    public void TestCrashDistribution()
    {
        Debug.Log("\n🎲 ТЕСТ 2: РАСПРЕДЕЛЕНИЕ ТОЧЕК КРАША");
        Debug.Log("-".PadRight(50, '-'));
        
        int sampleSize = 10000;
        int[] buckets = new int[7];
        float[] bucketRanges = { 1.01f, 2.0f, 5.0f, 10.0f, 20.0f, 50.0f, 100.0f, 1000.0f };
        float[] expectedPercentages = { 47.05f, 28.56f, 9.56f, 4.79f, 2.88f, 0.96f, 0.96f };
        
        Debug.Log($"Генерируем {sampleSize:N0} точек краша...");
        
        for (int i = 0; i < sampleSize; i++)
        {
            float crashPoint = CrashPointGenerator.GenerateCrashPoint();
            
            for (int j = 0; j < bucketRanges.Length - 1; j++)
            {
                if (crashPoint >= bucketRanges[j] && crashPoint < bucketRanges[j + 1])
                {
                    buckets[j]++;
                    break;
                }
            }
        }
        
        Debug.Log("Распределение крашей:");
        bool distributionCorrect = true;
        
        for (int i = 0; i < buckets.Length; i++)
        {
            float percentage = (float)buckets[i] / sampleSize * 100f;
            float expected = expectedPercentages[i];
            float difference = Mathf.Abs(percentage - expected);
            bool correct = difference < 2f; // Допустимое отклонение 2%
            
            string status = correct ? "✅" : "❌";
            Debug.Log($"{status} {bucketRanges[i]:F2}-{bucketRanges[i + 1]:F2}: {buckets[i]} ({percentage:F1}%) [ожидается: {expected:F1}%]");
            
            if (!correct) distributionCorrect = false;
        }
        
        Debug.Log(distributionCorrect ? "✅ ТЕСТ 2 ПРОЙДЕН" : "❌ ТЕСТ 2 НЕ ПРОЙДЕН");
    }
    
    /// <summary>
    /// Тест 3: RTP (Return to Player)
    /// </summary>
    [ContextMenu("Тест RTP")]
    public void TestRTP()
    {
        Debug.Log("\n💰 ТЕСТ 3: RTP (RETURN TO PLAYER)");
        Debug.Log("-".PadRight(50, '-'));
        
        Debug.Log($"Запускаем симуляцию на {rtpTestRounds:N0} раундов...");
        
        var result = RTPValidator.ValidateRTP(rtpTestRounds);
        
        Debug.Log($"Целевой RTP: 96.00%");
        Debug.Log($"Фактический RTP: {result.actualRTP:P4}");
        Debug.Log($"Отклонение: {Mathf.Abs(result.actualRTP - 0.96f):P4}");
        Debug.Log($"Винрейт: {result.successRate:P2}");
        Debug.Log($"Всего игр: {(result.successfulGames + result.failedGames):N0}");
        Debug.Log($"Успешных: {result.successfulGames:N0}");
        Debug.Log($"Проигрышных: {result.failedGames:N0}");
        Debug.Log($"Максимальный краш: x{result.maxCrashPoint:F2}");
        Debug.Log($"Максимальный выигрыш: {result.maxWin:F2}");
        Debug.Log($"Время тестирования: {result.testDuration:F2}с");
        
        bool rtpCorrect = Mathf.Abs(result.actualRTP - 0.96f) < 0.01f; // Допустимое отклонение 1%
        Debug.Log(rtpCorrect ? "✅ ТЕСТ 3 ПРОЙДЕН" : "❌ ТЕСТ 3 НЕ ПРОЙДЕН");
    }
    
    /// <summary>
    /// Тест 4: Ожидаемые значения
    /// </summary>
    [ContextMenu("Тест ожидаемых значений")]
    public void TestExpectedValues()
    {
        Debug.Log("\n📈 ТЕСТ 4: ОЖИДАЕМЫЕ ЗНАЧЕНИЯ");
        Debug.Log("-".PadRight(50, '-'));
        
        float[] testMultipliers = { 1.5f, 2.0f, 3.0f, 5.0f, 10.0f, 20.0f };
        bool allCorrect = true;
        
        Debug.Log("Проверяем, что E[payout] = 0.96 для любого мультипликатора:");
        
        foreach (float multiplier in testMultipliers)
        {
            float expectedValue = CrashPointGenerator.GetExpectedValue(multiplier);
            bool correct = Mathf.Abs(expectedValue - 0.96f) < 0.001f;
            string status = correct ? "✅" : "❌";
            Debug.Log($"{status} x{multiplier:F1}: E[payout] = {expectedValue:F4} (цель: 0.9600)");
            
            if (!correct) allCorrect = false;
        }
        
        Debug.Log(allCorrect ? "✅ ТЕСТ 4 ПРОЙДЕН" : "❌ ТЕСТ 4 НЕ ПРОЙДЕН");
    }
    
    /// <summary>
    /// Тест 5: Обратное вычисление времени
    /// </summary>
    [ContextMenu("Тест обратного вычисления")]
    public void TestReverseCalculation()
    {
        Debug.Log("\n🔄 ТЕСТ 5: ОБРАТНОЕ ВЫЧИСЛЕНИЕ ВРЕМЕНИ");
        Debug.Log("-".PadRight(50, '-'));
        
        float[] targetMultipliers = { 2.0f, 5.0f, 10.0f, 20.0f, 50.0f };
        bool allCorrect = true;
        
        Debug.Log("Проверяем обратное вычисление времени:");
        
        foreach (float target in targetMultipliers)
        {
            float time = MultiplierCalculator.CalculateTimeForMultiplier(target);
            float calculatedMultiplier = MultiplierCalculator.CalculateMultiplier(time);
            float difference = Mathf.Abs(calculatedMultiplier - target);
            bool correct = difference < 0.1f; // Допустимая погрешность 0.1
            
            string status = correct ? "✅" : "❌";
            Debug.Log($"{status} Цель: x{target:F1} | Время: {time:F2}с | Результат: x{calculatedMultiplier:F2} | Разница: {difference:F3}");
            
            if (!correct) allCorrect = false;
        }
        
        Debug.Log(allCorrect ? "✅ ТЕСТ 5 ПРОЙДЕН" : "❌ ТЕСТ 5 НЕ ПРОЙДЕН");
    }
    
    /// <summary>
    /// Быстрый тест для отладки
    /// </summary>
    [ContextMenu("Быстрый тест")]
    public void QuickTest()
    {
        Debug.Log("⚡ БЫСТРЫЙ ТЕСТ (основные проверки)");
        
        // Проверяем несколько ключевых точек
        float multiplier1 = MultiplierCalculator.CalculateMultiplier(8f);
        float multiplier2 = MultiplierCalculator.CalculateMultiplier(35f);
        float multiplier3 = MultiplierCalculator.CalculateMultiplier(50f);
        
        Debug.Log($"t=8s: x{multiplier1:F2} (ожидается: x2.00)");
        Debug.Log($"t=35s: x{multiplier2:F2} (ожидается: x20.00)");
        Debug.Log($"t=50s: x{multiplier3:F2} (ожидается: x100.00)");
        
        // Проверяем несколько точек краша
        for (int i = 0; i < 5; i++)
        {
            float crashPoint = CrashPointGenerator.GenerateCrashPoint();
            Debug.Log($"Краш {i + 1}: x{crashPoint:F2}");
        }
        
        // Быстрый RTP тест
        var result = RTPValidator.ValidateRTP(1000);
        Debug.Log($"Быстрый RTP тест: {result.actualRTP:P4}");
    }
    
    /// <summary>
    /// Полный тест RTP (1,000,000 раундов)
    /// </summary>
    [ContextMenu("Полный тест RTP")]
    public void FullRTPTest()
    {
        Debug.Log("🔬 ПОЛНЫЙ ТЕСТ RTP (1,000,000 раундов)");
        Debug.Log("Это может занять несколько секунд...");
        
        var result = RTPValidator.ValidateRTP(1000000);
        RTPValidator.PrintValidationReport(result);
    }
} 