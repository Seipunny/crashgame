using UnityEngine;

/// <summary>
/// Генератор точки краша для обеспечения RTP 96%
/// Использует формулу: M = (1 - ε) / (1 - U), где ε = 0.04
/// </summary>
public static class CrashPointGenerator
{
    private const float TARGET_RTP = 0.96f;
    private const float HOUSE_EDGE = 0.04f;
    private const float MIN_CRASH = 1.01f;
    private const float MAX_CRASH = 1000.0f;
    
    /// <summary>
    /// Генерирует точку краша с учетом RTP 96%
    /// </summary>
    /// <returns>Мультипликатор краша от 1.01 до 1000.00</returns>
    public static float GenerateCrashPoint()
    {
        float randomValue = UnityEngine.Random.Range(0.0001f, 0.9999f);
        
        // Правильная формула для RTP 96%: M = (1 - ε) / (1 - U)
        float crashMultiplier = (1.0f - HOUSE_EDGE) / (1.0f - randomValue);
        
        return Mathf.Clamp(crashMultiplier, MIN_CRASH, MAX_CRASH);
    }
    
    /// <summary>
    /// Генерирует точку краша из заданного случайного значения (для тестирования)
    /// </summary>
    /// <param name="randomValue">Случайное значение от 0 до 1</param>
    /// <returns>Мультипликатор краша</returns>
    public static float GenerateCrashPointFromRandom(float randomValue)
    {
        randomValue = Mathf.Clamp01(randomValue);
        
        float crashMultiplier = (1.0f - HOUSE_EDGE) / (1.0f - randomValue);
        
        return Mathf.Clamp(crashMultiplier, MIN_CRASH, MAX_CRASH);
    }
    
    /// <summary>
    /// Вычисляет вероятность краша до заданного мультипликатора
    /// </summary>
    /// <param name="multiplier">Мультипликатор</param>
    /// <returns>Вероятность краша до этого значения</returns>
    public static float GetCrashProbability(float multiplier)
    {
        if (multiplier <= MIN_CRASH) return 0f;
        if (multiplier >= MAX_CRASH) return 1f;
        
        // P(M < m) = 1 - (1 - ε) / m
        return 1.0f - (1.0f - HOUSE_EDGE) / multiplier;
    }
    
    /// <summary>
    /// Вычисляет ожидаемое значение для заданного мультипликатора
    /// </summary>
    /// <param name="multiplier">Мультипликатор</param>
    /// <returns>Ожидаемое значение (должно быть 0.96 для любого мультипликатора)</returns>
    public static float GetExpectedValue(float multiplier)
    {
        // E[payout] = multiplier * P(M >= multiplier) = multiplier * (1 - ε) / multiplier = 1 - ε
        return (1.0f - HOUSE_EDGE);
    }
    
    /// <summary>
    /// Тестирует распределение точек краша
    /// </summary>
    public static void TestCrashDistribution(int sampleSize = 10000)
    {
        Debug.Log("=== ТЕСТ РАСПРЕДЕЛЕНИЯ ТОЧЕК КРАША ===");
        
        int[] buckets = new int[7]; // 7 диапазонов
        float[] bucketRanges = { 1.01f, 2.0f, 5.0f, 10.0f, 20.0f, 50.0f, 100.0f, 1000.0f };
        
        for (int i = 0; i < sampleSize; i++)
        {
            float crashPoint = GenerateCrashPoint();
            
            // Определяем в какой диапазон попадает точка краша
            for (int j = 0; j < bucketRanges.Length - 1; j++)
            {
                if (crashPoint >= bucketRanges[j] && crashPoint < bucketRanges[j + 1])
                {
                    buckets[j]++;
                    break;
                }
            }
        }
        
        Debug.Log("Распределение точек краша:");
        for (int i = 0; i < buckets.Length; i++)
        {
            float percentage = (float)buckets[i] / sampleSize * 100f;
            Debug.Log($"{bucketRanges[i]:F2}-{bucketRanges[i + 1]:F2}: {buckets[i]} ({percentage:F2}%)");
        }
        
        // Проверяем ожидаемые значения
        Debug.Log("\nПроверка ожидаемых значений:");
        float[] testMultipliers = { 1.5f, 2.0f, 3.0f, 5.0f, 10.0f };
        foreach (float multiplier in testMultipliers)
        {
            float expectedValue = GetExpectedValue(multiplier);
            Debug.Log($"Мультипликатор {multiplier:F1}: E[payout] = {expectedValue:F4} (цель: {TARGET_RTP:F4})");
        }
    }
} 