using GorillaShirts.Behaviours;
using GorillaShirts.Behaviours.UI;
using GorillaShirts.Models.Cosmetic;
using GorillaShirts.Models.UI;
using UnityEngine;

namespace GorillaShirts.Models.StateMachine
{
    internal class Menu_ShirtCustomColour(Stand stand, Menu_StateBase previousState, IGorillaShirt shirt) : Menu_SubState(stand, previousState)
    {
        protected IGorillaShirt Shirt = shirt;

        private ColourPicker colourPicker;

        private ShirtColour shirtColour;

        private bool hasUpdated;

        private Color Colour => shirtColour.CustomColour;
        private bool UsePlayerColour => shirtColour.UsePlayerColour;
        private Color PlayerColour => GorillaTagger.Instance.offlineVRRig.playerColor;
        private float RandomDigit => 0.1f * Random.Range(0, 11);

        public override void Enter()
        {
            shirtColour = new ShirtColour()
            {
                CustomColour = (Shirt.Colour.UsePlayerColour ? PlayerColour : Shirt.Colour.CustomColour) * 1f,
                UsePlayerColour = Shirt.Colour.UsePlayerColour
            };

            colourPicker = Stand.colourPicker;
            colourPicker.SliderR.SetValue(Colour.r);
            colourPicker.SliderG.SetValue(Colour.g);
            colourPicker.SliderB.SetValue(Colour.b);

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
            colourPicker.TextR.text = ShirtColour.ToDisplaySegment(Colour.r).ToString();
            colourPicker.TextG.text = ShirtColour.ToDisplaySegment(Colour.g).ToString();
            colourPicker.TextB.text = ShirtColour.ToDisplaySegment(Colour.b).ToString();
            colourPicker.ColourPreview.color = Colour;

            //Stand.colourPicker_ApplyButton.SetActive(!usePlayerColour);
            Stand.colourPicker_SyncButton.SetActive(!UsePlayerColour);

            Stand.Character.SetShirtColour(Shirt, new ShirtColour()
            {
                CustomColour = Colour,
                UsePlayerColour = UsePlayerColour
            });
        }

        public override void Update()
        {
            hasUpdated = false;

            Color colourToUpdate = shirtColour.CustomColour;

            float r = Colour.r;
            colourToUpdate.r = colourPicker.SliderR.Value;
            float g = Colour.g;
            colourToUpdate.g = colourPicker.SliderG.Value;
            float b = Colour.b;
            colourToUpdate.b = colourPicker.SliderB.Value;

            if (colourToUpdate.r != r || colourToUpdate.g != g || colourToUpdate.b != b)
            {
                hasUpdated = true;
                shirtColour.UsePlayerColour = false;
                shirtColour.CustomColour = colourToUpdate;
                DisplayColour();
            }
        }

        public override void OnButtonPress(EButtonType button)
        {
            switch(button)
            {
                case EButtonType.Button1: // submit custom colour
                    ShirtManager.Instance.ColourShirt(Shirt, Colour, UsePlayerColour);
                    Stand.Character.SetShirtColour(Shirt, Shirt.Colour);
                    ShirtManager.Instance.MenuStateMachine.SwitchState(PreviousState);
                    break;

                case EButtonType.Button2: // reset custom colour
                    shirtColour.CustomColour = PlayerColour * 1f;
                    shirtColour.UsePlayerColour = true;

                    colourPicker.SliderR.SetValue(Colour.r);
                    colourPicker.SliderG.SetValue(Colour.g);
                    colourPicker.SliderB.SetValue(Colour.b);
                    DisplayColour();
                    break;

                case EButtonType.Button3: // apply random custom colour
                    shirtColour.CustomColour = new Color(RandomDigit, RandomDigit, RandomDigit);
                    shirtColour.UsePlayerColour = false;

                    colourPicker.SliderR.SetValue(Colour.r);
                    colourPicker.SliderG.SetValue(Colour.g);
                    colourPicker.SliderB.SetValue(Colour.b);
                    DisplayColour();
                    break;

                case EButtonType.Return:
                    Stand.Character.SetShirtColour(Shirt, Shirt.Colour);
                    ShirtManager.Instance.MenuStateMachine.SwitchState(PreviousState);
                    break;
            }
        }

        public override void Exit()
        {
            Stand.mainMenuRoot.SetActive(false);

            GorillaTagger.Instance.offlineVRRig.OnColorChanged -= HandleLocalColorChanged;
        }

        private void HandleLocalColorChanged(Color newColor)
        {
            if (UsePlayerColour && hasUpdated) return;

            shirtColour.CustomColour = newColor;
            shirtColour.UsePlayerColour = true;
            DisplayColour();
        }
    }
}
