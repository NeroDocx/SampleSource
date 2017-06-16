using RobotsRTS.Helper;
using RobotsRTS.NetWork;
using RobotsRTS.SharedCode;
using RobotsRTS.UI3d;
using System;
using UnityEngine;
namespace RobotsRTS.Game
{
    public class Build : MonoBehaviour, IBuild
    {
        [SerializeField]
        private BuildType type;

        private ValueBar3d HealthBar;
        private float ShowHelthBatTimer;
        private const float ShowHelthBatTime = 5.0f;
        private bool isSelect;

        private FactoryValue3d FactoryValue;

        #region Interface
        public int Id { get; private set; }
        public BuildType Type { get { return type; } }
        public int Health { get; private set; }
        public NWBuildTask[] BuildTasks { get; private set; }
        public TurretPlatform[] TurretPlatforms { get; private set; }
        public void SetInfo(NWBuild _buildDescription)
        {
            Id = _buildDescription.Id;
            UpdateInfo(_buildDescription);

            isSelect = false;
            UI3dCanvasObj canvas = FindObjectOfType(typeof(UI3dCanvasObj)) as UI3dCanvasObj;
            if (canvas == null)
            {
                Console.LogError(ConsoleFilter.Build, "Plate Canvas is null");
                return;
            }

            if (canvas.ValueBar3dPrefab != null)
            {
                HealthBar = null;
                if (Util.InstancePrefabAndGetMBScript(canvas.ValueBar3dPrefab, "HealthBar_Build_" + Id, canvas.gameObject.transform, Vector3.zero, out HealthBar))
                {
                    HealthBar.trackTransform = transform;
                    HealthBar.offset = new Vector3(0.0f, 5.0f, 0.0f);
                    HealthBar.gameObject.SetActive(false);
                }
            }
            else
            {
                Console.LogError(ConsoleFilter.Build, "ValueBar3d Prefab is null");
            }

            if (canvas.FactoryValuePrefab != null)
            {
                FactoryValue = null;
                Util.InstancePrefabAndGetMBScript(canvas.FactoryValuePrefab, "FactoryValue_Build_" + Id, canvas.gameObject.transform, Vector3.zero, out FactoryValue);
                {
                    FactoryValue.trackTransform = transform;
                    FactoryValue.offset = new Vector3(0.0f, 10.0f, 0.0f);
                    FactoryValue.gameObject.SetActive(false);
                }
            }
            else
            {
                Console.LogError(ConsoleFilter.Build, "FactoryValue Prefab is null");
            }

        }
        public void SetTurretPlatforms(TurretPlatform[] _platforms)
        {
            if (!Util.ArrayIsNullOrEmpty(_platforms))
            {
                TurretPlatforms = _platforms;
            }
        }
        public void UpdateInfo(NWBuild _buildDescription)
        {
            Health = _buildDescription.Health;
            BuildTasks = _buildDescription.BuildTasks;
            if (HealthBar != null && HealthBar.gameObject.activeSelf)
            {
                HealthBar.SetValue = Health;
            }
            if (FactoryValue != null)
            {
                FactoryValue.gameObject.SetActive(_buildDescription.IsProduceResource);
                //FactoryValue.SetValue = _buildDescription.ProduceResourceValue;
            }
        }
        public void ShowHealthBar()
        {
            if (HealthBar != null)
            {
                HealthBar.gameObject.SetActive(true);
                HealthBar.SetValue = Health;
                ShowHelthBatTimer = 0.0f;
            }
        }
        public void OnDeselect()
        {
            //if (isSelect)
            //{

            //}
            isSelect = false;
        }

        public void OnSelect(int _id)
        {
            isSelect = true;
            ShowHealthBar();
        }

        public void OnSetTarget(TargetPoint _target)
        {

        }
        #endregion

        void Update()
        {
            if (ShowHelthBatTimer > ShowHelthBatTime || isSelect)
            {
                return;
            }

            ShowHelthBatTimer += Time.deltaTime;
            if (HealthBar != null && ShowHelthBatTimer > ShowHelthBatTime)
            {
                HealthBar.gameObject.SetActive(false);
            }
        }
        public void Dispose()
        {
            if (HealthBar != null)
            {
                UnityEngine.Object.Destroy(HealthBar.gameObject);
            }

            if (FactoryValue != null)
            {
                UnityEngine.Object.Destroy(FactoryValue.gameObject);
            }

            //if (!Util.ArrayIsNullOrEmpty(BuildTasks))
            //{

            //}

            if (!Util.ArrayIsNullOrEmpty(TurretPlatforms))
            {
                Array.Clear(TurretPlatforms, 0, TurretPlatforms.Length);    
            }


            HealthBar = null;
            FactoryValue = null;
            BuildTasks = null;
            TurretPlatforms = null;
        }
    }
}