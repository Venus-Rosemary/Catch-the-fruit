using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Player_Move();
    }
    private void Player_Move()
    {
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical).normalized;
        transform.position += movement * Time.deltaTime * moveSpeed;

        //float clampedX = Mathf.Clamp(transform.position.x, -boundaryX, boundaryX);
        //float clampedZ = Mathf.Clamp(transform.position.z, -boundaryZ, -4f);
        //transform.position = new Vector3(clampedX, transform.position.y, clampedZ);
    }
}
