using Assets.PlayVis;
using Bluecap.Lib.Game_Design.Enums;
using Bluecap.Lib.Game_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Script
{
    public class InteractiveGame : BaseGame
    {
        //A reference to the interactive game UI. Normally I wouldn't weld them together
        //so closely, but for the purposes of this example it's a bit easier to read.
        public BoardManager playableGame;
        public InteractiveGame(int w, int h) : base(w, h)
        {
            playableGame = null;
        }

        public override bool TapAction(int x, int y)
        {
            //! Here we make sure that there's no piece here already before accepting the tap
            //! If you wanted to expand the AGD system to allow for tapping pieces, you'd change this
            if (state.Value(x, y) == 0)
            {
                //! By default, tapping an empty space spawns our piece in
                state.Set(x, y, Player.CURRENT);
                //! If we're in interactive mode, we notify the board that we've placed a piece
                playableGame.QueueAddPiece(x, y, CurrentPlayer() - 1);
                //! Automatically end the turn after a player has placed a piece
                //! Again, if you wanted to allow multiple actions, you'd change this.
                EndTurn();
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void MovePiece(int fx, int fy, int tx, int ty)
        {
            base.MovePiece(fx, fy, tx, ty);
            playableGame.QueueMovePiece(fx, fy, tx, ty);
        }

        public override void FlipPiece(int x, int y)
        {
            base.FlipPiece(x, y);
            playableGame.QueueFlipPiece(x, y, state.Value(x, y) - 1);          
        }

        public override void DeletePiece(int x, int y)
        {
            base.DeletePiece(x, y);
            playableGame.QueueDeletePiece(x, y);
        }

        public override void PrintGame()
        {
            Debug.Log(GameToString());
        }
    }
}
