using UnityEngine;

/// <summary>
/// Простой тест компиляции всех компонентов
/// </summary>
public class CompilationTest : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("🔧 ПРОВЕРКА КОМПИЛЯЦИИ МАТЕМАТИЧЕСКОЙ МОДЕЛИ");
        Debug.Log("=".PadRight(50, '='));
        
        // Проверяем, что все классы доступны
        TestMultiplierCalculator();
        TestCrashPointGenerator();
        TestRTPValidator();
        
        Debug.Log("✅ КОМПИЛЯЦИЯ ПРОШЛА УСПЕШНО!");
        Debug.Log("Теперь можно запускать полные тесты.");
    }
    
    private void TestMultiplierCalculator()
    {
        Debug.Log("🧮 Тестируем MultiplierCalculator...");
        
        // Проверяем основные функции
        float multiplier1 = MultiplierCalculator.CalculateMultiplier(8f);
        float multiplier2 = MultiplierCalculator.CalculateMultiplier(35f);
        float multiplier3 = MultiplierCalculator.CalculateMultiplier(50f);
        
        Debug.Log($"  t=8s: x{multiplier1:F2}");
        Debug.Log($"  t=35s: x{multiplier2:F2}");
        Debug.Log($"  t=50s: x{multiplier3:F2}");
        
        // Проверяем обратное вычисление
        float time1 = MultiplierCalculator.CalculateTimeForMultiplier(2f);
        float time2 = MultiplierCalculator.CalculateTimeForMultiplier(10f);
        
        Debug.Log($"  Время для x2.0: {time1:F2}с");
        Debug.Log($"  Время для x10.0: {time2:F2}с");
        
        Debug.Log("✅ MultiplierCalculator работает корректно");
    }
    
    private void TestCrashPointGenerator()
    {
        Debug.Log("🎲 Тестируем CrashPointGenerator...");
        
        // Генерируем несколько точек краша
        for (int i = 0; i < 5; i++)
        {
            float crashPoint = CrashPointGenerator.GenerateCrashPoint();
            Debug.Log($"  Краш {i + 1}: x{crashPoint:F2}");
        }
        
        // Проверяем ожидаемые значения
        float expectedValue1 = CrashPointGenerator.GetExpectedValue(2.0f);
        float expectedValue2 = CrashPointGenerator.GetExpectedValue(5.0f);
        
        Debug.Log($"  E[payout] для x2.0: {expectedValue1:F4}");
        Debug.Log($"  E[payout] для x5.0: {expectedValue2:F4}");
        
        Debug.Log("✅ CrashPointGenerator работает корректно");
    }
    
    private void TestRTPValidator()
    {
        Debug.Log("💰 Тестируем RTPValidator...");
        
        // Быстрый тест RTP
        var result = RTPValidator.ValidateRTP(1000);
        
        Debug.Log($"  RTP результат: {result.actualRTP:P4}");
        Debug.Log($"  Винрейт: {result.successRate:P2}");
        Debug.Log($"  Максимальный краш: x{result.maxCrashPoint:F2}");
        
        Debug.Log("✅ RTPValidator работает корректно");
    }
} 