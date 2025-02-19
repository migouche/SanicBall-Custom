﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sanicball.Data;
using Sanicball.Logic;
using SanicballCore;
using UnityEngine;
using UnityEngine.UI;

namespace Sanicball.UI
{
    public class PlayerUI : MonoBehaviour
    {
        [SerializeField]
        private RectTransform fieldContainer;

        [SerializeField]
        private Text speedField = null;

        [SerializeField]
        private Text speedFieldLabel = null;

        [SerializeField]
        private Text lapField = null;

        [SerializeField]
        private Text timeField = null;

        [SerializeField]
        private Text checkpointTimeField = null;

        [SerializeField]
        private Text checkpointTimeDiffField = null;

        [SerializeField]
        private AudioClip checkpointSound;
        [SerializeField]
        private AudioClip respawnSound;

        [SerializeField]
        private Marker markerPrefab;

        [SerializeField]
        private RectTransform markerContainer;
		
		[System.NonSerialized]
		public string speedUnits = "fast/h";
		[System.NonSerialized]
		public string speedUnitsPlural = "fasts/h";
		[System.NonSerialized]
		public float speedPercent = 1;
		[System.NonSerialized]
		public float speedUnitsFontSize = 7*96/7;

        private Marker checkpointMarker;
        private List<Marker> playerMarkers = new List<Marker>();

        private RacePlayer targetPlayer;
        private RaceManager targetManager;

        private readonly Color finishedColor = new Color(0f, 0.5f, 1f);
		
		private bool controlTypeSet = false;
		
		//GO means GameObject
		private GameObject leftPowerupKbdGO;
		private GameObject rightPowerupKbdGO;
		private GameObject leftPowerupGamepadGO;
		private GameObject rightPowerupGamepadGO;
		private GameObject leftPowerupImageGO;
		private GameObject rightPowerupImageGO;
		
        public RacePlayer TargetPlayer
        {
            get { return targetPlayer; }
            set
            {
                if (targetPlayer != null)
                {
                    targetPlayer.NextCheckpointPassed -= TargetPlayer_NextCheckpointPassed;
                    targetPlayer.Respawned -= TargetPlayer_Respawned;
                    Destroy(checkpointMarker.gameObject);
                    foreach (Marker m in playerMarkers)
                    {
                        Destroy(m.gameObject);
                    }
                }

                targetPlayer = value;

                targetPlayer.NextCheckpointPassed += TargetPlayer_NextCheckpointPassed;
                targetPlayer.Respawned += TargetPlayer_Respawned;

                //Marker following next checkpoint
                checkpointMarker = Instantiate(markerPrefab);
                checkpointMarker.transform.SetParent(markerContainer, false);
                checkpointMarker.Text = "Checkpoint";
                checkpointMarker.Clamp = true;

                //Markers following each player
                for (int i = 0; i < TargetManager.PlayerCount; i++)
                {
                    RacePlayer p = TargetManager[i];
                    if (p == TargetPlayer) continue;

                    var playerMarker = Instantiate(markerPrefab);
                    playerMarker.transform.SetParent(markerContainer, false);
                    playerMarker.Text = p.Name;
                    playerMarker.Target = p.Transform;
                    playerMarker.Clamp = false;

                    //Disabled for now, glitchy as fuck
                    //playerMarker.HideImageWhenInSight = true;

                    Data.CharacterInfo character = ActiveData.Characters[p.Character];
                    //playerMarker.Sprite = character.icon;
                    Color c = character.color;
                    c.a = 0.2f;
                    playerMarker.Color = c;

                    playerMarkers.Add(playerMarker);
                }
            }
        }

        public RaceManager TargetManager
        {
            get { return targetManager; }
            set
            {
                targetManager = value;
            }
        }

        public Camera TargetCamera { get; set; }
        
        private void TargetPlayer_Respawned(object sender, bool penalty)	
        {
            UISound.Play(respawnSound);

            if (!penalty) return;

            checkpointTimeField.text = "Respawn lap time penalty";
            checkpointTimeField.GetComponent<ToggleCanvasGroup>().ShowTemporarily(2f);

            checkpointTimeDiffField.color = Color.red;
            checkpointTimeDiffField.text = "+" + Utils.GetTimeString(TimeSpan.FromSeconds(5));
            checkpointTimeDiffField.GetComponent<ToggleCanvasGroup>().ShowTemporarily(2f);
        }

        private void TargetPlayer_NextCheckpointPassed(object sender, NextCheckpointPassArgs e)
        {
            UISound.Play(checkpointSound);
            checkpointTimeField.text = Utils.GetTimeString(e.CurrentLapTime);
            checkpointTimeField.GetComponent<ToggleCanvasGroup>().ShowTemporarily(2f);

            if (TargetPlayer.LapRecordsEnabled)
            {
				CharacterTier tier = ActiveData.Characters[targetPlayer.Character].tier;
                string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                int stage = ActiveData.Stages.Where(a => a.sceneName == sceneName).First().id;

                float time = (float)e.CurrentLapTime.TotalSeconds;

                RaceRecord bestRecord = ActiveData.RaceRecords
                    .Where(a => a.Tier == tier && a.Stage == stage && a.GameVersion == GameVersion.AS_FLOAT && a.WasTesting == GameVersion.IS_TESTING)
                    .OrderBy(a => a.Time)
                    .FirstOrDefault();

                if (bestRecord != null)
                {
                    float diff = time - bestRecord.CheckpointTimes[e.IndexOfPreviousCheckpoint];

                    bool faster = diff < 0;
                    TimeSpan diffSpan = TimeSpan.FromSeconds(Mathf.Abs(diff));

                    checkpointTimeDiffField.text = (faster ? "-" : "+") + Utils.GetTimeString(diffSpan);
                    checkpointTimeDiffField.color = faster ? Color.blue : Color.red;
                    checkpointTimeDiffField.GetComponent<ToggleCanvasGroup>().ShowTemporarily(2f);

                    if (e.IndexOfPreviousCheckpoint == StageReferences.Active.checkpoints.Length - 1 && faster)
                    {
                        checkpointTimeDiffField.text = "New lap record!";
                    }
                }
                else
                {
                    if (e.IndexOfPreviousCheckpoint == StageReferences.Active.checkpoints.Length - 1)
                    {
                        checkpointTimeDiffField.text = "Lap record set!";
                        checkpointTimeDiffField.color = Color.blue;
                        checkpointTimeDiffField.GetComponent<ToggleCanvasGroup>().ShowTemporarily(2f);
                    }
                }
            }
        }

        private void Awake()
        {
			//Execute the UI Modifying per Track
			GameObject modifierGO = GameObject.Find("StageModifier");
			if(modifierGO != null){
				StageModifier modifier = (StageModifier)modifierGO.GetComponent(typeof(StageModifier));
				modifier.ModifyUI(gameObject);
			}
        }
		
		private void Start() {
			leftPowerupKbdGO = GetLocalGameObjectByPath("Container/LeftPowerup/Keyboard");
			rightPowerupKbdGO = GetLocalGameObjectByPath("Container/RightPowerup/Keyboard");
			leftPowerupGamepadGO = GetLocalGameObjectByPath("Container/LeftPowerup/Gamepad");
			rightPowerupGamepadGO = GetLocalGameObjectByPath("Container/RightPowerup/Gamepad");
			leftPowerupImageGO = GetLocalGameObjectByPath("Container/LeftPowerup/Image");
			rightPowerupImageGO = GetLocalGameObjectByPath("Container/RightPowerup/Image");
            GetLocalGameObjectByPath("Container/Minimap").SetActive(!Application.isMobilePlatform);
            GetLocalGameObjectByPath("Container/LeftPowerup").SetActive(ActiveData.MatchSettings.PowerupsEnabled);
            GetLocalGameObjectByPath("Container/RightPowerup").SetActive(ActiveData.MatchSettings.PowerupsEnabled);
            GetLocalGameObjectByPath("Container/Minimap").SetActive(ActiveData.GameSettings.minimapEnabled);
        }

        private void Update()
        {
            if (TargetCamera)
            {
                fieldContainer.anchorMin = TargetCamera.rect.min;
                fieldContainer.anchorMax = TargetCamera.rect.max;
            }

            if (TargetPlayer == null || TargetManager == null) return;
			
			if(!controlTypeSet && ActiveData.MatchSettings.PowerupsEnabled){
				if(TargetPlayer.CtrlType != ControlType.Keyboard) {
					if(leftPowerupKbdGO != null && rightPowerupKbdGO != null) {
						leftPowerupKbdGO.SetActive(false);
						rightPowerupKbdGO.SetActive(false);
						controlTypeSet = true;
					}
				}else if(TargetPlayer.CtrlType == ControlType.Keyboard) {
					if(leftPowerupGamepadGO != null && rightPowerupGamepadGO != null) {
						leftPowerupGamepadGO.SetActive(false);
						rightPowerupGamepadGO.SetActive(false);
						controlTypeSet = true;
					}
				}
			}

            float speed = TargetPlayer.Speed*speedPercent;
            string postfix = " ";

            //Speed label
            if (!ActiveData.GameSettings.useImperial)
            {
                postfix += (Mathf.Floor(speed) == 1f) ? speedUnits : speedUnitsPlural;
            }
            else
            {
                speed *= 0.62f;
                postfix += (Mathf.Floor(speed) == 1f) ? "lightspeed" : "lightspeeds";
                speedFieldLabel.fontSize = 62;
            }

            //Speed field size and color
            var min = 96;
            var max = 150;
            var size = max - (max - min) * Mathf.Exp(-speed * 0.02f);
            speedField.fontSize = (int)size;
            speedField.text = Mathf.Floor(speed).ToString();
			speedFieldLabel.fontSize = (int)speedUnitsFontSize;
            speedFieldLabel.text = postfix;

            //Lap counter
            if (!TargetPlayer.RaceFinished)
            {
                lapField.text = "Lap " + TargetPlayer.Lap + "/" + TargetManager.Settings.Laps;
            }
            else
            {
                if (TargetPlayer.FinishReport.Disqualified)
                {
                    lapField.text = "Disqualified";
                    lapField.color = Color.red;
                }
                else
                {
                    lapField.text = "Race finished";
                    lapField.color = finishedColor;
                }
            }

            //Race time
            TimeSpan timeToUse = TargetManager.RaceTime;
            if (TargetPlayer.FinishReport != null)
            {
                timeToUse = TargetPlayer.FinishReport.Time;
                timeField.color = finishedColor;
            }
            timeField.text = Utils.GetTimeString(timeToUse);

            if (TargetPlayer.Timeout > 0)
            {
                timeField.text += Environment.NewLine + "<b>Timeout</b> " + Utils.GetTimeString(TimeSpan.FromSeconds(TargetPlayer.Timeout));
            }

            //Checkpoint marker
            if (TargetPlayer.NextCheckpoint != null)
                checkpointMarker.Target = TargetPlayer.NextCheckpoint.transform;
            else
                checkpointMarker.Target = null;
            checkpointMarker.CameraToUse = TargetCamera;

            playerMarkers.RemoveAll(a => a == null); //Remove destroyed markers from the list (Markers are destroyed if the player they're following leaves)
            foreach (Marker m in playerMarkers.ToList())
            {
                m.CameraToUse = TargetCamera;
            }
			
			//Powerup Section
			if(TargetPlayer.ball.changeUIPowerups) {
				if(leftPowerupImageGO != null){
                    Image leftPowerup = leftPowerupImageGO.GetComponent<Image>();//(typeof(Image)) as Image;
					if(TargetPlayer.Powerups[0] != null) {
						leftPowerupImageGO.SetActive(true);
						leftPowerup.sprite = TargetPlayer.Powerups[0].image;
					}else {
						leftPowerupImageGO.SetActive(false);
					}
				}
				
				if(rightPowerupImageGO != null){
                    Image rightPowerup = rightPowerupImageGO.GetComponent<Image>();//(typeof(Image)) as Image;
					if(TargetPlayer.Powerups[1] != null) {
						rightPowerupImageGO.SetActive(true);
						rightPowerup.sprite = TargetPlayer.Powerups[1].image;
					}else {
						rightPowerupImageGO.SetActive(false);
					}
				}
				TargetPlayer.ball.changeUIPowerups = false;
			}
        }
		
		private GameObject GetLocalGameObjectByPath(string path){
			var goTransform = transform.Find(path);
			if(goTransform != null) {
				return goTransform.gameObject;
			}
			return null;
		}
    }
}