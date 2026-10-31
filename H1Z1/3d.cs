using System.Numerics;

namespace view;

class viewMat {
    public static Vector2? WorldToScreen(
        Vector3 worldPos,
        Matrix4x4 camera,
        int screenWidth,
        int screenHeight)
    {
        Vector3 camPos = new(camera.M41, camera.M42, camera.M43);
        Vector3 right   = new(camera.M11, camera.M12, camera.M13);
        Vector3 up      = new(camera.M21, camera.M22, camera.M23);
        Vector3 forward = new(camera.M31, camera.M32, camera.M33);

        Vector3 delta = worldPos - camPos;

        float x = Vector3.Dot(delta, right);
        float y = Vector3.Dot(delta, up);
        float z = Vector3.Dot(delta, forward);
        //Console.WriteLine($"Delta : {delta}, z : {z}");

        if (z <= 0.01f)
            return null;

        float fov = 65.0f;
        float focal = screenWidth * 0.5f /
                    MathF.Tan(fov * 0.5f * MathF.PI / 180.0f);

        return new Vector2(
            screenWidth * 0.5f + (x * focal / z),
            screenHeight * 0.5f - (y * focal / z)
        );
    }
}