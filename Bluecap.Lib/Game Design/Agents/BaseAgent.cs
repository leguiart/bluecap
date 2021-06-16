using Bluecap.Lib.Game_Model;

namespace Bluecap.Lib.Game_Design.Agents
{
    public class BaseAgent
    {

        protected int playerCode;

        /// <summary>
        /// Take a turn, but use no more than the allotted time limit.
        /// </summary>
        /// <param name="g">The game to play</param>
        /// <param name="timeLimit">Time limit in seconds, default is 1 second.</param>
        /// <returns>Returns whether we managed to take a turn or not.</returns>
        public virtual bool TakeTurn(BaseGame g, float timeLimit = 1f)
        {
            return false;
        }

    }
}

