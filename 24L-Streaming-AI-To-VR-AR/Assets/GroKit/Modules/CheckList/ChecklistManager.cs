using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Core3lb
{
    public class ChecklistManager : MonoBehaviour
    {
        public int currentScore = 0;
        public int maxScore;
        public List<ChecklistTask> tasks;
        [CoreReadOnly]
        public List<ChecklistTask> completedTasks;
        [CoreReadOnly]
        public ChecklistTask lastTaskCompleted;
        //Score System
        //
        public bool failOnMistake;
        //public bool doInOrder;
        public UnityEvent checklistTaskCompleted;
        public UnityEvent checklistSuccessful;
        public UnityEvent checklistFailed;
        public UnityEvent reset;

        [CoreReadOnly]
        public List<ChecklistTask> succeededTasks;

        [CoreReadOnly]
        public List<ChecklistTask> failedTasks;


        public virtual void Start()
        {
            _ResetTasks();
        }

        public void _ResetTasks()
        {
            maxScore = 0;
            currentScore = 0;
            foreach (ChecklistTask task in tasks)
            {
                task.Init(this);
                maxScore += task.maxScore;
            }
            completedTasks.Clear();
            succeededTasks.Clear();
            failedTasks.Clear();
            reset.Invoke();
        }


        //Task Tells it!
        public void TaskCompleted(ChecklistTask task)
        {
            lastTaskCompleted = task;

            completedTasks.Add(task);

            CheckForSuccess(task);

            checklistTaskCompleted.Invoke();

            _CheckForComplete();
        }

        //Did it succeed
        protected virtual void CheckForSuccess(ChecklistTask task)
        {
            if (task.isSuccess)
            {
                succeededTasks.Add(task);
                currentScore += task.maxScore;
            }
            else
            {
                failedTasks.Add(task);
            }
        }
        
        //Whole list is complete
        public void _CheckForComplete()
        {
            int count = 0;
            foreach (var item in tasks)
            {
                if(item.isComplete && item.isSuccess == false)
                {
                    if(failOnMistake)
                    {
                        checklistFailed.Invoke();
                        return;
                    }
                }
                count++;
            }
            if(count == completedTasks.Count) 
            {
                checklistSuccessful.Invoke();
            }
        }
    }
}
