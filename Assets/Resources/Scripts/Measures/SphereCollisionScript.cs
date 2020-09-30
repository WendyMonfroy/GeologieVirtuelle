using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereCollisionScript : MonoBehaviour
{
    private List<GameObject> terrainPartsColliding;

    // Start is called before the first frame update
    void Start()
    {
        terrainPartsColliding = new List<GameObject>();
    }

    void OnTriggerStay(Collider collider)
    {
        if (collider.gameObject.tag == "TerrainMeasure" && !terrainPartsColliding.Contains(collider.gameObject))
        {
            terrainPartsColliding.Add(collider.gameObject);
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (terrainPartsColliding.Contains(collider.gameObject))
        {
            terrainPartsColliding.Remove(collider.gameObject);
        }
    }

    // method to calculate the mean normal vector of the selected points on terrain
    public Vector3 CalculateMeanNormal()
    {
        Vector3 center = transform.position;
        Vector3[] vertices;

        Vector3 normalSum = Vector3.zero;
        int normalCount = 0;
        
        if (terrainPartsColliding.Count == 0)
        {
            EventManager.TriggerEvent("ErrorMessage", new EventParam("Impossible de calculer un plan moyen, assurez-vous de bien positionner l'élément de sélection sur le terrain."));
            return Vector3.one * -1;
        }

        for (int i = 0; i < terrainPartsColliding.Count; i++)
        {
            vertices = terrainPartsColliding[i].GetComponent<MeshFilter>().mesh.vertices;
            for (int j = 0; j < vertices.Length; j++)
            {
                // remplacer par les dimensions du collider sur x et z avec y une hauteur fixe
                if ((vertices[j].x < center.x + 1 && vertices[j].x > center.x - 1) && (vertices[j].y < center.y + 0.2 && vertices[j].y > center.y - 0.2) && (vertices[j].z < center.z + 1 && vertices[j].z > center.z - 1))
                {
                    normalSum += terrainPartsColliding[i].GetComponent<MeshFilter>().mesh.normals[j];
                    normalCount += 1;
                }
            }
        }
        if (normalCount == 0)
        {
            EventManager.TriggerEvent("ErrorMessage", new EventParam("Impossible de trouver des sommets dans la zone sélectionnée."));
            return Vector3.one * -1;
        }
        gameObject.GetComponent<BoxCollider>().enabled = false;
        return normalSum / normalCount;
    }
}
