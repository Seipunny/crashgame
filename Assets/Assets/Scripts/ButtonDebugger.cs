using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Отладочный скрипт для проверки состояния кнопок
/// </summary>
public class ButtonDebugger : MonoBehaviour
{
    [Header("Отладка")]
    public bool debugOnStart = true;
    public bool debugOnClick = true;
    
    private CrashGamePrototype prototype;
    
    private void Start()
    {
        prototype = FindObjectOfType<CrashGamePrototype>();
        
        if (debugOnStart)
        {
            DebugButtonStates();
        }
    }
    
    [ContextMenu("Проверить состояние кнопок")]
    public void DebugButtonStates()
    {
        Debug.Log("🔍 ОТЛАДКА СОСТОЯНИЯ КНОПОК");
        Debug.Log("=".PadRight(50, '='));
        
        if (prototype == null)
        {
            Debug.LogError("❌ CrashGamePrototype не найден!");
            return;
        }
        
        // Проверяем кнопки
        CheckButton("PlaceBetButton", prototype.placeBetButton);
        CheckButton("CashoutButton", prototype.cashoutButton);
        CheckButton("IncreaseBetButton", prototype.increaseBetButton);
        CheckButton("DecreaseBetButton", prototype.decreaseBetButton);
        
        // Проверяем тексты
        CheckText("MultiplierText", prototype.multiplierText);
        CheckText("BalanceText", prototype.balanceText);
        CheckText("BetText", prototype.betText);
        CheckText("StatusText", prototype.statusText);
        CheckText("HistoryText", prototype.historyText);
        
        Debug.Log("=".PadRight(50, '='));
    }
    
    private void CheckButton(string name, Button button)
    {
        if (button == null)
        {
            Debug.LogError($"❌ {name}: НЕ НАЙДЕН");
            return;
        }
        
        Debug.Log($"✅ {name}: найден");
        Debug.Log($"   - Interactable: {button.interactable}");
        Debug.Log($"   - Listeners count: {button.onClick.GetPersistentEventCount()}");
        
        if (debugOnClick)
        {
            // Добавляем тестовый обработчик
            button.onClick.AddListener(() => Debug.Log($"🎯 {name} нажата!"));
        }
    }
    
    private void CheckText(string name, TMPro.TextMeshProUGUI text)
    {
        if (text == null)
        {
            Debug.LogError($"❌ {name}: НЕ НАЙДЕН");
            return;
        }
        
        Debug.Log($"✅ {name}: найден");
        Debug.Log($"   - Text: {text.text}");
        Debug.Log($"   - Enabled: {text.enabled}");
    }
    
    [ContextMenu("Перезапустить UI")]
    public void RestartUI()
    {
        Debug.Log("🔄 ПЕРЕЗАПУСК UI");
        
        if (prototype != null)
        {
            prototype.SetupUI();
            prototype.UpdateUI();
            Debug.Log("✅ UI перезапущен");
        }
        else
        {
            Debug.LogError("❌ CrashGamePrototype не найден!");
        }
    }
} 