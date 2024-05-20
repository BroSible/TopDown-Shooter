using Krem.AppCore.Runtime.Implementation.Attributes;
using UnityEngine;
using Component = Krem.AppCore.Runtime.Implementation.Component;

namespace TopDownShooter.Components
{
    [GroupName("TopDownShooter")]
    public class PlayerComponent : Component
    {
        [Header("Move Input")] 
        public KeyCode upKey = KeyCode.W;
        public KeyCode downKey = KeyCode.S;
        public KeyCode leftKey = KeyCode.A;
        public KeyCode rightKey = KeyCode.D;
    }
}
