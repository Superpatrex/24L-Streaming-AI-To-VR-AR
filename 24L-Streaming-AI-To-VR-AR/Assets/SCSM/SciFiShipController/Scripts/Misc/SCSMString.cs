using System.Collections.Generic;
using UnityEngine;

// Copyright (c) 2018-2023 SCSM Pty Ltd. All rights reserved.
namespace scsmmedia
{
    /// <summary>
    /// This class provides a subset of functions of string class which
    /// will do less memory allocations and should result in less garabage.
    /// It is created for specific use within SCSM assets.
    /// NOTE: It only deals with fixed length strings.
    /// </summary>
    public class SCSMString
    {
        #region private variables

        private char[] charArray = null;
        private int currentCapacity = 0;
        private string outputString = null;
        // Is the output ok or does it need to be updated?
        private bool isDirty = true;
        private int endIdxPos = 0;
        private char[] numArray = null;
        private int intMaxLength = 9;
        private int intMaxValue = 0;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a string and pre-allocate enough space or capacity
        /// for the expected whole string.
        /// </summary>
        /// <param name="capacity"></param>
        public SCSMString(int capacity)
        {
            // allocate space for the total string
            currentCapacity = capacity;
            charArray = new char[currentCapacity];
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Faster version of Mathf.Pow for integer exponents and bases
        /// Works with pow range 0 to 9.
        /// </summary>
        /// <param name="num"></param>
        /// <param name="pow"></param>
        /// <returns></returns>
        private int IntPow(int num, int pow)
        {
            if (pow != 0)
            {
                int ans = num;
                for (int i = 1; i < pow; i++)
                {
                    ans *= num;
                }
                return ans;
            }
            else { return 1; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Set the contents of the current string. This will overwrite
        /// any current contents in the string.
        /// Characters that overflow the capacity will be truncated
        /// </summary>
        /// <param name="contents"></param>
        public void Set(string contents)
        {
            Empty();
            Add(contents);
        }

        /// <summary>
        /// Add an integer to the end of the current string. Numbers that
        /// don't fit will be truncated. If leftJustify is false and the number
        /// is positive, a space will be inserted before the number.
        /// </summary>
        /// <param name="value"></param>
        public void Set(int value, bool leftJustify = true)
        {
            Empty();
            Add(value, leftJustify);
        }

        /// <summary>
        /// Add a string onto the end of the current string. Characters
        /// that don't fit will be truncated.
        /// NOTE: this is not the same as Append (coming later). 
        /// </summary>
        /// <param name="contents"></param>
        public void Add(string contents)
        {
            int contentsLength = contents.Length;

            // will the for-loop update the array?
            if (contentsLength > 0 && endIdxPos < currentCapacity) { isDirty = true; }

            for (int i = 0; i < contentsLength && endIdxPos < currentCapacity; i++)
            {
                charArray[endIdxPos] = contents[i];
                endIdxPos++;
            }
        }

        /// <summary>
        /// Add a char to the end of the current string. If it doesn't fit,
        /// it will not be added.
        /// </summary>
        /// <param name="value"></param>
        public void Add(char value)
        {
            if (endIdxPos < currentCapacity)
            {
                charArray[endIdxPos++] = value;
                isDirty = true;
            }
        }

        /// <summary>
        /// Add a space to the end of the current string. If it doesn't fit,
        /// it will not be added.
        /// </summary>
        public void AddSpace()
        {
            if (endIdxPos < currentCapacity)
            {
                charArray[endIdxPos++] = ' ';
                isDirty = true;
            }
        }

        /// <summary>
        /// Add an integer to end of the current string. Numbers that
        /// don't fit will be truncated. If leftJustify is false and the number
        /// is positive, a space will be inserted before the number.
        /// Limits: +/- 999,999,999
        /// </summary>
        /// <param name="value"></param>
        /// <param name="leftJustify"></param>
        public void Add(int value, bool leftJustify = true)
        {
            if (value < 0) { Add("-"); value = -value; }
            // if +ve and right justified, add a space for the (missing) sign
            else if (!leftJustify) { Add(" "); }

            if (numArray == null) { SetMaxIntLength(intMaxLength); }

            if (value < intMaxValue + 1 && numArray != null)
            {
                int numDigits = 0;

                // fill array with null characters for safety
                for (int i=0; i < intMaxLength; i++) { numArray[i] = '\0'; } 

                // We don't know how many digits there are just yet, so process the number in reverse
                do
                {
                    // ASCII (and therefore ANSI) codes for numerals go from 48 (0) to 57 (9)
                    numArray[numDigits++] = (char)(48 + value % 10);
                    // Remove the last digit
                    value /= 10;
                } while (value != 0);

                // Copy them into the actual array
                for (int i = numDigits-1; i >= 0 && endIdxPos < currentCapacity; i--)
                {
                    charArray[endIdxPos++] = numArray[i];
                }
            }

            isDirty = true;
        }

        /// <summary>
        /// Empty or clear the string
        /// </summary>
        public void Empty()
        {
            endIdxPos = 0;
            isDirty = true;
        }

        /// <summary>
        /// Is the string empty?
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return endIdxPos == 0;
        }

        /// <summary>
        /// When adding integers to the string, an array is created to hold the
        /// incoming numerals before they are add to the string. By default this
        /// is created with 10 digits if/when the first Add(int) is called.
        /// To change this allocation, call this before first calling Add(int).
        /// </summary>
        /// <param name="value"></param>
        public void SetMaxIntLength(int value)
        {
            // Max int should be 2,147,483,647 but our faster IntPow() only returns max 1.4 billion for some reason
            // So set limit to 9 digits rather than 10.
            if (value < 0 || value > 9)
            {
                #if UNITY_EDITOR
                Debug.LogWarning("ERROR: SCSMString.SetMaxIntLength value must be between 0 and 9");
                #endif
            }   
            else if (value > 0 && numArray == null)
            {
                intMaxLength = value;
                numArray = new char[intMaxLength];

                intMaxValue = IntPow(10, intMaxLength) - 1;
                //intMaxValue = (int)System.Math.Pow(10, intMaxLength) - 1;               
            }
        }

        public override string ToString()
        {
            // The output string needs to be update
            if (isDirty)
            {
                isDirty = false;
                outputString = new string(charArray, 0, endIdxPos);
            }
            return outputString;
        }


        #endregion

        #region Static Format Time methods

        /// <summary>
        /// Returns the time given the hours and minutes
        /// Displays -- if the values exceed their limits.
        /// For the sake of performance, assumes all values are +ve.
        /// </summary>
        /// <param name="hours"></param>
        /// <param name="minutes"></param>
        /// <returns></returns>
        public static string FormatTime(int hours, int minutes)
        {
            SCSMString sg = new SCSMString(6);

            if (hours == 0) { sg.Set("00"); }
            else if (hours < 10) { sg.Set("0"); sg.Add(hours); }
            else if (hours < 99) { sg.Set(hours); }
            else { sg.Set("--"); }
            sg.Add(":");

            if (minutes > 59) { sg.Add("--"); }
            else if (minutes > 9) { sg.Add(minutes); }
            else { sg.Add("0"); sg.Add(minutes); }

            return sg.ToString();
        }

        /// <summary>
        /// Returns the time given the hours, minutes, and seconds
        /// Displays -- if the values exceed their limits.
        /// For the sake of performance, assumes all values are +ve.
        /// </summary>
        /// <param name="hours"></param>
        /// <param name="minutes"></param>
        /// <param name="secs"></param>
        /// <returns></returns>
        public static string FormatTime(int hours, int minutes, int secs)
        {
            SCSMString sg = new SCSMString(15);

            // Deal with most common scenarios first
            if (hours == 0) { sg.Set("00"); }
            else if (hours < 10) { sg.Set("0"); sg.Add(hours); }
            else if (hours < 99) { sg.Set(hours); }
            else { sg.Set("--"); }
            sg.Add(":");

            if (minutes > 59) { sg.Add("--"); }
            else if (minutes > 9) { sg.Add(minutes); }
            else { sg.Add("0"); sg.Add(minutes); }
            sg.Add(":");

            if (secs > 59) { sg.Add("--"); }
            else if (secs > 9) { sg.Add(secs); }
            else { sg.Add("0"); sg.Add(secs); }

            return sg.ToString();
        }

        /// <summary>
        /// Return the time given the hours, minutes, seconds and milliseconds.
        /// Show msecs as tenths or hundredths of a second.
        /// Display -- or --- if the values exceed their limits.
        /// For the sake of performance, assumes all values are +ve.
        /// </summary>
        /// <param name="hours"></param>
        /// <param name="minutes"></param>
        /// <param name="secs"></param>
        /// <param name="msecs"></param>
        /// <param name="showTenths"></param>
        /// <returns></returns>
        public static string FormatTime(int hours, int minutes, int secs, int msecs, bool showTenths)
        {
            // Equivalent to: String.Format("{0:00}:{1:00}:{2:00}." + (showTenths ? "{3:00}" : "{3:000}"), hours, minutes, seconds, msecs);

            SCSMString sg = new SCSMString(15);

            // Deal with most common scenarios first
            if (hours == 0) { sg.Set("00"); }
            else if (hours < 10) { sg.Set("0"); sg.Add(hours); }
            else if (hours < 99) { sg.Set(hours); }
            else { sg.Set("--"); }
            sg.Add(":");

            if (minutes > 59) { sg.Add("--"); }
            else if (minutes > 9) { sg.Add(minutes); }
            else { sg.Add("0"); sg.Add(minutes); }
            sg.Add(":");

            if (secs > 59) { sg.Add("--"); }
            else if (secs > 9) { sg.Add(secs); }
            else { sg.Add("0"); sg.Add(secs); }
            sg.Add(".");

            if (showTenths)
            {
                if (msecs > 99) { sg.Add("--"); }
                else if (msecs > 9) { sg.Add(msecs); }
                else { sg.Add("0"); sg.Add(msecs); }
            }
            else
            {
                if (msecs > 999) { sg.Add("---"); }
                else if (msecs > 99) { sg.Add(msecs); }
                else if (msecs > 9) { sg.Add("0"); sg.Add(msecs); }
                else { sg.Add("00"); sg.Add(msecs); }
            }

            return sg.ToString();
        }

        /// <summary>
        /// Return the time given the minutes, seconds and milliseconds.
        /// Show msecs as tenths or hundredths of a second.
        /// Display -- or --- if the values exceed their limits.
        /// For the sake of performance, assumes all values are +ve.
        /// </summary>
        /// <param name="minutes"></param>
        /// <param name="secs"></param>
        /// <param name="msecs"></param>
        /// <param name="showTenths"></param>
        /// <returns></returns>
        public static string FormatTime(int minutes, int secs, int msecs, bool showTenths)
        {
            // Equivalent to: String.Format("{0:00}:{1:00}." + (showTenths ? "{2:00}" : "{2:000}"), minutes, seconds, msecs);

            SCSMString sg = new SCSMString(10);

            if (minutes > 59) { sg.Set("--"); }
            else if (minutes > 9) { sg.Set(minutes); }
            else { sg.Set("0"); sg.Add(minutes); }
            sg.Add(":");

            if (secs > 59) { sg.Add("--"); }
            else if (secs > 9) { sg.Add(secs); }
            else { sg.Add("0"); sg.Add(secs); }
            sg.Add(".");

            if (showTenths)
            {
                if (msecs > 99) { sg.Add("--"); }
                else if (msecs > 9) { sg.Add(msecs); }
                else { sg.Add("0"); sg.Add(msecs); }
            }
            else
            {
                if (msecs > 999) { sg.Add("---"); }
                else if (msecs > 99) { sg.Add(msecs); }
                else if (msecs > 9) { sg.Add("0"); sg.Add(msecs); }
                else { sg.Add("00"); sg.Add(msecs); }
            }

            return sg.ToString();
        }

        #endregion
    }
}