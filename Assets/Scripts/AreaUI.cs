using UnityEngine;
using TMPro;

public class AreaUI : MonoBehaviour
{
    public TextMeshProUGUI AreaTxt;

    public void UpdateArea(string area)
    {
        AreaTxt.text = area;
    }
}
