using System;
using System.Linq;
using UnityEngine;
using Helper;

/*
 * Represent bitset in a unit block.
 */
public struct BitBlock
{
    public BitBlock(int[] m_)
    {
        m = Enumerable
            .Range(0, 8 /* capacity */)
            .Select(i => m_[i]).ToArray();
        // m = new int[8];
        // Array.Copy(m_, m, 8);
    }

    public BitBlock(int bitValue)
    {
        // m = new int[8];
        m = Enumerable
            .Range(0, 8 /* capacity */)
            .Select(i => bitValue & (1 << i)).ToArray();
        // for (int i = 0; i < 8; ++i)
        // {
        //     m[i] = bitValue & (1 << i);
        // }
    }
    int[] m;

    /*      
    * 2(2) __________________ 1(1)
    *     |\       |\       |\
    *     | \      | \      | \
    *     |  \_____|__\_____|__\
    *     | _|\ _ _| _ _\ _ |   \
    *     |\ | \    \   \    \   \ 
    *     | \|_ \3(4)\___\____\___\ 4(8)
    *6(32)|  \  |     \  |  5(16) |  
    *     \  |\ |      \ |      \ |
    *      \ | \|_______\|_______\|
    *       \|  |        |        |        +z  +y
    *        \  |        |        |          \ |
    *   7(64) \ |________|________| 8(128)    \|_____ +x
    *     
    *               _____________              _____________ 
    *   High Layer | 2(2) | 1(1) |  Low Layer |6(32) |5(16) |
    *              |______|______|            |______|______|
    *              | 3(4) | 4(8) |            |7(64) |8(128)|
    *              |______|______|            |______|______|  
    *             
    */
    public void Rotate()
    {
        m.RotateRight(0, 4); // High
        m.RotateRight(4, 4); // Low
    }

    public void Mirror(int axis = 0 /* 0:along x-axis, 1:along y-axis*/)
    {
        if (axis == 0)
        {
            (m[1], m[2]) = (m[2], m[1]);
            (m[0], m[3]) = (m[3], m[0]);
            (m[5], m[6]) = (m[6], m[5]);
            (m[4], m[7]) = (m[7], m[4]);
        }
        if (axis == 1)
        {
            (m[0], m[1]) = (m[1], m[0]);
            (m[2], m[3]) = (m[3], m[2]);
            (m[4], m[5]) = (m[5], m[4]);
            (m[6], m[7]) = (m[7], m[6]);
        }
    }

    public int IntValue
    {
        get
        {
            int bitValue = 0;
            foreach (var i in m.Select((value, index) => new { value, index }))
            {
                if (i.value > 0) bitValue += (1 << i.index);
            }
            return bitValue;
        }
    }

    public override string ToString()
    {
        return $"{IntValue}";        
    }
}

/*
 * Represent a bitset array in a container.
 */
public struct BitCube
{
    static int[] cubeCorners = new int[8];

    int sizeX;
    int sizeY;
    int sizeZ;
    int[] rawData;
    
    public int Length => sizeX * sizeY * sizeZ;
    public (int, int, int) Size => (sizeX, sizeY, sizeZ);

    public BitCube(int width, int height, int depth)
    {
        sizeX = width;
        sizeY = height;
        sizeZ = depth;

        rawData = new int[width * height * depth];        
    }

    int indexFromCoord(int x, int y, int z)
    {
        return z + sizeZ * (x + sizeX * y);
    }

    public int CalculateIsoSurface(Vector3Int id, int isoLevel = 1)
    {
        Array.Clear(cubeCorners, 0, 8);

        if (id.x < sizeX && id.y < sizeY && id.z < sizeZ) cubeCorners[0] = rawData[indexFromCoord(id.x, id.y, id.z)];
        if (id.x > 0 && id.y < sizeY && id.z < sizeZ) cubeCorners[1] = rawData[indexFromCoord(id.x - 1, id.y, id.z)];
        if (id.x > 0 && id.y < sizeY && id.z > 0) cubeCorners[2] = rawData[indexFromCoord(id.x - 1, id.y, id.z - 1)];
        if (id.x < sizeX && id.y < sizeY && id.z > 0) cubeCorners[3] = rawData[indexFromCoord(id.x, id.y, id.z - 1)];
        if (id.x < sizeX && id.y > 0 && id.z < sizeZ) cubeCorners[4] = rawData[indexFromCoord(id.x, id.y - 1, id.z)];
        if (id.x > 0 && id.y > 0 && id.z < sizeZ) cubeCorners[5] = rawData[indexFromCoord(id.x - 1, id.y - 1, id.z)];
        if (id.x > 0 && id.y > 0 && id.z > 0) cubeCorners[6] = rawData[indexFromCoord(id.x - 1, id.y - 1, id.z - 1)];
        if (id.x < sizeX && id.y > 0 && id.z > 0) cubeCorners[7] = rawData[indexFromCoord(id.x, id.y - 1, id.z - 1)];
        
        int cubeIndex = 0;
        for (int it = 0; it < 8; ++it)
        {
            if (cubeCorners[it] >= isoLevel) cubeIndex |= (1 << it);
        }
        return cubeIndex;
    }

    public int this[Vector3Int id]
    {
        get => rawData[indexFromCoord(id.x, id.y, id.z)];
        set => rawData[indexFromCoord(id.x, id.y, id.z)] = value;
    }

    public int this[int i, int j, int k]
    {
        get => rawData[indexFromCoord(i, j, k)];
        set => rawData[indexFromCoord(i, j, k)] = value;
    }
}
