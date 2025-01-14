using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using StarkSDKSpace;
using TTSDK.UNBridgeLib.LitJson;
using TTSDK;

public class MenuManager : MonoBehaviour, IListener
{
    public static MenuManager Instance;
    public GameObject Shoot;
    public GameObject StartUI;
    public GameObject UI;
    public GameObject VictotyUI;
    public GameObject FailUI;
    public GameObject PauseUI;
    public GameObject LoadingUI;
    public GameObject HelperUI;

    [Header("Sound and Music")]
    public Image soundImage;
    public Image musicImage;
    public Sprite soundImageOn, soundImageOff, musicImageOn, musicImageOff;

    private StarkAdManager starkAdManager;

    public string clickid;

    UI_UI uiControl;

    private void Awake()
    {
        Instance = this;
        StartUI.SetActive(false);
        VictotyUI.SetActive(false);
        FailUI.SetActive(false);
        PauseUI.SetActive(false);
        LoadingUI.SetActive(false);
        HelperUI.SetActive(false);
        uiControl = gameObject.GetComponentInChildren<UI_UI>(true);
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
    IEnumerator Start()
    {
        soundImage.sprite = GlobalValue.isSound ? soundImageOn : soundImageOff;
        musicImage.sprite = GlobalValue.isMusic ? musicImageOn : musicImageOff;
        if (!GlobalValue.isSound)
            SoundManager.SoundVolume = 0;
        if (!GlobalValue.isMusic)
            SoundManager.MusicVolume = 0;

        StartUI.SetActive(true);

        yield return new WaitForSeconds(1);
        StartUI.SetActive(false);
        UI.SetActive(true);
        GameManager.Instance.StartGame();
    }

    public void UpdateHealthbar(float currentHealth, float maxHealth/*, HEALTH_CHARACTER healthBarType*/)
    {
        uiControl.UpdateHealthbar(currentHealth, maxHealth/*, healthBarType*/);
    }

    public void UpdateEnemyWavePercent(float currentSpawn, float maxValue)
    {
        uiControl.UpdateEnemyWavePercent(currentSpawn, maxValue);
    }

    float currentTimeScale;
    public void Pause()
    {
        SoundManager.PlaySfx(SoundManager.Instance.soundPause);
        if (Time.timeScale != 0)
        {
            currentTimeScale = Time.timeScale;
            Time.timeScale = 0;
            UI.SetActive(false);
            PauseUI.SetActive(true);
            SoundManager.Instance.PauseMusic(true);
        }
        else
        {
            Time.timeScale = currentTimeScale;
            UI.SetActive(true);
            PauseUI.SetActive(false);
            SoundManager.Instance.PauseMusic(false);
        }
    }

    public void IPlay()
    {
       
    }

    public void ISuccess()
    {
        StartCoroutine(VictoryCo());
    }

    IEnumerator VictoryCo()
    {
        UI.SetActive(false);
        yield return new WaitForSeconds(1.5f);
        VictotyUI.SetActive(true);
        ShowInterstitialAd("46r27p1jqqq17ob8oo",
            () => {

            },
            (it, str) => {
                Debug.LogError("Error->" + str);
            });
    }


    public void IPause()
    {
      
    }

    public void IUnPause()
    {
        
    }

    public void IGameOver()
    {
        StartCoroutine(GameOverCo());
    }

    IEnumerator GameOverCo()
    {
        UI.SetActive(false);

        yield return new WaitForSeconds(1.5f);
        FailUI.SetActive(true);
        ShowInterstitialAd("46r27p1jqqq17ob8oo",
            () => {

            },
            (it, str) => {
                Debug.LogError("Error->" + str);
            });
    }

    public void IOnRespawn()
    {
        
    }

    public void IOnStopMovingOn()
    {
        
    }

    public void IOnStopMovingOff()
    {
       
    }

    
    #region Music and Sound
    public void TurnSound()
    {
        GlobalValue.isSound = !GlobalValue.isSound;
        soundImage.sprite = GlobalValue.isSound ? soundImageOn : soundImageOff;

        SoundManager.SoundVolume = GlobalValue.isSound ? 1 : 0;
    }

    public void TurnMusic()
    {
        GlobalValue.isMusic = !GlobalValue.isMusic;
        musicImage.sprite = GlobalValue.isMusic ? musicImageOn : musicImageOff;

        SoundManager.MusicVolume = GlobalValue.isMusic ? SoundManager.Instance.musicsGameVolume : 0;
    }
    #endregion

    #region Load Scene
    public void LoadHomeMenuScene()
    {
        SoundManager.Click();
        StartCoroutine(LoadAsynchronously("Menu"));
    }

    public void RestarLevel()
    {
        SoundManager.Click();
        StartCoroutine(LoadAsynchronously(SceneManager.GetActiveScene().name));
    }

    public void LoadNextLevel()
    {
        SoundManager.Click();
        GlobalValue.levelPlaying++;
        StartCoroutine(LoadAsynchronously(SceneManager.GetActiveScene().name));
    }

    [Header("Load scene")]
    public Slider slider;
    public Text progressText;
    IEnumerator LoadAsynchronously(string name)
    {
        LoadingUI.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(name);
        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            slider.value = progress;
            progressText.text = (int)progress * 100f + "%";
            //			Debug.LogError (progress);
            yield return null;
        }
    }
    #endregion

    private void OnDisable()
    {
        Time.timeScale = 1;
    }

    public void OpenHelper(bool open)
    {
        SoundManager.Click();
        HelperUI.SetActive(open);
        Pause();
    }
}
