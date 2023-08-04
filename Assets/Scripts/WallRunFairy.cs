using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunFairy : MonoBehaviour
{
    public GameObject player;
    private bool wallRunEnable;

    [SerializeField] private Rigidbody2D rb;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject == player)
        {
            this.gameObject.SetActive(false);
            wallRunEnable = true;
            PowerUps.wallRun = wallRunEnable;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
