using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlideFairy : MonoBehaviour
{
    public GameObject player;
    private bool glideEnable;

    [SerializeField] private Rigidbody2D rb;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject == player)
        {
            this.gameObject.SetActive(false);
            glideEnable = true;
            PowerUps.glidePower = glideEnable;
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
