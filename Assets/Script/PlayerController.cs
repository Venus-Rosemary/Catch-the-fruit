using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("玩家设置")]
    public float moveSpeed = 5f;
    public float circleRadius = 5f;
    public GameObject triggerObject;
    public GameObject VFXSlowDown;
    public GameObject PlayerHitVFX;
    public GameObject triggerVFX;
    public GameObject scoreTextPrefab; // 3D分数文本预制体
    public float scoreTextDuration = 1.5f; // 分数文本显示时间
    public float scoreTextFloatSpeed = 1.0f; // 分数文本上浮速度
    
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
                PlayerHitVFX.SetActive(false);
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
        PlayerHitVFX.SetActive(false);
        triggerVFX.SetActive(false);
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
        triggerVFX.SetActive(false);
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
            triggerVFX.SetActive(true);
            SoundManagement.Instance.PlayerNumberTwoSFX(0);

            // 获取水果分数
            int fruitScore = other.GetComponent<FruitSettings>().score;
            
            // 显示3D分数文本
            ShowScoreText(fruitScore);
            
            // 添加分数
            ScoreController.Instance.Score(fruitScore);
            
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Stone"))
        {
            triggerVFX.SetActive(true);
            SoundManagement.Instance.PlaySFX(1);
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
            triggerVFX.SetActive(true);
            Destroy(other.gameObject);
        }
    }
    
    // 添加被能量球击中的方法
    public void GetStunned(float duration)
    {
        isDisabled = true;
        disableTimer = duration;
        if (PlayerHitVFX != null)
        {
            PlayerHitVFX.SetActive(true);
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


    // 显示3D分数文本
    void ShowScoreText(int score)
    {
        if (scoreTextPrefab != null)
        {
            // 确定生成位置
            Vector3 spawnPosition;
            // 如果没有指定生成点，就在玩家头顶上方生成
            spawnPosition = transform.position + Vector3.up * 2f;

            // 实例化分数文本
            GameObject scoreTextObj = Instantiate(scoreTextPrefab, spawnPosition, Quaternion.identity);

            // 设置文本内容
            TMP_Text textMesh = scoreTextObj.GetComponentInChildren<TMP_Text>();
            if (textMesh != null)
            {
                textMesh.text = "+" + score.ToString();
            }

            // 启动协程控制文本动画和销毁
            StartCoroutine(AnimateScoreText(scoreTextObj));
        }
    }

    // 控制分数文本动画和销毁
    IEnumerator AnimateScoreText(GameObject scoreTextObj)
    {
        float timer = 0f;
        Vector3 startPosition = scoreTextObj.transform.position;
        Vector3 endPosition = startPosition + Vector3.up * 1.5f;

        // 淡入
        CanvasGroup canvasGroup = scoreTextObj.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            float fadeInTime = 0.2f;
            while (timer < fadeInTime)
            {
                canvasGroup.alpha = timer / fadeInTime;
                timer += Time.deltaTime;
                yield return null;
            }
            canvasGroup.alpha = 1f;
            timer = 0f;
        }

        // 上浮动画
        while (timer < scoreTextDuration)
        {
            float t = timer / scoreTextDuration;
            scoreTextObj.transform.position = Vector3.Lerp(startPosition, endPosition, t);

            // 如果有CanvasGroup组件，在结束前开始淡出
            if (canvasGroup != null && timer > scoreTextDuration * 0.7f)
            {
                float fadeOutProgress = (timer - scoreTextDuration * 0.7f) / (scoreTextDuration * 0.3f);
                canvasGroup.alpha = 1f - fadeOutProgress;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // 销毁分数文本对象
        Destroy(scoreTextObj);
    }
}



