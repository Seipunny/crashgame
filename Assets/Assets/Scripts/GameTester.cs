using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Тестовый скрипт для проверки математической модели
/// </summary>
public class GameTester : MonoBehaviour
{
    [Header("Test UI")]
    public Button testMultiplierButton;
    public Button testCrashButton;
    public Button testRTPButton;
    public Button fullRTPTestButton;
    public TextMeshProUGUI testResultsText;
    
    private void Start()
    {
        SetupTestUI();
        RunInitialTests();
    }
    
    /// <summary>
    /// Настраивает тестовые кнопки
    /// </summary>
    private void SetupTestUI()
    {
        if (testMultiplierButton != null)
            testMultiplierButton.onClick.AddListener(TestMultiplierFormula);
        
        if (testCrashButton != null)
            testCrashButton.onClick.AddListener(TestCrashDistribution);
        
        if (testRTPButton != null)
            testRTPButton.onClick.AddListener(TestRTP);
        
        if (fullRTPTestButton != null)
            fullRTPTestButton.onClick.AddListener(FullRTPTest);
    }
    
    /// <summary>
    /// Запускает начальные тесты
    /// </summary>
    private void RunInitialTests()
    {
        Debug.Log("=== НАЧАЛЬНЫЕ ТЕСТЫ МАТЕМАТИЧЕСКОЙ МОДЕЛИ ===");
        
        // Тест формулы мультипликатора
        TestMultiplierFormula();
        
        // Тест распределения крашей
        TestCrashDistribution();
        
        // Быстрый тест RTP
        TestRTP();
        
        Debug.Log("=== НАЧАЛЬНЫЕ ТЕСТЫ ЗАВЕРШЕНЫ ===");
    }
    
    /// <summary>
    /// Тестирует формулу мультипликатора
    /// </summary>
    public void TestMultiplierFormula()
    {
        Debug.Log("🧮 Тестируем формулу мультипликатора...");
        MultiplierCalculator.TestMultiplierFormula();
        
        if (testResultsText != null)
        {
            testResultsText.text = "✅ Тест формулы мультипликатора завершен\n";
        }
    }
    
    /// <summary>
    /// Тестирует распределение крашей
    /// </summary>
    public void TestCrashDistribution()
    {
        Debug.Log("🎲 Тестируем распределение крашей...");
        CrashPointGenerator.TestCrashDistribution();
        
        if (testResultsText != null)
        {
            testResultsText.text += "✅ Тест распределения крашей завершен\n";
        }
    }
    
    /// <summary>
    /// Тестирует RTP
    /// </summary>
    public void TestRTP()
    {
        Debug.Log("💰 Тестируем RTP (10,000 раундов)...");
        var result = RTPValidator.ValidateRTP(10000);
        RTPValidator.PrintValidationReport(result);
        
        if (testResultsText != null)
        {
            testResultsText.text += $"✅ Тест RTP завершен: {result.actualRTP:P4}\n";
        }
    }
    
    /// <summary>
    /// Полный тест RTP
    /// </summary>
    public void FullRTPTest()
    {
        Debug.Log("🔬 Полный тест RTP (1,000,000 раундов)...");
        var result = RTPValidator.ValidateRTP(1000000);
        RTPValidator.PrintValidationReport(result);
        
        if (testResultsText != null)
        {
            testResultsText.text += $"✅ Полный тест RTP завершен: {result.actualRTP:P4}\n";
        }
    }
    
    /// <summary>
    /// Тестирует контрольные точки мультипликатора
    /// </summary>
    public void TestControlPoints()
    {
        Debug.Log("📊 Тестируем контрольные точки мультипликатора...");
        
        float[] testTimes = { 8f, 35f, 50f };
        float[] expectedValues = { 2.0f, 20.0f, 100.0f };
        
        for (int i = 0; i < testTimes.Length; i++)
        {
            float calculated = MultiplierCalculator.CalculateMultiplier(testTimes[i]);
            float expected = expectedValues[i];
            float difference = Mathf.Abs(calculated - expected);
            float accuracy = (1f - difference / expected) * 100f;
            
            Debug.Log($"t={testTimes[i]:F1}s: Ожидается={expected:F2}, Вычислено={calculated:F2}, Точность={accuracy:F1}%");
        }
    }
    
    /// <summary>
    /// Тестирует обратное вычисление времени
    /// </summary>
    public void TestReverseCalculation()
    {
        Debug.Log("🔄 Тестируем обратное вычисление времени...");
        
        float[] targetMultipliers = { 2.0f, 5.0f, 10.0f, 20.0f, 50.0f };
        
        foreach (float target in targetMultipliers)
        {
            float time = MultiplierCalculator.CalculateTimeForMultiplier(target);
            float calculatedMultiplier = MultiplierCalculator.CalculateMultiplier(time);
            float difference = Mathf.Abs(calculatedMultiplier - target);
            
            Debug.Log($"Цель: x{target:F1} | Время: {time:F2}с | Результат: x{calculatedMultiplier:F2} | Разница: {difference:F3}");
        }
    }
    
    /// <summary>
    /// Тестирует ожидаемые значения для разных мультипликаторов
    /// </summary>
    public void TestExpectedValues()
    {
        Debug.Log("📈 Тестируем ожидаемые значения...");
        
        float[] testMultipliers = { 1.5f, 2.0f, 3.0f, 5.0f, 10.0f, 20.0f };
        
        foreach (float multiplier in testMultipliers)
        {
            float expectedValue = CrashPointGenerator.GetExpectedValue(multiplier);
            Debug.Log($"Мультипликатор x{multiplier:F1}: E[payout] = {expectedValue:F4} (цель: 0.9600)");
        }
    }
    
    /// <summary>
    /// Запускает все тесты
    /// </summary>
    public void RunAllTests()
    {
        Debug.Log("🚀 Запуск всех тестов...");
        
        TestControlPoints();
        TestReverseCalculation();
        TestExpectedValues();
        TestMultiplierFormula();
        TestCrashDistribution();
        TestRTP();
        
        Debug.Log("✅ Все тесты завершены!");
    }
} 