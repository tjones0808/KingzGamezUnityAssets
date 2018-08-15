using EasyBuildSystem.Runtimes.Extensions;
using EasyBuildSystem.Runtimes.Internal.Builder;
using EasyBuildSystem.Runtimes.Internal.Managers;
using EasyBuildSystem.Runtimes.Internal.Managers.Data;
using UnityEngine;

public class DefaultBuilderBehaviour : BuilderBehaviour
{
    #region Public Fields

    public bool CreativeMode = true;

    public AudioSource Audio;

    #endregion Public Fields

    #region Public Methods

    public override void UpdateModes()
    {
        base.UpdateModes();

        if (BuildManager.Instance == null)
            return;

        if (BuildManager.Instance.PartsCollection == null)
        {
            Debug.LogWarning("<b><color=yellow>[Easy Build System]</color></b> : Empty Parts Collection in the component Build Manager -> Parts Collection.");
            return;
        }

        if (CreativeMode && InputsCollection == null)
        {
            Debug.LogError("<b><color=red>[Easy Build System]</color></b> : Please select a Inputs Collection in the component Builder Behaviour -> Inputs Collection.");
            return;
        }

        if (CreativeMode)
        {
            if (InputsCollection.InputPlacementIsCustom ? Input.GetButtonDown(InputsCollection.InputPlacementName) : Input.GetKeyDown(InputsCollection.InputPlacementKey))
                ChangeMode(BuildMode.Placement);

            if (InputsCollection.InputDestructionIsCustom ? Input.GetButtonDown(InputsCollection.InputDestructionName) : Input.GetKeyDown(InputsCollection.InputDestructionKey))
                ChangeMode(BuildMode.Destruction);

            if (InputsCollection.InputEditionIsCustom ? Input.GetButtonDown(InputsCollection.InputEditionName) : Input.GetKeyDown(InputsCollection.InputEditionKey))
                ChangeMode(BuildMode.Edition);

            if (CurrentMode != BuildMode.Placement)
                UpdatePrefabSelection();
        }

        if (InputsCollection.InputCancelIsCustom ? Input.GetButtonDown(InputsCollection.InputCancelName) : Input.GetKeyDown(InputsCollection.InputCancelKey))
            ChangeMode(BuildMode.None);

        if (CurrentMode == BuildMode.Placement)
        {
            if (UIExtension.IsCursorOverUserInterface())
                return;

            if (InputsCollection.InputActionIsCustom ? Input.GetButtonDown(InputsCollection.InputActionName) : Input.GetKeyDown(InputsCollection.InputActionKey))
                PlacePrefab();

            float WheelAxis = Input.GetAxis(InputsCollection != null ? InputsCollection.InputSwitchName : "Mouse ScrollWheel");

            if (WheelAxis > 0)
                RotatePreview(SelectedPrefab.RotationAxis);
            else if (WheelAxis < 0)
                RotatePreview(-SelectedPrefab.RotationAxis);
        }
        else if (CurrentMode == BuildMode.Edition)
        {
            if (InputsCollection.InputActionIsCustom ? Input.GetButtonDown(InputsCollection.InputActionName) : Input.GetKeyDown(InputsCollection.InputActionKey))
                EditPrefab();
        }
        else if (CurrentMode == BuildMode.Destruction)
        {
            if (UIExtension.IsCursorOverUserInterface())
                return;

            if (InputsCollection.InputActionIsCustom ? Input.GetButtonDown(InputsCollection.InputActionName) : Input.GetKeyDown(InputsCollection.InputActionKey))
                RemovePrefab();
        }
    }

    #endregion Public Methods

    #region Private Methods

    private void UpdatePrefabSelection()
    {
        float WheelAxis = Input.GetAxis(InputsCollection != null ? InputsCollection.InputSwitchName : "Mouse ScrollWheel");

        if (WheelAxis > 0)
        {
            if (SelectedIndex < BuildManager.Instance.PartsCollection.Parts.Count - 1)
                SelectedIndex++;
            else
                SelectedIndex = 0;
        }
        else if (WheelAxis < 0)
        {
            if (SelectedIndex > 0)
                SelectedIndex--;
            else
                SelectedIndex = BuildManager.Instance.PartsCollection.Parts.Count - 1;
        }

        SelectPrefab(BuildManager.Instance.PartsCollection.Parts[SelectedIndex]);
    }

    #endregion Private Methods
}