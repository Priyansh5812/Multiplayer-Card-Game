using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardView : MonoBehaviour , IDisposable , IPointerClickHandler
{
    public int id
    {
        get; private set;
    }
    [SerializeField] TextMeshProUGUI cardName;
    [SerializeField] TextMeshProUGUI cost;
    [SerializeField] TextMeshProUGUI power;
    
    public bool IsSelected
    {
        get => m_IsSelected;
        set 
        {
            SetSelectionView(value);
        }
    }

    private bool m_IsSelected;

    public void Initialize(CardData data)
    {   
        id = data.id;
        cardName?.SetText(data.Name);
        cost?.SetText(data.Cost.ToString());
        power?.SetText(data.Power.ToString());
    }

    public void Dispose()
    {
        EventManager.SetCard.Invoke(this);
    }

    private void SetSelectionView(bool value)
    { 
        this.transform.localScale = value ? Vector3.one * 1.25f : Vector3.one;
        m_IsSelected = value;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        EventManager.SetCardSelection.Invoke(this);
    }
}
