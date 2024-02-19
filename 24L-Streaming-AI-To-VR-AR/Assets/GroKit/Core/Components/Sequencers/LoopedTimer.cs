
namespace Core3lb
{
    public class LoopedTimer : TimedEventBase
    {
        public bool infiniteLoops;
        [CoreHideIf("infiniteLoops")]
        public int repeats;
        int startRepeats;


        public override void _Start()
        {
            startRepeats = repeats;
            base._Start();
        }
        public override void TimerReached()
        {
            if (repeats == -1)
            {
                repeats--;
                timer = 0;
            }
            else
            {
                if (infiniteLoops)
                {
                    timer = 0;
                }
                else
                {

                    if (repeats >= 0)
                    {
                        _Stop();
                    }
                    timer = 0;
                }

            }
        }

        public override void _ResetTime()
        {
            repeats = 0;
            base._ResetTime();
        }
    }
}
