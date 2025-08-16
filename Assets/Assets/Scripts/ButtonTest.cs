using UnityEngine;
using UnityEngine.UI;

public class ButtonTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("🧪 ButtonTest запущен");
        
        // Находим все кнопки в сцене
        Button[] buttons = FindObjectsOfType<Button>();
        Debug.Log($"🧪 Найдено кнопок: {buttons.Length}");
        
        foreach (Button button in buttons)
        {
            Debug.Log($"🧪 Кнопка: {button.name}, активна: {button.interactable}");
            
            // Добавляем тестовый обработчик
            button.onClick.AddListener(() => {
                Debug.Log($"🧪 Кнопка {button.name} нажата!");
            });
        }
    }
} 