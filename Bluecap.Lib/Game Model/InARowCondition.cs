using Bluecap.Lib.Game_Design.Enums;

namespace Bluecap.Lib.Game_Model
{
    public class InARowCondition : Condition
    {

        public Direction Direction;
        public int Length;

        public InARowCondition(Direction d, int length)
        {
            Length = length;
            Direction = d;
        }

        //? Does any valid line of the valid length exist, for any player
        override public bool Check(BaseGame g, Player p)
        {
            return g.FindLines(Direction, Length, p, true).Count > 0;
        }

        override public string ToCode()
        {
            return "MATCH " + Direction.ToString() + " " + Length;
        }

        public override string Print()
        {
            string exp = "If they have at least " + Length + " pieces in a sequence ";
            switch (Direction)
            {
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

            return exp;
        }

    }
}

