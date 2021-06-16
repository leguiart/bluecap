using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Script.Game_Design;
using Bluecap.Lib.Game_Design.Enums;
using Bluecap.Lib.Game_Model;
using Bluecap.Lib.Game_Design.Generators;

namespace Assets.Script.Game_Design 
{
    public class GameGeneration : MonoBehaviour
    {

        //In Unity, if you put a line like this in a file, above a field, it adds a little header to the Editor window.
        //You can see it by clicking on any object that has the "Game Generation" component in a scene.
        [Header("Board Settings")]
        public int minBoardDimension = 3;
        public int maxBoardDimension = 6;
        public bool forceSquareBoard = false;

        [Header("Update Rule Settings")]
        public int minUpdateEffects = 1;
        public int maxUpdateEffects = 2;
        public Heading[] allowedFallDirections;
        public TriggeredEffect[] allowedTriggeredEffects;

        [Header("Win/Loss Settings")]
        public bool includeLossCondition = false;
        public Direction[] allowedLineDirections;
        public int minLineLength = 3;
        public int maxLineLength = 4;


        private const int boardWidthId = 0, boardHeightId = 1, winConditionId = 2, loseConditionId = 3, effectsId = 4;
        private GenerationSettings settings;
        //Rather than min/max numbers that we randomise between, we might just want to offer fixed values.
        //I chose these as they seem like chunky milestones that would be interesting to try. This is
        //also an example of data which depends on other data (i.e. board size) which we don't validate here.
        public int[] pieceCountTargets = new int[] { 5, 10, 15 };

        public void Mutate(int geneToMutate, List<object> genotype)
        {
            GameGenerationUtils.Mutate(geneToMutate, genotype, settings);
        }

        public Condition GenerateCondition()
        {
            return GameGenerationUtils.GenerateCondition(settings);
        }

        public Effect GenerateEffect()
        {
            return GameGenerationUtils.GenerateEffect(settings);
        }

        public List<Effect> GenerateEffects(int minUpdateEffects, int maxUpdateEffects)
        {
            return GameGenerationUtils.GenerateEffects(settings);
        }

        public int GenerateRandomWidth()
        {
            return GameGenerationUtils.GenerateRandomWidth(settings);
        }

        public int GenerateRandomHeight()
        {
            return GameGenerationUtils.GenerateRandomHeight(settings);
        }

        /*
            Random game generation is one of the places that your choice of system design really comes up.
            If your setup is based more on a design language, enums, rule chunks, then it can be as simple
            as shuffling cards - you list the rule components you want to be legal, and you just uniformly
            pick from them.

            In the setup I've designed here, where rules are built as class objects, it's a bit clumsier.
            I did it this way here so the code was easier and more modular, simpler to read and parse.
            It has some other small advantages (for example, we can easily specify the exact probability that
            a particular rule should appear in a game). But it's not my personal favourite way to do it.
        */
        public BaseGame GenerateRandomGame()
        {
            return GameGenerationUtils.GenerateRandomGame(settings);
        }


        public static GameGeneration instance;
        void Awake()
        {
            settings = new GenerationSettings()
            {
                minBoardDimension = minBoardDimension,
                maxBoardDimension = maxBoardDimension,
                forceSquareBoard = forceSquareBoard,
                minUpdateEffects = minUpdateEffects,
                maxUpdateEffects = maxUpdateEffects,
                allowedFallDirections = allowedFallDirections,
                allowedTriggeredEffects = allowedTriggeredEffects,
                includeLossCondition = includeLossCondition,
                allowedLineDirections = allowedLineDirections,
                minLineLength = minLineLength,
                maxLineLength = maxLineLength,
                pieceCountTargets = pieceCountTargets,
            };
            instance = this;
        }

    }
}

