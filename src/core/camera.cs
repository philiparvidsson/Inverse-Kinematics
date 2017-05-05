namespace InverseKinematics.Core {

using Microsoft.Xna.Framework;

public class Camera {
    public Matrix Proj { get; set; } =
        Matrix.CreatePerspectiveFieldOfView(fieldOfView       : MathHelper.ToRadians(90.0f),
                                            aspectRatio       : 1.0f,
                                            nearPlaneDistance : 0.1f,
                                            farPlaneDistance  : 100.0f);

    public Vector3 Pos { get; set; }
    public Vector3 Target { get; set; }

}

}
