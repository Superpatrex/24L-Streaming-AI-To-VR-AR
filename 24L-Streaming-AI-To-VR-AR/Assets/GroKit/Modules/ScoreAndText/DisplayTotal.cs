using UnityEngine;

namespace Core3lb
{
    public class DisplayTotal : DisplayTMPBase
    {

        [SerializeField] ScoreSystem[] scores;
        [SerializeField] int scoreList;

        //TO DO
        public Color highColor = Color.green;
        public Color midColor = Color.yellow;
        public Color lowColor = Color.red;

        public void FixedUpdate()
        {
            var scoreHolder = 0;
            foreach (ScoreSystem score in scores)
            {
                scoreHolder += score.score;
            }
            if(scoreList != scoreHolder)
            {
                scoreList = scoreHolder;
                _SetText(scoreList.ToString());
            }
        }
    }
}
