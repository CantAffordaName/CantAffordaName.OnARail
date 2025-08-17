using NewHorizons.Utility;
using UnityEngine;

namespace OnARail.Components
{
    internal class CoordMusicTrigger : MonoBehaviour
    {
        public bool shouldPlay = false;
        public AudioSource whistleAudio;

        public virtual void OnTriggerEnter(Collider hitCollider)
        {
            if (hitCollider.attachedRigidbody == Locator.GetPlayerBody()._rigidbody)
            {
                if (shouldPlay)
                {
                    //OnARail.DebugLog("Found Coords: Begin Music!", OWML.Common.MessageType.Success);
                    //Disable for Final Voyage, but re-enable the loop later to prevent game over!
                    TimeLoop.SetTimeLoopEnabled(false);
                    GlobalMessenger<OWRigidbody>.FireEvent("ExitTimeLoopCentral", Locator.GetPlayerBody());
                    whistleAudio.volume = 0.5f;
                }
                /*else
                {
                    OnARail.DebugLog("Found Coords: Advanced Core needs to be collected first!", OWML.Common.MessageType.Success);
                }*/
            }
        }

        public static CoordMusicTrigger CreateCoordMusicTrigger(GameObject planet, float radius)
        {
            var volume = new GameObject("CoordMusicTrigger");
            volume.transform.parent = planet.transform;
            volume.transform.localPosition = Vector3.zero;
            volume.layer = LayerMask.NameToLayer("BasicEffectVolume");

            var sphere = volume.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.radius = radius;

            var triggerVolume = volume.AddComponent<CoordMusicTrigger>();

            return triggerVolume;
        }
    }
}