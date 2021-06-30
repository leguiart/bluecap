using Bluecap.Lib.Game_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bluecap.Lib.Game_Design.Generators
{
    public static class GameGenerationUtils
    {
        private const int boardWidthId = 0, boardHeightId = 1, winConditionId = 2, loseConditionId = 3, effectsId = 4;
        static Random rand2 = new Random();
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
                    //Thread.Sleep(20);
                    //System.Random rand = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
                    if (rand2.NextDouble()*5f <= 0.2f)
                    {
                        genotype[geneToMutate] = GenerateEffects(settings);
                    }
                    else
                    {
                        int effectsToSubstitute = rand2.Next(0, ((List<Effect>)genotype[geneToMutate]).Count);
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

        public static dynamic GetDirection(GenerationSettings settings)
        {
            //Thread.Sleep(20);
            //Random rand = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            return settings.allowedLineDirections[rand2.Next(0, settings.allowedLineDirections.Length)];
        }

        public static dynamic GetLength(GenerationSettings settings)
        {
            //Thread.Sleep(20);
            //Random rand = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            return rand2.Next(settings.minLineLength, settings.maxLineLength + 1);
        }

        public static dynamic GetPieceCount(GenerationSettings settings)
        {
            //Thread.Sleep(20);
            //Random rand = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            return rand2.Next(0, settings.pieceCountTargets.Length);
        }

        public static dynamic GetHeading(GenerationSettings settings)
        {
            //Thread.Sleep(20);
            //Random rand = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            return settings.allowedFallDirections[rand2.Next(0, settings.allowedFallDirections.Length)];
        }

        public static dynamic GetTriggeredEffect(GenerationSettings settings)
        {
            //Thread.Sleep(20);
            //Random rand = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            return settings.allowedTriggeredEffects[rand2.Next(0, settings.allowedTriggeredEffects.Length)];
        }

        public static Condition GenerateCondition(GenerationSettings settings)
        {
            //We only have two condition types: in-a-row, or piece count, so here we toss a coin
            //to include them equally. You could parameterise this if you wanted, to tip the balance.
            //Or you could balance it to reflect the actual distribution of rule types (i.e. there
            //are more ways to make an in-a-row condition than a piece-count one).
            //Thread.Sleep(20);
            //Random rand = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            if (rand2.NextDouble() < 0.5f)
            {
                return new InARowCondition(
                        settings.allowedLineDirections[rand2.Next(0, settings.allowedLineDirections.Length)],
                        rand2.Next(settings.minLineLength, settings.maxLineLength + 1));
            }
            else
            {
                return new PieceCountCondition(
                        settings.pieceCountTargets[rand2.Next(0, settings.pieceCountTargets.Length)]);
            }
        }

        public static Effect GenerateEffect(GenerationSettings settings)
        {
            Effect e = new Effect();
            //Thread.Sleep(20);
            //Random rand = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            //3 effect types, so let's toss a, uh, 3-sided coin
            switch (rand2.Next(0, 3))
            {
                case 0:
                    //Settle the board/fall pieces in a certain direction
                    e = new FallPiecesEffect(settings.allowedFallDirections[rand2.Next(0, settings.allowedFallDirections.Length)]);
                    break;
                case 1:
                    //End-to-end piece capturing in the style of Reversi
                    e = new CappedEffect(settings.allowedTriggeredEffects[rand2.Next(0, settings.allowedTriggeredEffects.Length)]);
                    break;
                case 2:
                    //X-in-a-row logic
                    //Note I reuse the valid triggered effects, and the valid line lengths. You could imagine
                    //having custom limits here, or specifying it some other way.
                    e = new InARowEffect(
                        settings.allowedTriggeredEffects[rand2.Next(0, settings.allowedTriggeredEffects.Length)],
                        settings.allowedLineDirections[rand2.Next(0, settings.allowedLineDirections.Length)],
                        rand2.Next(settings.minLineLength, settings.maxLineLength));
                    break;
            }
            return e;
        }

        public static List<Effect> GenerateEffects(GenerationSettings settings)
        {
            //! Update Effects
            //Thread.Sleep(20);
            //Random rand = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            int numberOfUpdateEffects = rand2.Next(settings.minUpdateEffects, settings.maxUpdateEffects + 1);
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
            //Thread.Sleep(20);
            //Random rand = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);           
            return rand2.Next(settings.minBoardDimension, settings.maxBoardDimension + 1);
        }

        public static int GenerateRandomHeight(GenerationSettings settings)
        {
            //Thread.Sleep(20);
            //Random rand = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            return rand2.Next(settings.minBoardDimension, settings.maxBoardDimension + 1);
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
