using Bluecap.Lib.Game_Design.Enums;

namespace Bluecap.Lib.Game_Model
{
    public class FallPiecesEffect : Effect
    {
        public Heading Heading;

        public FallPiecesEffect(Heading h)
        {
            Heading = h;
        }

        override public string ToCode()
        {
            return "FALL " + Heading.ToString();
        }

        override public void Apply(BaseGame g)
        {
            Point drop;

            //We need slightly different methods for each direction, 
            //because we need to start from a different end each time
            if (Heading == Heading.DOWN)
            {
                for (int i = 0; i < (int)g.Genotype[0]; i++)
                {
                    for (int j = (int)g.Genotype[1] - 1; j >= 0; j--)
                    {
                        if (g.state.Value(i, j) > 0)
                        {
                            drop = FindDrop(g, i, j, Heading);
                            //Update the piece, assuming it needs to
                            if (drop.x != i || drop.y != j)
                                g.MovePiece(i, j, drop.x, drop.y);
                        }
                    }
                }
            }
            if (Heading == Heading.UP || Heading == Heading.LEFT)
            {
                for (int i = 0; i < (int)g.Genotype[0]; i++)
                {
                    for (int j = 0; j < (int)g.Genotype[1]; j++)
                    {
                        if (g.state.Value(i, j) > 0)
                        {
                            drop = FindDrop(g, i, j, Heading);
                            //Update the piece, assuming it needs to
                            if (drop.x != i || drop.y != j)
                                g.MovePiece(i, j, drop.x, drop.y);
                        }
                    }
                }
            }
            if (Heading == Heading.RIGHT)
            {
                for (int i = (int)g.Genotype[0] - 1; i >= 0; i--)
                {
                    for (int j = 0; j < (int)g.Genotype[1]; j++)
                    {
                        if (g.state.Value(i, j) > 0)
                        {
                            drop = FindDrop(g, i, j, Heading);
                            //Update the piece, assuming it needs to
                            if (drop.x != i || drop.y != j)
                                g.MovePiece(i, j, drop.x, drop.y);
                        }
                    }
                }
            }

        }

        public Point FindDrop(BaseGame game, int x, int y, Heading dir)
        {
            if (Heading == Heading.UP)
            {
                for (int i = y + 1; i < (int)game.Genotype[1]; i++)
                {
                    if (game.state.Value(x, i) != 0)
                    {
                        return new Point(x, i - 1);
                    }
                }
                return new Point(x, (int)game.Genotype[1] - 1);
            }
            else if (Heading == Heading.DOWN)
            {
                for (int i = y - 1; i >= 0; i--)
                {
                    if (game.state.Value(x, i) != 0)
                    {
                        return new Point(x, i + 1);
                    }
                }
                return new Point(x, 0);
            }
            else if (Heading == Heading.RIGHT)
            {
                for (int i = x + 1; i < (int)game.Genotype[0]; i++)
                {
                    if (game.state.Value(i, y) != 0)
                    {
                        return new Point(i - 1, y);
                    }
                }
                return new Point((int)game.Genotype[0] - 1, y);
            }
            else if (Heading == Heading.LEFT)
            {
                for (int i = x - 1; i >= 0; i--)
                {
                    if (game.state.Value(i, y) != 0)
                    {
                        return new Point(i + 1, y);
                    }
                }
                return new Point(0, y);
            }
            return new Point(x, y);
        }

        public override string Print()
        {
            return "All pieces on the board fall " + Heading.ToString().ToLower() + ".";
        }

    }

}
