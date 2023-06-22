using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamSim : MonoBehaviour
{
    public LayerMask roadWalls;
    public LayerMask road;
    public LayerMask wall;

    public float maxDist = 1000;

    public float iAngle = 20;
    public float jAngle = 20;
    [Space(10)]
    public Camera carCam;
    public int count_x = 65, count_y = 32;
    private RenderTexture camTexture;
    private Texture2D texture;
    Rect rectReadPicture;
    [Space(10)]
    public bool fullScan = false;

    private void Start()
    {
        camTexture = new RenderTexture(65, 32, 24);
        camTexture.filterMode = FilterMode.Point;
        carCam.targetTexture = camTexture;

        texture = new Texture2D(count_x, count_y, TextureFormat.RGB24, false);
        rectReadPicture = new Rect(0, 0, count_x, count_y);
    }

    public List<int> Scan()
    {
        List<int> toReturn = new List<int>();

        if (fullScan)
        {
            RenderTexture.active = camTexture;
            // Read pixels
            texture.ReadPixels(rectReadPicture, 0, 0);
            texture.Apply();

            for (int x = 0; x <= 65; x += 16)
            {
                for (int y = 16; y <= 24; y += 2)
                {
                    if(texture.GetPixel(x, y).r > texture.GetPixel(x, y).g)
                    {
                        toReturn.Add(1);
                        //Debug.Log(x + " " + y);
                    }
                    else
                    {
                        toReturn.Add(0);
                    }
                }
            }

            RenderTexture.active = null;
        }
        else
        {
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position, transform.TransformDirection(Quaternion.AngleAxis(iAngle * (i - 2), Vector3.up) * Quaternion.AngleAxis(jAngle * (j - 2), Vector3.left) * Vector3.forward), out hit, maxDist, roadWalls))
                    {
                    
                        if (hit.collider.gameObject.tag == "Road")
                        {
                            toReturn.Add(0);
                        }
                        else
                        {
                            toReturn.Add(1);
                        }
                    }
                    else
                    {
                        toReturn.Add(0);
                    }

                }
            }
        }

        return toReturn;
    }

    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < 5; i++)
        {
            for(int j = 0; j < 5; j++)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.TransformDirection(Quaternion.AngleAxis(iAngle * (i-2), Vector3.up) * Quaternion.AngleAxis(jAngle * (j-2), Vector3.left) * Vector3.forward), out hit, maxDist, roadWalls))
                {
                    //Debug.Log("Did Hit");
                    if(hit.collider.gameObject.tag == "Road")
                    {
                        Debug.DrawRay(transform.position, transform.TransformDirection(Quaternion.AngleAxis(iAngle * (i-2), Vector3.up) * Quaternion.AngleAxis(jAngle * (j-2), Vector3.left) * Vector3.forward) * hit.distance, Color.white);
                    }
                    else
                    {
                        Debug.DrawRay(transform.position, transform.TransformDirection(Quaternion.AngleAxis(iAngle * (i - 2), Vector3.up) * Quaternion.AngleAxis(jAngle * (j - 2), Vector3.left) * Vector3.forward) * hit.distance, Color.black);
                    }
                }
                else
                {
                    //Debug.Log("Did not Hit");
                    Debug.DrawRay(transform.position, transform.TransformDirection(Quaternion.AngleAxis(iAngle * (i-2), Vector3.up) * Quaternion.AngleAxis(jAngle * (j-2), Vector3.left) * Vector3.forward) * maxDist, Color.black);
                }

            }
        }
    }
}
