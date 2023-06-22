using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class LoadUI : MonoBehaviour
{
    public TMPro.TextMeshProUGUI fileName;
    public TMPro.TextMeshProUGUI input;
    public TMPro.TextMeshProUGUI output;
    public TMPro.TextMeshProUGUI dateTime;
    public TMPro.TextMeshProUGUI mode;

    public GameObject loadButton;
    public UnityEngine.UI.Image image;

    private UnitySharpNEAT.SaveFiles save;
    private UnitySharpNEAT.NeatUICustom neatUICustom;

    public void LoadData(UnitySharpNEAT.SaveFiles _save, bool loadable, UnitySharpNEAT.NeatUICustom _neatUICustom)
    {
        save = _save;
        neatUICustom = _neatUICustom;
        fileName.text = _save.fileName;
        input.text = _save.input.ToString();
        output.text = _save.output.ToString();
        dateTime.text = _save.dateTime.ToString();
        if(_save.hidden == 0)
        {
            mode.text = "Topology Augumentation";
        }
        else
        {
            mode.text = "Fully Connected (" + _save.hidden + ")";
        }
        if (loadable)
        {
            loadButton.SetActive(true);
            image.color = Color.green;
        }
        else
        {
            loadButton.SetActive(false);
            image.color = Color.red;
        }
    }

    public void DeleteMe()
    {
        neatUICustom.DeleteThis(save);
    }

    public void LoadMe()
    {
        neatUICustom.LoadThis(save);
    }
}
