using UnityEngine;

namespace Core3lb
{
    public class ManipulateScore : MonoBehaviour
    {

        [CoreEmphasize]
        public ScoreSystem scoreSystem;
        [Tooltip("Will search if it is missing a ScoreSystem")]
        public string scoreID = "MainScore";
        public void Start()
        {
            if(scoreSystem = null)
            {
                var scores = GetComponents<ScoreSystem>();
                foreach (var score in scores)
                {
                    if (score.scoreID == scoreID)
                    {
                        scoreSystem = score;
                        return;
                    }
                }
            }      
            Debug.LogError("No Score System found by that ID");
        }

        //Transfer all of these to the score system

        public virtual void _Add1Score()
        {
            scoreSystem._Add1Score();
        }
        public virtual void _Subtract1Score()
        {
            scoreSystem._Subtract1Score();
        }

        public virtual void _AddToScore(int chg)
        {
            scoreSystem._AddToScore(chg);
        }

        public virtual void _SetScoreTo(int chg)
        {
            scoreSystem._SetScoreTo(chg);
        }

        public virtual void _SubtractFromScore(int chg)
        {
            scoreSystem._SubtractFromScore(chg);

        }

        public virtual void _ResetScore()
        {
            scoreSystem._ResetScore();
        }
    }
}
