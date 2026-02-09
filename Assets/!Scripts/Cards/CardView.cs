using TMPro;
using UnityEngine;

public class CardView : MonoBehaviour
{
    private int id;
    [SerializeField] TextMeshProUGUI cardName;
    [SerializeField] TextMeshProUGUI cost;
    [SerializeField] TextMeshProUGUI power;

    public void Initialize(CardData data)
    {   
        id = data.id;
        cardName?.SetText(data.Name);
        cost?.SetText(data.Cost.ToString());
        power?.SetText(data.Power.ToString());
    }
}
