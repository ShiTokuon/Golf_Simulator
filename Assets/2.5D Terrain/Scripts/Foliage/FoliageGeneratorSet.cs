using System.Collections.Generic;
using UnityEngine;
using Kamgam.Terrain25DLib.Helpers;

namespace Kamgam.Terrain25DLib
{
	[System.Serializable]
	public class FoliageGeneratorSet
	{
		[Tooltip("The of the set. This is also the name of the root object in the hierarchy.")]
		public string Name = "Trees";

		[Tooltip("Turn off to skip this set during generation.")]
		public bool Enabled = true;

		[Tooltip("Settings for the set. Create one in the Project with: Assets > Create > 2.5D Terrain > FoliageSet Settings")]
		public FoliageGeneratorSetSettings Settings;

		[Tooltip("Mark the placed objects as static?")]
		public bool StaticMeshes = false;

		[Range(0, 100), Tooltip("Start position in percentage along the terrain X axis.")]
		public float Start = 0f;

		[Range(0, 100), Tooltip("End position in percentage along the terrain X axis.")]
		public float End = 100f;

		[Tooltip("How many objects should be generated along the local X axis. Useful to get organic looking incrase or decrease of foliage.")]
		public AnimationCurve Curve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(1, 1) });

		protected Transform root;

		public Transform GetRoot(FoliageGenerator generator)
		{
			if (root == null)
			{
				if(generator != null)
                {
					root = generator.transform.Find(Name);
					if (root == null)
					{
						var obj = new GameObject(Name);
						obj.transform.parent = generator.transform;
						obj.transform.localPosition = Vector3.zero;
						obj.transform.localRotation = Quaternion.identity;
						obj.transform.localScale = Vector3.one;
						root = obj.transform;
#if UNITY_EDITOR
						UnityEditor.Undo.RegisterCreatedObjectUndo(obj, Name + " created");
#endif
					}
					return root;
				}
				else
                {
                    Debug.LogError("generator is null.");
				}
			}

			// Update name while we are at it.
			if (root != null && root.name != Name)
				root.name = Name;

			return root;
		}

		public void Destroy(FoliageGenerator generator)
		{
			if(GetRoot(generator) != null)
			{
				Utils.SmartDestroy(root.gameObject);
			}
		}


		public static int IntersectRayMesh(Ray ray, MeshFilter meshFilter, List<RaycastHit> hits)
		{
#if UNITY_EDITOR
			return UtilsEditor.IntersectRayMeshMultiple(ray, meshFilter, hits);
#else
			// TODO: Use this at runtime https://forum.unity.com/threads/editor-raycast-against-scene-meshes-without-collider-editor-select-object-using-gui-coordinate.485502/#post-7246292
			//       Has no normal info though.
			Debug.LogError("FoliageGenerator.IntersectRayMesh not implementend for runtime use.");
			return -1;
#endif
		}

		protected static void AssignMaterialFromRatios(GameObject obj, FoliageGeneratorSetSettings.PrefabRatio prefabRatio)
		{
			var meshRenderer = obj.GetComponent<MeshRenderer>();
			if (meshRenderer != null)
			{
				for (int i = 0; i < meshRenderer.sharedMaterials.Length; i++)
				{
					if (prefabRatio.HasMaterialForIndex(i))
					{
						var sharedMaterials = meshRenderer.sharedMaterials;
						var marterial = prefabRatio.GetRandomMaterial(i);
						sharedMaterials[i] = marterial;
						meshRenderer.sharedMaterials = sharedMaterials;
					}
				}
			}
			else if (obj.transform.childCount > 0)
			{
				// maybe the meshes are in children, if yes, then interpret the materialIndex as childIndex and assign the material to the childs first material
				for (int c = 0; c < obj.transform.childCount; c++)
				{
					meshRenderer = obj.transform.GetChild(c).GetComponent<MeshRenderer>();
					if (meshRenderer != null)
					{
						if (prefabRatio.HasMaterialForIndex(c))
						{
							meshRenderer.sharedMaterial = prefabRatio.GetRandomMaterial(c);
						}
					}
				}
			}
		}

		public void DestroyInSelectedArea(FoliageGenerator generator)
		{
			if (generator == null)
				return;

			if (GetRoot(generator) == null)
				return;

			(float minX, float maxX, _, _) = CalculateMinMaxXZ(generator, Settings.Margin, inLocalSpace: true);
			float distanceX = maxX - minX;
			destroyInsideMinMaxX(generator, minX + distanceX * (Start / 100f), minX + distanceX * (End / 100f));
		}

		protected void destroyInsideMinMaxX(FoliageGenerator generator, float minX, float maxX)
        {
			var root = GetRoot(generator);
			for (int t = root.childCount - 1; t >= 0; t--)
			{
				var pos = root.GetChild(t).localPosition;
				if (pos.x >= minX && pos.x <= maxX)
				{
					Utils.SmartDestroy(root.GetChild(t).gameObject);
				}
			}
		}

		public static (float, float, float, float) CalculateMinMaxXZ(FoliageGenerator generator, float margin, bool inLocalSpace)
        {
			var bounds = generator.GetBounds();

			float fullMinX = bounds.min.x;
			float fullMaxX = bounds.max.x;
			float fullDistance = fullMaxX - fullMinX;
			
			float minX = Mathf.Clamp(fullMinX               , fullMinX + margin, fullMaxX - margin);
			float maxX = Mathf.Clamp(fullMinX + fullDistance, fullMinX + margin, fullMaxX - margin);

			if (!inLocalSpace)
			{
				return (minX, maxX, bounds.min.z, bounds.max.z);
			}
			else
			{
				var min = generator.transform.InverseTransformPoint(bounds.min);
				var max = generator.transform.InverseTransformPoint(bounds.max);
				return (min.x + margin, max.x - margin, min.z, max.z);
			}
		}

		public void Generate(FoliageGenerator generator, MeshRenderer[] meshRenderers)
		{
			if (!Enabled)
				return;

			if (Settings == null)
            {
				Debug.LogError("Foliage Generator Set '" + Name + "' has no Settings. Skipping this set.");
				return;
            }

			if (generator.MeshRenderers == null || generator.MeshRenderers.Length == 0)
				return;

			(float minX, float maxX, _, _) = CalculateMinMaxXZ(generator, Settings.Margin, inLocalSpace: true);
			float distanceX = maxX - minX;
			destroyInsideMinMaxX(generator, minX + distanceX * (Start / 100f), minX + distanceX * (End / 100f));

#if UNITY_EDITOR
			if (meshRenderers.Length > 1)
                Debug.Log("Using the FoliageGenerator on multiple small meshes is slower than using it on one mesh. Try using the 'Combine Meshes' setting in the MeshGenerator.");
#endif


			foreach (var renderer in meshRenderers)
            {
				GenerateForMesh(generator, renderer);
            }
		}

		public void GenerateForMesh(FoliageGenerator generator, MeshRenderer meshRenderer)
		{
			if (!Enabled)
				return;

			if (Settings == null)
			{
				Debug.LogError("Foliage Generator Set '" + Name + "' has no Settings. Skipping this set.");
				return;
			}

			if (!Settings.HasValidPrefabRatios())
			{
				Debug.LogError("Foliage Generator Set '" + Name + "' Settings do not have any Prefabs assigned. Skipping this set.");
				return;
			}

			MeshFilter meshFilter = meshRenderer.gameObject.GetComponent<MeshFilter>();
			if (meshFilter == null)
				return;

			if (Start >= End)
				return;

			var bounds = generator.GetBounds();
			(float minX, float maxX, float minZ, float maxZ) = CalculateMinMaxXZ(generator, Settings.Margin, inLocalSpace: true);
			float xDistance = maxX - minX;

			float distanceFront = Settings.DensityMinDistance + (1f - Settings.DensityFront) * (Settings.DensityMaxDistance - Settings.DensityMinDistance);
			float distanceBack = Settings.DensityMinDistance + (1f - Settings.DensityBack) * (Settings.DensityMaxDistance - Settings.DensityMinDistance);

			// create
			var triangles = meshFilter.sharedMesh.triangles;
			var vertices = meshFilter.sharedMesh.vertices;
			float progress = 0; // 0 to 1
			Vector3 normal, inPlane, v0, v1, v2;
			int counter = 0;
			List<RaycastHit> hits = new List<RaycastHit>();
			for (int i = 0; i < 2; i++)
            {
				bool isFront = i == 0;
				
				float distance = isFront ? distanceFront : distanceBack;
				float middleWidth = isFront ? generator.FrontMiddleWidth : generator.BackMiddleWidth;
				float fullBevelWidth = isFront ? generator.FrontBevelWidth : generator.BackBevelWidth;
				float bevelWidth = Random.Range(0, fullBevelWidth);
				float currentX = minX;
				while (currentX < maxX)
				{
					progress = (currentX - minX) / xDistance;

					// skip if out of gen limits
					if (progress < Start / 100f || progress > End / 100f)
                    {
						currentX += distance;
						continue;
					}

                    float rayRegularX = currentX;
                    float rayRegularY = generator.transform.InverseTransformPoint(bounds.max).y;
                    float rayRegularZ = isFront ? -middleWidth : middleWidth;

                    float rayRandomX = currentX + Random.Range(-distance * 0.4f, distance * 0.4f);
                    float rayRandomY = generator.transform.InverseTransformPoint(bounds.max).y;
                    float rayRandomZ = Random.Range(0, isFront ? minZ : maxZ);

                    float rayX = Mathf.Lerp(rayRegularX, rayRandomX, Settings.Randomness);
                    float rayY = Mathf.Lerp(rayRegularY, rayRandomY, Settings.Randomness);
                    float rayZ = Mathf.Lerp(rayRegularZ, rayRandomZ, Settings.Randomness);


                    // In the middle
                    if (!Settings.PlaceInMiddle && Mathf.Abs(rayZ) < Mathf.Abs(middleWidth) -  (isFront ? Settings.MiddleWidthOverlapFront : Settings.MiddleWidthOverlapBack))
					{
						currentX += distance;
						continue;
					}

					// Outside of depth limits?
					if (   ( isFront && Settings.DepthLimitFront >= 0f && rayZ < -Settings.DepthLimitFront)
						|| (!isFront && Settings.DepthLimitBack  >= 0f && rayZ >  Settings.DepthLimitBack))
                    {
						currentX += distance;
						continue;
					}

					Vector3 rayStartPosLocal = new Vector3(rayX, rayY, rayZ);
					var rayStartPos = generator.transform.TransformPoint(rayStartPosLocal);
					rayStartPos.y += 1f + generator.RayStartOffsetY;

					currentX += distance;

					// fall of in z direction
					float likelyhoodForZAxis = Settings.DepthCurve.Evaluate(Mathf.Abs(rayStartPosLocal.z) / Mathf.Max(Mathf.Abs(minZ), Mathf.Abs(maxZ)));
					float likelyhoodForXAxis = Curve.Evaluate(progress);
					bool spawn = Utils.RandomResult(likelyhoodForZAxis) && Utils.RandomResult(Curve.Evaluate(progress));
					if (!spawn)
						continue;

					// Draw for debugging
					//Debug.DrawRay(rayStartPos, Vector3.down * 200, Color.blue, 10f);

					// Check if there is a mesh
					Ray ray = new Ray(rayStartPos, Vector3.down);
					hits.Clear();
					int hitCount = IntersectRayMesh(ray, meshFilter, hits);
					if (hitCount == 0)
						continue;

                    foreach (var hit in hits)
                    {
						// Draw for debugging
						//Debug.DrawRay(hit.point, Vector3.up * 0.5f, Color.blue, 10f);
						//Debug.DrawRay(hit.point, hit.normal * 0.5f, Color.cyan, 10f);

						if (Settings.UseMeshNormalsForAlignment)
						{
							normal = hit.normal;
							inPlane = Utils.CreateNormal(hit.normal);
						}
						else
						{
							// calc none smoothed normal for the hit triangle
							v0 = meshFilter.transform.TransformPoint(vertices[triangles[hit.triangleIndex * 3]]);
							v1 = meshFilter.transform.TransformPoint(vertices[triangles[hit.triangleIndex * 3 + 1]]);
							v2 = meshFilter.transform.TransformPoint(vertices[triangles[hit.triangleIndex * 3 + 2]]);
							normal = Vector3.Cross(v1 - v0, v2 - v0).normalized;
							inPlane = v0 - v1;
						}

                        bool flitZRotation = false;
						var hitOrientation = Vector3.Dot(Vector3.up, normal.normalized);
                        if (hitOrientation < 0)
                        {
                            if (Settings.AllowUpsideDown)
                            {
                                flitZRotation = true;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        if (hitOrientation > 0 && Settings.AllowUpsideDown && Settings.OnlyUpsideDown)
                            continue;


                        // Draw for debugging
                        //var m = 1 - Vector3.Project(normal.normalized, Vector3.up).magnitude;
                        //UtilsDebug.DrawVectorLERP(hit.point, Vector3.up, m, m < Settings.GrassSlopeLimit ? Color.green : Color.red, 15);

                        float groundIntersectionY = flitZRotation ? Settings.GroundIntersection : -Settings.GroundIntersection;
                        if (1 - Vector3.Project(normal.normalized, Vector3.up).magnitude < Settings.SlopeLimit)
						{
							var prefabSettings = Settings.GetRandomPrefab();
							var obj = Utils.SmartInstantiate(prefabSettings.Prefab, GetRoot(generator));
							obj.isStatic = StaticMeshes;
							var localScale = Vector3.one * Random.Range(Settings.MinScale, Settings.MaxScale);
                            localScale.x *= Random.Range(Settings.MinScaleXYZ.x, Settings.MaxScaleXYZ.x);
                            localScale.y *= Random.Range(Settings.MinScaleXYZ.y, Settings.MaxScaleXYZ.y);
                            localScale.z *= Random.Range(Settings.MinScaleXYZ.z, Settings.MaxScaleXYZ.z);
                            obj.transform.localScale = localScale;
                            obj.transform.position = hit.point + new Vector3(0, groundIntersectionY * obj.transform.localScale.y, 0); // stick them in the earth a little
							obj.transform.rotation = Quaternion.identity;

							if (Settings.AlignRotationWithTerrain)
							{
								var rot = obj.transform.rotation;
								rot.SetLookRotation(inPlane, normal);
								obj.transform.rotation = rot;
							}
						
							obj.transform.Rotate(
                                Random.Range(-0.5f * Settings.RotXVarianceInDeg, 0.5f * Settings.RotXVarianceInDeg),
                                Random.Range(-0.5f * Settings.RotYVarianceInDeg, 0.5f * Settings.RotYVarianceInDeg), 
                                Random.Range(-0.5f * Settings.RotZVarianceInDeg, 0.5f * Settings.RotZVarianceInDeg),
                                Space.Self);

                            if (flitZRotation)
                            {
                                obj.transform.Rotate(0f, 0f, 180f, Space.World);
                            }

                            AssignMaterialFromRatios(obj, prefabSettings);
							counter++;
	#if UNITY_EDITOR
							UnityEditor.Undo.RegisterCreatedObjectUndo(obj, "Foliage Obj " + obj.name);

                            //UtilsDebug.DrawVector(hit.point, normal * 10, Color.blue, 10); 
							//UtilsDebug.DrawVector(hit.point, inPlane, Color.yellow, 10);
	#endif
						}
						//UtilsDebug.DrawVector(ray.origin, ray.direction * 100, Color.blue, 10);

						// stop after first hit
						if (!Settings.PlaceInCaves)
							break;
					}
				}
			}

#if UNITY_EDITOR
			if (Terrain25DSettings.GetOrCreateSettings().ShowLogs)
				Debug.Log("Generated " + counter + " objects in " + Name + ".");
#endif
		}
	}
}