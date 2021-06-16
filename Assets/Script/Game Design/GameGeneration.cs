using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Script.Game_Design;
using Bluecap.Lib.Game_Design.Enums;
using Bluecap.Lib.Game_Model;

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
        //Rather than min/max numbers that we randomise between, we might just want to offer fixed values.
        //I chose these as they seem like chunky milestones that would be interesting to try. This is
        //also an example of data which depends on other data (i.e. board size) which we don't validate here.
        public int[] pieceCountTargets = new int[] { 5, 10, 15 };

        public void Mutate(int geneToMutate, List<object> genotype)
        {
            switch (geneToMutate)
            {
                case boardWidthId:
                    genotype[geneToMutate] =  GenerateRandomWidth();
                    break;
                case boardHeightId:
                    genotype[geneToMutate] = GenerateRandomHeight();
                    break;
                case winConditionId:
                    genotype[geneToMutate] = GenerateCondition();
                    break;
                case loseConditionId:
                    genotype[geneToMutate] = GenerateCondition();
                    break;
                case effectsId:
                    if(Random.Range(0f, 1f) <= 0.2f)
                    {
                        genotype[geneToMutate] = GenerateEffects(minUpdateEffects, maxUpdateEffects);
                    }
                    else
                    {
                        int effectsToSubstitute = Random.Range(0, ((List<Effect>)genotype[geneToMutate]).Count);
                        System.Type t1, t2;
                        do
                        {
                            var eff = GenerateEffect();
                            t1 = ((List<Effect>)genotype[geneToMutate])[effectsToSubstitute].GetType();
                            t2 = eff.GetType();
                        } while(!Equals(t1, t2));
                    }
                    break;
                default:
                    break;
            }
        }

        public Condition GenerateCondition()
        {
            //We only have two condition types: in-a-row, or piece count, so here we toss a coin
            //to include them equally. You could parameterise this if you wanted, to tip the balance.
            //Or you could balance it to reflect the actual distribution of rule types (i.e. there
            //are more ways to make an in-a-row condition than a piece-count one).
            if (Random.Range(0f, 1f) < 0.5f)
            {
                //g.winCondition =
                //    new InARowCondition(
                //        allowedLineDirections[Random.Range(0, allowedLineDirections.Length)],
                //        Random.Range(minLineLength, maxLineLength + 1));
                return new InARowCondition(
                        allowedLineDirections[Random.Range(0, allowedLineDirections.Length)],
                        Random.Range(minLineLength, maxLineLength + 1));
            }
            else
            {
                //g.winCondition =
                //    new PieceCountCondition(
                //        pieceCountTargets[Random.Range(0, pieceCountTargets.Length)]
                //    );
                return new PieceCountCondition(
                        pieceCountTargets[Random.Range(0, pieceCountTargets.Length)]
                    );
            }
        }

        public Effect GenerateEffect()
        {
            Effect e = new Effect();
            //3 effect types, so let's toss a, uh, 3-sided coin
            switch (Random.Range(0, 3))
            {
                case 0:
                    //Settle the board/fall pieces in a certain direction
                    //g.updatePhase.Add(new FallPiecesEffect(allowedFallDirections[Random.Range(0, allowedFallDirections.Length)]));
                    e = new FallPiecesEffect(allowedFallDirections[Random.Range(0, allowedFallDirections.Length)]);
                    break;
                case 1:
                    //End-to-end piece capturing in the style of Reversi
                    //g.updatePhase.Add(new CappedEffect(allowedTriggeredEffects[Random.Range(0, allowedTriggeredEffects.Length)]));
                    e = new CappedEffect(allowedTriggeredEffects[Random.Range(0, allowedTriggeredEffects.Length)]);
                    break;
                case 2:
                    //X-in-a-row logic
                    //Note I reuse the valid triggered effects, and the valid line lengths. You could imagine
                    //having custom limits here, or specifying it some other way.
                    //g.updatePhase.Add(new InARowEffect(
                    //    allowedTriggeredEffects[Random.Range(0, allowedTriggeredEffects.Length)],
                    //    Random.Range(minLineLength, maxLineLength)));
                    e = new InARowEffect(
                        allowedTriggeredEffects[Random.Range(0, allowedTriggeredEffects.Length)],
                        Random.Range(minLineLength, maxLineLength));
                    break;
            }
            return e;
        }

        public List<Effect> GenerateEffects(int minUpdateEffects, int maxUpdateEffects)
        {
            //! Update Effects
            int numberOfUpdateEffects = Random.Range(minUpdateEffects, maxUpdateEffects + 1);
            Dictionary<System.Type, Effect> typeEffects = new Dictionary<System.Type, Effect>();
            
            //Lots more options here! We could stop duplicate effects in the same game, for example.
            //As usual, we're keeping it simple here and not worrying, but try tweaking it!
            for (int i = 0; i < numberOfUpdateEffects; i++)
            {
                var eff = GenerateEffect();
                if (!typeEffects.ContainsKey(eff.GetType()))
                    typeEffects.Add(eff.GetType(), eff);
                else
                    typeEffects[eff.GetType()] = eff;
            }
            return new List<Effect>(typeEffects.Values);
        }

        public int GenerateRandomWidth()
        {
            return Random.Range(minBoardDimension, maxBoardDimension + 1);
        }

        public int GenerateRandomHeight()
        {
            return Random.Range(minBoardDimension, maxBoardDimension + 1);
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
            int w = GenerateRandomWidth();
            int h = GenerateRandomHeight();
            if (forceSquareBoard)
            {
                h = w;
            }
            BaseGame g = new BaseGame(w, h);

            //! Win Condition
            g.Genotype[winConditionId] = GenerateCondition();

            //! Lose Condition
            //You might want to make this a dice roll, like 50% of games have a loss condition.
            if (includeLossCondition)
                g.Genotype[loseConditionId] = GenerateCondition();


            g.Genotype[effectsId] = GenerateEffects(minUpdateEffects, maxUpdateEffects);
            return g;
        }


        public static GameGeneration instance;
        void Awake()
        {
            instance = this;
        }

    }
}

