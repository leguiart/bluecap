using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
using Assets.Script.Game_Design;
using System.Collections;
using System.Diagnostics;

namespace Assets.Script.Game_Design
{
    public abstract class BaseGameGenerator : MonoBehaviour
    {
        [Header("Best Rules")]
        [TextArea]
        [SerializeField]
        protected string bestRulesCode;
        [SerializeField]
        protected bool gameTestingFinished;
        [SerializeField]
        [TextArea]
        protected List<string> bestRulesCodes;

        [Header("Evaluation Time")]
        [SerializeField]
        protected float totalTimeSpent, estimatedTotalTime, estimatedTimeLeft;

        protected static ILogger logger = Debug.unityLogger;
        protected static string kTAG = "BaseGameGenerator";
        protected LogHandler logHandler;

        string goodScoreColor = "<#33aa33>";
        string averageScoreColor = "<#E57517>";
        string badScoreColor = "<#CF1200>";
        string regularText = "<#3D2607>";

        void Start()
        {
            logHandler = new LogHandler();
            logger.Log(kTAG, "Starting game generation run...");
            StartCoroutine(StartGenerationProcess());
            StartCoroutine(EstimatedTimeUpdater());
        }

        protected abstract IEnumerator StartGenerationProcess();

        protected IEnumerator EstimatedTimeUpdater()
        {
            var timer = Stopwatch.StartNew();
            while (!gameTestingFinished)
            {
                var t = TimeSpan.FromSeconds(estimatedTimeLeft);
                yield return UIManager.Instance.SetTimeRemainingText("Estimated time remaining: " + string.Format("{0:D2}h:{1:D2}m:{2:D2}s",
                    t.Hours,
                    t.Minutes,
                    t.Seconds));



                //Remove the elapsed time since we last updated from estimatedTimeLeft.
                estimatedTimeLeft -= timer.ElapsedMilliseconds / 1000f;
                //estimatedTimeLeft -= Time.realtimeSinceStartup;
                timer.Restart();
            }

            yield return 0;
        }


        protected string ToScore(float val)
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
    }
}
