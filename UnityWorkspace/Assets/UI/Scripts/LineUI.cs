using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LineUI : MonoBehaviour
{
    [SerializeField]
    private Image sprite;
    [SerializeField]
    private TMPro.TextMeshProUGUI weight;

    public void SetColor(Color _color, float _weight)
    {
        sprite.color = _color;
        if(_weight > 0f)
        {
            weight.text = "+" + _weight.ToString("N3");
        }
        else if(_weight < 0f)
        {
            weight.text = _weight.ToString("N3");
        }
        else
        {
            weight.text = "";
        }
    }
}
