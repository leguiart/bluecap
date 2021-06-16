using Bluecap.Lib.Game_Design.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bluecap.Lib.Game_Design.Generators
{
    public class GenerationSettings
    {
        public int minBoardDimension = 3;
        public int maxBoardDimension = 6;
        public bool forceSquareBoard = false;

        public int minUpdateEffects = 1;
        public int maxUpdateEffects = 2;
        public Heading[] allowedFallDirections;
        public TriggeredEffect[] allowedTriggeredEffects;

        public bool includeLossCondition = false;
        public Direction[] allowedLineDirections;
        public int minLineLength = 3;
        public int maxLineLength = 4;
        public int[] pieceCountTargets = new int[] { 5, 10, 15 };
    }
}
