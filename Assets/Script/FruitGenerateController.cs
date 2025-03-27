using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitGenerateController : MonoBehaviour
{
    [Header("生成设置")]
    public List<GameObject> fruitPrefab;
    public GameObject stonePrefab;
    public float spawnRadius = 6f;
    public float minSpeed = 0.5f;
    public float maxSpeed = 2f;
    public float stoneChance = 0.1f; // 10%的概率生成石头
    public float minDistanceBetweenObjects = 1.5f; // 对象之间的最小距离

    private List<Vector3> activeObjectPositions = new List<Vector3>(); // 存储活跃对象的位置

    [Header("关卡设置")]
    public bool isNormalPass=false;
    public bool isHardPass = false;

    [Header("AI机器人设置")]
    public GameObject aiBotPrefab; // AI机器人预制体
    public float aiBotSpeed = 3.5f; // AI机器人移动速度
    private GameObject aiBot; // AI机器人实例
    
    void Start()
    {

    }

    public void StartGame()
    {
        InvokeRepeating("SpawnObject", 1f, 3f);

        if (isNormalPass)
        {
            CreatAiBot();
            if (isHardPass)
            {
                InvokeRepeating("SetHaedPass", 5f, Random.Range(30f,40f));
            }
        }

    }

    #region 初始化设置
    public void InitializationSettings()
    {
        isNormalPass = false;
        isHardPass = false;
        CancelInvoke("SpawnObject");//关闭循环调用
        CancelInvoke("SpawnAIBot");
        CancelInvoke("SetHaedPass");
        GameObject[] activeFruits = GameObject.FindGameObjectsWithTag("Fruit");
        GameObject[] activeStones = GameObject.FindGameObjectsWithTag("Stone");
        foreach (GameObject fruit in activeFruits)
        {
            Destroy(fruit);
        }
        foreach (GameObject stone in activeStones)
        {
            Destroy(stone);
        }
        Destroy(aiBot);
    }
    #endregion

    void SetHaedPass()
    {
        ScoreController.Instance.ActivateScoreChallenge();
    }

    #region 生成aiBot
    public void CreatAiBot()
    {
        Invoke("SpawnAIBot",10f);
    }

    void SpawnAIBot()
    {
        if (aiBotPrefab != null)
        {
            // 在随机位置生成AI机器人
            Vector3 spawnPos = GetRandomPositionInCircle(spawnRadius * 0.5f);
            spawnPos.y = 1f; // 设置适当的高度
            
            aiBot = Instantiate(aiBotPrefab, spawnPos, Quaternion.identity);
            
            // 添加AI控制脚本
            AIBotController botController = aiBot.GetComponent<AIBotController>();
            botController.moveSpeed = aiBotSpeed;
            botController.circleRadius = spawnRadius;
        }
    }
    #endregion

    #region 水果、石头生成
    void SpawnObject()
    {
        // 清理已经不在场景中的对象位置
        CleanupInactiveObjectPositions();
        
        // 随机选择生成水果或石头
        if (Random.value < stoneChance)
        {
            // 尝试找到一个不重叠的位置
            Vector3 spawnPos = FindNonOverlappingPosition();
            
            if (spawnPos != Vector3.zero) // 如果找到了有效位置
            {
                SpawnStone(spawnPos);
                
                // 添加新位置到列表
                activeObjectPositions.Add(spawnPos);
            }
        }
        if(true)
        {
            if (isNormalPass)
            {
                // 在普通关卡模式下，随机生成1-3个水果
                int fruitCount = Random.Range(1, 4); // 1到3个水果
                
                for (int i = 0; i < fruitCount; i++)
                {
                    // 为每个水果找一个不重叠的位置
                    Vector3 fruitPos = FindNonOverlappingPosition();
                    
                    if (fruitPos != Vector3.zero)
                    {
                        SpawnFruit(fruitPos);
                        
                        // 添加新位置到列表
                        activeObjectPositions.Add(fruitPos);
                    }
                }
            }
            else
            {
                // 在非普通关卡模式下，只生成一个水果
                Vector3 spawnPos = FindNonOverlappingPosition();
                
                if (spawnPos != Vector3.zero)
                {
                    SpawnFruit(spawnPos);
                    
                    // 添加新位置到列表
                    activeObjectPositions.Add(spawnPos);
                }
            }
        }
    }
    #endregion

    #region 检测生成重叠问题
    void CleanupInactiveObjectPositions()
    {
        // 查找场景中所有的水果和石头
        GameObject[] activeFruits = GameObject.FindGameObjectsWithTag("Fruit");
        GameObject[] activeStones = GameObject.FindGameObjectsWithTag("Stone");
        
        // 创建一个新的列表来存储当前场景中所有对象的位置
        List<Vector3> currentPositions = new List<Vector3>();
        
        // 添加所有水果的位置
        foreach (GameObject fruit in activeFruits)
        {
            currentPositions.Add(fruit.transform.position);
        }
        
        // 添加所有石头的位置
        foreach (GameObject stone in activeStones)
        {
            currentPositions.Add(stone.transform.position);
        }
        
        // 清空原有位置列表，使用当前场景中实际存在的对象位置
        activeObjectPositions.Clear();
        activeObjectPositions.AddRange(currentPositions);
    }
    
    // 寻找不与现有对象重叠的位置
    Vector3 FindNonOverlappingPosition()
    {
        const int maxAttempts = 30; // 最大尝试次数
        
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 candidatePos = GetRandomPositionInCircle(spawnRadius);
            bool isOverlapping = false;
            
            // 检查是否与任何现有对象重叠
            foreach (Vector3 existingPos in activeObjectPositions)
            {
                // 只比较X和Z坐标（水平面）
                Vector2 candidatePos2D = new Vector2(candidatePos.x, candidatePos.z);
                Vector2 existingPos2D = new Vector2(existingPos.x, existingPos.z);
                
                if (Vector2.Distance(candidatePos2D, existingPos2D) < minDistanceBetweenObjects)
                {
                    isOverlapping = true;
                    break;
                }
            }
            
            if (!isOverlapping)
            {
                return candidatePos; // 找到了不重叠的位置
            }
        }
        
        // 如果尝试多次后仍找不到合适位置，可以选择跳过这次生成
        Debug.Log("无法找到不重叠的位置，跳过本次生成");
        return Vector3.zero;
    }
    #endregion

    #region 圆形范围随机
    Vector3 GetRandomPositionInCircle(float radius)
    {
        // 随机生成角度（0到360度）
        float angle = Random.Range(0f, 360f);

        // 随机生成距离（0到半径）
        float distance = Random.Range(0f, radius);

        // 角度转直角坐标
        float x = Mathf.Cos(angle * Mathf.Deg2Rad) * distance;
        float z = Mathf.Sin(angle * Mathf.Deg2Rad) * distance;

        // 返回生成位置（Y轴固定高度）
        return new Vector3(x, 10f, z);
    }
    #endregion

    #region 水果生成和效果水果添加
    void SpawnFruit(Vector3 pos)
    {
        GameObject fruit = Instantiate(fruitPrefab[Random.Range(0,fruitPrefab.Count)], pos, Quaternion.identity);
        fruit.GetComponent<FruitSettings>().SetSpeed(
            Random.Range(minSpeed, maxSpeed)
        );
        // 检查是否应该隐藏新生成的水果
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null && playerController.ShouldHideFruits())
        {
            MeshRenderer renderer = fruit.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }
        if (isNormalPass&&Random.value<0.2f)
        {
            Outline outline=
                fruit.GetComponent<Outline>();
            outline.enabled = true;
            int a = Random.Range(0, 2);
            if (a==0)
            {
                outline.OutlineColor = Color.yellow;
            }
            else
            {
                outline.OutlineColor = Color.red;
            }
        }
    }
    #endregion

    #region 石头生成
    void SpawnStone(Vector3 pos)
    {
        GameObject stone = Instantiate(stonePrefab, pos, Quaternion.identity);
        stone.GetComponent<FruitSettings>().SetSpeed(
            Random.Range(minSpeed, maxSpeed)
        );
        // 检查是否应该隐藏新生成的水果
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null && playerController.ShouldHideFruits())
        {
            MeshRenderer renderer = stone.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }
    }
    #endregion
}
