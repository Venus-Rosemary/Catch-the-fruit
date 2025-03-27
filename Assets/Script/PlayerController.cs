using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("玩家设置")]
    public float moveSpeed = 5f;
    public float circleRadius = 5f;
    public GameObject triggerObject;
    public GameObject VFXSlowDown;
    public GameObject PlayerHitVFS;
    private bool isDisabled = false;//处于被石头减速状态
    private float disableTimer = 0f;
    private Vector3 originalScale; // 保存原始缩放
    private Vector3 origunalPos;
    private bool isScaleEnlarged = false; // 是否处于放大状态
    private float scaleTimer = 0f; // 放大效果计时器

    private bool isFruitsHidden = false; // 添加一个标志来跟踪水果是否处于隐藏状态


    void Start()
    {
        if (triggerObject != null)
        {
            originalScale = triggerObject.transform.localScale;
        }
        origunalPos = transform.position;
    }


    void Update()
    {
        if (isDisabled)
        {
            disableTimer -= Time.deltaTime;
            if (disableTimer <= 0) { 
                isDisabled = false;
                VFXSlowDown.SetActive(false);
                PlayerHitVFS.SetActive(false);
            }
            return;
        }
        
        if (isScaleEnlarged)
        {
            scaleTimer -= Time.deltaTime;
            if (scaleTimer <= 0)
            {
                // 恢复原始大小
                isScaleEnlarged = false;
                if (triggerObject != null)
                {
                    triggerObject.transform.localScale = originalScale;
                }
            }
        }
        
        Player_Move();
    }

    private void OnDisable()
    {
        PlayerInitializationSettings();
    }
    #region 初始化设置
    public void PlayerInitializationSettings()
    {
        isDisabled = false;
        disableTimer = 0;
        VFXSlowDown.SetActive(false);
        PlayerHitVFS.SetActive(false);
        triggerObject.transform.localScale = originalScale;
        isScaleEnlarged = false;
        scaleTimer = 0;
        isFruitsHidden = false;
        transform.position = origunalPos;
    }
    #endregion

    #region 移动
    public void Player_Move()
    {
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical).normalized;
        transform.position += movement * moveSpeed * Time.deltaTime ;

        Vector3 pos = transform.position;
        float distance = pos.magnitude;
        if (distance > circleRadius)
        {
            pos = pos.normalized * circleRadius;
            transform.position = new Vector3(pos.x, transform.position.y, pos.z);
        }

    }
    #endregion


    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<FruitSettings>()!=null)
        {
            if (other.GetComponent<FruitSettings>().touchTheGround) return;
        }
        //if (other.CompareTag("AiBot")) return;
        if (other.CompareTag("Fruit"))
        {
            // 检查是否有 Outline 组件并且已启用
            Outline outline = other.GetComponent<Outline>();
            if (outline != null && outline.enabled)
            {
                // 黄色轮廓 - 放大触发器
                if (outline.OutlineColor == Color.yellow)
                {
                    EnlargeTrigger(10f);
                }
                // 红色轮廓 - 隐藏所有水果
                else if (outline.OutlineColor == Color.red)
                {
                    StartCoroutine(HideAllFruits(10f));
                }
            }
            
            ScoreController.Instance.Score(other.GetComponent<FruitSettings>().score);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Stone"))
        {
            Destroy(other.gameObject);
            DisableControl(3f);
        }
        // 添加能量球交互
        else if (other.CompareTag("EnergyBall"))
        {
            // 玩家捡起能量球，使AI受到影响
            AIBotController[] aiBots = FindObjectsOfType<AIBotController>();
            foreach (AIBotController aiBot in aiBots)
            {
                aiBot.GetStunned(3f);
            }
            
            Destroy(other.gameObject);
        }
    }
    
    // 添加被能量球击中的方法
    public void GetStunned(float duration)
    {
        isDisabled = true;
        disableTimer = duration;
        if (PlayerHitVFS != null)
        {
            PlayerHitVFS.SetActive(true);
        }
    }

    #region 减速状态设置
    void DisableControl(float duration)
    {
        isDisabled = true;
        disableTimer = duration;
        VFXSlowDown.SetActive(true);
    }

    #endregion

    #region 放大触发器
    void EnlargeTrigger(float duration)
    {
        if (triggerObject != null)
        {
            triggerObject.transform.localScale = new Vector3(originalScale.x * 1.5f, triggerObject.transform.localScale.y,
                originalScale.z * 1.5f);
            isScaleEnlarged = true;
            scaleTimer = duration;
        }
    }
    #endregion

    #region 隐藏/激活所有mesh render(有协程)
    IEnumerator HideAllFruits(float duration)
    {
        isFruitsHidden = true;
        SetForbidden("Fruit");
        SetForbidden("Stone");

        // 等待指定时间
        yield return new WaitForSeconds(duration);

        SetActivate("Fruit");
        SetActivate("Stone");
        isFruitsHidden = false;
    }
    void SetForbidden(string TagName)
    {
        GameObject[] Targets = GameObject.FindGameObjectsWithTag(TagName);
        // 禁用所有的渲染器
        foreach (GameObject Target in Targets)
        {
            MeshRenderer renderer = Target.GetComponent<MeshRenderer>();
            if (renderer != null && renderer.enabled)
            {
                renderer.enabled = false;
            }
        }

    }
    void SetActivate(string TagName)
    {
        GameObject[] Targets = GameObject.FindGameObjectsWithTag(TagName);
        // 恢复所有的渲染器
        Targets = GameObject.FindGameObjectsWithTag(TagName);
        foreach (GameObject Target in Targets)
        {
            MeshRenderer renderer = Target.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.enabled = true;
            }
        }
    }

    
    public bool ShouldHideFruits()
    {
        return isFruitsHidden;
    }
    #endregion
}
