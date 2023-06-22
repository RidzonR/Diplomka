using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NeuronUI : MonoBehaviour
{
    [SerializeField]
    private Image sprite;


    public void SetColor(Color color)
    {
        sprite.color = color;
    }

}
