using UnityEngine;
using System.Collections.Generic;
public class CardFactory : MonoBehaviour
{
    [SerializeField] CardView cardViewPrefab;
    [SerializeField, Range(1, 10)] int cardCacheCount;
    private readonly Queue<CardView> inactiveViews = new();
    int maxSize;
    private void OnEnable()
    { 
        AllocateCardCache();
        InitListeners();
    }

    private void InitListeners()
    {
        EventManager.GetCard.AddListener(GetCard);
    }

    private async void AllocateCardCache()
    {
        var cards = await InstantiateAsync(cardViewPrefab,cardCacheCount,this.transform,Vector3.zero , Quaternion.identity);
        
        foreach(var i in cards)
        {
            i.gameObject.SetActive(false);
            inactiveViews.Enqueue(i);
        }

        maxSize = Mathf.Max(maxSize, inactiveViews.Count);
    }


    private CardView GetCard(CardData data)
    {
        CardView view = null;

        if (inactiveViews.Count > 0)
            view = inactiveViews.Dequeue();
        else
            view = GetFreshCardView();

        view.gameObject.SetActive(true);
        view.Initialize(data, this);

        if (inactiveViews.Count <= maxSize / 4)
            AllocateCardCache();

        return view;
    }

    private CardView GetFreshCardView() => Instantiate(cardViewPrefab, Vector3.zero, Quaternion.identity, this.transform);

    private void DeInitListeners()
    {
        EventManager.GetCard.RemoveListener(GetCard);
    }

    private void OnDisable()
    {
        DeInitListeners();
    }
}
