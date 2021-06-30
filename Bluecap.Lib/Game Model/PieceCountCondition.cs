using Bluecap.Lib.Game_Design.Enums;

namespace Bluecap.Lib.Game_Model
{
    public class PieceCountCondition : Condition
    {
        public int PieceCount;

        /*
            If you wanted to expand this, you'd probably add a little enum for LESS THAN, GREATER THAN, etc.
            I didn't add it here as I wanted to keep this fairly simple, but it's an easy add-on.
        */
        public PieceCountCondition(int count)
        {
            PieceCount = count;
        }

        public override bool Check(BaseGame g, Player playerType)
        {
            int code = g.state.GetPlayerValue(playerType);
            int count = 0;
            for (int i = 0; i < (int)g.Genotype[0]; i++)
            {
                for (int j = 0; j < (int)g.Genotype[1]; j++)
                {
                    if (g.state.Value(i, j) == code)
                        count++;
                }
            }
            return count == PieceCount;
        }

        override public string ToCode()
        {
            return "COUNT " + PieceCount;
        }

        public override string Print()
        {
            return "if they have " + PieceCount + " pieces on the board.";
        }

    }
}