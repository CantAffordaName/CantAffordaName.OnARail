using NewHorizons.Utility;
using OWML.ModHelper;
using System.Collections;
using UnityEngine;

namespace OnARail.Components
{
    public class CoordInterfaceController : MonoBehaviour
    {
        private NomaiCoordinateInterface _interface;
        private INewHorizons newHorizons = OnARail.Instance.newHorizons;

        //Coords start at top left, increments clockwise
        private int[] coord1 = new int[] { 0, 3, 4, 2 };
        private int[] coord2 = new int[] { 0, 4, 3, 1 };
        private int[] coord3 = new int[] { 2, 5, 0, 3, 1};

        void Awake()
        {
            GameObject.Destroy(GetComponentInChildren<EyeCoordinatePromptTrigger>().gameObject);
            _interface = GetComponent<NomaiCoordinateInterface>();

            _interface._raisePillarSlot.OnSlotActivated += (NomaiInterfaceSlot _) =>
            {
                foreach (var node in _interface._nodeControllers)
                {
                    //Eye coords cause a soft lock, have to reset
                    node.ResetNodes();
                }
                _interface.SetPillarRaised(true, true);
            };
            _interface._lowerPillarSlot.OnSlotActivated += (NomaiInterfaceSlot _) => _interface.SetPillarRaised(false, false);
            _interface._lowerPillarSlot.OnSlotActivated += (NomaiInterfaceSlot _) => CheckIfCorrect();
        }

        void Start()
        {
            _interface._upperOrb.SetOrbPosition(_interface._raisePillarSlot.transform.TransformPoint(new Vector3(0, 2.6f, 0)));
        }

        void CheckIfCorrect()
        {
            if (IsCorrect())
            {
                //OnARail.DebugLog("Correctly entered the coords!", OWML.Common.MessageType.Success);
                //Doesn't deactivate properly, destroy it
                Destroy(_interface._upperOrb);
                //OnARail.SolvedCoords();

                GameObject train = newHorizons.GetPlanet("Stellar Express");
                if (train != null)
                {
                    GameObject warpSwitch = SearchUtilities.Find("StellarExpress_Body/Sector/TrainInterface/WarpSwitch");
                    GameObject pillarRoot = SearchUtilities.Find("StellarExpress_Body/Sector/TrainInterface/PillarPivot/PillarRoot");

                    warpSwitch.transform.SetParent(pillarRoot.transform);
                    warpSwitch.transform.localPosition = new Vector3(0, 0.4f, 0);

                    GameObject endingGM = SearchUtilities.Find("StellarExpress_Body/Sector/EndingGM");
                    GameObject trainWhistle = SearchUtilities.Find("StellarExpress_Body/Sector/TrainBase/WhistleController");
                    endingGM.SetActive(true);
                    Destroy(trainWhistle);
                    StartCoroutine(EndingCoroutine(endingGM));
                }

                GameObject star = newHorizons.GetPlanet("The Stellar Express");
                if (star != null)
                {
                    GameObject destructionVolume = SearchUtilities.Find("TheStellarExpress_Body/Sector/Star/DestructionFluidVolume");
                    GameObject planetDestructionVolume = SearchUtilities.Find("TheStellarExpress_Body/Sector/Star/PlanetDestructionVolume");

                    destructionVolume.SetActive(false);
                    planetDestructionVolume.SetActive(false);
                }
            }
        }

        private IEnumerator EndingCoroutine(GameObject endingGM)
        {
            yield return new WaitForSeconds(7.8f);
            GameObject revealVolume = SearchUtilities.Find("StellarExpress_Body/Sector/RevealFinal");
            revealVolume.SetActive(true);
            endingGM.SetActive(false);
            yield return new WaitForSeconds(0.2f);
            GameObject creditsVolume = SearchUtilities.Find("StellarExpress_Body/Sector/LoadCreditsVolume");
            creditsVolume.SetActive(true);
        }

        bool IsCorrect()
        {
            bool correct = CheckCoords();
            return correct;
        }

        bool CheckCoords()
        {
            bool first = _interface._nodeControllers[0].CheckCoordinate(coord1);
            bool second = _interface._nodeControllers[1].CheckCoordinate(coord2);
            bool third = _interface._nodeControllers[2].CheckCoordinate(coord3);
            //OnARail.DebugLog("1: " + first + " | 2: " + second + " | 3: " + third, OWML.Common.MessageType.Info);

            return first && second && third;
        }
    }
}