using UnityEngine;

/// <summary>
/// Быстрый тест математической модели
/// Запускается автоматически при старте
/// </summary>
public class QuickTest : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("🚀 БЫСТРЫЙ ТЕСТ МАТЕМАТИЧЕСКОЙ МОДЕЛИ КРЕШ ИГРЫ");
        Debug.Log("=".PadRight(60, '='));
        
        // Тест 1: Формула мультипликатора
        TestMultiplierFormula();
        
        // Тест 2: Распределение крашей
        TestCrashDistribution();
        
        // Тест 3: RTP
        TestRTP();
        
        Debug.Log("=".PadRight(60, '='));
        Debug.Log("✅ БЫСТРЫЙ ТЕСТ ЗАВЕРШЕН!");
    }
    
    private void TestMultiplierFormula()
    {
        Debug.Log("\n🧮 ТЕСТ 1: ФОРМУЛА МУЛЬТИПЛИКАТОРА");
        
        float[] testTimes = { 0f, 8f, 35f, 50f, 80f, 120f };
        float[] expectedValues = { 1.00f, 2.00f, 20.00f, 100.00f, 500.00f, 1000.00f };
        
        bool allPassed = true;
        
        for (int i = 0; i < testTimes.Length; i++)
        {
            float calculated = MultiplierCalculator.CalculateMultiplier(testTimes[i]);
            float expected = expectedValues[i];
            float difference = Mathf.Abs(calculated - expected);
            float accuracy = (1f - difference / expected) * 100f;
            
            string status = accuracy > 95f ? "✅" : "❌";
            Debug.Log($"{status} t={testTimes[i]:F1}s: Ожидается={expected:F2}, Вычислено={calculated:F2}, Точность={accuracy:F1}%");
            
            if (accuracy <= 95f) allPassed = false;
        }
        
        Debug.Log(allPassed ? "✅ ТЕСТ 1 ПРОЙДЕН" : "❌ ТЕСТ 1 НЕ ПРОЙДЕН");
    }
    
    private void TestCrashDistribution()
    {
        Debug.Log("\n🎲 ТЕСТ 2: РАСПРЕДЕЛЕНИЕ КРАШЕЙ");
        
        int sampleSize = 10000;
        int[] buckets = new int[7];
        float[] bucketRanges = { 1.01f, 2.0f, 5.0f, 10.0f, 20.0f, 50.0f, 100.0f, 1000.0f };
        
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
        for (int i = 0; i < buckets.Length; i++)
        {
            float percentage = (float)buckets[i] / sampleSize * 100f;
            Debug.Log($"  {bucketRanges[i]:F2}-{bucketRanges[i + 1]:F2}: {buckets[i]} ({percentage:F1}%)");
        }
        
        // Проверяем ожидаемые значения
        Debug.Log("\nПроверка ожидаемых значений:");
        float[] testMultipliers = { 1.5f, 2.0f, 3.0f, 5.0f, 10.0f };
        bool allCorrect = true;
        
        foreach (float multiplier in testMultipliers)
        {
            float expectedValue = CrashPointGenerator.GetExpectedValue(multiplier);
            bool correct = Mathf.Abs(expectedValue - 0.96f) < 0.001f;
            string status = correct ? "✅" : "❌";
            Debug.Log($"{status} x{multiplier:F1}: E[payout] = {expectedValue:F4} (цель: 0.9600)");
            
            if (!correct) allCorrect = false;
        }
        
        Debug.Log(allCorrect ? "✅ ТЕСТ 2 ПРОЙДЕН" : "❌ ТЕСТ 2 НЕ ПРОЙДЕН");
    }
    
    private void TestRTP()
    {
        Debug.Log("\n💰 ТЕСТ 3: RTP (10,000 раундов)");
        
        var result = RTPValidator.ValidateRTP(10000);
        
        Debug.Log($"Целевой RTP: 96.00%");
        Debug.Log($"Фактический RTP: {result.actualRTP:P4}");
        Debug.Log($"Отклонение: {Mathf.Abs(result.actualRTP - 0.96f):P4}");
        Debug.Log($"Винрейт: {result.successRate:P2}");
        Debug.Log($"Максимальный краш: x{result.maxCrashPoint:F2}");
        Debug.Log($"Время тестирования: {result.testDuration:F2}с");
        
        bool rtpCorrect = Mathf.Abs(result.actualRTP - 0.96f) < 0.01f;
        Debug.Log(rtpCorrect ? "✅ ТЕСТ 3 ПРОЙДЕН" : "❌ ТЕСТ 3 НЕ ПРОЙДЕН");
    }
} 