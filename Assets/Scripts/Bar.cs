using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Bar : MonoBehaviour
{
    public Image Progress;
    public TextMeshProUGUI Label;

    public float InitialWidth;

    public Gradient BarGradient;

    // Start is called before the first frame update
    void Awake()
    {
        InitialWidth = Progress.rectTransform.rect.width;
        print(InitialWidth);
    }

    public void UpdateProgress(float progress, string label = "")
    {
        Progress.rectTransform.sizeDelta = new Vector2(InitialWidth * progress, Progress.rectTransform.rect.height);
        Progress.color = BarGradient.Evaluate(progress);
        Label.text = label;
    }

}
