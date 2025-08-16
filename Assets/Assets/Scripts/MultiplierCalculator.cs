using UnityEngine;

/// <summary>
/// Калькулятор роста мультипликатора для краш игры
/// Упрощенная формула с контрольными точками: x2 за 8с, x20 за 35с, x100 за 50с
/// </summary>
public static class MultiplierCalculator
{
    /// <summary>
    /// Вычисляет мультипликатор на основе времени игры
    /// </summary>
    /// <param name="timeInSeconds">Время с начала раунда в секундах</param>
    /// <returns>Мультипликатор от 1.00 до 1000.00</returns>
    public static float CalculateMultiplier(float timeInSeconds)
    {
        if (timeInSeconds <= 0) return 1.0f;
        
        // Упрощенная формула роста с контрольными точками
        if (timeInSeconds <= 8.0f)
        {
            // Линейный рост от x1.00 до x2.00 за 8 секунд
            return 1.0f + (timeInSeconds / 8.0f);
        }
        else if (timeInSeconds <= 35.0f)
        {
            // Квадратичный рост от x2.00 до x20.00 за 27 секунд
            float normalizedTime = (timeInSeconds - 8.0f) / 27.0f;
            float growthFactor = normalizedTime * normalizedTime;
            return 2.0f + 18.0f * growthFactor;
        }
        else if (timeInSeconds <= 50.0f)
        {
            // Кубический рост от x20.00 до x100.00 за 15 секунд
            float normalizedTime = (timeInSeconds - 35.0f) / 15.0f;
            float growthFactor = normalizedTime * normalizedTime * normalizedTime;
            return 20.0f + 80.0f * growthFactor;
        }
        else
        {
            // Экспоненциальный рост к x1000.00
            float excessTime = timeInSeconds - 50.0f;
            float growthFactor = 1.0f - Mathf.Exp(-excessTime / 30.0f);
            return 100.0f + 900.0f * growthFactor;
        }
    }
    
    /// <summary>
    /// Вычисляет время для достижения определенного мультипликатора
    /// </summary>
    /// <param name="targetMultiplier">Целевой мультипликатор</param>
    /// <returns>Время в секундах</returns>
    public static float CalculateTimeForMultiplier(float targetMultiplier)
    {
        if (targetMultiplier <= 1.0f) return 0f;
        if (targetMultiplier >= 1000.0f) return 120f; // Примерное время для x1000
        
        // Бинарный поиск для обратного вычисления
        float minTime = 0f;
        float maxTime = 120f;
        float epsilon = 0.01f;
        
        while (maxTime - minTime > epsilon)
        {
            float midTime = (minTime + maxTime) / 2f;
            float calculatedMultiplier = CalculateMultiplier(midTime);
            
            if (calculatedMultiplier < targetMultiplier)
                minTime = midTime;
            else
                maxTime = midTime;
        }
        
        return (minTime + maxTime) / 2f;
    }
    
    /// <summary>
    /// Тестирует формулу роста мультипликатора
    /// </summary>
    public static void TestMultiplierFormula()
    {
        Debug.Log("=== ТЕСТ ФОРМУЛЫ РОСТА МУЛЬТИПЛИКАТОРА ===");
        
        // Тестируем контрольные точки
        float[] testTimes = { 0f, 8f, 35f, 50f, 80f, 120f };
        float[] expectedValues = { 1.00f, 2.00f, 20.00f, 100.00f, 500.00f, 1000.00f };
        
        for (int i = 0; i < testTimes.Length; i++)
        {
            float calculated = CalculateMultiplier(testTimes[i]);
            float expected = expectedValues[i];
            float difference = Mathf.Abs(calculated - expected);
            float accuracy = (1f - difference / expected) * 100f;
            
            Debug.Log($"t={testTimes[i]:F1}s: Ожидается={expected:F2}, Вычислено={calculated:F2}, Точность={accuracy:F1}%");
            
            if (accuracy > 95f)
                Debug.Log("✅ ПРОЙДЕНО");
            else
 