using System;
using System.Collections;
using System.Collections.Generic;
using Bluecap.Lib.Game_Design;
using Bluecap.Lib.Game_Design.Enums;

namespace Bluecap.Lib.Game_Model
{
    public class CappedEffect : Effect
    {

        /*
        *  If we were making a big serious catalogue of game mechanics, you'd definitely want
        *  Go's "surround" mechanic here, but I coded this on a fairly tight timescale, and so
        *  I opted to just do Reversi's simpler mechanic.
        *
        *  i.e. if you have:
        *  OXXXX_
        *
        *  and you play an O in the _ gap, all the Xs are affected by this event. Go's is
        *  considerably more complicated as it requires a full surround and you need to take into
        *  account holes and such. However it obviously offers richer mechanics - try adding it
        *  yourself and experimenting!
        */
        public TriggeredEffect Effect;

        public CappedEffect(TriggeredEffect e)
        {
            Effect = e;
            //* Another extension idea: adding in a min/max/exact length for the capture.
            //* Pente is an example of a game with this rule - you can only cap lines of 2.
        }

        public override void Apply(BaseGame g)
        {
            int edgeValue = 0;
            int innerValue = 0;
            int boardWidth = (int)g.Genotype[0];
            int boardHeight = (int)g.Genotype[1];
            GameState state = g.state;

            //! My official policy on copy-pasting code is it's good, actually.
            List<Point> matchList = new List<Point>();
            for (int i = 0; i < boardWidth; i++)
            {
                for (int j = 0; j < boardHeight; j++)
                {
                    edgeValue = state.Value(i, j);
                    innerValue = (edgeValue % 2) + 1;

                    //Check for lines in the specified directions
                    if (edgeValue > 0)
                    {
                        //The minimum we need is 2 more spots (smallest cap is XOX)
                        if (j <= boardHeight - 3)
                        {

                            bool hasEnd = false;
                            int capLength = 0;
                            for (int l = j + 1; l < boardHeight; l++)
                            {
                                if (state.Value(i, l) == innerValue)
                                {
                                    capLength++;
                                }
                                else if (state.Value(i, l) == edgeValue)
                                {
                                    hasEnd = true;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            if (capLength > 0 && hasEnd)
                            {
                                for (int l = 0; l < capLength; l++)
                                {
                                    matchList.Add(new Point(i, j + 1 + l));
                                }
                            }
                        }
                        if (i <= boardWidth - 3)
                        {

                            bool hasEnd = false;
                            int capLength = 0;
                            for (int l = i + 1; l < boardWidth; l++)
                            {
                                if (state.Value(l, j) == innerValue)
                                {
                                    capLength++;
                                }
                                else if (state.Value(l, j) == edgeValue)
                                {
                                    hasEnd = true;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            if (capLength > 0 && hasEnd)
                            {
                                for (int l = 0; l < capLength; l++)
                                {
                                    matchList.Add(new Point(i + 1 + l, j));
                                }
                            }
                        }
                        if (i <= boardWidth - 3 && j <= boardWidth - 3)
                        {

                            bool hasEnd = false;
                            int capLength = 0;
                            for (int l = 1; l < Math.Min(boardWidth - i, boardHeight - j); l++)
                            {
                                if (state.Value(i + l, j + l) == innerValue)
                                {
                                    capLength++;
                                }
                                else if (state.Value(i + l, j + l) == edgeValue)
                                {
                                    hasEnd = true;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            if (capLength > 0 && hasEnd)
                            {
                                for (int l = 0; l < capLength; l++)
                                {
                                    matchList.Add(new Point(i + 1 + l, j + 1 + l));
                                }
                            }
                        }
                        if (i <= boardWidth - 3 && j >= 2)
                        {

                            bool hasEnd = false;
                            int capLength = 0;
                            for (int l = 1; l < Math.Min(boardWidth - i, j + 1); l++)
                            {
                                if (state.Value(i + l, j - l) == innerValue)
                                {
                                    capLength++;
                                }
                                else if (state.Value(i + l, j - l) == edgeValue)
                                {
                                    hasEnd = true;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            if (capLength > 0 && hasEnd)
                            {
                                for (int l = 0; l < capLength; l++)
                                {
                                    matchList.Add(new Point(i + 1 + l, j - 1 - l));
                                }
                            }
                        }
                    }
                }
            }

            //Now apply effect to the capped pieces
            foreach (Point p in matchList)
            {
                if (Effect == TriggeredEffect.DELETE)
                {
                    g.DeletePiece(p.x, p.y);
                }
                else if (Effect == TriggeredEffect.FLIP)
                {
                    g.FlipPiece(p.x, p.y);
                }
            }
        }

        override public string ToCode()
        {
            return "CAP " + Effect.ToString();
        }

        public override string Print()
        {
            string exp = "If a player places a piece at either end of a line of opponent pieces, ";
            switch (Effect)
            {
                case TriggeredEffect.DELETE:
                    exp += "the captured pieces are removed from play.";
                    break;
                case TriggeredEffect.FLIP:
                    exp += "the captured pieces flip to the player's colour.";
                    break;
            }
            return exp;
        }

    }
}