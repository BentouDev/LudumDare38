using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyUI : MonoBehaviour
{
    public Pawn Pawn;
    public Image UI;
    public float RotateAngle = 90.0f;
    public float RotateCoefficient = 0.70f;

    public Gradient Color;

    void Update()
    {
        if (!Pawn)
            return;

        var coefficient = (Pawn.CurrentHealth / (float) Pawn.MaximumHealth);

        UI.color = Color.Evaluate(1 - coefficient);
        UI.fillAmount = coefficient * RotateCoefficient;
        UI.canvas.transform.rotation = Quaternion.LookRotation (
            Vector3.up,
            new Vector3 (
                Camera.main.transform.forward.x,
                0,
                Camera.main.transform.forward.z
            )
        );
    }
}
