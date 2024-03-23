using UnityEngine;
using UnityEngine.Events;

namespace Core3lb
{
    public class DisplayScore : DisplayTMPBase
    {
        public ScoreSystem scoreSystem;
        int lastScore;
        public UnityEvent scoreUpdated;

        public override void Awake()
        {
            base.Awake();
            if (scoreSystem == null)
            {
                Debug.LogError("Missing Score System",gameObject);
            }
            _SetText(scoreSystem.score.ToString());
            lastScore = scoreSystem.score;

        }

        public virtual void FixedUpdate()
        {
            if(scoreSystem.score != lastScore)
            {
                lastScore = scoreSystem.score;
                UpdateScore();
            }
        }

        public virtual void UpdateScore()
        {
            scoreUpdated.Invoke();
            _SetText(scoreSystem.score.ToString());
        }
    }
}
