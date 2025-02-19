﻿using System;
using SanicballCore;

namespace Sanicball.Data
{

    [Serializable]
    public class RaceRecord
    {
		private CharacterTier tier;
        private float time;
        private DateTime date;
        private int stage;
        private int character;
        private float[] checkpointTimes;
        private float rawTime;
        private float gameVersion;
        private bool wasTesting;
        private bool hadPowerups;

        public CharacterTier Tier { get { return tier; } }
        public float Time { get { return time; } }
        public float RawTime { get { return rawTime; } }
        public DateTime Date { get { return date; } }
        public int Stage { get { return stage; } }
        public int Character { get { return character; } }
        public float[] CheckpointTimes { get { return checkpointTimes; } }
        public float GameVersion { get { return gameVersion; } }
        public bool WasTesting { get { return wasTesting; } }
        public bool HadPowerups { get { return hadPowerups; } }

        public void SetTier(CharacterTier newTier) {
            tier = newTier;
        }

        public RaceRecord(CharacterTier tier, float time, float rawTime, DateTime date, int stage, int character, float[] checkpointTimes, float gameVersion, bool isTesting, bool hadPowerups)
        {
            this.tier = tier;
            this.time = time;
            this.rawTime = rawTime;
            this.date = date;
            this.stage = stage;
            this.character = character;
            this.checkpointTimes = checkpointTimes;
            this.gameVersion = gameVersion;
            this.wasTesting = isTesting;
            this.hadPowerups = hadPowerups;
        }
    }
}