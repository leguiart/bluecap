
namespace Bluecap.Lib.Game_Model
{
    public class Effect
    {
        public virtual void Apply(BaseGame g)
        {

        }

        public virtual string Print()
        {
            return "Generic effect";
        }

        public virtual string ToCode()
        {
            return "<error - did not override ToCode()>";
        }

    }

}
