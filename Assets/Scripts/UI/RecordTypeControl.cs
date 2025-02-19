﻿using System;
using Sanicball.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Sanicball.UI
{
    public class RecordTypeControl : MonoBehaviour
    {
		public Text titleField;
        public Text timeField;
        public Text rawTimeField;
        public Text characterField;
        public Text dateField;
        public Text conditionsField;
        public Color timeColor = new Color(50 / 255f, 50 / 255f, 50 / 255f);
        public Color noRecordColor = new Color(0.5f,0.5f,0.5f);
        public static DateTime conditionsAddedDate = new DateTime(2021, 5, 8);
        public static DateTime rawTimeFixedDate = new DateTime(2021, 5, 6);

        public void SetRecord(RaceRecord r)
        {
            if (r != null)
            {
                var timespan = TimeSpan.FromSeconds(r.Time);
                timeField.color = timeColor;
                timeField.text = string.Format("{0:00}:{1:00}.{2:000}", timespan.Minutes, timespan.Seconds, timespan.Milliseconds);
                characterField.text = "Set with " + ActiveData.Characters[r.Character].name;
                dateField.text = r.Date.ToString();
                if(r.RawTime != 0 && r.Date > rawTimeFixedDate) {
                    var rawTimespan = TimeSpan.FromSeconds(r.RawTime);
                    rawTimeField.text = string.Format("{0:00}:{1:00}.{2:000}", rawTimespan.Minutes, rawTimespan.Seconds, rawTimespan.Milliseconds);
                    rawTimeField.color = timeColor;
                } else{
                    rawTimeField.text = "No Raw Time Found.";
                    rawTimeField.color = noRecordColor;
                }
                conditionsField.text = "";
                if (r.Date > conditionsAddedDate) {
                    conditionsField.text += r.HadPowerups ? (char)65 : (char)66;
                }else {
                    conditionsField.text += (char)67;
                }
            }
            else
            {
                timeField.text = "No records found";
                timeField.color = noRecordColor;
                rawTimeField.text = "";
                characterField.text = "";
                dateField.text = "";
                conditionsField.text = "";
            }
        }
    }
}