using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using MoreMountains.NiceVibrations;

public class Player : MonoBehaviour {
    public static Player instance;
    public float speed;
    public EZObjectPool objectPool;
    public GameObject explodeEffect;
    public static List<GameObject> trapList = new List<GameObject>();
    public GameObject pauseMenu;
    public GameObject startMenu;
    public GameObject resultMenu;
    public GameObject smokePrefab;
    public Text tutorialText;
    public Text distanceText;
    public Text scoreText;
    public Text scoringText;
    public RectTransform mainCanvas;
    public static float distance;
    public static float score = 0;
    public static float combo;
    public static float playerLife;
    public static bool isCountDistance = true;

    // Use this for initialization
    void Awake()
    {
        Application.targetFrameRate = 60;
    }

    void Start () {
        OnStartMenu();
        combo = 0;
        score = 0;
        distance = 0;
        playerLife = 3;
        isCountDistance = true;
        trapList.Clear();
        StartCoroutine(delaySpawnEnemy());
        Camera.main.transform.DOLocalMoveZ(45, 5);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, transform.position.y, transform.position.z + 1), speed * Time.deltaTime);
        if (isCountDistance)
        {
            distance += 0.1f;
            distanceText.text = "DISTANCE: " + ((int)distance).ToString() + "KM";
        }

        //if (Input.GetMouseButtonDown(0))
        //{
        //    Physics.gravity = new Vector3(0, -40f, 0);
        //    trapList[0].GetComponent<Rigidbody>().useGravity = true;
        //    trapList.RemoveAt(0);
        //}
    }

    public void OnTap()
    {
        Physics.gravity = new Vector3(0, -60f, 0);
        trapList[0].GetComponent<Rigidbody>().useGravity = true;
        trapList.RemoveAt(0);
    }

    IEnumerator delaySpawnEnemy()
    {
        var time = Random.Range(1, 3);
        yield return new WaitForSeconds(time);
        GameObject enemy;
        objectPool.TryGetNextObject(new Vector3(Random.Range(-10, 10), Random.Range(-20,8), transform.position.z - 120), Quaternion.Euler(new Vector3(0, 0, 0)), out enemy);
        //StartCoroutine(delayDeactive(enemy));
        StartCoroutine(delaySpawnEnemy());
    }

    //IEnumerator delayDeactive(GameObject obstacle)
    //{
    //    yield return new WaitForSeconds(3);
    //    obstacle.SetActive(false);
    //}

    IEnumerator delayLoadScene()
    {
        yield return new WaitForSeconds(1);
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentScene);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Enemy")
        {
            Debug.Log("Hit Enemy");
            other.gameObject.SetActive(false);
            var explode = Instantiate(explodeEffect, transform.position, Quaternion.identity);
            playerLife -= 1;
            if (playerLife <= 2)
            {
                var smoke = Instantiate(smokePrefab, transform.position, Quaternion.identity);
                smoke.transform.parent = transform;
                smoke.transform.localPosition = Vector3.zero;
            }
            if (playerLife <= 0)
            {
                var airCraft = transform.GetChild(0);
                Destroy(airCraft.gameObject);
                OnDie();
            }
        }
    }

    public void buttonPause()
    {
        isCountDistance = false;
        pauseMenu.SetActive(true);
        Time.timeScale = 0;
    }

    public void buttonResume()
    {
        isCountDistance = true;
        pauseMenu.SetActive(false);
        startMenu.SetActive(false);
        Time.timeScale = 1;
    }

    public void OnScoring(Vector3 offsetPos)
    {
        combo++;
        var randomComboText = Random.Range(1, 4);
        Color color = new Color32();
        switch(randomComboText)
        {
            case 1:
                scoringText.text = "PERFECT" + "\n" + "+" + combo;
                ColorUtility.TryParseHtmlString("#9D35EF", out color);
                break;
            case 2:
                scoringText.text = "AWESOME" + "\n" + "+" + combo;
                ColorUtility.TryParseHtmlString("#87D46D", out color);
                StartCoroutine(delaySlowMotion());
                break;
            case 3:
                scoringText.text = "GOOD" + "\n" + "+" + combo;
                ColorUtility.TryParseHtmlString("#FFFC1E", out color);
                break;
            case 4:
                scoringText.text = "NICE" + "\n" + "+" + combo;
                ColorUtility.TryParseHtmlString("#FF9700", out color);
                break;
            default:
                break;
        }
        score += combo;
        scoreText.text = "SCORE: " + score.ToString();
        scoringText.color = color;
        scoringText.DOKill();
        Vector2 canvasPos;
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(offsetPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(mainCanvas, screenPoint, null, out canvasPos);
        scoringText.transform.localPosition = canvasPos;
        scoringText.transform.localScale = new Vector3(0, 0, 0);
        scoringText.transform.DOScale(1, 1f);
        //scroringText.transform.DOMoveY(2, 2);
        scoringText.DOFade(0, 2);
    }

    public void OnDie()
    {
        isCountDistance = true;
        var explode = Instantiate(explodeEffect, transform.position, Quaternion.identity);
        Time.timeScale = 0.1f;
        StartCoroutine(delayShowResult());
    }

    IEnumerator delayShowResult()
    {
        yield return new WaitForSeconds(0.2f);
        resultMenu.SetActive(true);
        resultMenu.GetComponent<Image>().DOFade(0, 0);
        resultMenu.GetComponent<Image>().DOFade(1, 0.2f);
        var bestScore = PlayerPrefs.GetFloat("bestScore");
        if (score > bestScore)
        {
            PlayerPrefs.SetFloat("bestScore", score);
        }
        var resultBestScoreText = resultMenu.transform.GetChild(0);
        resultBestScoreText.GetComponent<Text>().text = "BEST SCORE: " + bestScore.ToString();
        var resultScoreText = resultMenu.transform.GetChild(1);
        resultScoreText.GetComponent<Text>().text = "SCORE: " + score.ToString();
        var resultDistanceText = resultMenu.transform.GetChild(2);
        resultDistanceText.GetComponent<Text>().text = "DISTANCE: " + ((int)distance).ToString() + "KM";
        Time.timeScale = 0;
    }

    IEnumerator delaySlowMotion()
    {
        Time.timeScale = 0.2f;
        yield return new WaitForSeconds(0.4f);
        Time.timeScale = 1;
    }

    public void OnStartMenu()
    {
        isCountDistance = false;
        startMenu.SetActive(true);
        tutorialText.DOFade(0, 0.8f).SetLoops(-1, LoopType.Yoyo);
        Time.timeScale = 0;
    }

    public void OnTryAgain()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentScene);
    }
}
