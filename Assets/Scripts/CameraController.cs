using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    GameObject player;
    public CinemachineCamera vCam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        player = GameObject.FindWithTag("Player");

        if (player != null )
        {
            vCam.Follow = player.transform;
            vCam.LookAt = player.transform;
        }
    }
}
