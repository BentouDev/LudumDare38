using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NexusUI : MonoBehaviour
{
    public Nexus Nexus;
    public Image UI;
    public float RotateAngle = 90.0f;

    public Gradient Color;

    void Update()
    {
        if (!Nexus)
            return;

        var coefficient = (Nexus.CurrentHealth / (float)Nexus.MaxHealth);

        UI.color = Color.Evaluate(1 - coefficient);
        UI.fillAmount = coefficient;
    }
}
