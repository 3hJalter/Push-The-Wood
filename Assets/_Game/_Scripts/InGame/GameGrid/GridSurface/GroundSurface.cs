using System.Collections.Generic;
using _Game.Data;
using _Game.DesignPattern;
using _Game.Managers;
using UnityEngine;
using VinhLB;

namespace _Game.GameGrid.GridSurface
{
    public class GroundSurface : GridSurface, ICombineMesh
    {
        [SerializeField]
        private MeshRenderer grassRenderer;
        [SerializeField]
        private MeshFilter grassMeshFilter;
        [SerializeField]
        private MeshRenderer groundMeshRenderer;
        [SerializeField]
        private MeshFilter groundMeshFilter;
        [SerializeField]
        private List<MeshFilter> combineMeshs;
        [SerializeField]
        private Transform _flowersParentTF;
        [SerializeField]
        private Transform[] _flowerSpawnPoints;

        public MaterialEnum groundMaterialEnum = MaterialEnum.None;

        private List<Flower> _flowerList = new List<Flower>();
        public MeshFilter GroundMeshFilter => groundMeshFilter;
        public MeshFilter GrassMeshFilter => grassMeshFilter;
        
        
        public override void OnInit(int levelIndex, Vector2Int gridCellPos, Vector2Int gridSize,
            Direction rotateDirection = Direction.Forward, MaterialEnum materialEnum = MaterialEnum.None,
            ThemeEnum themeEnum = ThemeEnum.Default, bool hasUnitInMap = false)
        {
            transform.localRotation = Quaternion.Euler(0, BuildingUnitData.GetRotationAngle(rotateDirection), 0);
            // Change material in mesh renderer
            if (groundMeshRenderer is null) return; 
            groundMaterialEnum = materialEnum;
            // groundMeshRenderer.material = DataManager.Ins.GetSurfaceMaterial(materialEnum);
            if (groundMaterialEnum is MaterialEnum.None || grassRenderer is null) return;
            // if (themeEnum is ThemeEnum.Winter)
            // {
            //     grassRenderer.gameObject.SetActive(false);
            // }
            // else
            // {
            //     grassRenderer.gameObject.SetActive(true);
            //     grassRenderer.material = DataManager.Ins.GetGrassMaterial(materialEnum);
            //
            // }
            // Spawn flowers if it does not has unit in map
            if (CanSpawnFlowers(levelIndex, gridCellPos, gridSize, hasUnitInMap))
            {
                List<int> indexList = new List<int>();
                int flowerAmount = Random.Range(1, Mathf.Min(3, _flowerSpawnPoints.Length));
                while (indexList.Count < flowerAmount)
                {
                    int randomIndex = Random.Range(0, _flowerSpawnPoints.Length);
                    if (!indexList.Contains(randomIndex))
                    {
                        indexList.Add(randomIndex);
                    }
                }

                for (int i = 0; i < indexList.Count; i++)
                {
                    Transform spawnPoint = _flowerSpawnPoints[indexList[i]];
                    Flower flower = SimplePool.Spawn<Flower>(
                        DataManager.Ins.GetRandomEnvironmentObject(PoolType.Flower),
                        spawnPoint.position,
                        Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                    flower.Tf.SetParent(_flowersParentTF, true);

                    _flowerList.Add(flower);
                }
            }
        }
        public List<MeshFilter> CombineMeshs(bool isActiveMesh)
        {
            foreach (MeshFilter mesh in combineMeshs)
            {
                GameObject meshObj = mesh.gameObject;
                if (meshObj.activeInHierarchy != isActiveMesh)
                    meshObj.SetActive(isActiveMesh);
            }
            return combineMeshs;
        }

        public void ActiveMesh(bool isActiveMesh, ThemeEnum themeEnum)
        {
            GroundMeshFilter.gameObject.SetActive(isActiveMesh);
            GrassMeshFilter.gameObject.SetActive(isActiveMesh);
            if (isActiveMesh)
            {
                groundMeshRenderer.material = DataManager.Ins.GetSurfaceMaterial(groundMaterialEnum);
                if (themeEnum is ThemeEnum.Winter)
                {
                    grassRenderer.gameObject.SetActive(false);
                }
                else
                {
                    grassRenderer.gameObject.SetActive(true);
                    grassRenderer.material = DataManager.Ins.GetGrassMaterial(groundMaterialEnum);
                
                }
            }
        }
        
        public override void OnDespawn()
        {
            for (int i = _flowerList.Count - 1; i >= 0; i--)
            {
                _flowerList[i].Despawn();

                _flowerList.RemoveAt(i);
            }

            base.OnDespawn();
        }

        [ContextMenu("Set Material to Ground")]
        public void SetMaterialToGround()
        {
            if (groundMeshRenderer == null) return;
            groundMeshRenderer.material = DataManager.Ins.GetSurfaceMaterial(groundMaterialEnum);
        }

        private bool CanSpawnFlowers(int levelIndex, Vector2Int gridCellPos, Vector2Int gridSize, bool hasUnitInMap)
        {
            if (hasUnitInMap)
            {
                return false;
            }

            if (_flowersParentTF == null)
            {
                return false;
            }

            if (_flowerSpawnPoints.Length < 1)
            {
                return false;
            }

            bool result = false;
            // Random spawn
            // switch (PoolType)
            // {
            //     case PoolType.SurfaceGroundC:
            //         result = Random.Range(0, 5) < 2;
            //         break;
            //     case PoolType.SurfaceGroundEdge:
            //         result = Random.Range(0, 8) < 2;
            //         break;
            //     case PoolType.SurfaceGroundCorner:
            //         result = Random.Range(0, 10) < 2;
            //         break;
            // }

            // Procedure spawn
            float scale = 5f;
            Vector2 offset = new Vector2(levelIndex * 0.1f, levelIndex * 0.1f);
            float xCoord = (float)gridCellPos.x / gridSize.x * scale + offset.x;
            float yCoord = (float)gridCellPos.y / gridSize.y * scale + offset.y;
            float value = Mathf.PerlinNoise(xCoord, yCoord);
            // Debug.Log($"Level {levelIndex + 1} | Cell[{gridCellPos.x}, {gridCellPos.y}]: {value}");
            result = value > 0.5f;

            return result;
        }
    }
}