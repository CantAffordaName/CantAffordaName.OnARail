using UnityEngine;

namespace OnARail.Components
{
    internal class EndMusicTrigger : MonoBehaviour
    {
        public CoordMusicTrigger coordMusicTrigger;
        public AudioSource whistleAudio;

        public virtual void OnTriggerExit(Collider hitCollider)
        {
            if (hitCollider.attachedRigidbody == Locator.GetPlayerBody()._rigidbody)
            {
                if (Locator.GetShipLogManager().IsFactRevealed("SPRUCECABOOSE_COORDS_FACT"))
                {
                    //OnARail.DebugLog("Exiting: Begin Music!", OWML.Common.MessageType.Success);
                    //Disable for Final Voyage, but re-enable the loop later to prevent game over!
                    TimeLoop.SetTimeLoopEnabled(false);
                    GlobalMessenger<OWRigidbody>.FireEvent("ExitTimeLoopCentral", Locator.GetPlayerBody());
                    whistleAudio.volume = 0.5f;
                }
                else
                {
                    //OnARail.DebugLog("Exiting: Fact needs to be learned first! Will play if coords are found this loop!", OWML.Common.MessageType.Success);
                    coordMusicTrigger.shouldPlay = true;
                }
            }
        }

        public static EndMusicTrigger CreateEndMusicTrigger(GameObject obj, float radius)
        {
            var volume = new GameObject("EndMusicTrigger");
            volume.transform.parent = obj.transform;
            volume.transform.localPosition = Vector3.zero;
            volume.layer = LayerMask.NameToLayer("BasicEffectVolume");

            var sphere = volume.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.radius = radius;

            var triggerVolume = volume.AddComponent<EndMusicTrigger>();

            return triggerVolume;
        }
    }
}