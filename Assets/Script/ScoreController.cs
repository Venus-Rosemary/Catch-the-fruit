using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreController : Singleton<ScoreController>
{
    [Header("分数设置")]
    public int playerTotalPoints = 0;
    public int aiTotalPoints = 0;
    public FruitGenerateController FGCon;
    public PlayerController PCon;

    [Header("计时器")]
    public TMP_Text timerText;
    public TMP_Text PTSText;
    public TMP_Text ATSText;
    public float currentTime = 60f;
    
    [Header("分值匹配挑战")]
    public GameObject rewardPrefab; // 奖励预制体
    public TMP_Text targetScoreText; // 显示目标分值的文本
    public TMP_Text challengeTimerText; // 显示挑战剩余时间的文本
    private bool isChallengeActive = false; // 挑战是否激活
    private int targetScore = 0; // 目标分值
    [SerializeField]
    private int challengeScore = 0; // 挑战期间获得的分数
    private float challengeTimer = 0f; // 挑战计时器
    
    void Start()
    {
        if (targetScoreText != null)
            targetScoreText.gameObject.SetActive(false);
        if (challengeTimerText != null)
            challengeTimerText.gameObject.SetActive(false);

        PTSText.text = "玩家总分: " + playerTotalPoints.ToString();
        ATSText.text = "玩家总分: " + aiTotalPoints.ToString();
    }

    void Update()
    {
        KeepTime();
        if (playerTotalPoints < aiTotalPoints)
        {
            FGCon.InitializationSettings();
            OtherInitializationSettings();
        }
        
        // 更新挑战计时器
        if (isChallengeActive)
        {
            UpdateChallengeTimer();
        }
        PTSText.text = "玩家总分: " + playerTotalPoints.ToString();
        ATSText.text = "玩家总分: " + aiTotalPoints.ToString();
    }

    #region 生成能量球挑战
    // 更新挑战计时器
    void UpdateChallengeTimer()
    {
        challengeTimer -= Time.deltaTime;
        
        if (challengeTimerText != null)
            challengeTimerText.text = "剩余时间: " + Mathf.CeilToInt(challengeTimer).ToString() + "秒";
        
        // 挑战时间结束
        if (challengeTimer <= 0)
        {
            EndChallenge();
        }
    }
    
    // 激活分值匹配挑战
    public void ActivateScoreChallenge()
    {
        // 设置随机目标分值(3-10)
        targetScore = Random.Range(3, 11);
        challengeScore = 0;
        challengeTimer = 10f; // 10秒挑战时间
        isChallengeActive = true;
        
        // 显示目标分值
        if (targetScoreText != null)
        {
            targetScoreText.gameObject.SetActive(true);
            targetScoreText.text = "目标分值: " + targetScore.ToString();
        }
        
        // 显示挑战计时器
        if (challengeTimerText != null)
        {
            challengeTimerText.gameObject.SetActive(true);
            challengeTimerText.text = "剩余时间: 10秒";
        }
        
        Debug.Log("分值匹配挑战开始! 目标分值: " + targetScore);
    }
    
    // 结束挑战
    void EndChallenge()
    {
        isChallengeActive = false;
        
        // 隐藏UI元素
        if (targetScoreText != null)
            targetScoreText.gameObject.SetActive(false);
        if (challengeTimerText != null)
            challengeTimerText.gameObject.SetActive(false);
        
        // 检查是否达成目标
        if (challengeScore == targetScore)
        {
            // 生成奖励
            SpawnReward();
            Debug.Log("挑战成功! 获得奖励!");
        }
        else
        {
            Debug.Log("挑战失败! 获得分数: " + challengeScore + ", 目标分数: " + targetScore);
        }
    }
    
    // 生成奖励
    void SpawnReward()
    {
        if (rewardPrefab != null)
        {
            Vector3 EnergyBallV3 = GetRandomPIC(10f);
            Instantiate(rewardPrefab, new Vector3(EnergyBallV3.x, 1.2f, EnergyBallV3.z), Quaternion.identity);
        }
    }

    Vector3 GetRandomPIC(float radius)
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

    public void OtherInitializationSettings()
    {
        isChallengeActive = false;
        GameObject[] activeEnergyBall = GameObject.FindGameObjectsWithTag("EnergyBall");
        foreach (GameObject EnergyBall in activeEnergyBall)
        {
            Destroy(EnergyBall);
        }
        PTSText.gameObject.SetActive(false);
        ATSText.gameObject.SetActive(false);
        playerTotalPoints = 0;
        aiTotalPoints = 0;
        PCon.gameObject.SetActive(false);
        targetScoreText.gameObject.SetActive(false);
        challengeTimerText.gameObject.SetActive(false);
        targetScore = 0;
        challengeScore = 0;
        challengeTimer = 0;
    }

    public void OverallStartGame()
    {
        OtherInitializationSettings();
        FGCon.StartGame();
        PCon.gameObject.SetActive(true);
    }
    public void StartEasyGame()//简单开始
    {
        FGCon.isNormalPass = false;
        FGCon.isHardPass = false;
        PTSText.gameObject.SetActive(true);
        ATSText.gameObject.SetActive(false);
        currentTime = 90f;
        OverallStartGame();
    }
    public void StartNormalGame()//普通开始
    {
        FGCon.isNormalPass = true;
        FGCon.isHardPass = false;
        PTSText.gameObject.SetActive(true);
        ATSText.gameObject.SetActive(true);
        currentTime = 150f;
        OverallStartGame();
    }
    public void StartHardGame()//困难开始
    {
        FGCon.isNormalPass = true;
        FGCon.isHardPass = true;
        PTSText.gameObject.SetActive(true);
        ATSText.gameObject.SetActive(true);
        currentTime = 210f;
        OverallStartGame();
    }

    public void EndGame()
    {
        FGCon.InitializationSettings();//水果生成初始化
        OtherInitializationSettings();//分值和玩家都以在里面初始化了
    }

    public void Score(int addScore)
    {
        playerTotalPoints += addScore;
        
        // 如果挑战激活，记录挑战期间获得的分数
        if (isChallengeActive)
        {
            challengeScore += addScore;
        }
    }
    
    public void AiScore(int addScore)
    {
        aiTotalPoints += addScore;
    }

    #region 计时器
    public void KeepTime()
    {
        currentTime -= Time.deltaTime;

        timerText.text = FormatTime(currentTime);
        if (currentTime <= 0)
        {
            currentTime = 0;
            timerText.text = "00:00";
        }
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    #endregion
}
