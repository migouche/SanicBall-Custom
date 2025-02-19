using Sanicball.Data;
using UnityEngine;

namespace Sanicball.Gameplay
{
    [RequireComponent(typeof(TrailRenderer))]
    [RequireComponent(typeof(Rigidbody))]
    public class SpeedTrail : MonoBehaviour
    {
		[System.NonSerialized]
        public TrailRenderer tr;
		[System.NonSerialized]
		public bool changeWithSize = false;
		[System.NonSerialized]
		public float size = 1;

        // Use this for initialization
        private void Start()
        {
            tr = GetComponent<TrailRenderer>();
            tr.enabled = ActiveData.GameSettings.trails;
        }

        private void Update()
        {
            if (!tr.enabled) return;

            float spd = Mathf.Max(0, GetComponent<Rigidbody>().velocity.magnitude - 60);
            tr.time = Mathf.Clamp(spd / 20, 0, 5);
			if(changeWithSize) {
				tr.startWidth = Mathf.Clamp(spd / 80, 0, size*0.8f);
			}else{
				tr.startWidth = Mathf.Clamp(spd / 80, 0, 0.8f);
			}
            tr.material.mainTextureScale = new Vector2(tr.time * 100, 1);
            tr.material.mainTextureOffset = new Vector2((tr.material.mainTextureOffset.x - 2 * Time.deltaTime) % 1, 0);
        }
    }
}
