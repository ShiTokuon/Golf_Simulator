using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kamgam.Terrain25DLib
{
	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(MeshFilter))]
	public class MeshMiddleSegment : MonoBehaviour
	{
		public MeshFilter MeshFilter;
		public MeshRenderer MeshRenderer;

		/// <summary>
		/// Creates the flat middle segments of the mesh.
		/// </summary>
		/// <returns>Middle Mesh Segment</returns>
		public static MeshMiddleSegment Create(
			Transform parent, Vector3[] pointsInWorldSpace, float widthFront, float widthBack, List<MeshPointInfo> infos,
			Material material, bool castShadows, bool frontProjectUVs = false, float middleUVScale = 1f, int zDivisions = 1)
		{
			int nr = parent.GetComponentsInChildren<MeshMiddleSegment>().Length;
			var segment = new GameObject("Mesh Middle " + nr, typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshMiddleSegment)).GetComponent<MeshMiddleSegment>();
			segment.transform.SetParent(parent);
			segment.transform.localPosition = Vector3.zero;
			segment.transform.localScale = Vector3.one;
			segment.transform.localRotation = Quaternion.identity;
			segment.Reset();

			segment.MeshRenderer.sharedMaterial = material;
			segment.MeshRenderer.shadowCastingMode = castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
			segment.MeshRenderer.lightProbeUsage = LightProbeUsage.Off;
			segment.MeshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
			segment.MeshRenderer.allowOcclusionWhenDynamic = false;

			segment.GenerateGeometry(pointsInWorldSpace, infos, widthFront, widthBack, frontProjectUVs, 1f / middleUVScale, zDivisions);

			return segment;
		}

		public void Reset()
		{
			MeshFilter = this.gameObject.GetComponent<MeshFilter>();
			if (MeshFilter == null)
			{
				MeshFilter = this.gameObject.AddComponent<MeshFilter>();
			}

			MeshRenderer = this.gameObject.GetComponent<MeshRenderer>();
			if (MeshRenderer == null)
			{
				MeshRenderer = this.gameObject.AddComponent<MeshRenderer>();
			}
		}

		public void SetMaterial(Material material)
		{
			MeshRenderer.sharedMaterial = material;
		}

		public void GenerateGeometry(Vector3[] pointsInWorldSpace, List<MeshPointInfo> infos, float widthFront, float widthBack, bool frontProjectUVs, float middleUVScale, int zDivisions)
		{
			var zPosFront = widthFront;
			var zPosBack = widthBack;

			Mesh mesh;
			if (MeshFilter.sharedMesh == null)
			{
				mesh = new Mesh();
			}
			else
			{
				mesh = MeshFilter.sharedMesh;
				mesh.Clear();
			}

			int numOfMeshSegments = pointsInWorldSpace.Length;
			Vector3 pointA, pointB;
			int trisPerSegment = 2 * zDivisions; // 2
			int verticesPerSegment = 2 + zDivisions * 2; // 4
			Vector3[] meshVertices = new Vector3[numOfMeshSegments * verticesPerSegment];
			Vector2[] meshUVs = new Vector2[numOfMeshSegments * verticesPerSegment];
			int[] meshTriangles = new int[numOfMeshSegments * trisPerSegment * 3];
			int meshVertexIndex = 0;
			int meshUVIndex = 0;
			int meshTrianglesIndex = 0;
			int pointCounter = 0;
			MeshPointInfo infoA;
			MeshPointInfo infoB;
			float offsetTopA, frontMiddleMultiplierA, backMiddleMultiplierA;
			float offsetTopB, frontMiddleMultiplierB, backMiddleMultiplierB;
			Vector3 p0, p1, p2, p3;
			float uvY = 0f;
			float uvOriginV = pointsInWorldSpace[0].x;

			Vector2 forward = Vector2.zero;
			for (int i = 0; i < numOfMeshSegments; i++) // all except last
			{
				pointA = transform.InverseTransformPoint(pointsInWorldSpace[i]);
				pointB = transform.InverseTransformPoint(pointsInWorldSpace[(i + 1) % numOfMeshSegments]);
				pointA.z = 0;
				pointB.z = 0;
				
				infoA = MeshPointInfo.FindClosest(infos, pointsInWorldSpace[i]);
				offsetTopA = MeshPointInfo.CalculateTopOffset(infoA);
				frontMiddleMultiplierA = MeshPointInfo.CalculateFrontMiddleMultiplier(infoA);
				backMiddleMultiplierA = MeshPointInfo.CalculateBackMiddleMultiplier(infoA);

				infoB = MeshPointInfo.FindClosest(infos, pointsInWorldSpace[(i + 1) % numOfMeshSegments]);
				offsetTopB = MeshPointInfo.CalculateTopOffset(infoB);
				frontMiddleMultiplierB = MeshPointInfo.CalculateFrontMiddleMultiplier(infoB);
				backMiddleMultiplierB = MeshPointInfo.CalculateBackMiddleMultiplier(infoB);

				//UtilsDebug.DrawCircle(transform.TransformPoint(pointA), 0.2f, Color.red, 10.0f);
				//UtilsDebug.DrawCircle(transform.TransformPoint(pointB), 0.2f, Color.green, 10.0f);

				float stepSize = 1f / zDivisions;
				
				float maxZA = zPosBack * backMiddleMultiplierA;
				float minZA = -zPosFront * frontMiddleMultiplierA;
				float zRangeA = maxZA - minZA;
				float zBackA, zFrontA;

				float maxZB = zPosBack * backMiddleMultiplierB;
				float minZB = -zPosFront * frontMiddleMultiplierB;
				float zRangeB = maxZB - minZB;
				float zBackB, zFrontB;

				// first vertices (the initial 2)
				zBackA = maxZA;
				zBackB = maxZB;
				p2 = meshVertices[meshVertexIndex++] = pointA + new Vector3(0, offsetTopA, zBackA);
				p3 = meshVertices[meshVertexIndex++] = pointB + new Vector3(0, offsetTopB, zBackB);

				// advance around the middle loop
				uvY += forward.magnitude;
				forward = p3 - p2;

				// first UVs (the initial 2)
				if (frontProjectUVs)
				{
					meshUVs[meshUVIndex++] = new Vector2(p2.x, p2.y);
					meshUVs[meshUVIndex++] = new Vector2(p3.x, p3.y);
				}
				else
                {
					meshUVs[meshUVIndex++] = new Vector2(0, uvOriginV + ( uvY                      * middleUVScale));
					meshUVs[meshUVIndex++] = new Vector2(0, uvOriginV + ((uvY + forward.magnitude) * middleUVScale));

					// Ensure tiling is aligned at the front. Remove these two lines to align with back.
					// TODO: Maybe make this configurable and add tiling from center and streching.
					meshUVs[meshUVIndex - 2].x -= zRangeA * middleUVScale;
					meshUVs[meshUVIndex - 1].x -= zRangeB * middleUVScale;
				}

				for (int v = 0; v < zDivisions; v++)
                {
					//zBackA  = maxZA - (zRangeA *  v      * stepSize);
					zFrontA = maxZA - (zRangeA * (v + 1) * stepSize);

					//zBackB  = maxZB - (zRangeB *  v      * stepSize);
					zFrontB = maxZB - (zRangeB * (v + 1) * stepSize);

					// vertices
					//p0 = meshVertices[meshVertexIndex++] = pointA + new Vector3(0, offsetTopA, zBackA);
					//p1 = meshVertices[meshVertexIndex++] = pointB + new Vector3(0, offsetTopB, zBackB);
					p0 = p3;
					p1 = p2;
					p2 = meshVertices[meshVertexIndex++] = pointA + new Vector3(0, offsetTopA, zFrontA);
					p3 = meshVertices[meshVertexIndex++] = pointB + new Vector3(0, offsetTopB, zFrontB);

					// UVs
					if (frontProjectUVs)
					{
						meshUVs[meshUVIndex++] = new Vector2(p2.x, p2.y);
						meshUVs[meshUVIndex++] = new Vector2(p3.x, p3.y);
					}
					else
					{
						// TODO: If the middle part is stretched in depth by MeshBezierPointInfo then the middle UVs will be distorted.
						// To fix that we would have to either take the MeshBezierPointInfo into account here or recalculate the UVs later.
						//Vector2 forward = p3 - p2;
						// uvY -= (int)uvY; // not needed
						//meshUVs[meshUVIndex++] = new Vector2(0, uvY * middleUVScale);
						//meshUVs[meshUVIndex++] = new Vector2(0, (uvY + forward.magnitude) * middleUVScale);
                        //meshUVs[meshUVIndex++] = new Vector2( (zRangeA * multA * (v+1) * stepSize * middleUVScale),  uvY                      * middleUVScale);
                        //meshUVs[meshUVIndex++] = new Vector2( (zRangeB * multB * (v+1) * stepSize * middleUVScale), (uvY + forward.magnitude) * middleUVScale);
						meshUVs[meshUVIndex++] = new Vector2(zRangeA * (v + 1) * stepSize * middleUVScale, uvOriginV + ( uvY                      * middleUVScale));
						meshUVs[meshUVIndex++] = new Vector2(zRangeB * (v + 1) * stepSize * middleUVScale, uvOriginV + ((uvY + forward.magnitude) * middleUVScale));

						// Ensure tiling is aligned at the front. Remove these two lines to align with back.
						// TODO: Maybe make this configurable and add tiling from center and streching.
						meshUVs[meshUVIndex - 2].x -= zRangeA * middleUVScale;
						meshUVs[meshUVIndex - 1].x -= zRangeB * middleUVScale;
					}

					// tri1
					meshTriangles[meshTrianglesIndex++] = meshVertexIndex - 2;
					meshTriangles[meshTrianglesIndex++] = meshVertexIndex - 3;
					meshTriangles[meshTrianglesIndex++] = meshVertexIndex - 4;
					
					// tri2
					meshTriangles[meshTrianglesIndex++] = meshVertexIndex - 1;
					meshTriangles[meshTrianglesIndex++] = meshVertexIndex - 3;
					meshTriangles[meshTrianglesIndex++] = meshVertexIndex - 2;
					
				}

				pointCounter++;
			}

			mesh.vertices = meshVertices;
			mesh.uv = meshUVs;
			mesh.triangles = meshTriangles;
			mesh.RecalculateNormals();
			MeshFilter.sharedMesh = mesh;
		}
	}
}