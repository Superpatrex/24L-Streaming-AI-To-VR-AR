using System.Collections.Generic;
using UnityEngine;
using System;

namespace Core3lb
{
    //The base trigger is used for inheriting the basic trigger functions do not use this script directly
    public class AdvancedTrigger : BaseTrigger
    {
        [CoreHeader("Advanced Settings")]

        [Tooltip("Only runs enter once, no exit.")]
        public bool isOneShot;

        [CoreReadOnly]
        [CoreShowIf("isOneShot")]
        public bool wasTriggered;
        [Tooltip("Only runs the enter on first object that enters, exit on last object that exits.")]
        //private bool onlyAcceptsOneObjectAtATime = false;

        [CoreReadOnly]
        [CoreShowIf("runEventsOnce")]
        public bool isEntered = false;

        [HideInInspector]
        public List<Collider> colliderList = new List<Collider>();

        public override bool DoesThisCountAsEntered(Collider collision)
        {
            if (isOneShot)
            {
                if (wasTriggered)
                {
                    return false;
                }
                wasTriggered = true;
            }
            //if (onlyAcceptsOneObjectAtATime)
            //{
            //    if (colliderList.Count > 0)
            //    {
            //        return false;
            //    }
            //    if (!colliderList.Contains(collision))
            //    {
            //        colliderList.Add(collision);
            //    }
            //}
            return true;
        }

        public override void OnTriggerExit(Collider collision)
        {
            //if (onlyAcceptsOneObjectAtATime)
            //{
            //    if (colliderList.Contains(collision))
            //    {
            //        colliderList.Remove(collision);
            //    }
            //}
            base.OnTriggerExit(collision);
        }

        public override bool DoesThisCountAsExit(Collider collision)
        {
            if (isOneShot)
            {
                if (wasTriggered)
                {
                    return false;
                }
                wasTriggered = true;
            }
            if (!runsExit)
            {
                if (heldReaction)
                {
                    heldReaction._ExitTrigger();
                }
                heldReaction = null;
                return false;
            }
            return true;
        }

        public override void _Reset()
        {
            heldReaction = null;
            wasTriggered = false;
            colliderList.Clear();
        }
    }
}