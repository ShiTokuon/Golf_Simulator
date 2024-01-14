using UnityEngine;

namespace Kamgam.Terrain25DLib
{
    // Since the 3D collider can only be generated after the mesh the collider generation is done in MeshGenerator.
    // This is just a helper class.
    public static class Collider3DGenerator
    {
        public static MeshCollider GenerateCollider(MeshFilter meshFilter, PhysicMaterial material)
        {
            if (meshFilter == null || meshFilter.gameObject == null)
                return null;

            var collider = meshFilter.gameObject.AddComponent<MeshCollider>();
            collider.material = material;

            return collider;
        }
    }
}
