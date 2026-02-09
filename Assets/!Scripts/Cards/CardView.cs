using System;
using TMPro;
using UnityEngine;

public class CardView : MonoBehaviour , IDisposable
{
    private int id;
    private CardFactory factory;
    [SerializeField] TextMeshProUGUI cardName;
    [SerializeField] TextMeshProUGUI cost;
    [SerializeField] TextMeshProUGUI power;


    public void Initialize(CardData data , CardFactory factory)
    {   
        id = data.id;
        cardName?.SetText(data.Name);
        cost?.SetText(data.Cost.ToString());
        power?.SetText(data.Power.ToString());
        this.factory = factory;
    }

    public void Dispose()
    {
        // Self Return
    }

}
