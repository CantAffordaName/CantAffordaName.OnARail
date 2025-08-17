using NewHorizons.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace OnARail.Components
{
    internal class WaterTrigger : MonoBehaviour
    {
        private INewHorizons newHorizons = OnARail.Instance.newHorizons;
        public GameObject[] ghostMatterArray;
        public GameObject visorEffects;
        public float wetTimer = 60f;

        private void Awake()
        {
            GameObject star = newHorizons.GetPlanet("The Stellar Express");
            if (star != null)
            {
                GameObject visorRoot = SearchUtilities.Find("TheStellarExpress_Body/Sector/VisorEffects");
                if (visorRoot != null)
                {
                    visorEffects = visorRoot;
                }
                else { OnARail.DebugLog("Can't find VisorEffects!", OWML.Common.MessageType.Error); }
            }

            List<GameObject> ghostMatterList = new List<GameObject>();

            GameObject frozenPlanet = newHorizons.GetPlanet("Frost Car");
            if (frozenPlanet != null)
            {
                GameObject ghostMatterFrozen = SearchUtilities.Find("FrostCar_Body/Sector/FrozenInterior/GM_Root_Main");
                if (ghostMatterFrozen != null)
                {
                    ghostMatterList.Add(ghostMatterFrozen);
                }
                else { OnARail.DebugLog("Can't find GM_Root_Main!", OWML.Common.MessageType.Error); }
            }

            GameObject sprucePlanet = newHorizons.GetPlanet("Spruce Caboose");
            if (frozenPlanet != null)
            {
                GameObject ghostMatterFrozen = SearchUtilities.Find("SpruceCaboose_Body/Sector/SpruceBase/GM_Root_Main");
                if (ghostMatterFrozen != null)
                {
                    ghostMatterList.Add(ghostMatterFrozen);
                }
                else { OnARail.DebugLog("Can't find GM_Root_Main!", OWML.Common.MessageType.Error); }
            }

            ghostMatterArray = ghostMatterList.ToArray();
        }

        private void Update()
        {
            var currentTime = TimeLoop.GetMinutesElapsed();

            if (currentTime - wetTimer > 1)
            {
                //OnARail.DebugLog("60 seconds have passed, ghost matter reactivated!", OWML.Common.MessageType.Info);
                GhostMatterSetActive(true);
            }
        }

        public virtual void OnTriggerExit(Collider hitCollider)
        {
            if (hitCollider.attachedRigidbody == Locator.GetPlayerBody()._rigidbody)
            {
                //OnARail.DebugLog("Player exiting water, 60 seconds remaining!", OWML.Common.MessageType.Info);
                wetTimer = TimeLoop.GetMinutesElapsed();
                GhostMatterSetActive(false);
            }
        }

        private void GhostMatterSetActive(bool active)
        {
            visorEffects.SetActive(!active);

            foreach (GameObject obj in ghostMatterArray)
            {
                obj.SetActive(active);
            }
        }

        public static WaterTrigger CreateWaterTrigger(GameObject planet, float radius)
        {
            var volume = new GameObject("WaterTrigger");
            volume.transform.parent = planet.transform;
            volume.transform.localPosition = Vector3.zero;
            volume.layer = LayerMask.NameToLayer("BasicEffectVolume");

            var sphere = volume.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.radius = radius;

            var triggerVolume = volume.AddComponent<WaterTrigger>();

            return triggerVolume;
        }
    }
}