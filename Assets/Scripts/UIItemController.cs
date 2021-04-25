using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIItemController : MonoBehaviour
{
    public ResourceType type;
    ItemDetails resource;

    public Image icon;
    public TMPro.TextMeshProUGUI countText;
    public Button buyButton;

    // Start is called before the first frame update
    void Start()
    {
        ResourceManager.onItemChange.AddListener(OnResourceChanged);
        resource = ResourceManager.GetResourceOfType(type);
        OnResourceChanged(resource);
    }

    private void OnResourceChanged(ItemDetails r)
    {
        if (r.type == this.resource.type)
            this.resource = r;
        countText.text = this.resource.count.ToString();
        if (!this.resource.isBuyable)
            buyButton.gameObject.SetActive(false);
        else
        {
            if (ResourceManager.CanBuy(this.resource.type) != null)
                buyButton.interactable = true;
            else
                buyButton.interactable = false;
        }
    }

    public void Buy()
    {
        ResourceManager.Buy(resource.type);
    }

    private void OnDestroy()
    {
        ResourceManager.onItemChange.RemoveListener(OnResourceChanged);
    }

}
