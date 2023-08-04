using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControls : MonoBehaviour
{
    public GameObject player;
    [SerializeField] private float horizontalOffSet;
    [SerializeField] private float verticalOffSet;
    [SerializeField] private float offSetSmoothing;
    [SerializeField] private float vertical;
    private Vector3 playerPosition;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        vertical = Input.GetAxisRaw("Vertical");

        playerPosition = new Vector3(player.transform.position.x, player.transform.position.y, transform.position.z);

        if(player.transform.localScale.x > 0f)
        {
            playerPosition = new Vector3(playerPosition.x + horizontalOffSet, playerPosition.y, playerPosition.z);
        }
        else
        {
            playerPosition = new Vector3(playerPosition.x - horizontalOffSet, playerPosition.y, playerPosition.z);
        }

        if (player.transform.localScale.y > 0f && Input.GetButtonDown("Jump"))
        {
            playerPosition = new Vector3(playerPosition.x, playerPosition.y + verticalOffSet, playerPosition.z);
        }
        else if(vertical > 0f)
        {
            playerPosition = new Vector3(playerPosition.x, playerPosition.y + verticalOffSet, playerPosition.z);
        }
        else if (vertical < 0f)
        {
            playerPosition = new Vector3(playerPosition.x, playerPosition.y - verticalOffSet, playerPosition.z);
        }
        else if(player.transform.localScale.y < 0f)
        {
            playerPosition = new Vector3(playerPosition.x, playerPosition.y - verticalOffSet, playerPosition.z);
        }

        transform.position = Vector3.Lerp(transform.position, playerPosition, offSetSmoothing * Time.deltaTime);
    }
}
