using Bluecap.Lib.Game_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bluecap.Lib.Game_Design.Generators
{
    public static class GameGenerationUtils
    {
        private const int boardWidthId = 0, boardHeightId = 1, winConditionId = 2, loseConditionId = 3, effectsId = 4;

        public static void Mutate(int geneToMutate, List<object> genotype, GenerationSettings settings)
        {
            switch (geneToMutate)
            {
                case boardWidthId:
                    genotype[geneToMutate] = GenerateRandomWidth(settings);
                    break;
                case boardHeightId:
                    genotype[geneToMutate] = GenerateRandomHeight(settings);
                    break;
                case winConditionId:
                    genotype[geneToMutate] = GenerateCondition(settings);
                    break;
                case loseConditionId:
                    genotype[geneToMutate] = GenerateCondition(settings);
                    break;
                case effectsId:
                    System.Random rand = new Random();
                    if (rand.NextDouble()*5f <= 0.2f)
                    {
                        genotype[geneToMutate] = GenerateEffects(settings);
                    }
                    else
                    {
                        int effectsToSubstitute = rand.Next(0, ((List<Effect>)genotype[geneToMutate]).Count);
                        System.Type t1, t2;
                        do
                        {
                            var eff = GenerateEffect(settings);
                            t1 = ((List<Effect>)genotype[geneToMutate])[effectsToSubstitute].GetType();
                            t2 = eff.GetType();
                        } while (!Equals(t1, t2));
                    }
                    break;
                default:
                    break;
            }
        }

        public static Condition GenerateCondition(GenerationSettings settings)
        {
            //We only have two condition types: in-a-row, or piece count, so here we toss a coin
            //to include them equally. You could parameterise this if you wanted, to tip the balance.
            //Or you could balance it to reflect the actual distribution of rule types (i.e. there
            //are more ways to make an in-a-row condition than a piece-count one).
            Random rand = new Random();
            if (rand.NextDouble() < 0.5f)
            {
                //g.winCondition =
                //    new InARowCondition(
                //        allowedLineDirections[Random.Range(0, allowedLineDirections.Length)],
                //        Random.Range(minLineLength, maxLineLength + 1));
                return new InARowCondition(
                        settings.allowedLineDirections[rand.Next(0, settings.allowedLineDirections.Length)],
                        rand.Next(settings.minLineLength, settings.maxLineLength + 1));
            }
            else
            {
                //g.winCondition =
                //    new PieceCountCondition(
                //        pieceCountTargets[Random.Range(0, pieceCountTargets.Length)]
                //    );
                return new PieceCountCondition(
                        settings.pieceCountTargets[rand.Next(0, settings.pieceCountTargets.Length)]
                    );
            }
        }

        public static Effect GenerateEffect(GenerationSettings settings)
        {
            Effect e = new Effect();
            Random rand = new Random();
            //3 effect types, so let's toss a, uh, 3-sided coin
            switch (rand.Next(0, 3))
            {
                case 0:
                    //Settle the board/fall pieces in a certain direction
                    //g.updatePhase.Add(new FallPiecesEffect(allowedFallDirections[Random.Range(0, allowedFallDirections.Length)]));
                    e = new FallPiecesEffect(settings.allowedFallDirections[rand.Next(0, settings.allowedFallDirections.Length)]);
                    break;
                case 1:
                    //End-to-end piece capturing in the style of Reversi
                    //g.updatePhase.Add(new CappedEffect(allowedTriggeredEffects[Random.Range(0, allowedTriggeredEffects.Length)]));
                    e = new CappedEffect(settings.allowedTriggeredEffects[rand.Next(0, settings.allowedTriggeredEffects.Length)]);
                    break;
                case 2:
                    //X-in-a-row logic
                    //Note I reuse the valid triggered effects, and the valid line lengths. You could imagine
                    //having custom limits here, or specifying it some other way.
                    //g.updatePhase.Add(new InARowEffect(
                    //    allowedTriggeredEffects[Random.Range(0, allowedTriggeredEffects.Length)],
                    //    Random.Range(minLineLength, maxLineLength)));
                    e = new InARowEffect(
                        settings.allowedTriggeredEffects[rand.Next(0, settings.allowedTriggeredEffects.Length)],
                        rand.Next(settings.minLineLength, settings.maxLineLength));
                    break;
            }
            return e;
        }

        public static List<Effect> GenerateEffects(GenerationSettings settings)
        {
            //! Update Effects
            Random rand = new Random();
            int numberOfUpdateEffects = rand.Next(settings.minUpdateEffects, settings.maxUpdateEffects + 1);
            Dictionary<System.Type, Effect> typeEffects = new Dictionary<System.Type, Effect>();

            //Lots more options here! We could stop duplicate effects in the same game, for example.
            //As usual, we're keeping it simple here and not worrying, but try tweaking it!
            for (int i = 0; i < numberOfUpdateEffects; i++)
            {
                var eff = GenerateEffect(settings);
                if (!typeEffects.ContainsKey(eff.GetType()))
                    typeEffects.Add(eff.GetType(), eff);
                else
                    typeEffects[eff.GetType()] = eff;
            }
            return new List<Effect>(typeEffects.Values);
        }

        public static int GenerateRandomWidth(GenerationSettings settings)
        {
            Random rand = new Random();
            return rand.Next(settings.minBoardDimension, settings.maxBoardDimension + 1);
        }

        public static int GenerateRandomHeight(GenerationSettings settings)
        {
            Random rand = new Random();
            return rand.Next(settings.minBoardDimension, settings.maxBoardDimension + 1);
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
        public static BaseGame GenerateRandomGame(GenerationSettings settings)
        {
            int w = GenerateRandomWidth(settings);
            int h = GenerateRandomHeight(settings);
            if (settings.forceSquareBoard)
            {
                h = w;
            }
            BaseGame g = new BaseGame(w, h);

            //! Win Condition
            g.Genotype[winConditionId] = GenerateCondition(settings);

            //! Lose Condition
            //You might want to make this a dice roll, like 50% of games have a loss condition.
            if (settings.includeLossCondition)
                g.Genotype[loseConditionId] = GenerateCondition(settings);


            g.Genotype[effectsId] = GenerateEffects(settings);
            return g;
        }
    }
}
