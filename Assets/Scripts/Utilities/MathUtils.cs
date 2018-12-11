
using Unity.Mathematics;



public struct HyperDistance
{
    public int octantDist;
    public float preciseDist;
}



public class MathUtils {

	public static float3 right(quaternion q)
    {
        quaternion turn = quaternion.RotateY((float)(math.PI / 2f));
        q = math.mul(q, turn);
        return math.forward(q);
    }

    public static float3 up(quaternion q)
    {
        quaternion turn = quaternion.RotateX((float)(-math.PI / 2f));
        q = math.mul(q, turn);
        return math.forward(q);
    }

    /// <summary>
    /// Must input initial position first, then final
    /// </summary>
    /// <param name="oct1">Initial octant position</param>
    /// <param name="prc1">Initial precise position</param>
    /// <param name="oct2">Final octant position</param>
    /// <param name="prc2">Final precise position</param>
    /// <returns></returns>
    public static HyperDistance Distance(int3 oct1, float3 prc1, int3 oct2, float3 prc2)
    {
        if(Equals(oct1, oct2))
        {
            return new HyperDistance { octantDist = 0, preciseDist = math.distance(prc1, prc2) };
        }
        else
        {
            float octantSize = HyperposStaticReferences.OctantSize;

            int3 octDist = oct2 - oct1;
            float3 prcDist;

            if(octDist.x == 0)
            {
                prcDist.x = prc2.x - prc1.x;
            }
            else
            {
                int dir = octDist.x > 0 ? 1 : -1;
                --octDist.x;
                if (dir == 1)
                    prcDist.x = (octantSize - prc1.x) + prc2.x;
                else
                    prcDist.x = (octantSize - prc2.x) + prc1.x;

                float overflowF = math.floor(prcDist.x / octantSize);
                int overflow = (int)overflowF;
                prcDist.x -= overflowF * octantSize;
                octDist.x += overflow;
            }

            if (octDist.y == 0)
            {
                prcDist.y = prc2.y - prc1.y;
            }
            else
            {
                int dir = octDist.y > 0 ? 1 : -1;
                --octDist.y;
                if (dir == 1)
                    prcDist.y = (octantSize - prc1.y) + prc2.y;
                else
                    prcDist.y = (octantSize - prc2.y) + prc1.y;

                float overflowF = math.floor(prcDist.y / octantSize);
                int overflow = (int)overflowF;
                prcDist.y -= overflowF * octantSize;
                octDist.y += overflow;
            }

            if (octDist.z == 0)
            {
                prcDist.z = prc2.z - prc1.z;
            }
            else
            {
                int dir = octDist.z > 0 ? 1 : -1;
                --octDist.z;
                if (dir == 1)
                    prcDist.z = (octantSize - prc1.z) + prc2.z;
                else
                    prcDist.z = (octantSize - prc2.z) + prc1.z;

                float overflowF = math.floor(prcDist.z / octantSize);
                int overflow = (int)overflowF;
                prcDist.z -= overflowF * octantSize;
                octDist.z += overflow;
            }

            float octMag = math.length(octDist);
            float overflow2 = octMag % 1f;
            octMag -= overflow2;
            float prcMag = math.length(prcDist) + (overflow2 * octantSize);
            overflow2 = math.floor(prcMag / octantSize);
            prcMag -= overflow2 * octantSize;
            octMag += overflow2;

            return new HyperDistance { octantDist = (int)octMag, preciseDist = prcMag };
        }
    }

    public static bool Equals(int3 a, int3 b)
    {
        return a.x == b.x && a.y == b.y && a.z == b.z;
    }

}
