using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TTSDK.UNBridgeLib.LitJson;
using TTSDK;
using StarkSDKSpace;
using UnityEngine.Analytics;
//public enum HEALTH_CHARACTER { PLAYER, ENEMY}

[System.Serializable]
public class FortrestLevel
{
    public float maxHealth = 1000;
    public Sprite[] stateFortrestSprites;
}
public class TheFortrest : MonoBehaviour, ICanTakeDamage
{
    //public HEALTH_CHARACTER healthCharacter;
    public FortrestLevel[] fortrestLevels;

    [ReadOnly] public int fortrestLevel = 1;
    public int[] enemyFortrestHealth;

    [HideInInspector]
    public float maxHealth;
    Sprite[] stateFortrestSprites;

    [ReadOnly] public float extraHealth = 0;
    [ReadOnly] public float currentHealth;

    
    public SpriteRenderer fortrestSprite;

    [Header("SHAKNG")]
    public float speed = 30f; //how fast it shakes
    public float amount = 0.5f; //how much it shakes
    public float shakeTime = 0.3f;
    public bool shakeX, shakeY;

    Vector2 startingPos;
    IEnumerator ShakeCoDo;

    public GameObject GameOverPanel;

    public string clickid;
    private StarkAdManager starkAdManager;
    void Awake()
    {
        startingPos = transform.position;

        //defaultLevel = healthCharacter == HEALTH_CHARACTER.PLAYER ? GlobalValue.UpgradeStrongWall : defaultFortrest - 1;
        //if (healthCharacter == HEALTH_CHARACTER.PLAYER)
        //{
            maxHealth = fortrestLevels[Mathf.Min(fortrestLevels.Length - 1, GlobalValue.UpgradeStrongWall)].maxHealth;
            stateFortrestSprites = fortrestLevels[GlobalValue.UpgradeStrongWall].stateFortrestSprites;
            fortrestSprite.sprite = stateFortrestSprites[0];
        //}
        //else
        //{
        //    maxHealth = fortrestLevels[GameLevelSetup.Instance.GetEnemyFortrestLevel() - 1].maxHealth;
        //    stateFortrestSprites = fortrestLevels[GameLevelSetup.Instance.GetEnemyFortrestLevel() - 1].stateFortrestSprites;
        //    fortrestSprite.sprite = stateFortrestSprites[0];
        //}
    }

    IEnumerator ShakeCo(float time)
    {
        float counter = 0;
        while (counter < time)
        {
            transform.position = startingPos + new Vector2(Mathf.Sin(Time.time * speed) * amount * (shakeX ? 1 : 0), Mathf.Sin(Time.time * speed) * amount * (shakeY ? 1 : 0));

            yield return null;
            counter += Time.deltaTime;
        }

        transform.position = startingPos;
    }

    // Start is called before the first frame update
    void Start()
    {
        extraHealth = maxHealth * GlobalValue.StrongWallExtra;
        maxHealth += extraHealth;
        currentHealth = maxHealth;
        MenuManager.Instance.UpdateHealthbar(currentHealth, maxHealth/*, healthCharacter*/);
    }

    public void TakeDamage(float damage, Vector2 force, Vector2 hitPoint, GameObject instigator, BODYPART bodyPart = BODYPART.NONE, WeaponEffect weaponEffect = null, WEAPON_EFFECT forceEffect = WEAPON_EFFECT.NONE)
    {
        currentHealth -= damage;
        FloatingTextManager.Instance.ShowText("" + (int)damage, Vector2.up * 2, Color.yellow, transform.position);

        MenuManager.Instance.UpdateHealthbar(currentHealth, maxHealth/*, healthCharacter*/);

        if (currentHealth <= 0)
        {
            //if (healthCharacter == HEALTH_CHARACTER.PLAYER)
            GameOverPanel.SetActive(true);
            Time.timeScale = 0;
                //GameManager.Instance.GameOver();
            //else
            //    GameManager.Instance.Victory();
        }
        else
        {
            if (ShakeCoDo != null)
                StopCoroutine(ShakeCoDo);

            ShakeCoDo = ShakeCo(shakeTime);
            StartCoroutine(ShakeCoDo);
        }

        //update fortrest state
        if (currentHealth > 0)
        {
            for (int i = (stateFortrestSprites.Length - 1); i > 0 ; i--)
            {
                if (currentHealth < ((maxHealth / (stateFortrestSprites.Length - 1)) * i))
                {
                    fortrestSprite.sprite = stateFortrestSprites[(stateFortrestSprites.Length - 1) - i];
                }
            }
        }
        else
            fortrestSprite.sprite = stateFortrestSprites[stateFortrestSprites.Length - 1];
    }
    public void AddHealth()
    {
        ShowVideoAd("g5g63eqpj682r2etv6",
            (bol) => {
                if (bol)
                {

                    currentHealth = maxHealth;
                    GameOverPanel.SetActive(false);
                    Time.timeScale = 1;


                    clickid = "";
                    getClickid();
                    apiSend("game_addiction", clickid);
                    apiSend("lt_roi", clickid);

                }
                else
                {
                    StarkSDKSpace.AndroidUIManager.ShowToast("观看完整视频才能获取奖励哦！");
                }
            },
            (it, str) => {
                Debug.LogError("Error->" + str);
                //AndroidUIManager.ShowToast("广告加载异常，请重新看广告！");
            });
        
    }

    public void getClickid()
    {
        var launchOpt = StarkSDK.API.GetLaunchOptionsSync();
        if (launchOpt.Query != null)
        {
            foreach (KeyValuePair<string, string> kv in launchOpt.Query)
                if (kv.Value != null)
                {
                    Debug.Log(kv.Key + "<-参数-> " + kv.Value);
                    if (kv.Key.ToString() == "clickid")
                    {
                        clickid = kv.Value.ToString();
                    }
                }
                else
                {
                    Debug.Log(kv.Key + "<-参数-> " + "null ");
                }
        }
    }

    public void apiSend(string eventname, string clickid)
    {
        TTRequest.InnerOptions options = new TTRequest.InnerOptions();
        options.Header["content-type"] = "application/json";
        options.Method = "POST";

        JsonData data1 = new JsonData();

        data1["event_type"] = eventname;
        data1["context"] = new JsonData();
        data1["context"]["ad"] = new JsonData();
        data1["context"]["ad"]["callback"] = clickid;

        Debug.Log("<-data1-> " + data1.ToJson());

        options.Data = data1.ToJson();

        TT.Request("https://analytics.oceanengine.com/api/v2/conversion", options,
           response => { Debug.Log(response); },
           response => { Debug.Log(response); });
    }


    /// <summary>
    /// </summary>
    /// <param name="adId"></param>
    /// <param name="closeCallBack"></param>
    /// <param name="errorCallBack"></param>
    public void ShowVideoAd(string adId, System.Action<bool> closeCallBack, System.Action<int, string> errorCallBack)
    {
        starkAdManager = StarkSDK.API.GetStarkAdManager();
        if (starkAdManager != null)
        {
            starkAdManager.ShowVideoAdWithId(adId, closeCallBack, errorCallBack);
        }
    }

    /// <summary>
    /// 播放插屏广告
    /// </summary>
    /// <param name="adId"></param>
    /// <param name="errorCallBack"></param>
    /// <param name="closeCallBack"></param>
    public void ShowInterstitialAd(string adId, System.Action closeCallBack, System.Action<int, string> errorCallBack)
    {
        starkAdManager = StarkSDK.API.GetStarkAdManager();
        if (starkAdManager != null)
        {
            var mInterstitialAd = starkAdManager.CreateInterstitialAd(adId, errorCallBack, closeCallBack);
            mInterstitialAd.Load();
            mInterstitialAd.Show();
        }
    }
}
