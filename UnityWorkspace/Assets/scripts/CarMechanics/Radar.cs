using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Radar : MonoBehaviour
{

    public LayerMask layerMask;
    public int rayCount = 36;
    public float maxDist = 100.0f;
    public float angleSpread = 20.0f;

    public List<float> Scan()
    {
        List<float> toReturn = new List<float>();

        for (int i = 0; i < rayCount; i++)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Quaternion.AngleAxis(angleSpread * i, Vector3.up) * Vector3.forward), out hit, maxDist, layerMask))
            {
                toReturn.Add(hit.distance/ maxDist);
            }
            else
            {
                toReturn.Add(1);
            }
        }

        return toReturn;
    }

    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < rayCount; i++)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Quaternion.AngleAxis(angleSpread * i, Vector3.up) * Vector3.forward), out hit, maxDist, layerMask))
            {
                //Debug.Log("Did Hit");
                Debug.DrawRay(transform.position, transform.TransformDirection(Quaternion.AngleAxis(angleSpread * i, Vector3.up) * Vector3.forward) * hit.distance, Color.green);
            }
            else
            {
                //Debug.Log("Did not Hit");
                Debug.DrawRay(transform.position, transform.TransformDirection(Quaternion.AngleAxis(angleSpread * i, Vector3.up) * Vector3.forward) * maxDist, Color.white);
            }
        }
    }
}
