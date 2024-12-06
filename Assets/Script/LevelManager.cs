using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    [Header("Game Elements")]
    public GameObject letterPrefab;
    public GameObject slotPrefab;
    public Transform letterParent;
    public Transform slotParent;

    [Header("UI Elements")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI lifeText;
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI correctMessage; // Display "Correct!" message
    public Image clueImage; // Image clue for the current level

    [Header("Game Settings")]
    public List<string> words = new List<string>();
    public List<Sprite> clueImages = new List<Sprite>(); // Clue images for each word
    public int initialLives = 3;
    public float levelTime = 10f;
    public float levelStartDelay = 1.5f; // Adjusted for synchronization
    public float correctMessageDuration = 1.5f; // Duration for "Correct!" message

    [Header("Lives UI")]
    public Transform livesContainer; // The container that will hold the hearts
    public Sprite heartSprite; // The filled heart sprite to represent a life
    public Sprite emptyHeartSprite; // The empty heart sprite to represent a lost life

    private HorizontalLayoutGroup layoutGroup;
    private int currentLevel = 0;
    private string currentWord;
    private List<GameObject> currentSlots = new List<GameObject>();
    private List<GameObject> currentLetters = new List<GameObject>();

    private float timer;
    private bool isTimerRunning = false;
    private int lives;
    private bool gameOver = false;

    private void Start()
    {
        if (words.Count == 0)
        {
            Debug.LogError("No words have been added! Please add words to the 'words' list in the Inspector.");
            return;
        }

        if (clueImages.Count != words.Count)
        {
            Debug.LogError("The number of clue images must match the number of words!");
            return;
        }

        lives = initialLives;
        layoutGroup = letterParent.GetComponent<HorizontalLayoutGroup>();
        UpdateLifeDisplay(); // Initialize the heart display
        ClearCorrectMessage();
        StartCoroutine(DelayedLoadLevel(currentLevel));
    }

    private void Update()
    {
        if (isTimerRunning)
        {
            UpdateTimer();
        }
    }

    public void LoadLevel(int levelIndex)
    {
        ClearLevel();

        if (levelIndex < words.Count)
        {
            currentWord = words[levelIndex];
            List<char> scrambledLetters = ShuffleLetters(currentWord);

            // Disable the layout group to allow manual arrangement of letters
            if (layoutGroup != null)
            {
                layoutGroup.enabled = false;
            }

            CreateLettersAndSlots(scrambledLetters);

            // Enable the layout group after creating letters and slots
            if (layoutGroup != null)
            {
                layoutGroup.enabled = true;
            }

            // Set the clue image for the current level
            if (clueImage != null && clueImages[levelIndex] != null)
            {
                clueImage.sprite = clueImages[levelIndex];
            }

            Debug.Log($"Level {levelIndex + 1} loaded: {currentWord}");
            StartTimer();
        }
        else
        {
            Debug.Log("Congratulations! You completed all levels!");
            CalculateScore();
            StopTimer();
        }
    }

    private IEnumerator DelayedLoadLevel(int levelIndex)
    {
        Debug.Log($"Starting Level {levelIndex + 1} in {levelStartDelay} seconds...");
        yield return new WaitForSeconds(levelStartDelay);
        LoadLevel(levelIndex);
    }

    private List<char> ShuffleLetters(string word)
    {
        List<char> letters = new List<char>(word);
        for (int i = 0; i < letters.Count; i++)
        {
            int randomIndex = Random.Range(0, letters.Count);
            char temp = letters[i];
            letters[i] = letters[randomIndex];
            letters[randomIndex] = temp;
        }
        Debug.Log("Letters shuffled: " + string.Join("", letters));
        return letters;
    }

    private void CreateLettersAndSlots(List<char> scrambledLetters)
    {
        currentSlots.Clear();
        currentLetters.Clear();

        for (int i = 0; i < scrambledLetters.Count; i++)
        {
            GameObject letterObj = Instantiate(letterPrefab, letterParent);
            letterObj.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = scrambledLetters[i].ToString();
            letterObj.name = scrambledLetters[i].ToString();
            letterObj.GetComponent<Draggable>().originalPosition = letterObj.GetComponent<RectTransform>().anchoredPosition;
            currentLetters.Add(letterObj);

            GameObject slotObj = Instantiate(slotPrefab, slotParent);
            slotObj.tag = "Slot";
            slotObj.name = $"Slot_{i}";
            currentSlots.Add(slotObj);
        }

        Debug.Log("Letters and slots created.");
    }

    public void CheckIfAllSlotsFilled()
    {
        string formedWord = "";
        bool allSlotsFilled = true;

        foreach (GameObject slot in currentSlots)
        {
            if (slot.transform.childCount > 0)
            {
                formedWord += slot.transform.GetChild(0).GetComponentInChildren<TMPro.TextMeshProUGUI>().text;
            }
            else
            {
                allSlotsFilled = false;
                Debug.Log($"Slot {slot.name} is empty.");
                break;
            }
        }

        if (allSlotsFilled)
        {
            Debug.Log($"All slots filled. Formed word: {formedWord}");
            if (formedWord == currentWord)
            {
                Debug.Log("Congratulations! You've formed the correct word: " + formedWord);
                StartCoroutine(ShowCorrectMessageAndLoadNextLevel());
            }
            else
            {
                Debug.Log("Incorrect word. Resetting letters...");
                ResetLettersToContainer();
            }
        }
    }

    private IEnumerator ShowCorrectMessageAndLoadNextLevel()
    {
        if (correctMessage != null)
        {
            correctMessage.text = "Correct!";
        }

        Debug.Log("Displaying 'Correct!' message...");
        yield return new WaitForSeconds(correctMessageDuration);

        ClearCorrectMessage();
        yield return new WaitForSeconds(levelStartDelay - correctMessageDuration);
        AdvanceToNextLevel();
    }

    private void ClearCorrectMessage()
    {
        if (correctMessage != null)
        {
            correctMessage.text = "";
        }
    }

    private void AdvanceToNextLevel()
    {
        currentLevel++;
        StartCoroutine(DelayedLoadLevel(currentLevel));
    }

    private void ResetLettersToContainer()
    {
        foreach (GameObject letter in currentLetters)
        {
            letter.transform.SetParent(letterParent, true);

            RectTransform rectTransform = letter.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = letter.GetComponent<Draggable>().originalPosition;

            Debug.Log($"Resetting letter '{letter.name}' to its original position.");
        }
    }

    private void ClearLevel()
    {
        foreach (Transform child in letterParent) { Destroy(child.gameObject); }
        foreach (Transform child in slotParent) { Destroy(child.gameObject); }
        currentLetters.Clear();
        currentSlots.Clear();

        // Clear the clue image
        if (clueImage != null)
        {
            clueImage.sprite = null;
        }
    }

    private void StartTimer()
    {
        timer = levelTime;
        isTimerRunning = true;
        if (timerText != null)
        {
            timerText.text = levelTime.ToString("0");
        }
        Debug.Log("Time start: " + levelTime);
    }

    private void StopTimer()
    {
        isTimerRunning = false;
        if (timerText != null)
        {
            timerText.text = "";
        }
        Debug.Log("Timer stopped.");
    }

    private void UpdateTimer()
    {
        if (isTimerRunning)
        {
            if (timer > 0)
            {
                timer -= Time.deltaTime;
                if (timerText != null)
                {
                    timerText.text = Mathf.CeilToInt(timer).ToString();
                }
            }
            else
            {
                Debug.Log("Time's up!");
                StopTimer();
                LoseLife();
            }
        }
    }

    private void LoseLife()
    {
        lives--;
        Debug.Log($"Life lost! Remaining lives: {lives}");
        UpdateLifeDisplay(); // Update the heart display when a life is lost

        if (lives > 0)
        {
            StartCoroutine(DelayedLoadLevel(currentLevel));
        }
        else
        {
            Debug.Log("Game Over! No lives remaining.");
            gameOver = true;
            CalculateScore();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    private void UpdateLifeDisplay()
    {
        // Clear existing heart images
        foreach (Transform child in livesContainer)
        {
            Destroy(child.gameObject);
        }

        // Create heart images based on the current lives
        for (int i = 0; i < initialLives; i++)
        {
            // Create a new heart GameObject
            GameObject heart = new GameObject("Heart_" + i, typeof(Image));
            heart.transform.SetParent(livesContainer, false);

            // Get the Image component of the heart
            Image heartImage = heart.GetComponent<Image>();
            heartImage.sprite = i < lives ? heartSprite : emptyHeartSprite; // Use filled or empty heart
            heartImage.preserveAspect = true;

            // Set the position of each heart
            heart.GetComponent<RectTransform>().anchoredPosition = new Vector2(i * 50, 0); // Adjust the X position for spacing
            heart.GetComponent<RectTransform>().localScale = Vector3.one; // Ensure correct scale
        }
    }

    private void UpdateLifeText()
    {
        if (lifeText != null)
        {
            lifeText.text = $"Lives: {lives}";
        }
    }

    private void CalculateScore()
    {
        string rank = "No rank";
        if (lives == 3) rank = "Gold";
        else if (lives == 2) rank = "Silver";
        else if (lives == 1) rank = "Bronze";

        Debug.Log($"Game Over! Your rank: {rank}. Remaining lives: {lives}");

        if (rankText != null)
        {
            rankText.text = $"Your Rank: {rank}";
        }
    }
}
