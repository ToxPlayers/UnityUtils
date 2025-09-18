using Sirenix.OdinInspector;
using UnityEngine;
/// taken from https://github.com/llamacademy/juicy-springs/blob/main/Assets/Scripts/Spring/FloatSpring.cs

namespace Springs
{ 
    [System.Serializable]
    public class FloatSpring 
    {
        static public FloatSpring DefaultSpringySettings() => new FloatSpring() { Damping = 0.3f, Frequency = 10f, GoalPosition = 1f};

        [BoxGroup("Spring Settings"), Tooltip("Damping >= 1 = no overshooting.\nDamping = 0 -> bounces forever.") ]
        public float Damping = 0.3f;
        [BoxGroup("Spring Settings"), Tooltip("Higher Frequency (20) -> faster spring.\nLower Frequency (3) -> slower spring.")]
        public float Frequency = 10f;
        [BoxGroup("Updated Values")]
        public float CurrentPosition = 0f, Velocity, GoalPosition = 1f;

        public void UpdateSpring(float deltaTime)
        {
            SpringUtil.CalcDampedSimpleHarmonicMotion(ref CurrentPosition, ref Velocity, GoalPosition, deltaTime, Frequency, Damping);
        }
         
        public void DrawGizmos(Matrix4x4 matrix, Vector3 dir, float size = 0.5f)
        {
            DrawGizmos(matrix, dir, size, Color.yellow, Color.blue, Color.green);
        }

        public void DrawGizmos(Matrix4x4 matrix, Vector3 dir, float size, Color curValueColor, Color velocityColor, Color goalColor)
		{
			var prevColor = Gizmos.color;

			dir.Normalize();
			var prevMatrix = Gizmos.matrix;
			Gizmos.matrix = matrix;

			Gizmos.color = curValueColor;
			Gizmos.DrawWireSphere(CurrentPosition * dir, size / 2f);

			Gizmos.color = velocityColor;
			Gizmos.DrawLine( (Velocity + CurrentPosition) * dir , CurrentPosition * dir);

			Gizmos.color = goalColor;
			Gizmos.DrawWireCube(GoalPosition * dir, size * Vector3.one);

			Gizmos.color = prevColor;
			Gizmos.matrix = prevMatrix;
		}

    }
}
