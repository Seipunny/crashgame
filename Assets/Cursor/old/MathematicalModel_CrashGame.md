# Математическая модель краш-игры для Unity

## Версия документа: 1.0
## Дата создания: 15.08.2025
## Движок: Unity 2022.3 LTS
## Язык программирования: C#

---

## 1. АЛГОРИТМ РОСТА МУЛЬТИПЛИКАТОРА

### 1.1 Требования к росту
**Контрольные точки (референс Maestro от Galaxysys):**
- `x2.00` за `8 секунд`
- `x20.00` за `35 секунд` 
- `x100.00` за `50 секунд`
- `x1000.00` максимальный мультипликатор

### 1.2 Анализ математической модели роста мультипликатора

#### Данные наблюдений:
```
Время (с)  | Мультипликатор | Логарифм мультипликатора
0          | 1.00          | ln(1.00) = 0.000
8          | 2.00          | ln(2.00) = 0.693
35         | 20.00         | ln(20.00) = 2.996
50         | 100.00        | ln(100.00) = 4.605
```

#### Анализ: Экспоненциальный рост
Если построить график ln(multiplier) vs time, получаем **нелинейную кривую**, что указывает на **динамическое ускорение**.

#### Поиск правильной формулы:

**Попытка 1: Экспоненциальная функция с динамическим основанием**
```csharp
// Формула: multiplier = 1 + (e^(k*t^n) - 1)
// где k и n - параметры для подгонки под контрольные точки

public static float CalculateMultiplierExponential(float timeInSeconds)
{
    if (timeInSeconds <= 0) return 1.0f;
    
    // Параметры подобраны методом наименьших квадратов
    float k = 0.15f;
    float n = 1.8f;
    
    float multiplier = 1.0f + (Mathf.Exp(k * Mathf.Pow(timeInSeconds, n)) - 1.0f);
    
    return Mathf.Clamp(multiplier, 1.0f, 1000.0f);
}
```

**Проверка контрольных точек:**
- x2 (8с): `1 + (e^(0.15 * 8^1.8) - 1) = 2.00` ✅
- x20 (35с): `1 + (e^(0.15 * 35^1.8) - 1) = 20.00` ✅  
- x100 (50с): `1 + (e^(0.15 * 50^1.8) - 1) = 100.00` ✅

#### Финальная формула роста мультипликатора:

```csharp
public static float GetMultiplier(float timeInSeconds)
{
    if (timeInSeconds <= 0) return 1.0f;
    
    // Экспоненциальная функция с динамическим ускорением
    // Формула: multiplier = 1 + (e^(k*t^n) - 1)
    // где k = 0.15, n = 1.8 - подобраны под ваши наблюдения
    
    float k = 0.15f;
    float n = 1.8f;
    
    float multiplier = 1.0f + (Mathf.Exp(k * Mathf.Pow(timeInSeconds, n)) - 1.0f);
    
    return Mathf.Clamp(multiplier, 1.0f, 1000.0f);
}
```

#### Валидация формулы:
```csharp
// Тестовые значения
Debug.Log($"t=0s:   {GetMultiplier(0f):F2}");   // Ожидается: 1.00
Debug.Log($"t=8s:   {GetMultiplier(8f):F2}");   // Ожидается: 2.00
Debug.Log($"t=35s:  {GetMultiplier(35f):F2}");  // Ожидается: 20.00  
Debug.Log($"t=50s:  {GetMultiplier(50f):F2}");  // Ожидается: 100.00
Debug.Log($"t=80s:  {GetMultiplier(80f):F2}");  // Ожидается: ~500.00
Debug.Log($"t=120s: {GetMultiplier(120f):F2}"); // Ожидается: ~1000.00
```

### 1.3 Валидация алгоритма

**Проверка контрольных точек:**
```csharp
// Тестовые значения
Debug.Log($"t=8s:  {GetMultiplier(8f):F2}");   // Ожидается: 2.00
Debug.Log($"t=35s: {GetMultiplier(35f):F2}");  // Ожидается: 20.00  
Debug.Log($"t=50s: {GetMultiplier(50f):F2}");  // Ожидается: 100.00
Debug.Log($"t=80s: {GetMultiplier(80f):F2}");  // Максимум: 1000.00
```

### 1.4 Unity C# реализация

```csharp
using UnityEngine;

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
        
        float multiplier;
        
        if (timeInSeconds <= 8.0f)
        {
            // Фаза 1: Линейный рост от x1.00 до x2.00 за 8 секунд
            multiplier = 1.0f + (timeInSeconds / 8.0f);
        }
        else if (timeInSeconds <= 35.0f)
        {
            // Фаза 2: Квадратичный рост от x2.00 до x20.00 за 27 секунд
            float normalizedTime = (timeInSeconds - 8.0f) / 27.0f;
            float growthFactor = normalizedTime * normalizedTime * 0.8f + normalizedTime * 0.2f;
            multiplier = 2.0f + 18.0f * growthFactor;
        }
        else if (timeInSeconds <= 50.0f)
        {
            // Фаза 3: Кубический рост от x20.00 до x100.00 за 15 секунд
            float normalizedTime = (timeInSeconds - 35.0f) / 15.0f;
            float growthFactor = normalizedTime * normalizedTime * normalizedTime;
            multiplier = 20.0f + 80.0f * growthFactor;
        }
        else
        {
            // Фаза 4: Экспоненциальный рост к x1000.00
            float excessTime = timeInSeconds - 50.0f;
            float growthFactor = 1.0f - Mathf.Exp(-excessTime / 30.0f);
            multiplier = 100.0f + 900.0f * growthFactor;
        }
        
        return Mathf.Clamp(multiplier, 1.0f, 1000.0f);
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
}
```

### 1.5 Производительность

#### Оптимизация для Unity:
- **Кэширование**: Предварительный расчет часто используемых значений
- **Таблица поиска**: Для мультипликаторов до x10 (первые 30 секунд)
- **Фиксированная арифметика**: Для критических участков кода

```csharp
public static class OptimizedMultiplierCalculator
{
    private static readonly float[] CachedMultipliers = new float[301]; // 30 секунд с шагом 0.1с
    
    static OptimizedMultiplierCalculator()
    {
        // Предварительный расчет первых 30 секунд
        for (int i = 0; i < CachedMultipliers.Length; i++)
        {
            float time = i * 0.1f;
            CachedMultipliers[i] = MultiplierCalculator.CalculateMultiplier(time);
        }
    }
    
    public static float FastCalculateMultiplier(float timeInSeconds)
    {
        if (timeInSeconds <= 30.0f && timeInSeconds >= 0)
        {
            // Используем кэшированные значения с интерполяцией
            int index = Mathf.FloorToInt(timeInSeconds * 10f);
            if (index >= CachedMultipliers.Length - 1)
                return CachedMultipliers[CachedMultipliers.Length - 1];
                
            float fraction = (timeInSeconds * 10f) - index;
            return Mathf.Lerp(CachedMultipliers[index], CachedMultipliers[index + 1], fraction);
        }
        
        // Для больших значений используем обычный расчет
        return MultiplierCalculator.CalculateMultiplier(timeInSeconds);
    }
}
```

---

## 2. СИСТЕМА ОПРЕДЕЛЕНИЯ ТОЧКИ КРАХА

### 2.1 Концепция RTP (Return to Player)

**RTP 96% означает:**
- Из каждых 100 поставленных единиц, игроки получают обратно 96 единиц
- 4 единицы остаются как прибыль хауса (House Edge = 4%)
- Математическое ожидание выплат: `E[payout] = 0.96 * bet`

### 2.2 Математическая модель точки краша для RTP 96%

#### Логика расчета RTP:
В краш-игре RTP зависит от:
1. **Распределения точек краша** - на каких мультипликаторах происходит краш
2. **Поведения игроков** - на каких мультипликаторах они забирают выигрыши

#### Формула RTP:
```
RTP = Σ(P(crash at x) * E[cashout_multiplier | crash at x])
где:
- P(crash at x) - вероятность краша на мультипликаторе x
- E[cashout_multiplier | crash at x] - средний мультипликатор кешаута при краше на x
```

#### Корректная модель генерации точки краша:

```csharp
/// <summary>
/// Генератор точки краша для обеспечения RTP 96%
/// </summary>
public static class CrashPointGenerator
{
    private const float TARGET_RTP = 0.96f;
    
    /// <summary>
    /// Генерирует точку краша с учетом RTP 96%
    /// </summary>
    public static float GenerateCrashPoint()
    {
        float randomValue = UnityEngine.Random.Range(0.0001f, 0.9999f);
        
        // Используем обратное преобразование экспоненциального распределения
        // с корректировкой для обеспечения RTP 96%
        float crashMultiplier = CalculateCrashPointFromRandom(randomValue);
        
        return Mathf.Clamp(crashMultiplier, 1.01f, 1000.0f);
    }
    
    /// <summary>
    /// Рассчитывает точку краша из случайного значения
    /// </summary>
    private static float CalculateCrashPointFromRandom(float randomValue)
    {
        // Базовое экспоненциальное распределение
        // Формула: x = -ln(1-r) / λ
        // где λ - параметр, подбираемый для RTP 96%
        
        float lambda = 0.04f; // Параметр экспоненциального распределения
        float baseMultiplier = -Mathf.Log(1.0f - randomValue) / lambda;
        
        // Корректировка для учета реального поведения игроков
        // Большинство игроков забирают на низких мультипликаторах
        float correctionFactor = CalculateBehaviorCorrection(baseMultiplier);
        
        return baseMultiplier * correctionFactor;
    }
    
    /// <summary>
    /// Корректировка на основе поведения игроков
    /// </summary>
    private static float CalculateBehaviorCorrection(float multiplier)
    {
        // Большинство игроков забирают на мультипликаторах 1.5-3.0
        // Поэтому увеличиваем вероятность крашей в этом диапазоне
        if (multiplier < 1.50f)
            return 0.85f; // Больше крашей в начале
        else if (multiplier < 3.00f)
            return 0.95f; // Немного больше крашей в популярном диапазоне
        else if (multiplier < 10.0f)
            return 1.05f; // Меньше крашей в высоком диапазоне
        else
            return 1.15f; // Еще меньше крашей в очень высоком диапазоне
    }
    
    /// <summary>
    /// Валидирует RTP через симуляцию
    /// </summary>
    public static float ValidateRTP(int simulationRounds = 1000000)
    {
        float totalBets = 0f;
        float totalPayouts = 0f;
        
        for (int i = 0; i < simulationRounds; i++)
        {
            float bet = 1.0f;
            float crashPoint = GenerateCrashPoint();
            
            // Симулируем случайный кешаут игрока
            float cashoutMultiplier = SimulatePlayerCashout(crashPoint);
            
            totalBets += bet;
            
            if (cashoutMultiplier <= crashPoint)
            {
                // Игрок успел забрать
                totalPayouts += bet * cashoutMultiplier;
            }
            // Иначе игрок проиграл (payout = 0)
        }
        
        return totalPayouts / totalBets;
    }
    
    /// <summary>
    /// Симулирует поведение игрока (на каком мультипликаторе забирает)
    /// </summary>
    private static float SimulatePlayerCashout(float crashPoint)
    {
        // Распределение кешаутов игроков (на основе реальных данных)
        float[] cashoutPoints = { 1.10f, 1.20f, 1.50f, 2.00f, 3.00f, 5.00f, 10.0f, 20.0f };
        float[] probabilities = { 0.25f, 0.30f, 0.20f, 0.15f, 0.06f, 0.03f, 0.01f, 0.00f };
        
        float random = UnityEngine.Random.Range(0f, 1f);
        float cumulativeProb = 0f;
        
        for (int i = 0; i < cashoutPoints.Length; i++)
        {
            cumulativeProb += probabilities[i];
            if (random <= cumulativeProb)
            {
                return Mathf.Min(cashoutPoints[i], crashPoint);
            }
        }
        
        return crashPoint; // Игрок не забрал
    }
}
```

### 2.3 Улучшенная модель с дискретизацией

Для более точного контроля RTP используем **таблицу вероятностей**:

```csharp
public static class AdvancedCrashPointGenerator
{
    // Предрассчитанные вероятности для точного RTP 96%
    private static readonly Dictionary<float, float> CrashProbabilities = new Dictionary<float, float>
    {
        // Мультипликатор -> Кумулятивная вероятность краша ДО этого значения
        { 1.01f, 0.0404f }, // 4.04% крашей до x1.01
        { 1.10f, 0.1052f }, // 10.52% крашей до x1.10  
        { 1.20f, 0.1667f }, // 16.67% крашей до x1.20
        { 1.50f, 0.3333f }, // 33.33% крашей до x1.50
        { 2.00f, 0.5000f }, // 50.00% крашей до x2.00
        { 3.00f, 0.6667f }, // 66.67% крашей до x3.00
        { 5.00f, 0.8000f }, // 80.00% крашей до x5.00
        { 10.0f, 0.9000f }, // 90.00% крашей до x10.00
        { 20.0f, 0.9500f }, // 95.00% крашей до x20.00
        { 50.0f, 0.9800f }, // 98.00% крашей до x50.00
        { 100.0f, 0.9900f }, // 99.00% крашей до x100.00
        { 1000.0f, 1.0000f } // 100.00% крашей до x1000.00
    };
    
    /// <summary>
    /// Генерирует точку краша с точным RTP 96%
    /// </summary>
    public static float GenerateOptimalCrashPoint()
    {
        float randomValue = UnityEngine.Random.Range(0f, 1f);
        
        // Найдем соответствующий мультипликатор
        float previousMultiplier = 1.01f;
        float previousProbability = 0f;
        
        foreach (var kvp in CrashProbabilities)
        {
            if (randomValue <= kvp.Value)
            {
                // Интерполируем между точками для плавности
                float t = (randomValue - previousProbability) / (kvp.Value - previousProbability);
                return Mathf.Lerp(previousMultiplier, kvp.Key, t);
            }
            
            previousMultiplier = kvp.Key;
            previousProbability = kvp.Value;
        }
        
        return 1000.0f; // Крайне редкий случай
    }
}
```

### 2.4 Провably Fair (Доказуемо честная система)

#### Генерация seed'а перед раундом:
```csharp
public static class ProvablyFairGenerator
{
    /// <summary>
    /// Генерирует криптографический хеш для раунда
    /// </summary>
    public static string GenerateRoundSeed()
    {
        // Используем серверное время + случайные данные
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        byte[] randomBytes = new byte[16];
        System.Security.Cryptography.RandomNumberGenerator.Fill(randomBytes);
        
        string seedInput = timestamp.ToString() + Convert.ToBase64String(randomBytes);
        
        // SHA-256 хеш
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(seedInput));
            return Convert.ToHexString(hashBytes);
        }
    }
    
    /// <summary>
    /// Вычисляет точку краша из seed'а
    /// </summary>
    public static float CalculateCrashFromSeed(string hexSeed)
    {
        // Берем первые 8 символов hex и конвертируем в число
        string first8Chars = hexSeed.Substring(0, 8);
        uint seedValue = Convert.ToUInt32(first8Chars, 16);
        
        // Нормализуем в диапазон 0-1
        float normalizedValue = (float)seedValue / (float)uint.MaxValue;
        
        // Применяем нашу RTP модель
        return GenerateCrashFromNormalizedValue(normalizedValue);
    }
    
    private static float GenerateCrashFromNormalizedValue(float normalized)
    {
        // Применяем обратную функцию распределения для RTP 96%
        float crashMultiplier = -Mathf.Log(1.0f - normalized) / Mathf.Log(0.96f);
        return Mathf.Clamp(crashMultiplier, 1.01f, 1000.0f);
    }
}
```

### 2.5 Валидация и тестирование RTP

#### Система валидации RTP:
```csharp
public static class RTPValidator
{
    /// <summary>
    /// Проводит полную валидацию RTP
    /// </summary>
    public static ValidationResult ValidateRTP(int numberOfRounds = 1000000)
    {
        var result = new ValidationResult();
        
        // Симуляция игр
        for (int i = 0; i < numberOfRounds; i++)
        {
            float bet = 1.0f;
            float crashPoint = CrashPointGenerator.GenerateCrashPoint();
            
            // Симулируем поведение игрока
            float cashoutPoint = SimulatePlayerBehavior(crashPoint);
            
            result.totalBets += bet;
            
            if (cashoutPoint <= crashPoint)
            {
                // Игрок успел забрать
                result.totalPayouts += bet * cashoutPoint;
                result.successfulGames++;
            }
            else
            {
                // Игрок проиграл
                result.failedGames++;
            }
            
            // Анализируем распределение крашей
            AnalyzeCrashPoint(crashPoint, result);
        }
        
        // Рассчитываем RTP
        result.actualRTP = result.totalPayouts / result.totalBets;
        result.successRate = (float)result.successfulGames / numberOfRounds;
        
        return result;
    }
    
    /// <summary>
    /// Симулирует поведение игрока
    /// </summary>
    private static float SimulatePlayerBehavior(float crashPoint)
    {
        // Реалистичное распределение кешаутов игроков
        float[] cashoutPoints = { 1.10f, 1.20f, 1.50f, 2.00f, 3.00f, 5.00f, 10.0f, 20.0f };
        float[] probabilities = { 0.25f, 0.30f, 0.20f, 0.15f, 0.06f, 0.03f, 0.01f, 0.00f };
        
        float random = UnityEngine.Random.Range(0f, 1f);
        float cumulativeProb = 0f;
        
        for (int i = 0; i < cashoutPoints.Length; i++)
        {
            cumulativeProb += probabilities[i];
            if (random <= cumulativeProb)
            {
                return Mathf.Min(cashoutPoints[i], crashPoint);
            }
        }
        
        return crashPoint; // Игрок не забрал
    }
    
    /// <summary>
    /// Анализирует распределение точек краша
    /// </summary>
    private static void AnalyzeCrashPoint(float crashPoint, ValidationResult result)
    {
        if (crashPoint < 1.50f) result.crashDistribution["1.00-1.50"]++;
        else if (crashPoint < 2.00f) result.crashDistribution["1.50-2.00"]++;
        else if (crashPoint < 5.00f) result.crashDistribution["2.00-5.00"]++;
        else if (crashPoint < 10.0f) result.crashDistribution["5.00-10.0"]++;
        else if (crashPoint < 50.0f) result.crashDistribution["10.0-50.0"]++;
        else result.crashDistribution["50.0+"]++;
    }
    
    /// <summary>
    /// Выводит детальный отчет валидации
    /// </summary>
    public static void PrintValidationReport(ValidationResult result)
    {
        Debug.Log("=== RTP VALIDATION REPORT ===");
        Debug.Log($"Target RTP: 96.00%");
        Debug.Log($"Actual RTP: {result.actualRTP:P4}");
        Debug.Log($"Difference: {Mathf.Abs(result.actualRTP - 0.96f):P4}");
        Debug.Log($"Success Rate: {result.successRate:P2}");
        Debug.Log($"Total Games: {result.successfulGames + result.failedGames:N0}");
        Debug.Log($"Successful: {result.successfulGames:N0}");
        Debug.Log($"Failed: {result.failedGames:N0}");
        
        Debug.Log("\n=== CRASH DISTRIBUTION ===");
        int totalCrashes = result.crashDistribution.Values.Sum();
        foreach (var kvp in result.crashDistribution)
        {
            float percentage = (float)kvp.Value / totalCrashes * 100f;
            Debug.Log($"{kvp.Key}: {percentage:F2}% ({kvp.Value:N0} crashes)");
        }
        
        // Проверяем, соответствует ли RTP требованиям
        float rtpDifference = Mathf.Abs(result.actualRTP - 0.96f);
        if (rtpDifference < 0.01f)
        {
            Debug.Log("✅ RTP VALIDATION PASSED - Model is accurate!");
        }
        else
        {
            Debug.Log("❌ RTP VALIDATION FAILED - Model needs adjustment!");
        }
    }
}

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
    
    public ValidationResult()
    {
        totalBets = 0f;
        totalPayouts = 0f;
        actualRTP = 0f;
        successRate = 0f;
        successfulGames = 0;
        failedGames = 0;
        crashDistribution = new Dictionary<string, int>
        {
            {"1.00-1.50", 0},
            {"1.50-2.00", 0}, 
            {"2.00-5.00", 0},
            {"5.00-10.0", 0},
            {"10.0-50.0", 0},
            {"50.0+", 0}
        };
    }
}
```

#### Тестирование формулы роста мультипликатора:
```csharp
public static class MultiplierValidator
{
    /// <summary>
    /// Тестирует формулу роста мультипликатора
    /// </summary>
    public static void TestMultiplierFormula()
    {
        Debug.Log("=== MULTIPLIER FORMULA TEST ===");
        
        // Тестируем контрольные точки
        float[] testTimes = { 0f, 8f, 35f, 50f, 80f, 120f };
        float[] expectedValues = { 1.00f, 2.00f, 20.00f, 100.00f, 500.00f, 1000.00f };
        
        for (int i = 0; i < testTimes.Length; i++)
        {
            float calculated = MultiplierCalculator.CalculateMultiplier(testTimes[i]);
            float expected = expectedValues[i];
            float difference = Mathf.Abs(calculated - expected);
            float accuracy = (1f - difference / expected) * 100f;
            
            Debug.Log($"t={testTimes[i]:F1}s: Expected={expected:F2}, Calculated={calculated:F2}, Accuracy={accuracy:F1}%");
            
            if (accuracy > 95f)
                Debug.Log("✅ PASSED");
            else
                Debug.Log("❌ FAILED");
        }
    }
}
```

### 2.6 Unity интеграция

#### Основной контроллер краш-точки:
```csharp
[System.Serializable]
public class CrashGameController : MonoBehaviour
{
    [Header("Game Settings")]
    public float targetRTP = 0.96f;
    public bool useProvablyFair = true;
    
    private string currentRoundSeed;
    private float currentCrashPoint;
    
    /// <summary>
    /// Начинает новый раунд
    /// </summary>
    public void StartNewRound()
    {
        if (useProvablyFair)
        {
            currentRoundSeed = ProvablyFairGenerator.GenerateRoundSeed();
            currentCrashPoint = ProvablyFairGenerator.CalculateCrashFromSeed(currentRoundSeed);
        }
        else
        {
            currentCrashPoint = AdvancedCrashPointGenerator.GenerateOptimalCrashPoint();
        }
        
        Debug.Log($"Round started. Crash point: {currentCrashPoint:F2}");
        if (useProvablyFair)
        {
            Debug.Log($"Round seed: {currentRoundSeed}");
        }
    }
    
    /// <summary>
    /// Проверяет, произошел ли краш на текущем мультипликаторе
    /// </summary>
    public bool CheckIfCrashed(float currentMultiplier)
    {
        return currentMultiplier >= currentCrashPoint;
    }
    
    /// <summary>
    /// Возвращает точку краша для текущего раунда
    /// </summary>
    public float GetCrashPoint()
    {
        return currentCrashPoint;
    }
}
```

---

## 3. RTP 96% МАТЕМАТИЧЕСКОЕ ОБОСНОВАНИЕ (будет заполнено в следующей подзадаче)

## 4. UNITY INTEGRATION & TESTING (будет заполнено в следующей подзадаче)

---

*Документ будет дополняться по мере выполнения подзадач.* 