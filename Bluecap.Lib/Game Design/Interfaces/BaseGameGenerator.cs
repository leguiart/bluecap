using Bluecap.Lib.Game_Design.Generators;
using Bluecap.Lib.Game_Model;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bluecap.Lib.Game_Design.Interfaces
{
    public abstract class BaseGameGenerator
    {
        protected string bestRulesCode;
        protected bool gameTestingFinished;
        protected List<string> bestRulesCodes;
        protected float totalTimeSpent, estimatedTotalTime, estimatedTimeLeft;

        protected static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected static string kTAG = "BaseGameGeneratorLib: ";
        protected GenerationSettings settings;
        protected IEvaluator<BaseGame> gameEvaluator;

        string goodScoreColor = "<#33aa33>";
        string averageScoreColor = "<#E57517>";
        string badScoreColor = "<#CF1200>";
        string regularText = "<#3D2607>";

        public BaseGameGenerator(GenerationSettings settings, IEvaluator<BaseGame> gameEvaluator)
        {
            this.settings = settings;
            this.gameEvaluator = gameEvaluator;
            //logger.Log(kTAG, "Starting game generation run...");
            //StartCoroutine(StartGenerationProcess());
            //StartCoroutine(EstimatedTimeUpdater());
        }

        public abstract void StartGenerationProcess();

        //protected IEnumerator EstimatedTimeUpdater()
        //{
        //    var timer = Stopwatch.StartNew();
        //    while (!gameTestingFinished)
        //    {
        //        var t = TimeSpan.FromSeconds(estimatedTimeLeft);
        //        yield return UIManager.Instance.SetTimeRemainingText("Estimated time remaining: " + string.Format("{0:D2}h:{1:D2}m:{2:D2}s",
        //            t.Hours,
        //            t.Minutes,
        //            t.Seconds));



        //        //Remove the elapsed time since we last updated from estimatedTimeLeft.
        //        estimatedTimeLeft -= timer.ElapsedMilliseconds / 1000f;
        //        //estimatedTimeLeft -= Time.realtimeSinceStartup;
        //        timer.Restart();
        //    }

        //    yield return 0;
        //}


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
