using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitSettings : MonoBehaviour
{
    [Header("��������")]
    public int score=0;

    [Header("ˮ����������")]
    public float speed = 1f;
    public float rotateSpeed = 180f;

    [Header("������Ч����")]
    public GameObject GroundVFX = null;
    private GameObject GroundObject;
    private Vector3 GroundCreatePos;
    public bool touchTheGround = false;
    // Start is called before the first frame update
    void Start()
    {
        GroundCreatePos = new Vector3(this.transform.position.x, 0, this.transform.position.z);
        SetGroundVFX();
    }

    // Update is called once per frame
    void Update()
    {
        if (GroundObject != null)
        {
            GroundObject.transform.position = GroundCreatePos;
        }
        FMovement();
    }

    public void SetSpeed(float speedM)
    {
        speed = speedM;
    }

    void FMovement()
    {
        if (!touchTheGround)
        {
            transform.position += Vector3.down * speed * Time.deltaTime;
        }
        else
        {
            StartCoroutine(UpspringSetting());
        }
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime, Space.World);
    }

    //��غ�Ĵ���
    IEnumerator UpspringSetting()
    {
        transform.position += Vector3.up * 1f * Time.deltaTime;
        gameObject.GetComponent<Collider>().enabled = false;
        //�������ϵ�һ�ξ������ʧ
        yield return new WaitForSeconds(0.3f);
        Destroy(gameObject);
    }
    void SetGroundVFX()
    {
        if (GroundVFX!=null)
        {
            GroundObject =
                Instantiate(GroundVFX);
            GroundObject.transform.position = GroundCreatePos;
            GroundObject.transform.SetParent(this.gameObject.transform);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("����");
        if (collision.gameObject.CompareTag("Ground"))
        {
            touchTheGround = true;
            Destroy(GroundObject);
        }
    }
}
