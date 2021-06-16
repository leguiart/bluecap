using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Scene Interface")]
    [SerializeField]
    private  bool interfaceEnabled = false;
    [SerializeField]
    private TMPro.TextMeshProUGUI bestRulesText, bestScoresText, bestOverallScoreText, currentRulesText, currentScoresText, progressBarText, timeRemainingText;
    [SerializeField]
    private UnityEngine.UI.Image progressBar;

    public static UIManager Instance { get; private set; }

    string goodScoreColor = "<#33aa33>";
    string averageScoreColor = "<#E57517>";
    string badScoreColor = "<#CF1200>";
    string regularText = "<#3D2607>";

    void Awake()
    {
        Instance = this;
    }


    // Start is called before the first frame update
    //void Start()
    //{
        
    //}

    //// Update is called once per frame
    //void Update()
    //{
        
    //}
    public string ToScore(float val)
    {
        string sc = "";
        if (val < 0.25f)
            sc += badScoreColor;
        else if (val < 0.5f)
            sc += averageScoreColor;
        if (val > 0.75f)
            sc += goodScoreColor;
        sc += val.ToString("0%");
        sc += regularText;
        return sc;
    }

    public IEnumerator SetProgressBarText(string s)
    {
        if (interfaceEnabled)
            progressBarText.text = s;
        yield return 0;
    }
    public IEnumerator SetBestScoresText(string s)
    {
        if (interfaceEnabled)
            bestScoresText.text = s;
        yield return 0;
    }

    public IEnumerator SetBestOverallScoreText(string s)
    {
        if (interfaceEnabled)
            bestOverallScoreText.text = s;
        yield return 0;
    }
    public IEnumerator SetBestRuleText(string s)
    {
        if (interfaceEnabled)
            bestRulesText.text = s;
        yield return 0;
    }

    public IEnumerator SetProgressBarFill(float f)
    {
        if (interfaceEnabled)
            progressBar.fillAmount = f;
        yield return 0;
    }

    public IEnumerator SetTimeRemainingText(string timeString)
    {
        if (interfaceEnabled)
            timeRemainingText.text = timeString;
        yield return 0;
    }

    public IEnumerator SetCurrentRulesText(string rulesString)
    {        
        if (interfaceEnabled)
            currentRulesText.text = rulesString;
        yield return 0;
    }

    public IEnumerator SetCurrentScoresText(string scoresString)
    {
        if (interfaceEnabled)
            currentScoresText.text = scoresString;
            
        yield return 0;

    }

}
