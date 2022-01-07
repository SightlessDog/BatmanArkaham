namespace GameCreator.Melee
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class TargetMelee : MonoBehaviour
    {
        private Dictionary<int, CharacterMelee> trackers = new Dictionary<int, CharacterMelee>();

        public void SetTracker(CharacterMelee melee)
        {
            int meleeID = melee.GetInstanceID();
            if (this.trackers.ContainsKey(meleeID)) return;
            this.trackers.Add(meleeID, melee);
        }

        public void ReleaseTracker(CharacterMelee melee)
        {
            int meleeID = melee.GetInstanceID();
            this.trackers.Remove(meleeID);
        }

        private void OnDisable()
        {
            foreach(KeyValuePair<int, CharacterMelee> item in this.trackers)
            {
                if (item.Value == null) continue;
                item.Value.ReleaseTracker(item.Value);
            }

            this.trackers.Clear();
        }
    }
}