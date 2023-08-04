using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleJumpFairy : MonoBehaviour
{
    public GameObject player;
    private bool djEnable;

    [SerializeField] private Rigidbody2D rb;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject == player)
        {
            this.gameObject.SetActive(false);
            djEnable = true;
            PowerUps.doubleJump = djEnable;
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
