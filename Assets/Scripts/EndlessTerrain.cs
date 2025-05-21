using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    public MapGenerator mapGenerator;

    void Start()
    {
        mapGenerator = MapGenerator.mapGenerator;
        mapGenerator.CreatePartsMap();
    }
}
