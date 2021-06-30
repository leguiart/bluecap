using System.Collections.Generic;
using Bluecap.Lib.Game_Design.Enums;

namespace Bluecap.Lib.Game_Model {
    //Thinking about it now, this should be called InASequence or something. Ah, regrets.
    public class InARowEffect : Effect
    {
        public TriggeredEffect Effect;
        public Direction Direction;
        public int Length;

        //Note for this one we just always use checkDirection.LINE here, you can extend it if you like!
        public InARowEffect(TriggeredEffect e, Direction d, int n) {
            Effect = e;
            Length = n;
            //Direction = Direction.LINE;
            Direction = d;
        }

        override public string ToCode() {
            return "MATCH " + Direction.ToString() + " " + Length + " " + Effect.ToString();
        }

        public override void Apply(BaseGame g) {
            List<Point> ps = g.FindLines(Direction, Length, Player.CURRENT, false, Effect == TriggeredEffect.CASCADE);

            //? This is pretty inefficient! It would've been nicer to have a third option, Player.ANY.
            //? If I was building this as a big system I'd definitely refactor it, but I'm going to do 
            //? this tiny hack here in the name of saving time, and keeping the code elsewhere simple.
            if (Effect != TriggeredEffect.CASCADE)
            {
                ps.AddRange(g.FindLines(Direction, Length, Player.OPPONENT));
            }

            //Now apply the effect to any of the matched pieces
            foreach (Point p in ps) {
                if (Effect == TriggeredEffect.DELETE) {
                    g.DeletePiece(p.x, p.y);
                }
                else if (Effect == TriggeredEffect.FLIP || Effect == TriggeredEffect.CASCADE) {
                    g.FlipPiece(p.x, p.y);
                }
            }
        }

        public override string Print() {
            string exp = "If there are at least " + Length + " pieces of the same type in a sequence ";
            switch (Direction) {
                case Direction.LINE:
                    exp += "(in any direction)";
                    break;
                case Direction.ROW:
                    exp += "(in a horizontal row only)";
                    break;
                case Direction.COL:
                    exp += "(in a vertical column only)";
                    break;
                case Direction.CARDINAL:
                    exp += "(horizontal or vertical lines only)";
                    break;
            }

            exp += ", then ";

            switch (Effect) {
                case TriggeredEffect.DELETE:
                    exp += "the pieces are removed from play.";
                    break;
                case TriggeredEffect.FLIP:
                    exp += "the pieces flip to the other player's colour.";
                    break;
                case TriggeredEffect.CASCADE:
                    exp += "pieces connected to the latest move flip to the other player's colour.";
                    break;

            }
            return exp;
        }

    }
}
