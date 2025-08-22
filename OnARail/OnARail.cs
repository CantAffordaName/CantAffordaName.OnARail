using OWML.Common;
using OWML.ModHelper;
using NewHorizons.Utility;
using UnityEngine;
using OnARail.Components;
using System.Collections.Generic;
using System.Linq;
using NewHorizons.Components.Orbital;

namespace OnARail
{
    public class OnARail : ModBehaviour
    {
        public INewHorizons newHorizons;
        public static OnARail Instance;
        public Material porcelain, silver, black;
        private GameObject[] fishies = new GameObject[10];
        private GameObject warpScale;
        private AudioSource whistleAudio;

        private void Awake()
        {
            Instance = this;
        }

        internal void Start()
        {
            //ModHelper.Console.WriteLine($"My mod {nameof(OnARail)} is loaded!", MessageType.Success);

            // Get the New Horizons API and load configs
            newHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
            newHorizons.LoadConfigs(this);

            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                if (loadScene != OWScene.SolarSystem) return;
                //ModHelper.Console.WriteLine("Loaded into solar system with On A Rail!", MessageType.Success);
            };

            newHorizons.GetStarSystemLoadedEvent().AddListener(OnStarSystemLoaded);
            GlobalMessenger<DeathType>.AddListener("PlayerDeath", OnPlayerDeath);
            GlobalMessenger.AddListener("TriggerSupernova", OnTriggerSupernova);
            GlobalMessenger.AddListener("PutOnHelmet", OnPutOnHelmet);
            GlobalMessenger.AddListener("EnterShip", OnEnterShip);
            GlobalMessenger.AddListener("ExitShip", OnExitShip);
        }

        private void OnStarSystemLoaded(string systemName)
        {
            newHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");

            GameObject star = newHorizons.GetPlanet("The Stellar Express");
            if (star != null)
            {
                GameObject visorEffects = SearchUtilities.Find("TheStellarExpress_Body/Sector/VisorEffects");
                if (visorEffects != null)
                {
                    VisorRainEffectVolume visorRainEffectVolume = SearchUtilities.Find("TheStellarExpress_Body/Sector/VisorEffects/WetEffectVolume2").GetComponent<VisorRainEffectVolume>();
                    visorRainEffectVolume._rainDirection = VisorRainEffectVolume.RainDirection.Linear;
                    visorEffects.SetActive(false);
                }
                else { ModHelper.Console.WriteLine("Can't find VisorEffects!", MessageType.Error); }
            }
            else
            {
                ModHelper.Console.WriteLine("Couldn't locate planet: The Stellar Express!", MessageType.Error);
            }

            GameObject trainPlanet = newHorizons.GetPlanet("Stellar Express");
            if (trainPlanet != null)
            {
                GameObject trainWhistle = SearchUtilities.Find("StellarExpress_Body/Sector/TrainBase/WhistleController");
                if (trainWhistle != null)
                {
                    trainWhistle.AddComponent<WhistleController>();
                    whistleAudio = SearchUtilities.Find("StellarExpress_Body/Sector/TrainBase/WhistleController/WhistleRoot/TrainWhistle").GetComponent<AudioSource>();
                }
                else { ModHelper.Console.WriteLine("Can't find WhistleController!", MessageType.Error); }

                GameObject trainInterface = SearchUtilities.Find("StellarExpress_Body/Sector/TrainInterface");
                if (trainInterface != null)
                {
                    trainInterface.AddComponent<CoordInterfaceController>();
                }
                else { ModHelper.Console.WriteLine("Can't find TrainInterface!", MessageType.Error); }

                GameObject warpReceiverMain = SearchUtilities.Find("StellarExpress_Body/Sector/Warp_Train_Main");
                if (warpReceiverMain != null)
                {
                    warpReceiverMain.transform.localScale = new Vector3(0.92f, 0.95f, 0.92f);
                }
                else { ModHelper.Console.WriteLine("Can't find Warp_Train_Main!", MessageType.Error); }

                GameObject endingGM = SearchUtilities.Find("StellarExpress_Body/Sector/EndingGM");
                if (endingGM != null)
                {
                    endingGM.SetActive(false);
                    VisorFrostEffectVolume frostEffect = endingGM.GetComponent<VisorFrostEffectVolume>();
                    Destroy(frostEffect);
                }
                else { ModHelper.Console.WriteLine("Can't find EndingGM!", MessageType.Error); }

                GameObject creditsVolume = SearchUtilities.Find("StellarExpress_Body/Sector/LoadCreditsVolume");
                if (creditsVolume != null)
                {
                    creditsVolume.SetActive(false);
                }
                else { ModHelper.Console.WriteLine("Can't find LoadCreditsVolume!", MessageType.Error); }

                GameObject revealVolume = SearchUtilities.Find("StellarExpress_Body/Sector/RevealFinal");
                if (revealVolume != null)
                {
                    revealVolume.SetActive(false);
                }
                else { ModHelper.Console.WriteLine("Can't find RevealFinal!", MessageType.Error); }

                GameObject coordPromptParent = SearchUtilities.Find("StellarExpress_Body/Sector/CoordPromptParent");
                if (coordPromptParent != null)
                {
                    CoordPromptTrigger.CreateCoordPromptTrigger(coordPromptParent);
                }
                else { ModHelper.Console.WriteLine("Can't find CoordPromptParent!", MessageType.Error); }

                porcelain = Resources.FindObjectsOfTypeAll<Material>().First(x => x.name.Contains("Structure_NOM_PorcelainClean_mat"));
                silver = Resources.FindObjectsOfTypeAll<Material>().First(x => x.name.Contains("Structure_NOM_Silver_mat"));
                black = Resources.FindObjectsOfTypeAll<Material>().First(x => x.name.Contains("Structure_NOM_SilverPorcelain_mat"));

                var stellarExpress = newHorizons.GetPlanet("Stellar Express");
                ReplaceMaterials(stellarExpress);
            }
            else
            {
                ModHelper.Console.WriteLine("Couldn't locate planet: Stellar Express!", MessageType.Error);
            }

            GameObject frozenPlanet = newHorizons.GetPlanet("Frost Car");
            if (frozenPlanet != null)
            {
                SphereShape sectorSphere = SearchUtilities.Find("FrostCar_Body/Sector").GetComponent<SphereShape>();
                if (sectorSphere != null)
                {
                    sectorSphere.radius = 600f;
                }

                SphereCollider sphereCollider = SearchUtilities.Find("FrostCar_Body/Sector/Air").GetComponent<SphereCollider>();
                if (sphereCollider != null)
                {
                    sphereCollider.radius = 59f;
                }

                GameObject frozenInterior = SearchUtilities.Find("FrostCar_Body/Sector/FrozenInterior");
                if (frozenInterior != null)
                {
                    frozenInterior.GetComponent<SectorCullGroup>()._crossfadeLength = 0.01f;
                    frozenInterior.GetComponent<SectorCollisionGroup>()._colliderTimeSlicing = false;
                }

                SectorCullGroup projectionPlatform = SearchUtilities.Find("FrostCar_Body/Sector/RemoteProjectionFrostCar").GetComponent<SectorCullGroup>();
                projectionPlatform._crossfadeLength = 0f;

                GameObject warpPlatform = SearchUtilities.Find("FrostCar_Body/Sector/WarpRoot");
                warpPlatform.GetComponent<SectorCullGroup>()._crossfadeLength = 0f;
                warpPlatform.transform.GetChild(0).gameObject.GetComponent<SectorCullGroup>()._crossfadeLength = 0f;

                AtmosphereTrigger.CreateAtmosphereTrigger(frozenPlanet, new Vector3(-60f, 0f, 0f), new Vector3(0f, 0f, 0f));
            }
            else
            {
                ModHelper.Console.WriteLine("Couldn't locate planet: Frost Car!", MessageType.Error);
            }

            GameObject oceanPlanet = newHorizons.GetPlanet("Locomocean");
            if (oceanPlanet != null)
            {
                SphereShape sectorSphere = SearchUtilities.Find("Locomocean_Body/Sector").GetComponent<SphereShape>();
                if (sectorSphere != null)
                {
                    sectorSphere.radius = 250f;
                }

                SphereCollider sphereCollider = SearchUtilities.Find("Locomocean_Body/Sector/Air").GetComponent<SphereCollider>();
                if (sphereCollider != null)
                {
                    sphereCollider.radius = 110f;
                }

                GameObject water = SearchUtilities.Find("Locomocean_Body/Sector/Water");
                if (water != null)
                {
                    Material[] waterMaterial = water.GetComponent<TessellatedSphereRenderer>().GetComponent<TessellatedRenderer>()._materials;
                    waterMaterial[1].color = new Color(1f, 1f, 1f, 1f);
                }
                else { ModHelper.Console.WriteLine("Can't find Locomocean_Body/Sector/Water!", MessageType.Error); }

                GiantsDeepSunOverrideVolume sunOverride = SearchUtilities.Find("Locomocean_Body/Sector/SunOverride").GetComponent<GiantsDeepSunOverrideVolume>();
                if (sunOverride != null)
                {
                    sunOverride._innerIntensity = 0f;
                    sunOverride._cloudsOuterRadius = 115f;
                }
                else { ModHelper.Console.WriteLine("Can't find Locomocean_Body/Sector/SunOverride!", MessageType.Error); }

                GameObject munchVolume = SearchUtilities.Find("Locomocean_Body/Sector/MunchVolume");
                if (munchVolume != null)
                {
                    munchVolume.SetActive(false);
                }
                else { ModHelper.Console.WriteLine("Can't find MunchVolume!", MessageType.Error); }

                GameObject fishStandardRoot = SearchUtilities.Find("Locomocean_Body/Sector/Fish_Standard");
                if (fishStandardRoot != null)
                {
                    GameObject fish = SearchUtilities.Find("Locomocean_Body/Sector/Fish_Standard/Fish");
                    if (fish != null)
                    {
                        fish.AddComponent<FishController>();
                        fish.transform.GetChild(0).transform.GetChild(0).gameObject.AddComponent<FishTrigger>();
                        ModHelper.Console.WriteLine("Adding fish!", MessageType.Info);
                        fishies[0] = fish;
                        for (int i = 0; i <= 5; i++)
                        {
                            GameObject fishClone = Instantiate(fish, fishStandardRoot.transform);
                            ModHelper.Console.WriteLine("Adding fish!", MessageType.Info);
                            fishies[i+1] = fishClone;
                        }
                        fish.transform.rotation = new Quaternion(180f, 0f, 0f, 0f);
                        for (int i = 0; i <= 4; i++)
                        {
                            GameObject fishClone = Instantiate(fish, fishStandardRoot.transform);
                            ModHelper.Console.WriteLine("Adding fish!", MessageType.Info);
                            fishies[i+5] = fishClone;
                        }
                    }
                    else { ModHelper.Console.WriteLine("Can't find Fish!", MessageType.Error); }
                }
                else { ModHelper.Console.WriteLine("Can't find Fish_Standard!", MessageType.Error); }

                GameObject fishPuzzleRoot = SearchUtilities.Find("Locomocean_Body/Sector/Fish_Puzzle");
                if (fishPuzzleRoot != null)
                {
                    GameObject fish = SearchUtilities.Find("Locomocean_Body/Sector/Fish_Puzzle/Fish");
                    if (fish != null)
                    {
                        fish.AddComponent<FishController>();
                    }
                    else { ModHelper.Console.WriteLine("Can't find Fish!", MessageType.Error); }

                    warpScale = SearchUtilities.Find("Locomocean_Body/Sector/Fish_Puzzle/Fish/FishCollider/WarpScale");

                    Material material = SearchUtilities.Find("Locomocean_Body/Sector/Fish_Puzzle/Fish/FishCollider/WarpScale/Warp_Angler_Entrance/BlackHole/BlackHoleSingularity").GetComponent<MeshRenderer>().material;
                    if (material != null)
                    {
                        material.renderQueue = 3601;
                    }
                    else { ModHelper.Console.WriteLine("Can't find BlackHoleSingularity!", MessageType.Error); }
                }
                else { ModHelper.Console.WriteLine("Can't find Fish_Puzzle!", MessageType.Error); }

                GameObject endMusicParent = SearchUtilities.Find("Locomocean_Body/Sector/EndMusicParent");
                if (endMusicParent != null)
                {
                    EndMusicTrigger.CreateEndMusicTrigger(endMusicParent, 26.5f);
                }
                else { ModHelper.Console.WriteLine("Can't find EndMusicParent!", MessageType.Error); }

                SectorCullGroup oceanInterior = SearchUtilities.Find("Locomocean_Body/Sector/OceanInterior").GetComponent<SectorCullGroup>();
                oceanInterior._crossfadeLength = 0f;

                SectorCullGroup projectionPlatform = SearchUtilities.Find("Locomocean_Body/Sector/RemoteProjectionOcean1").GetComponent<SectorCullGroup>();
                projectionPlatform._crossfadeLength = 0f;

                GravityAssistTrigger.CreateGravityAssistTrigger(oceanPlanet, new Vector3(0f, -24.65f, 0f), 3f);
                CheaterTrigger.CreateCheaterTrigger(oceanPlanet, new Vector3(0f, -24.65f, 0), 3f);
            }
            else
            {
                ModHelper.Console.WriteLine("Couldn't locate planet: Locomocean!", MessageType.Error);
            }

            GameObject sprucePlanet = newHorizons.GetPlanet("Spruce Caboose");
            if (sprucePlanet != null)
            {
                SphereShape sectorSphere = SearchUtilities.Find("SpruceCaboose_Body/Sector").GetComponent<SphereShape>();
                if (sectorSphere != null)
                {
                    sectorSphere.radius = 3000f;
                }

                SphereCollider sphereCollider = SearchUtilities.Find("SpruceCaboose_Body/Sector/Air").GetComponent<SphereCollider>();
                if (sphereCollider != null)
                {
                    sphereCollider.radius = 110f;
                }

                GameObject spruceWater = SearchUtilities.Find("SpruceCaboose_Body/Sector/Water");
                if (spruceWater != null)
                {
                    spruceWater.layer = LayerMask.NameToLayer("IgnoreSun");

                    Material[] waterMaterial = spruceWater.GetComponent<TessellatedSphereRenderer>().GetComponent<TessellatedRenderer>()._materials;
                    waterMaterial[1].color = new Color(1.1f, 1.1f, 1.1f, 1f);
                }
                else { ModHelper.Console.WriteLine("Can't find SpruceCaboose_Body/Sector/Water!", MessageType.Error); }

                GameObject warpPlatform = SearchUtilities.Find("SpruceCaboose_Body/Sector/WarpRoot");
                warpPlatform.GetComponent<SectorCullGroup>()._crossfadeLength = 0f;
                warpPlatform.transform.GetChild(0).gameObject.GetComponent<SectorCullGroup>()._crossfadeLength = 0f;

                WaterTrigger.CreateWaterTrigger(sprucePlanet, 29.6f);
                AtmosphereTrigger.CreateAtmosphereTrigger(sprucePlanet, new Vector3(0f, 0f, 100f), new Vector3(0f, 90f, 0f));
                AtmosphereTrigger.CreateAtmosphereTrigger(sprucePlanet, new Vector3(0f, 0f, -100f), new Vector3(90f, 270f, 0f));
                AtmosphereTrigger.CreateAtmosphereTrigger(sprucePlanet, new Vector3(100f, 0f, 0f), new Vector3(0f, 0f, 0f));
                AtmosphereTrigger.CreateAtmosphereTrigger(sprucePlanet, new Vector3(-100f, 0f, 0f), new Vector3(0f, 0f, 0f));
                AtmosphereTrigger.CreateAtmosphereTrigger(sprucePlanet, new Vector3(0f, 100f, 0f), new Vector3(0f, 0f, 90f));
                AtmosphereTrigger.CreateAtmosphereTrigger(sprucePlanet, new Vector3(0f, -100f, 0f), new Vector3(0f, 0f, 90f));
            }
            else
            {
                ModHelper.Console.WriteLine("Couldn't locate planet: Spruce Caboose!", MessageType.Error);
            }

            GameObject beaconPlanet = newHorizons.GetPlanet("Beacon Blocker");
            if (beaconPlanet != null)
            {
                SphereShape sectorSphere = SearchUtilities.Find("BeaconBlocker_Body/Sector").GetComponent<SphereShape>();
                if (sectorSphere != null)
                {
                    sectorSphere.radius = 100f;
                }

                CoordMusicTrigger.CreateCoordMusicTrigger(beaconPlanet, 100f);

                EndMusicTrigger endMusicTrigger = SearchUtilities.Find("Locomocean_Body/Sector/EndMusicParent/EndMusicTrigger").GetComponent<EndMusicTrigger>();
                CoordMusicTrigger coordMusicTrigger = SearchUtilities.Find("BeaconBlocker_Body/CoordMusicTrigger").GetComponent<CoordMusicTrigger>();
                endMusicTrigger.coordMusicTrigger = coordMusicTrigger;
                endMusicTrigger.whistleAudio = whistleAudio;
                coordMusicTrigger.whistleAudio = whistleAudio;
            }
            else
            {
                ModHelper.Console.WriteLine("Couldn't locate planet: Beacon Blocker!", MessageType.Error);
            }

            GameObject startPlatform = newHorizons.GetPlanet("CantAffordaName_OnARail_Platform");
            if (startPlatform != null)
            {
                porcelain = Resources.FindObjectsOfTypeAll<Material>().First(x => x.name.Contains("Structure_NOM_PorcelainClean_mat"));
                silver = Resources.FindObjectsOfTypeAll<Material>().First(x => x.name.Contains("Structure_NOM_Silver_mat"));
                black = Resources.FindObjectsOfTypeAll<Material>().First(x => x.name.Contains("Structure_NOM_SilverPorcelain_mat"));

                var platform = newHorizons.GetPlanet("CantAffordaName_OnARail_Platform");
                ReplaceMaterials(startPlatform);

                //GameObject ship = Locator.GetShipTransform().GetChild(5).transform.GetChild(2).transform.GetChild(0).gameObject;
                GameObject shipFishCollider = SearchUtilities.Find("CantAffordaName_OnARail_Platform_Body/Sector/ShipFishCollider");
                if (shipFishCollider != null)
                {
                    GameObject ship = SearchUtilities.Find("Ship_Body");
                    shipFishCollider.transform.parent = ship.transform;
                    shipFishCollider.transform.localPosition = Vector3.zero;
                    shipFishCollider.transform.localRotation = new Quaternion(0f, 0f, 0f, 0f);
                }
                else { ModHelper.Console.WriteLine("Can't find the ship!", MessageType.Error); }
            }
            else
            {
                ModHelper.Console.WriteLine("Couldn't locate planet: CantAffordaName_OnARail_Platform!", MessageType.Error);
            }
        }

        private void OnPutOnHelmet()
        {
            newHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");

            GameObject trainOrbit = SearchUtilities.Find("StellarExpress_Body/Orbit");
            if (trainOrbit != null)
            {
                trainOrbit.GetComponent<NHOrbitLine>()._lineWidth = 1;
                LineRenderer lineRenderer = trainOrbit.GetComponent<LineRenderer>();
                lineRenderer.material.renderQueue = 0;
                lineRenderer.endColor = new Color(0, 0, 0, 0);
                lineRenderer.endWidth = 0;
                lineRenderer.numPositions = 128;
                lineRenderer.positionCount = 128;
            }
            else { ModHelper.Console.WriteLine("Can't find Orbit!", MessageType.Error); }

            CoordPromptTrigger coordPromptTrigger = SearchUtilities.Find("StellarExpress_Body/Sector/CoordPromptParent/CoordPromptTrigger").GetComponent<CoordPromptTrigger>();
            if (coordPromptTrigger != null)
            {
                Sprite coordSprite = SearchUtilities.Find("BeaconBlocker_Body/Sector/ProbeRoot/CoordinateSprite").GetComponent<SpriteRenderer>().sprite;
                coordPromptTrigger.SetupPrompt(coordSprite);
            }
            else { ModHelper.Console.WriteLine("Can't find CoordPromptTrigger!", MessageType.Error); }

            //lightRadius of the sun gets overridden by base mod, set it back to 2500m after player gains control!
            //(This may or may not happen anymore, but too bad! Band-aid solution stays!)
            Light starLight = SearchUtilities.Find("TheStellarExpress_Body/Sector/Star/StarLight").GetComponent<Light>();
            starLight.range = 2500;
        }

        private void OnEnterShip()
        {
            warpScale.SetActive(false);

            foreach (GameObject fish in fishies)
            {
                fish.transform.GetChild(0).transform.GetChild(0).gameObject.SetActive(false);
                //OnARail.DebugLog("FishTrigger " + fish + " disabled!", OWML.Common.MessageType.Info);
            }
        }

        private void OnExitShip()
        {
            warpScale.SetActive(true);

            foreach (GameObject fish in fishies)
            {
                fish.transform.GetChild(0).transform.GetChild(0).gameObject.SetActive(true);
                //OnARail.DebugLog("FishTrigger " + fish + " enabled!", OWML.Common.MessageType.Info);
            }
        }

        private static void OnPlayerDeath(DeathType deathType)
        {
            TimeLoop.SetTimeLoopEnabled(true);
        }

        private void OnTriggerSupernova()
        {
            TimeLoop.SetTimeLoopEnabled(true);
        }

        public static void DebugLog(string line, MessageType type)
        {
            Instance.ModHelper.Console.WriteLine($"DEBUG: {line}", MessageType.Info);
        }

        public void ReplaceMaterials(GameObject go)
        {
            foreach (var renderer in go.GetComponentsInChildren<Renderer>())
            {
                renderer.materials = renderer.materials.Select(GetReplacementMaterial).ToArray();
            }
        }

        private Material GetReplacementMaterial(Material material)
        {
            if (material.name.Contains("Structure_NOM_Whiteboard_mat") ||
                material.name.Contains("Structure_NOM_SandStone_mat") ||
                material.name.Contains("Structure_NOM_SandStone_Dark_mat")
                )
            {
                return porcelain;
            }
            else if (material.name.Contains("Structure_NOM_CopperOld_mat") ||
                material.name.Contains("Structure_NOM_TrimPattern_mat") ||
                material.name.Contains("Structure_NOM_CopperOld_Dark_mat")
                )
            {
                return silver;
            }
            else if (material.name.Contains("Structure_NOM_PropTile_Color_mat") ||
                material.name.Contains("Structure_NOM_SandStone_Darker_mat") ||
                material.name.Contains("Structure_NOM_WarpReceiver_mat")
                )
            {
                return black;
            }
            else if (material.name.Contains("Props_NOM_Scroll_mat")
            )
            {
                material.color = new Color(0.05f, 0.05f, 0.05f);
            }

            return material;
        }
    }
}