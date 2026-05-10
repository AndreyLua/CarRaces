#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using KrisDevelopment.DistributedInternalUtilities;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace KrisDevelopment.ERMG.Tests
{
    internal class EasyRoadsMeshGenTests
    {
        private class TestScene : EditorTestScene
        {
            protected override string sceneResourceName => "ermg_test_scene";
        }

        /// <summary>
        /// Unit test loading the test scene
        /// </summary>
        [UnityTest]
        public IEnumerator TestScene_Test()
        {
            using (new TestScene())
            {
                yield return null;
            }
        }

        [Test]
        public void CreateERMeshGen_Test()
        {
            using (new TestScene()) {
                try
                {
                    Util.CreateERMeshGen();
                }
                catch (Exception ex)
                {
                    Assert.Fail(ex.Message);
                }
            }
        }

        [Test]
        public void MeshGeneration_Test()
        {
            using (new TestScene())
            {
                var _obj = new GameObject();
                _obj.SetActive(false);
                var _ermg = _obj.AddComponent<ERMeshGen>();
                _ermg.includeCollider = 1;
                _ermg.subdivision = 10;

                _ermg.CreateNavPoint().transform.position = Vector3.zero;
                _ermg.CreateNavPoint().transform.position = Vector3.forward * 10;
                _ermg.CreateNavPoint().transform.position = Vector3.forward * 20;
                _ermg.CreateNavPoint().transform.position = Vector3.forward * 30;

                _ermg.enableMeshBorders = 1;
                _ermg.UpdateMesh();

                _obj.SetActive(true);

                AssertMeshAndColliders(_ermg);
                SETUtil.SceneUtil.SmartDestroy(_obj);
            }
        }

        [Test]
        public void NegativeSubdivision_Test()
        {
            using (new TestScene())
            {
                var _obj = new GameObject();
                _obj.SetActive(false);
                var _ermg = _obj.AddComponent<ERMeshGen>();
                _ermg.includeCollider = 1;
                _ermg.subdivision = -1;

                _ermg.CreateNavPoint().transform.position = Vector3.zero;
                _ermg.CreateNavPoint().transform.position = Vector3.forward * 30;

                _ermg.enableMeshBorders = 1;
                _ermg.UpdateMesh();

                _obj.SetActive(true);

                AssertMeshAndColliders(_ermg);
                SETUtil.SceneUtil.SmartDestroy(_obj);
            }
        }

        [Test]
        public void FromPrefabState_OneFrame_TestAutomatic()
        {
            using (new TestScene())
            {
                ERMeshGen _ermg = SpawnMeshGen("ermg_test_prefab");
                Assert.IsTrue(_ermg.updatePointsMode == (int)UpdateMode.Automatic, "Invalid setup on ERMG testing prefab");

                AssertMeshAndColliders(_ermg);
                SETUtil.SceneUtil.SmartDestroy(_ermg.gameObject);
            }
        }

        [UnityTest]
        public IEnumerator FromPrefabState_NextFrame_TestAutomatic()
        {
            using (new TestScene())
            {
                ERMeshGen _ermg = SpawnMeshGen("ermg_test_prefab");
                Assert.IsTrue(_ermg.updatePointsMode == (int)UpdateMode.Automatic, "Invalid setup on ERMG testing prefab");

                // skip a frame
                yield return null;
                AssertMeshAndColliders(_ermg);
                SETUtil.SceneUtil.SmartDestroy(_ermg.gameObject);
            }
        }

        [Test]
        public void FromPrefabState_OneFrame_TestManual()
        {
            using (new TestScene())
            {
                ERMeshGen _ermg = SpawnMeshGen("ermg_test_prefab_manual");
                Assert.IsTrue(_ermg.updatePointsMode == (int)UpdateMode.Manual, "Invalid setup on ERMG testing prefab");

                AssertMeshAndColliders(_ermg);
                SETUtil.SceneUtil.SmartDestroy(_ermg.gameObject);
            }
        }

        [UnityTest]
        public IEnumerator FromPrefabState_NextFrame_TestManual()
        {
            using (new TestScene())
            {
                ERMeshGen _ermg = SpawnMeshGen("ermg_test_prefab_manual");
                Assert.IsTrue(_ermg.updatePointsMode == (int)UpdateMode.Manual, "Invalid setup on ERMG testing prefab");
                // skip a frame
                yield return null;
                AssertMeshAndColliders(_ermg);
                SETUtil.SceneUtil.SmartDestroy(_ermg.gameObject);
            }
        }

        [Test]
        public void OnDestroy_Test()
        {
            using (new TestScene())
            {
                ERMeshGen _ermg = SpawnMeshGen("ermg_test_prefab_manual");
                SETUtil.SceneUtil.SmartDestroy(_ermg.gameObject);
            }
        }

        // -------------------------------------------------
        // Utilities:
        // -------------------------------------------------
#region Utilities

        private static ERMeshGen SpawnMeshGen(string prefabResourceName)
        {
            Assert.IsNotEmpty(prefabResourceName, "prefabResourceName empty");
            var _prefab = SETUtil.ResourceLoader.EditorObjectResource.Get<GameObject>(prefabResourceName);

            Assert.IsTrue(_prefab, "Invalid setup on ERMG testing missing prefab");

            var _ermg = SETUtil.SceneUtil.Instantiate(_prefab).GetComponent<ERMeshGen>();

            Assert.IsTrue(_ermg.enableMeshBorders == 1 && _ermg.includeCollider == 1, "Invalid setup on ERMG testing prefab");
            return _ermg;
        }

        /// <summary>
        /// Assert for the presense of collider, filter, renderer and mesh (do the same for borders)
        /// </summary>
        private static void AssertMeshAndColliders(ERMeshGen _ermg, bool testColliders = true, bool testBorders = true)
        {
            if(testColliders) Assert.IsTrue(_ermg.GetComponent<MeshCollider>(), "Missing MAIN MeshCollider component");
            Assert.IsTrue(_ermg.GetComponent<MeshFilter>(), "Missing MAIN MeshFilter component");
            Assert.IsTrue(_ermg.GetComponent<MeshRenderer>(), "Missing MAIN MeshRenderer component");

            if (testBorders) if (testColliders) Assert.IsTrue(_ermg.leftBorder.GetComponent<MeshCollider>(), "Missing LB MeshCollider component");
            if (testBorders) Assert.IsTrue(_ermg.leftBorder.GetComponent<MeshFilter>(), "Missing LB MeshFilter component");
            if (testBorders) Assert.IsTrue(_ermg.leftBorder.GetComponent<MeshRenderer>(), "Missing LB MeshRenderer component");
            if (testBorders) if (testColliders) Assert.IsTrue(_ermg.rightBorder.GetComponent<MeshCollider>(), "Missing RB MeshCollider component");
            if (testBorders) Assert.IsTrue(_ermg.rightBorder.GetComponent<MeshFilter>(), "Missing RB MeshFilter component");
            if (testBorders) Assert.IsTrue(_ermg.rightBorder.GetComponent<MeshRenderer>(), "Missing RB MeshRenderer component");

            Assert.IsTrue(_ermg.GetComponent<MeshFilter>().sharedMesh, "Missing mesh on MAIN MeshFilter component");
            if (testBorders) Assert.IsTrue(_ermg.leftBorder.GetComponent<MeshFilter>().sharedMesh, "Missing mesh on LB MeshFilter component");
            if (testBorders) Assert.IsTrue(_ermg.rightBorder.GetComponent<MeshFilter>().sharedMesh, "Missing mesh on RB MeshFilter component");

            Assert.IsTrue(_ermg.GetComponent<MeshFilter>().sharedMesh.vertexCount > 0, "Missing mesh vertices on MAIN");
            if (testBorders) Assert.IsTrue(_ermg.leftBorder.GetComponent<MeshFilter>().sharedMesh.vertexCount > 0, "Missing mesh vertices on LB");
            if (testBorders) Assert.IsTrue(_ermg.rightBorder.GetComponent<MeshFilter>().sharedMesh.vertexCount > 0, "Missing mesh vertices on RB");
        }


#endregion
    }
}
#endif