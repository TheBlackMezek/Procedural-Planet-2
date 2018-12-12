
using Unity.Mathematics;



[System.Serializable]
public struct HyperDistance
{
    public int oct;
    public float prs;

    public static HyperDistance operator+(HyperDistance hyd, float fl)
    {
        float octantSize = HyperposStaticReferences.OctantSize;
        hyd.prs += fl;
        float overflow = math.floor(hyd.prs / octantSize);
        hyd.prs -= overflow * octantSize;
        hyd.oct += (int)overflow;

        return hyd;
    }

    public static HyperDistance operator+(HyperDistance lhs, HyperDistance rhs)
    {
        float octantSize = HyperposStaticReferences.OctantSize;

        HyperDistance final;

        final.prs = lhs.prs + rhs.prs;
        final.oct = lhs.oct + rhs.oct;
        float overflow = math.floor(final.prs / octantSize);
        final.prs -= overflow * octantSize;
        final.oct += (int)overflow;

        return final;
    }

    public static HyperDistance operator*(HyperDistance hyd, float fl)
    {
        float octantSize = HyperposStaticReferences.OctantSize;

        hyd.prs *= fl;
        float oct = hyd.oct * fl;

        float overflow = math.floor(hyd.prs / octantSize);
        hyd.prs -= overflow * octantSize;
        hyd.oct += (int)overflow;

        return hyd;
    }

    public static HyperDistance operator*(float fl, HyperDistance hyd)
    {
        float octantSize = HyperposStaticReferences.OctantSize;

        hyd.prs *= fl;
        float oct = hyd.oct * fl;

        float overflow = math.floor(hyd.prs / octantSize);
        hyd.prs -= overflow * octantSize;
        hyd.oct += (int)overflow;

        return hyd;
    }

    public static HyperPosition operator*(float3 fl3, HyperDistance hyd)
    {
        HyperPosition hyp = new HyperPosition { prs = fl3, oct = new int3() };

        return hyp * hyd;
    }

    public static HyperPosition operator*(HyperPosition hyp, HyperDistance hyd)
    {
        int octantSize = (int)HyperposStaticReferences.OctantSize;

        HyperPosition a = hyp * hyd.prs;
        int3 oct = a.oct + (hyd.oct * hyp.oct * octantSize);
        float3 octF = hyp.prs * hyd.oct;

        float3 overflow = octF % 1f;
        octF -= overflow;
        a.prs += overflow * octantSize;
        overflow = math.floor(a.prs / octantSize);
        a.prs -= overflow * octantSize;
        octF += overflow;

        return new HyperPosition { prs = a.prs, oct = oct + (int3)octF };
    }

    public static HyperDistance operator*(HyperDistance lhs, HyperDistance rhs)
    {
        float octantSize = HyperposStaticReferences.OctantSize;

        HyperDistance rhsprs = new HyperDistance { prs = rhs.prs, oct = 0 };
        HyperDistance rhsoct = new HyperDistance { prs = 0, oct = rhs.oct };
        HyperDistance lhsprs = new HyperDistance { prs = lhs.prs, oct = 0 };
        HyperDistance lhsoct = new HyperDistance { prs = 0, oct = lhs.oct };

        HyperDistance a = rhsprs.prs * lhsprs;
        HyperDistance b = rhsprs.prs * lhsoct;
        float oct = (rhsoct.oct * lhsprs.prs) + (rhsoct.oct * lhsoct.oct * octantSize);
        float overflow = oct % 1f;
        oct -= overflow;
        overflow *= octantSize;
        a.prs += overflow;
        a.oct += (int)oct;

        return a + b;
        //HyperDistance a = lhs * rhs.prs;
        //int oct = a.oct + (rhs.oct * lhs.oct * (int)octantSize);
        //
        //float octF = lhs.prs * rhs.oct;
        //float overflow = octF % 1f;
        //octF -= overflow;
        //a.prs += overflow * octantSize;
        //overflow = math.floor(a.prs / octantSize);
        //a.prs -= overflow * octantSize;
        //octF += overflow;
        //
        //return new HyperDistance { prs = a.prs, oct = oct + (int)octF };
    }
}

[System.Serializable]
public struct HyperPosition
{
    public int3 oct;
    public float3 prs;

    public static HyperPosition operator+(HyperPosition hyp, float fl)
    {
        float octantSize = HyperposStaticReferences.OctantSize;

        hyp.prs += fl;
        float3 overflow = math.floor(hyp.prs / octantSize);
        hyp.prs -= overflow * octantSize;
        hyp.oct += (int3)overflow;

        return hyp;
    }

    public static HyperPosition operator-(HyperPosition lhs, HyperPosition rhs)
    {
        float octantSize = HyperposStaticReferences.OctantSize;

        lhs.prs -= rhs.prs;
        lhs.oct -= rhs.oct;

        float3 overflow = math.floor(lhs.prs / octantSize);
        lhs.prs -= overflow * octantSize;
        lhs.oct += (int3)overflow;

        return lhs;
    }

    public static HyperPosition operator*(float3 fl3, HyperPosition hyp)
    {
        float octantSize = HyperposStaticReferences.OctantSize;

        hyp.prs *= fl3;
        float3 oct = hyp.oct * fl3;
        float3 overflow = oct % 1f;
        oct -= overflow;
        hyp.prs += overflow * octantSize;
        overflow = math.floor(hyp.prs / octantSize);
        hyp.prs -= overflow * octantSize;
        oct += overflow;
        hyp.oct = (int3)oct;

        return hyp;
    }

    public static HyperPosition operator*(HyperPosition hyp, float fl)
    {
        float octantSize = HyperposStaticReferences.OctantSize;

        hyp.prs *= fl;
        float3 oct = (float3)hyp.oct * fl;
        float3 overflow = oct % 1f;
        oct -= overflow;
        hyp.prs += overflow * octantSize;
        overflow = math.floor(hyp.prs / octantSize);
        hyp.prs -= overflow * octantSize;
        oct += overflow;
        hyp.oct = (int3)oct;

        return hyp;
    }
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
            return new HyperDistance { oct = 0, prs = math.distance(prc1, prc2) };
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

            return new HyperDistance { oct = (int)octMag, prs = prcMag };
        }
    }

    public static HyperDistance Distance(HyperPosition p1, HyperPosition p2)
    {
        if (Equals(p1.oct, p2.oct))
        {
            return new HyperDistance { oct = 0, prs = math.distance(p1.prs, p2.prs) };
        }
        else
        {
            float octantSize = HyperposStaticReferences.OctantSize;

            int3 octDist = p2.oct - p1.oct;
            float3 prcDist;

            if (octDist.x == 0)
            {
                prcDist.x = p2.prs.x - p1.prs.x;
            }
            else
            {
                int dir = octDist.x > 0 ? 1 : -1;
                octDist.x -= dir;
                if (dir == 1)
                    prcDist.x = (octantSize - p1.prs.x) + p2.prs.x;
                else
                    prcDist.x = (octantSize - p2.prs.x) + p1.prs.x;

                float overflowF = math.floor(prcDist.x / octantSize);
                int overflow = (int)overflowF;
                prcDist.x -= overflowF * octantSize;
                octDist.x += overflow;
            }

            if (octDist.y == 0)
            {
                prcDist.y = p2.prs.y - p1.prs.y;
            }
            else
            {
                int dir = octDist.y > 0 ? 1 : -1;
                octDist.y -= dir;
                if (dir == 1)
                    prcDist.y = (octantSize - p1.prs.y) + p2.prs.y;
                else
                    prcDist.y = (octantSize - p2.prs.y) + p1.prs.y;

                float overflowF = math.floor(prcDist.y / octantSize);
                int overflow = (int)overflowF;
                prcDist.y -= overflowF * octantSize;
                octDist.y += overflow;
            }

            if (octDist.z == 0)
            {
                prcDist.z = p2.prs.z - p1.prs.z;
            }
            else
            {
                int dir = octDist.z > 0 ? 1 : -1;
                octDist.z -= dir;
                if (dir == 1)
                    prcDist.z = (octantSize - p1.prs.z) + p2.prs.z;
                else
                    prcDist.z = (octantSize - p2.prs.z) + p1.prs.z;

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

            return new HyperDistance { oct = (int)octMag, prs = prcMag };
        }
    }

    public static bool Equals(int3 a, int3 b)
    {
        return a.x == b.x && a.y == b.y && a.z == b.z;
    }

    public static string ToString(HyperPosition hyp)
    {
        return hyp.oct.ToString() + " : " + hyp.prs.ToString();
    }

    public static string ToString(HyperDistance hyd)
    {
        return (hyd.oct + " : " + hyd.prs);
    }

}
