
using UnityEngine;
using static KrisDevelopment.ERMG.EasyRoadsMeshGen_Array.ArrayObject;

namespace KrisDevelopment.ERMG
{
    /// <summary>
    /// This class marks objects spawned by the ERMG Array system, so they are kept track of.
    /// </summary>
    public class ERRoadCapObjectInstance : MonoBehaviour
    {
        [SerializeField] private InstanceType instanceType;
        [SerializeField] private EasyRoadsMeshGen_Array arrayBind;


        public void BindTo(EasyRoadsMeshGen_Array array, InstanceType type)
        {
            this.arrayBind = array;
            this.instanceType = type;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        public EasyRoadsMeshGen_Array GetBinding()
        {
            return this.arrayBind;
        }

        public EasyRoadsMeshGen_Array.ArrayObject.InstanceType GetInstanceType()
        {
            return instanceType;
        }
    }
}
