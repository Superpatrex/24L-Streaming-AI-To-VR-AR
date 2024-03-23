using UnityEngine;

namespace Core3lb
{
    public class ScoreSystem : MonoBehaviour
    {
        public int score = 0;
        public bool noNegatives;
        public string scoreID = "DEFAULT";
        public bool autoSaveScore;

        public virtual void _LoadScore()
        {
            PlayerPrefs.SetInt("SCORE"+ scoreID, score);
        }

        public virtual void _SaveScore()
        {
            score = PlayerPrefs.GetInt("SCORE" + scoreID, score);
        }

        public virtual void _Add1Score()
        {
            _AddToScore(1);
        }

        public virtual void _Subtract1Score()
        {
            _SubtractFromScore(1);
        }

        public virtual void _AddToScore(int chg)
        {
            chg = Mathf.Abs(chg);
            score += chg;
            if (autoSaveScore)
            {
                _SaveScore();
            }
        }

        public virtual void _SetScoreTo(int chg)
        {
            score = chg;
            if (autoSaveScore)
            {
                _SaveScore();
            }
        }

        public virtual void _SubtractFromScore(int chg)
        {
            chg = Mathf.Abs(chg);
            score -= chg;
            if (noNegatives)
            {
                if (score <= 0)
                {
                    score = 0;
                }
            }
            if (autoSaveScore)
            {
                _SaveScore();
            }
        }

        public virtual void _ResetScore()
        {
            score = 0;
        }
    }
}
