using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBotController : MonoBehaviour
{
    public float moveSpeed = 3.5f;
    public float circleRadius = 5f;
    public float updateTargetInterval = 0.5f;
    public float playerAvoidanceRadius = 3f; // 玩家避开半径

    
    public GameObject AiBotHitVFX;
    public GameObject AiBotTriggerVFX;

    private GameObject currentTarget;
    private float targetUpdateTimer;
    private GameObject player;
    
    void Start()
    {
        targetUpdateTimer = updateTargetInterval;
        // 查找玩家对象
        player = GameObject.FindGameObjectWithTag("Player");
    }
    
    // 在类中添加新的变量
    private bool isStunned = false;
    private float stunnedTimer = 0f;
    
    void Update()
    {
        // 如果被击晕，倒计时
        if (isStunned)
        {
            stunnedTimer -= Time.deltaTime;
            if (stunnedTimer <= 0)
            {
                isStunned = false;
                if (AiBotHitVFX != null)
                {
                    AiBotHitVFX.SetActive(false);
                }
            }
            return; // 被击晕时不执行其他逻辑
        }
        
        targetUpdateTimer -= Time.deltaTime;
        
        // 定期更新目标
        if (targetUpdateTimer <= 0 || currentTarget == null)
        {
            // 优先寻找能量球，如果没有再找水果
            if (!FindEnergyBall())
            {
                FindBestFruit();
            }
            targetUpdateTimer = updateTargetInterval;
        }
        // 如果当前目标附近有玩家，立即更换目标
        if (IsPlayerNearTarget())
        {
            // 优先寻找能量球，如果没有再找水果
            if (!FindEnergyBall())
            {
                FindBestFruit();
            }
        }
        
        // 如果有目标，向目标移动
        if (currentTarget != null)
        {
            MoveTowardsTarget();
        }
        
        // 确保AI机器人不会超出圆形区域
        ConstrainToCircle();
    }
    
    // 寻找场景中的能量球
    bool FindEnergyBall()
    {
        GameObject[] energyBalls = GameObject.FindGameObjectsWithTag("EnergyBall");
        
        if (energyBalls.Length > 0)
        {
            // 找到最近的能量球
            float closestDistance = float.MaxValue;
            GameObject closestEnergyBall = null;
            
            foreach (GameObject energyBall in energyBalls)
            {
                float distance = Vector3.Distance(transform.position, energyBall.transform.position);
                
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnergyBall = energyBall;
                }
            }
            
            // 设置能量球为当前目标
            if (closestEnergyBall != null)
            {
                currentTarget = closestEnergyBall;
                return true;
            }
        }
        
        return false;
    }
    
    // 添加被击晕的方法
    public void GetStunned(float duration)
    {
        isStunned = true;
        stunnedTimer = duration;
        if (AiBotHitVFX != null)
        {
            AiBotHitVFX.SetActive(true);
        }
    }
    
    // 碰撞检测
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) return;
        
        if (isStunned) return; // 被击晕时不处理碰撞

        AiBotTriggerVFX.SetActive(false);

        if (other.CompareTag("Fruit"))
        {

            FruitSettings fruitSettings = other.GetComponent<FruitSettings>();
            if (fruitSettings != null && !fruitSettings.touchTheGround)
            {
                ScoreController.Instance.AiScore(other.GetComponent<FruitSettings>().score);
                Destroy(other.gameObject);
                AiBotTriggerVFX.SetActive(true);
                // 立即寻找新目标
                currentTarget = null;
                targetUpdateTimer = 0;
            }
        }
        // 添加能量球交互
        else if (other.CompareTag("EnergyBall"))
        {
            // AI捡起能量球，使玩家受到影响
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                player.GetStunned(3f);
            }
            AiBotTriggerVFX.SetActive(true);
            Destroy(other.gameObject);
        }
    }
    
    // 检查玩家是否在目标附近
    bool IsPlayerNearTarget()
    {
        if (player == null || currentTarget == null)
            return false;
            
        float distancePlayerToTarget = Vector3.Distance(
            player.transform.position, 
            currentTarget.transform.position
        );
        
        return distancePlayerToTarget < playerAvoidanceRadius;
    }
    
    // 寻找最佳的水果（考虑玩家位置）
    void FindBestFruit()
    {
        GameObject[] fruits = GameObject.FindGameObjectsWithTag("Fruit");
        
        List<GameObject> validFruits = new List<GameObject>();
        
        // 首先筛选出有效的水果
        foreach (GameObject fruit in fruits)
        {
            FruitSettings fruitSettings = fruit.GetComponent<FruitSettings>();
            if (fruitSettings != null && !fruitSettings.touchTheGround)
            {
                validFruits.Add(fruit);
            }
        }
        
        if (validFruits.Count == 0)
        {
            currentTarget = null;
            return;
        }
        
        // 尝试找到玩家不在附近的水果
        List<GameObject> fruitsWithoutPlayer = new List<GameObject>();
        
        if (player != null)
        {
            foreach (GameObject fruit in validFruits)
            {
                Vector3 xzFruit = new Vector3(fruit.transform.position.x, player.transform.position.y, fruit.transform.position.z);
                float distancePlayerToFruit = Vector3.Distance(
                    player.transform.position,
                    xzFruit
                );
                
                if (distancePlayerToFruit > playerAvoidanceRadius)
                {
                    fruitsWithoutPlayer.Add(fruit);
                }
            }
        }
        
        // 如果有玩家不在附近的水果，从中选择最近的
        if (fruitsWithoutPlayer.Count > 0)
        {
            currentTarget = FindNearestFruitFromList(fruitsWithoutPlayer);
        }
        else if (fruitsWithoutPlayer.Count == 0)
        {
            currentTarget = null;
        }
        // 否则从所有有效水果中选择最近的
        //else
        //{
        //    currentTarget = FindNearestFruitFromList(validFruits);
        //}
    }
    
    // 从水果列表中找到最近的水果
    GameObject FindNearestFruitFromList(List<GameObject> fruits)
    {
        float closestDistance = float.MaxValue;
        GameObject closestFruit = null;
        
        foreach (GameObject fruit in fruits)
        {
            float distance = Vector3.Distance(transform.position, fruit.transform.position);
            
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestFruit = fruit;
            }
        }
        
        return closestFruit;
    }
    
    // 向目标移动
    void MoveTowardsTarget()
    {
        // 计算水平方向的移动
        Vector3 targetPosition = new Vector3(
            currentTarget.transform.position.x,
            transform.position.y,
            currentTarget.transform.position.z
        );
        
        // 计算方向并移动
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
        
        // 让AI机器人朝向移动方向
        if (direction != Vector3.zero)
        {
            transform.forward = direction;
        }
    }
    
    // 限制AI机器人在圆形区域内
    void ConstrainToCircle()
    {
        Vector3 pos = transform.position;
        pos.y = transform.position.y; // 保持原有高度
        
        float distance = new Vector2(pos.x, pos.z).magnitude;
        if (distance > circleRadius)
        {
            Vector2 normalized = new Vector2(pos.x, pos.z).normalized;
            pos.x = normalized.x * circleRadius;
            pos.z = normalized.y * circleRadius;
            transform.position = pos;
        }
    }
    
    
}