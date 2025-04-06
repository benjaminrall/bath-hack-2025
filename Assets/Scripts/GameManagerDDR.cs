using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameManagerDDR : MonoBehaviour
{
    public int player1ID;
    public int player2ID;
    
    public PlayerController player1;
    public PlayerController player2;

    private PlayerData _player1Data;
    private PlayerData _player2Data;

    public float minY = -340;
    public float maxY = 340;

    public GameObject arrowParent;
    public GameObject left;
    public GameObject down;
    public GameObject up;
    public GameObject right;

    public float perfectTol = 8;
    public float missTol = 32;

    public float startNoteDelay = 0.8f;
    public float endNoteDelay = 0.4f;
    private float _noteDelayCounter;
    private float _noteDelay;
    
    public int perfectScore = 100;
    public int goodScore = 50;
    public int missScore = -10;
    public float hitHeight = 220;

    public float startSpeed = 250f;
    public float endSpeed = 500f;
    private float _speed;

    public float duration = 120;
    private float _elapsed;
    
    private Queue<ArrowScript> _lefts;
    private Queue<ArrowScript> _downs;
    private Queue<ArrowScript> _ups;
    private Queue<ArrowScript> _rights;

    public TextMeshProUGUI p1ScoreText;
    public TextMeshProUGUI p2ScoreText;

    public GameObject gameWindow;
    public GameObject gameOverWindow;
    
    private int[] _scores;

    public float trashTalkDelay = 9;
    private float _trashTalkTimer;
    public float danceChangeDelay = 14;
    private float _danceChangeTimer;

    private bool _gameOver;

    private AudioSource _audioSource;

    private Queue<string> trashTalkQueue;

    private Animator _animator1;
    private Animator _animator2;

    public TextMeshProUGUI winnerText;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameWindow.SetActive(true);
        gameOverWindow.SetActive(false);
        _audioSource = GetComponent<AudioSource>();
        _lefts = new Queue<ArrowScript>();
        _rights = new Queue<ArrowScript>();
        _ups = new Queue<ArrowScript>();
        _downs = new Queue<ArrowScript>();
        trashTalkQueue = new Queue<string>();

        _animator1 = player1.GetComponent<Animator>();
        _animator2 = player2.GetComponent<Animator>();
        
        List<string> insults = new();
        for (int i = 1; i <= 30; i++)
        {
            insults.Add($"insult-{i}");
        }
        Shuffle(insults);
        foreach (string insult in insults)
        {
            trashTalkQueue.Enqueue(insult);
        }
        
        _player1Data = PlayerDataManager.Instance.PlayerDatas[player1ID];
        _player2Data = PlayerDataManager.Instance.PlayerDatas[player2ID];
        
        Debug.Log(_player1Data.bodyMaterial);
        Debug.Log(_player1Data.faceMaterial);
        Debug.Log(_player2Data.faceMaterial);
        Debug.Log(_player2Data.bodyMaterial);
        
        player1.UpdateMaterials(_player1Data.bodyMaterial, _player1Data.faceMaterial);
        player2.UpdateMaterials(_player2Data.bodyMaterial, _player2Data.faceMaterial);

        _noteDelayCounter = 2f;
        _trashTalkTimer = trashTalkDelay;
        _danceChangeTimer = danceChangeDelay;
        SetDances();
        _speed = startSpeed;
        _noteDelay = startNoteDelay;

        _scores = new[] { 0, 0 };
    }

    void SetDances()
    {
        Debug.Log("Shuffling");
        int n1 = Random.Range(1, 7);
        int n2 = Random.Range(1, 7);
        _animator1.SetTrigger(n1.ToString());
        _animator2.SetTrigger(n2.ToString());
    }

    void Shuffle(List<string> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        if (_gameOver)
        {
            WiimoteState wiimote = WiimoteInitialiser.Instance.Wiimote1;
            wiimote.UpdateWiimoteData();
            foreach (WiimoteEvent wiimoteEvent in wiimote.GetEvents())
            {
                switch (wiimoteEvent)
                {
                    case WiimoteEvent.B_DOWN:
                        SceneManager.LoadScene(0);
                        break;
                }
            }
            return;
        };
        
        _elapsed += Time.deltaTime;

        float t = _elapsed / duration;
        _speed = Mathf.Lerp(startSpeed, endSpeed, t);
        _noteDelay = Mathf.Lerp(startNoteDelay, endNoteDelay, t);
        
        if (!WiimoteInitialiser.Instance.Initialised)
        {
            return;
        }

        WiimoteState wiimote1 = WiimoteInitialiser.Instance.Wiimote1;
        WiimoteState wiimote2 = WiimoteInitialiser.Instance.Wiimote2;

        wiimote1.UpdateWiimoteData();
        wiimote2.UpdateWiimoteData();
        
        foreach (WiimoteEvent wiimoteEvent in wiimote1.GetEvents())
        {
            switch (wiimoteEvent)
            {
                case WiimoteEvent.D_UP_DOWN:
                    CheckQueue(0, _ups);
                    break;
                case WiimoteEvent.D_LEFT_DOWN:
                    CheckQueue(0, _lefts);
                    break;
                case WiimoteEvent.D_RIGHT_DOWN:
                    CheckQueue(0, _rights);
                    break;
                case WiimoteEvent.D_DOWN_DOWN:
                    CheckQueue(0, _downs);
                    break;
            }
        }
        
        foreach (WiimoteEvent wiimoteEvent in wiimote2.GetEvents())
        {
            switch (wiimoteEvent)
            {
                case WiimoteEvent.D_UP_DOWN:
                    CheckQueue(1, _ups);
                    break;
                case WiimoteEvent.D_LEFT_DOWN:
                    CheckQueue(1, _lefts);
                    break;
                case WiimoteEvent.D_RIGHT_DOWN:
                    CheckQueue(1, _rights);
                    break;
                case WiimoteEvent.D_DOWN_DOWN:
                    CheckQueue(1, _downs);
                    break;
            }
        }

        _noteDelayCounter -= Time.deltaTime;
        if (_noteDelayCounter < 0f && _elapsed < duration)
        {
            switch (Random.Range(0, 4))
            {
                case 0:
                    CreateNote(left, _lefts);
                    break;
                case 1:
                    CreateNote(right, _rights);
                    break;
                case 2:
                    CreateNote(up, _ups);
                    break;
                case 3:
                    CreateNote(down, _downs);
                    break;
            }
            _noteDelayCounter = _noteDelay;
        }

        p1ScoreText.text = _scores[0].ToString();
        p2ScoreText.text = _scores[1].ToString();
        
        UpdateArrows(_ups);
        UpdateArrows(_downs);
        UpdateArrows(_lefts);
        UpdateArrows(_rights);

        _trashTalkTimer -= Time.deltaTime;
        if (_trashTalkTimer < 0)
        {
            string trashTalk = trashTalkQueue.Dequeue();
            _audioSource.clip = _scores[0] > _scores[1]
                ? _player1Data.AudioClips[trashTalk]
                : _player2Data.AudioClips[trashTalk];
            _audioSource.Play();
            _trashTalkTimer = trashTalkDelay;
        }

        _danceChangeTimer -= Time.deltaTime;
        if (_danceChangeTimer < 0)
        {
            SetDances();
            _danceChangeTimer = danceChangeDelay;
        }

        if (_elapsed > duration)
        {
            GameOver();
        }
    }

    public void GameOver()
    {
        _gameOver = true;
        gameWindow.SetActive(false);
        gameOverWindow.SetActive(true);
        string winner = _scores[0] > _scores[1]
            ? _player1Data.playerName
            : _player2Data.playerName;
        string trashTalk = trashTalkQueue.Dequeue();
        _audioSource.clip = _scores[0] > _scores[1]
            ? _player1Data.AudioClips[trashTalk]
            : _player2Data.AudioClips[trashTalk];
        _audioSource.Play();
        winnerText.text = winner + " wins!";
    }

    public void CreateNote(GameObject prefab, Queue<ArrowScript> queue)
    {
        GameObject newObj = Instantiate(prefab, arrowParent.transform);
        ArrowScript arrow = newObj.GetComponent<ArrowScript>();
        arrow.speed = _speed;
        queue.Enqueue(arrow);
    }

    public void UpdateArrows(Queue<ArrowScript> queue)
    {
        if (queue.Count < 1)
        {
            return;
        }
        
        ArrowScript topArrow = queue.Peek();
        if (topArrow.transform.localPosition.y >= topArrow.maxHeight)
        {
            queue.Dequeue();
            if (!topArrow.hits[0]) _scores[0] += missScore;
            if (!topArrow.hits[1]) _scores[1] += missScore;
            Destroy(topArrow.gameObject);
        }
    }
    
    public void CheckQueue(int i, Queue<ArrowScript> queue)
    {
        Debug.Log(i + ", " + queue.Count);
        
        if (queue.Count < 1)
        {
            _scores[i] += missScore;
            return;
        }
        
        ArrowScript topArrow = queue.Peek();
        float diff = Mathf.Abs(topArrow.transform.localPosition.y - hitHeight);

        if (topArrow.hits[i])
        {
            _scores[i] += missScore;
        }
        else if (diff < perfectTol)
        {
            _scores[i] += perfectScore;
            topArrow.hits[i] = true;
        } 
        else if (diff < missTol)
        {
            _scores[i] += goodScore;
            topArrow.hits[i] = true;
        }
        else
        {
            _scores[i] += missScore;
        }
    }
}
