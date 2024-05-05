using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class PlayerData
{
    public string name;
    public int score;
}

[System.Serializable]
public class RankingData
{
    public string date;
    public List<PlayerData> players;
}

public class GameController : MonoBehaviour
{
    public ItemState[] buttons;
    public float gameTime = 60f;
    public int pointsPerHit = 10;
    public int penaltyPerMiss = 20;
    public int maxMisses = 3;

    public int score = 0;
    public int misses = 0;
    public bool gameEnded = false;
    public Coroutine gameCoroutine;
    public Coroutine buttonCoroutine;
    public Coroutine waitCoroutine;
    public float velocity = 0.4f;
    private WaitForSeconds buttonActiveWait;
    public int CurrentTime = 60;
    public Sprite MissingSpriteRed;
    public Sprite MissingSpriteBlue;
    public GameObject GameOverObject;
    public GameObject TryAgainObject;
    public GameObject StartObject;
    public GameObject WinnerObject;
    public GameObject EndObject;
    public Image TimerImage;
    public TextMeshProUGUI CountText;
    public GameObject AllHideObjects;
    public Image Life;
    public Sprite[] LifeSprites;
    public TextMeshProUGUI PointsText;
    public TextMeshProUGUI PointsWinnerText;

    public Image fill1;
    public Image fill2;

    public TMP_InputField inputFieldName;
    public Button button;

    private string jsonFilePath;
    public TextMeshProUGUI[] playerNameTexts;
    public TextMeshProUGUI[] playerScoreTexts;

    public TextMeshProUGUI SpeedText;
    public TextMeshProUGUI MinScoreText;

    public Image imageToFade;
    public float fadeDuration = 2.0f;
    public float waitTime = 8.0f;
    public GameObject ButtonNextRanking;

    public bool canFade = true;
    public int minScore = 80;
    public void ResetFadeState()
    {
        if (canFade)
        {
            // Reset the alpha of the image to 0
            Color newColor = imageToFade.color;
            newColor.a = 0f;
            imageToFade.color = newColor;

            // Set the GameObject to inactive
            imageToFade.gameObject.SetActive(false);

            // Reset any other elements that need to be reset
            ButtonNextRanking.SetActive(true);
        }
    }

    IEnumerator FadeObject()
    {
        while (true)
        {
            if (canFade)
            {
                ButtonNextRanking.SetActive(false);
                // Fade in
                imageToFade.gameObject.SetActive(true);
                float timer = 0f;
                while (timer < fadeDuration)
                {
                    timer += Time.deltaTime;
                    float alpha = timer / fadeDuration;
                    Color newColor = imageToFade.color;
                    newColor.a = alpha;
                    imageToFade.color = newColor;
                    yield return null;
                }

                // Wait
                yield return new WaitForSeconds(waitTime - (fadeDuration * 2));

                // Fade out
                timer = 0f;
                while (timer < fadeDuration)
                {
                    timer += Time.deltaTime;
                    float alpha = 1f - (timer / fadeDuration);
                    Color newColor = imageToFade.color;
                    newColor.a = alpha;
                    imageToFade.color = newColor;
                    yield return null;
                }

                imageToFade.gameObject.SetActive(false);
                yield return new WaitForSeconds(3f);
            }
            else
            {
                yield return null;
            }
        }
    }

    public void SetSpeed(bool increase)
    {
        if (increase)
        {
            velocity += 0.1f;
        }
        else
        {
            if (velocity > 0.2f)
            {
                velocity -= 0.1f;
            }
        }

        velocity = (float)Math.Round(velocity, 1); // Arredonda para uma casa decimal
        SetGameSpeed(velocity);
    }

    public void SetMinScore(bool increase)
    {
        if (increase)
        {
            minScore += 10;
        }
        else
        {
            minScore -= 10;
        }

        SetGameMinScore(minScore);
    }

    IEnumerator WaitToStartFade()
    {
        yield return new WaitForSeconds(5);
        StartCoroutine(FadeObject());
    }
    private void Start()
    {
        LoadGameSpeed();
        LoadGameMinScore();
        string currentDate = DateTime.Now.ToString("dd-MM-yyyy");

        jsonFilePath = Application.persistentDataPath + "/" + currentDate + "ranking.json";
        StartCoroutine(WaitToStartFade());
        UpdateRankingUI();
    }

    void LoadGameSpeed()
    {
        if (PlayerPrefs.HasKey("GameSpeed"))
        {
            velocity = PlayerPrefs.GetFloat("GameSpeed");
        } else
        {
            SetGameSpeed(velocity);
        }
    }

    public void SetGameSpeed(float newSpeed)
    {
        velocity = newSpeed;
        PlayerPrefs.SetFloat("GameSpeed", velocity);
        PlayerPrefs.Save();
    }

    void LoadGameMinScore()
    {
        if (PlayerPrefs.HasKey("GameScore"))
        {
            minScore = PlayerPrefs.GetInt("GameScore");
        }
        else
        {
            SetGameMinScore(minScore);
        }
    }

    public void SetGameMinScore(int newScore)
    {
        minScore = newScore;
        PlayerPrefs.SetInt("GameScore", minScore);
        PlayerPrefs.Save();
    }

    public void UpdateRankingUI()
    {
        RankingData rankingData = LoadRankingData();

        if (rankingData == null || rankingData.players == null)
        {
            Debug.LogWarning("Ranking data is null or empty.");
            return;
        }

        for (int i = 0; i < playerNameTexts.Length; i++)
        {
            if (i < rankingData.players.Count)
            {
                playerNameTexts[i].text = rankingData.players[i].name;
                playerScoreTexts[i].text = rankingData.players[i].score.ToString();
            }
            else
            {
                playerNameTexts[i].text = "Vazio";
                playerScoreTexts[i].text = "0";
            }
        }
    }

    private RankingData LoadRankingData()
    {
        string currentDate = DateTime.Now.ToString("dd-MM-yyyy");

        string filePath = Application.persistentDataPath + "/" + currentDate + "ranking.json";

        if (!File.Exists(filePath))
        {
            // Se o arquivo não existir, cria um novo com um ranking vazio
            RankingData newRankingData = new RankingData();
            SaveRankingData(newRankingData);
            return newRankingData;
        }

        string jsonString = File.ReadAllText(filePath);
        return JsonUtility.FromJson<RankingData>(jsonString);
    }
    private void SaveRankingData(RankingData rankingData)
    {
        string jsonString = JsonUtility.ToJson(rankingData);
        Debug.Log(jsonString);
        File.WriteAllText(jsonFilePath, jsonString);
    }

    public void Save()
    {
        Color newColor = imageToFade.color;
        newColor.a = 1f;
        imageToFade.color = newColor;
        AddPlayerToRanking(inputFieldName.text, score);
    }

    public void AddPlayerToRanking(string playerName, int playerScore)
    {
        // Carrega os dados do ranking
        RankingData rankingData = LoadRankingData();

        // Verifica se a data de hoje já existe no ranking
        if (rankingData != null && rankingData.date == DateTime.Now.ToString("dd/MM/yyyy"))
        {
            // Adiciona o jogador ao ranking existente
            rankingData.players.Add(new PlayerData { name = playerName, score = playerScore });
            Debug.Log("Adicionou " + playerName + "      Com pontuação de: " + playerScore + ")");

            // Ordena o ranking pelo score em ordem decrescente
            rankingData.players = rankingData.players.OrderByDescending(player => player.score).ToList();

            // Mantém apenas os 5 melhores pontuadores
            if (rankingData.players.Count > 5)
            {
                rankingData.players = rankingData.players.Take(5).ToList();
            }
        }
        else
        {
            // Cria um novo ranking com a data de hoje
            rankingData = new RankingData
            {
                date = DateTime.Now.ToString("dd/MM/yyyy"),
                players = new List<PlayerData> { new PlayerData { name = playerName, score = playerScore } }
            };
        }

        // Salva os dados atualizados do ranking
        SaveRankingData(rankingData);
        UpdateRankingUI();
    }


    private string[] names = { "Alice", "Bob", "Charlie", "David", "Eve", "Frank", "Grace", "Heidi", "Ivan", "Jane" };

    private string GetRandomName()
    {
        int index = UnityEngine.Random.Range(0, names.Length);
        return names[index];
    }

    private int GetRandomNumber()
    {
        return UnityEngine.Random.Range(0, 100);
    }

    void Update()
    {
        bool isInputFieldEmpty = string.IsNullOrEmpty(inputFieldName.text);

        button.interactable = !isInputFieldEmpty;

        SpeedText.text = velocity.ToString();
        MinScoreText.text = minScore.ToString();

        //if (Input.GetKeyDown(KeyCode.Escape))
        //{
        //    AddPlayerToRanking(GetRandomName(), GetRandomNumber());
        //}
    }
    void StartGame()
    {
        misses = 0;
        Life.sprite = LifeSprites[misses];
        gameEnded = false;
        DisableAllButtons();
        gameCoroutine = StartCoroutine(GameLoop());
        buttonCoroutine = StartCoroutine(ActivateRandomButton());
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(0);
    }

    public void ShowEndGame()
    {
        EndObject.SetActive(true);
    }

    public void StartCountdown()
    {
        canFade = false;
        score = 0;
        PointsText.text = score.ToString();
        buttonActiveWait = new WaitForSeconds(velocity);
        fill1.fillAmount = 1f;
        fill2.fillAmount = 1f;
        StartObject.SetActive(false);
        GameOverObject.SetActive(false);
        TryAgainObject.SetActive(false);
        TimerImage.gameObject.SetActive(true);
        CurrentTime = 60;
        StartCoroutine(Countdown());
    }

    IEnumerator Countdown()
    {
        CountText.text = "3";
        yield return new WaitForSeconds(1);
        CountText.text = "2";
        yield return new WaitForSeconds(1);
        CountText.text = "1";
        yield return new WaitForSeconds(1);
        AllHideObjects.SetActive(true);
        StartGame();
        TimerImage.gameObject.SetActive(false);
    }

    IEnumerator GameLoop()
    {
        StartCoroutine(DecreaseFillOverTime());
        StartCoroutine(PlayTimer());
        yield return new WaitForSeconds(gameTime);
        if (score >= minScore)
        {
            Winner();
        } else
        {
            TryAgain();
        }
    }

    IEnumerator DecreaseFillOverTime()
    {
        float timer = 0f;

        while (timer < gameTime)
        {
            float fillAmount = 1f - (timer / gameTime);
            fill1.fillAmount = fillAmount;
            fill2.fillAmount = fillAmount;

            timer += Time.deltaTime;
            yield return null;
        }

        fill1.fillAmount = 0f;
        fill2.fillAmount = 0f;
    }

    IEnumerator PlayTimer()
    {
        while (CurrentTime > 0)
        {
            yield return new WaitForSeconds(1f);
            CurrentTime--;
        }
        TryAgainObject.SetActive(true);
    }

    IEnumerator ActivateRandomButton()
    {
        DisableAllButtons();
        int randomIndex = UnityEngine.Random.Range(0, buttons.Length);
        buttons[randomIndex].EnableItem();
        yield return buttonActiveWait;
        if (!gameEnded)
        {
            buttons[randomIndex].DisableItem();
            buttonCoroutine = StartCoroutine(ActivateRandomButton());
        }
    }

    public void OnButtonClick(int buttonIndex)
    {
        if (buttonCoroutine != null)
        {
            StopCoroutine(buttonCoroutine);
        }
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
        }
        buttonCoroutine = null;
        waitCoroutine = StartCoroutine(WaitToActiveRandomButton());
        if (!gameEnded && buttons[buttonIndex].Enabled)
        {
            score += pointsPerHit;
            PointsText.text = score.ToString();
            buttons[buttonIndex].StartParticle();
            buttons[buttonIndex].DisableItem();
        }
        else if (!gameEnded && !buttons[buttonIndex].Enabled)
        {
            MissedButton();
            Debug.Log(buttons[buttonIndex].ItemImage.sprite.ToString());
            if (buttons[buttonIndex].ItemImage.sprite.name == "Red")
            {
                buttons[buttonIndex].ItemImage.sprite = MissingSpriteRed;
            } else
            {
                buttons[buttonIndex].ItemImage.sprite = MissingSpriteBlue;
            }
           
            StartCoroutine(WaitToResetMissing(buttons[buttonIndex]));
        }
    }

    IEnumerator WaitToActiveRandomButton()
    {
        yield return new WaitForSeconds(0.2f);
        buttonCoroutine = StartCoroutine(ActivateRandomButton());
    }

    IEnumerator WaitToResetMissing(ItemState item)
    {
        yield return new WaitForSeconds(0.7f);
        item.ItemImage.sprite = item.DisabledSprite;
    }

    void MissedButton()
    {
        misses++;
        Life.sprite = LifeSprites[misses];
        score -= penaltyPerMiss;
        PointsText.text = score.ToString();
        if (misses >= maxMisses)
        {
            EndGame();
            return;
        }
    }

    void DisableAllButtons()
    {
        foreach (ItemState button in buttons)
        {
            button.DisableItem();
        }
    }

    void EndGame()
    {
        StopAllCoroutines();
        gameEnded = true;
        DisableAllButtons();
        if (score >= minScore)
        {
            Winner();
        }
        else
        {
            GameOverObject.SetActive(true);
        }
    }
    void TryAgain()
    {
        StopAllCoroutines();
        gameEnded = true;
        DisableAllButtons();
        TryAgainObject.SetActive(true);
    }
    
    void Winner()
    {
        StopAllCoroutines();
        gameEnded = true;
        DisableAllButtons();
        PointsWinnerText.text = score.ToString();
        WinnerObject.SetActive(true);
    }
}
