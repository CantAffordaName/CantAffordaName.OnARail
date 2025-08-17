using NewHorizons.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace OnARail.Components
{
    internal class CoordPromptTrigger : MonoBehaviour
    {
        public GameObject coordScreenPrompt;

        //Have to wait until player gains control before ScreenPromptList is filled!
        public void SetupPrompt()
        {
            GameObject screenPromptList = SearchUtilities.Find("ScreenPromptCanvas/ScreenPromptListBottomLeft");
            GameObject originalPrompt = screenPromptList.transform.GetChild(1).gameObject;
            coordScreenPrompt = Instantiate(originalPrompt, screenPromptList.transform);

            Text text = coordScreenPrompt.transform.GetChild(0).gameObject.GetComponent<Text>();
            text.text = "Coordinates: ";
        }

        public virtual void OnTriggerEnter(Collider hitCollider)
        {
            if (hitCollider.attachedRigidbody == Locator.GetPlayerBody()._rigidbody)
            {
                if (Locator.GetShipLogManager().IsFactRevealed("SPRUCECABOOSE_COORDS_FACT"))
                {
                    coordScreenPrompt.SetActive(true);
                }
            }
        }

        public virtual void OnTriggerExit(Collider hitCollider)
        {
            if (hitCollider.attachedRigidbody == Locator.GetPlayerBody()._rigidbody)
            {
                coordScreenPrompt.SetActive(false);
            }
        }

        public static CoordPromptTrigger CreateCoordPromptTrigger(GameObject obj)
        {
            var volume = new GameObject("CoordPromptTrigger");
            volume.transform.parent = obj.transform;
            volume.transform.localPosition = Vector3.zero;
            volume.transform.localScale = new Vector3(12f, 9f, 15f);
            volume.layer = LayerMask.NameToLayer("BasicEffectVolume");

            var cube = volume.AddComponent<BoxCollider>();
            cube.isTrigger = true;

            var triggerVolume = volume.AddComponent<CoordPromptTrigger>();

            return triggerVolume;
        }
    }
}
