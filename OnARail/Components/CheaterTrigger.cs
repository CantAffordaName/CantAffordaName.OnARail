using NewHorizons.Utility;
using UnityEngine;

namespace OnARail.Components
{
    internal class CheaterTrigger : MonoBehaviour
    {
        private INewHorizons newHorizons = OnARail.Instance.newHorizons;
        private GameObject revealVolumes;

        private void Awake()
        {
            GameObject oceanPlanet = newHorizons.GetPlanet("Locomocean");
            if (oceanPlanet != null)
            {
                GameObject revealParent = SearchUtilities.Find("Locomocean_Body/Sector/RevealParent");
                if (revealParent != null)
                {
                    revealVolumes = revealParent;
                    revealVolumes.SetActive(false);
                }
            }
        }

        public virtual void OnTriggerEnter(Collider hitCollider)
        {
            if (hitCollider.attachedRigidbody == Locator.GetPlayerBody()._rigidbody)
            {
                if (Locator.GetShipLogManager().IsFactRevealed("RAILWAY_ENGINEROOM_ANGLERFISH_1"))
                {
                    revealVolumes.SetActive(true);
                }
            }
        }

        public static CheaterTrigger CreateCheaterTrigger(GameObject obj, Vector3 pos, float radius)
        {
            var volume = new GameObject("CheaterTrigger");
            volume.transform.parent = obj.transform;
            volume.transform.localPosition = pos;
            volume.layer = LayerMask.NameToLayer("BasicEffectVolume");

            var sphere = volume.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.radius = radius;

            var triggerVolume = volume.AddComponent<CheaterTrigger>();

            return triggerVolume;
        }
    }
}
