using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LayerHelper
{
    public static int Player { get; private set; }
    public static int Enemy { get; private set; }
    public static int Projectile { get; private set; }

    public static void Init()
    {
        Player = LayerMask.NameToLayer("Player");
        Enemy = LayerMask.NameToLayer("Enemy");
        Projectile = LayerMask.NameToLayer("Projectile");
    }
}
