using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ArtilleryUI : MonoBehaviour
{
    public ArtilleryStrike artilleryStrike;
    
    [Header("UI Elements")]
    public TextMeshProUGUI strikeCountText;
    public Image strikeIcon;
    public GameObject instructionPanel;
    public TextMeshProUGUI instructionText;
    
    [Header("Colors")]
    public Color availableColor = Color.white;
    public Color unavailableColor = Color.gray;

    void Update()
    {
        if (artilleryStrike == null) return;
        
        int availableStrikes = artilleryStrike.GetAvailableStrikes();
        
        // Обновляем счётчик
        if (strikeCountText != null)
        {
            strikeCountText.text = $"x{availableStrikes}";
            strikeCountText.color = availableStrikes > 0 ? availableColor : unavailableColor;
        }
        
        // Обновляем иконку
        if (strikeIcon != null)
        {
            strikeIcon.color = availableStrikes > 0 ? availableColor : unavailableColor;
        }
        
        // Показываем инструкции при выборе
        if (instructionPanel != null)
        {
            instructionPanel.SetActive(artilleryStrike.isSelectingTarget);
        }
    }
}