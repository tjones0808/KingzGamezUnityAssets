using UnityEngine;

namespace EasyBuildSystem.Runtimes.Internal.Managers.Data
{
    public class InputsCollection : ScriptableObject
    {
        #region Public Fields

        public KeyCode InputPlacementKey = KeyCode.E;
        public bool InputPlacementIsCustom = false;
        public string InputPlacementName = "Ebs_Placement";

        public KeyCode InputDestructionKey = KeyCode.R;
        public bool InputDestructionIsCustom = false;
        public string InputDestructionName = "Ebs_Destruction";

        public string InputSwitchName = "Mouse ScrollWheel";

        public KeyCode InputEditionKey = KeyCode.T;
        public bool InputEditionIsCustom = false;
        public string InputEditionName = "Ebs_Edition";

        public KeyCode InputActionKey = KeyCode.Mouse0;
        public bool InputActionIsCustom = false;
        public string InputActionName = "Ebs_Validate";

        public KeyCode InputCancelKey = KeyCode.Mouse1;
        public bool InputCancelIsCustom = false;
        public string InputCancelName = "Ebs_Cancel";

        #endregion Public Fields
    }
}