using UnityEngine;

/// <summary>
/// Тестовый скрипт для проверки работы прототипа
/// </summary>
public class PrototypeTester : MonoBehaviour
{
    [Header("Тестирование")]
    public bool testOnStart = true;
    public bool autoTest = false;
    
    private CrashGamePrototype prototype;
    
    private void Start()
    {
        prototype = FindObjectOfType<CrashGamePrototype>();
        
        if (prototype == null)
        {
            Debug.LogError("❌ CrashGamePrototype не найден в сцене!");
            return;
        }
        
        if (testOnStart)
        {
            TestPrototype();
        }
    }
    
    [ContextMenu("Тест прототипа")]
    public void TestPrototype()
    {
        Debug.Log("🧪 ТЕСТИРОВАНИЕ ПРОТОТИПА");
        Debug.Log("=".PadRight(50, '='));
        
        if (prototype == null)
        {
            Debug.LogError("❌ CrashGamePrototype не найден!");
            return;
        }
        
        // Проверяем начальное состояние
        Debug.Log($"✅ Начальный баланс: {prototype.playerBalance}");
        Debug.Log($"✅ Текущая ставка: {prototype.currentBet}");
        Debug.Log($"✅ Игра запущена: {prototype.isGameRunning}");
        Debug.Log($"✅ Ставка размещена: {prototype.hasPlacedBet}");
        
        if (autoTest)
        {
            // Автоматический тест
            Invoke("RunAutoTest", 1f);
        }
        
        Debug.Log("✅ Тест прототипа завершен");
        Debug.Log("Теперь можно тестировать игру вручную!");
    }
    
    private void RunAutoTest()
    {
        Debug.Log("🤖 ЗАПУСК АВТОТЕСТА");
        
        // Размещаем ставку
        prototype.PlaceBet();
        
        // Ждем немного и делаем кешаут
        Invoke("AutoCashout", 3f);
    }
    
    private void AutoCashout()
    {
        if (prototype.isGameRunning && prototype.hasPlacedBet)
        {
            Debug.Log("🤖 Автоматический кешаут");
            prototype.Cashout();
        }
    }
    
    private void Update()
    {
        // Показываем текущее состояние в консоли
        if (prototype != null && prototype.isGameRunning)
        {
            if (Time.frameCount % 60 == 0) // Каждую секунду
            {
                Debug.Log($"🎮 Игра идет: x{prototype.currentMultiplier:F2} | Время: {prototype.gameTime:F1}s");
            }
        }
    }
} 