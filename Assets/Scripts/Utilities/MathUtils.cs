
using Unity.Mathematics;



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

}
