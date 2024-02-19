using UnityEngine;
using UnityEngine.Events;

namespace Core3lb
{
    public class ChecklistTask : MonoBehaviour
    {
        public string taskName;
        [TextArea]
        public string notes;

        //public bool successIsMaxScore;
        public int currentScore = 0;
        public int maxScore;

        [CoreReadOnly]
        public bool isSuccess;
        [CoreReadOnly]
        public bool isComplete;

        protected ChecklistManager manager;

        public UnityEvent onComplete; //OnUse
        public UnityEvent onSucceed;//On Use Right
        public UnityEvent onFail;//On use Wrong

        public void Init(ChecklistManager chg)
        {
            manager = chg;
            isSuccess = false;
            isComplete = false;
        }

        [CoreButton("Task Success")]
        public void _TaskCompleteSuccess()
        {
            if (isComplete) return;
            isSuccess = true;
            _TaskComplete();
        }

        [CoreButton("Task Failed")]
        public void _TaskCompleteFail()
        {
            if (isComplete) return;
            isSuccess = false;
            _TaskComplete();
        }

        [CoreButton("Task Complete")]
        public virtual void _TaskComplete()
        {
            if (isComplete) return;
            //Calcuate Score
            if (isSuccess)
            {
                TaskSucceed();
            }
            else
            {
                TaskFailed();
            }
            isComplete = true;
            manager.TaskCompleted(this);
        }


        public virtual void _SetSuccess(bool chg)
        {
            if (isComplete) return;
            isSuccess = chg;
        }

        public virtual void AddToScore(int chg)
        {
            currentScore += chg;
            if(currentScore >= maxScore)
            {
                _TaskComplete();
            }
        }

        public virtual void TaskSucceed()
        {
            onSucceed.Invoke();
        }

        public virtual void TaskFailed()
        {
            onFail.Invoke();
        }
    }
}
