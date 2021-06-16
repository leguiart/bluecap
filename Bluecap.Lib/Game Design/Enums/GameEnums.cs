using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bluecap.Lib.Game_Design.Enums
{
    //These are enums used for defining bits of the game. You'll see them used elsewhere.
    public enum Heading { UP, RIGHT, DOWN, LEFT };
    public enum Direction { LINE, ROW, COL, CARDINAL };
    public enum Player { CURRENT, OPPONENT, ANY };
    public enum TriggeredEffect { DELETE, FLIP, CASCADE };
}
