using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace Core3lb
{
    public class ChecklistUIBridge : MonoBehaviour
    {
        [CoreRequired]
        public ChecklistManager manager;
        [Space(10)]
        public TMP_Text listofTasks;
        public TMP_Text listofTasksCompleted;
        public TMP_Text listofTasksSucceeded;
        public TMP_Text listofTasksFailed;
        public TMP_Text currentScore;
        public TMP_Text maxScore;
        public TMP_Text finished;

        public virtual void Start()
        {
            if (finished)
            {
                finished.text = "";
            }

            manager.checklistTaskCompleted.AddListener(UpdateText);
            manager.reset.AddListener(UpdateText);
            manager.checklistSuccessful.AddListener(Finished);
            manager.checklistFailed.AddListener(Failed);

            UpdateText();
        }

        public void UpdateText()
        {
            if (listofTasks)
            {
                listofTasks.text = GetItemsAsText(manager.tasks);
            }
            if (listofTasksCompleted)
            {
                listofTasksCompleted.text = GetItemsAsText(manager.completedTasks);
            }
            if (listofTasksSucceeded)
            {
                listofTasksSucceeded.text = GetItemsAsText(manager.succeededTasks);
            }
            if (listofTasksFailed)
            {
                listofTasksFailed.text = GetItemsAsText(manager.failedTasks);
            }
            if (currentScore)
            {
                currentScore.text = manager.currentScore.ToString();
            }
            if (maxScore)
            {
                maxScore.text = manager.maxScore.ToString();
            }
        }

        public void Finished()
        {
            if (finished)
            {
                finished.text = "Finished";
            }
        }

        public void Failed()
        {
            if (finished)
            {
                finished.text = "Failed";
            }
        }

        public string GetItemsAsText(List<ChecklistTask> theList)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in theList)
            {
                sb.AppendLine(item.taskName);
            }
            return sb.ToString();
        }
    }
}
