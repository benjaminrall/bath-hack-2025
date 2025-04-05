using UnityEngine;
using System.Collections.Generic;
using System;

public class mapping_landmarks_to_sphere : MonoBehaviour
{
    public string imageName = "face"; // face.png
    public int outputTextureWidth = 512;
    public int outputTextureHeight = 512;

    public void mapFaceToSphere(LandmarkArrayWrapper landmarkWrapper)
    {
        if (landmarkWrapper == null || landmarkWrapper.landmarks == null || landmarkWrapper.landmarks.Length < 8)
        {
            Debug.LogError("LandmarkWrapper must have at least 8 landmarks.");
            return;
        }

        // Load the original face image
        Texture2D sourceImage = Resources.Load<Texture2D>(imageName);
        if (sourceImage == null)
        {
            Debug.LogError("face.png not found in Resources.");
            return;
        }

        // Source landmark points in pixel space
        Vector2[] srcPoints = new Vector2[8];
        for (int i = 0; i < 8; i++)
        {
            srcPoints[i] = new Vector2(landmarkWrapper.landmarks[i].x, landmarkWrapper.landmarks[i].y);
        }

        // Destination landmark points (normalized UV layout → scaled to texture size)
        Vector2[] dstPoints = new Vector2[]
        {
            new Vector2(0.3f, 0.7f),
            new Vector2(0.7f, 0.7f),
            new Vector2(0.4f, 0.55f),
            new Vector2(0.6f, 0.55f),
            new Vector2(0.35f, 0.35f),
            new Vector2(0.65f, 0.35f),
            new Vector2(0.5f, 0.4f),
            new Vector2(0.5f, 0.3f)
        };

        for (int i = 0; i < dstPoints.Length; i++)
        {
            dstPoints[i] = new Vector2(dstPoints[i].x * outputTextureWidth, dstPoints[i].y * outputTextureHeight);
        }

        // Triangles over landmarks (indices into the 8-point arrays)
        int[][] triangles = new int[][]
        {
            new int[] {0, 2, 4},
            new int[] {0, 4, 6},
            new int[] {1, 3, 5},
            new int[] {1, 5, 6},
            new int[] {2, 3, 6},
            new int[] {4, 5, 7}
        };

        Texture2D output = new Texture2D(outputTextureWidth, outputTextureHeight, TextureFormat.RGBA32, false);
        Color clearColor = Color.black;

        // Clear output texture
        for (int y = 0; y < outputTextureHeight; y++)
        {
            for (int x = 0; x < outputTextureWidth; x++)
            {
                output.SetPixel(x, y, clearColor);
            }
        }

        // Warp each triangle
        foreach (int[] tri in triangles)
        {
            WarpTriangle(
                sourceImage,
                output,
                srcPoints[tri[0]], srcPoints[tri[1]], srcPoints[tri[2]],
                dstPoints[tri[0]], dstPoints[tri[1]], dstPoints[tri[2]]
            );
        }

        output.Apply();

        // Assign to sphere's material
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.mainTexture = output;
            Debug.Log("Warped texture applied to sphere.");
        }

        // Save the output texture as PNG
        byte[] pngData = output.EncodeToPNG();
        if (pngData != null)
        {
            string savePath = Application.persistentDataPath + "warped_face.png";

            // Ensure directory exists
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(savePath));

            System.IO.File.WriteAllBytes(savePath, pngData);
            Debug.Log("Saved warped texture to: " + savePath);
        }

    }

    void WarpTriangle(Texture2D src, Texture2D dst,
        Vector2 srcP1, Vector2 srcP2, Vector2 srcP3,
        Vector2 dstP1, Vector2 dstP2, Vector2 dstP3)
    {
        // Affine transform matrix from destination to source triangle
        Matrix3x3 transform = CalculateAffineTransform(dstP1, dstP2, dstP3, srcP1, srcP2, srcP3);

        Rect bounds = GetBoundingRect(dstP1, dstP2, dstP3, dst.width, dst.height);

        for (int y = (int)bounds.yMin; y <= (int)bounds.yMax; y++)
        {
            for (int x = (int)bounds.xMin; x <= (int)bounds.xMax; x++)
            {
                Vector2 p = new Vector2(x, y);
                if (PointInTriangle(p, dstP1, dstP2, dstP3))
                {
                    Vector2 srcUV = transform.MultiplyPoint(p);
                    Color col = src.GetPixelBilinear(srcUV.x / src.width, srcUV.y / src.height);
                    dst.SetPixel(x, y, col);
                }
            }
        }
    }

    Rect GetBoundingRect(Vector2 p1, Vector2 p2, Vector2 p3, int texWidth, int texHeight)
    {
        float minX = Mathf.Clamp(Mathf.Min(p1.x, Mathf.Min(p2.x, p3.x)), 0, texWidth - 1);
        float maxX = Mathf.Clamp(Mathf.Max(p1.x, Mathf.Max(p2.x, p3.x)), 0, texWidth - 1);
        float minY = Mathf.Clamp(Mathf.Min(p1.y, Mathf.Min(p2.y, p3.y)), 0, texHeight - 1);
        float maxY = Mathf.Clamp(Mathf.Max(p1.y, Mathf.Max(p2.y, p3.y)), 0, texHeight - 1);

        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }

    bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float area = 0.5f * (-b.y * c.x + a.y * (-b.x + c.x) + a.x * (b.y - c.y) + b.x * c.y);
        float s = 1f / (2f * area) * (a.y * c.x - a.x * c.y + (c.y - a.y) * p.x + (a.x - c.x) * p.y);
        float t = 1f / (2f * area) * (a.x * b.y - a.y * b.x + (a.y - b.y) * p.x + (b.x - a.x) * p.y);
        float u = 1 - s - t;
        return s >= 0 && t >= 0 && u >= 0;
    }

    // Affine matrix utility
    struct Matrix3x3
    {
        private float[,] m;

        public Matrix3x3(float[,] values)
        {
            m = values;
        }

        public Vector2 MultiplyPoint(Vector2 p)
        {
            float x = m[0, 0] * p.x + m[0, 1] * p.y + m[0, 2];
            float y = m[1, 0] * p.x + m[1, 1] * p.y + m[1, 2];
            return new Vector2(x, y);
        }
    }

    Matrix3x3 CalculateAffineTransform(Vector2 src1, Vector2 src2, Vector2 src3, Vector2 dst1, Vector2 dst2, Vector2 dst3)
    {
        Matrix4x4 srcMat = MatrixFromPoints(src1, src2, src3);
        Matrix4x4 dstMat = MatrixFromPoints(dst1, dst2, dst3);

        Matrix4x4 transform = dstMat * srcMat.inverse;

        return new Matrix3x3(new float[,]
        {
            { transform.m00, transform.m01, transform.m03 },
            { transform.m10, transform.m11, transform.m13 },
            { 0, 0, 1 }
        });
    }

    Matrix4x4 MatrixFromPoints(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        Matrix4x4 mat = new Matrix4x4();
        mat.SetRow(0, new Vector4(p1.x, p1.y, 1, 0));
        mat.SetRow(1, new Vector4(p2.x, p2.y, 1, 0));
        mat.SetRow(2, new Vector4(p3.x, p3.y, 1, 0));
        mat.SetRow(3, new Vector4(0, 0, 0, 1));
        return mat;
    }
}
