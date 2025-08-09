using GorillaShirts.Behaviours;
using GorillaShirts.Behaviours.UI;
using GorillaShirts.Models.Cosmetic;
using GorillaShirts.Models.UI;
using UnityEngine;

namespace GorillaShirts.Models.StateMachine
{
    internal class Menu_ShirtColour(Stand stand, Menu_StateBase previousState, IGorillaShirt shirt) : Menu_SubState(stand, previousState)
    {
        protected IGorillaShirt Shirt = shirt;

        private ColourPicker colourPicker;

        private Color colour;
        private bool usePlayerColour;

        private bool hasUpdated;

        public override void Enter()
        {
            colour = (Shirt.Colour.UsePlayerColour ? GorillaTagger.Instance.offlineVRRig.playerColor : Shirt.Colour.CustomColour) * 1f;
            usePlayerColour = Shirt.Colour.UsePlayerColour;

            colourPicker = Stand.colourPicker;
            colourPicker.SliderR.SetValue(colour.r);
            colourPicker.SliderG.SetValue(colour.g);
            colourPicker.SliderB.SetValue(colour.b);

            Stand.mainMenuRoot.SetActive(true);
            Stand.mainContentRoot.SetActive(false);
            Stand.mainMenu_colourSubMenu.SetActive(true);

            Stand.colourPicker_NavText.text = Shirt.Descriptor.ShirtName;

            Stand.mainSideBar.SetSidebarState(Sidebar.SidebarState.None);

            DisplayColour();
            GorillaTagger.Instance.offlineVRRig.OnColorChanged += HandleLocalColorChanged;
        }

        public void DisplayColour()
        {
            colourPicker = Stand.colourPicker;
            colourPicker.TextR.text = ShirtColour.ToDisplaySegment(colour.r).ToString();
            colourPicker.TextG.text = ShirtColour.ToDisplaySegment(colour.g).ToString();
            colourPicker.TextB.text = ShirtColour.ToDisplaySegment(colour.b).ToString();
            colourPicker.ColourPreview.color = colour;

            //Stand.colourPicker_ApplyButton.SetActive(!usePlayerColour);
            Stand.colourPicker_SyncButton.SetActive(!usePlayerColour);

            Stand.Character.SetShirtColour(Shirt, new ShirtColour()
            {
                CustomColour = colour,
                UsePlayerColour = usePlayerColour
            });
        }

        public override void Update()
        {
            hasUpdated = false;

            float r = colour.r;
            colour.r = colourPicker.SliderR.Value;
            float g = colour.g;
            colour.g = colourPicker.SliderG.Value;
            float b = colour.b;
            colour.b = colourPicker.SliderB.Value;

            if (colour.r != r || colour.g != g || colour.b != b)
            {
                hasUpdated = true;
                usePlayerColour = false;
                DisplayColour();
            }
        }

        public override void OnButtonPress(EButtonType button)
        {
            if (button == EButtonType.Return)
            {
                Stand.Character.SetShirtColour(Shirt, Shirt.Colour);
                Main.Instance.MenuStateMachine.SwitchState(PreviousState);
                return;
            }

            if (button == EButtonType.Button1)
            {
                Main.Instance.ColourShirt(Shirt, colour, usePlayerColour);
                Stand.Character.SetShirtColour(Shirt, Shirt.Colour);
                Main.Instance.MenuStateMachine.SwitchState(PreviousState);
                return;
            }

            if (button == EButtonType.Button2)
            {
                colour = GorillaTagger.Instance.offlineVRRig.playerColor * 1f;
                usePlayerColour = true;

                colourPicker.SliderR.SetValue(colour.r);
                colourPicker.SliderG.SetValue(colour.g);
                colourPicker.SliderB.SetValue(colour.b);

                DisplayColour();
                return;
            }
        }

        public override void Exit()
        {
            Stand.mainMenuRoot.SetActive(false);

            GorillaTagger.Instance.offlineVRRig.OnColorChanged -= HandleLocalColorChanged;
        }

        private void HandleLocalColorChanged(Color newColor)
        {
            if (usePlayerColour && hasUpdated) return;

            colour = newColor;
            usePlayerColour = true;
            DisplayColour();
        }
    }
}
