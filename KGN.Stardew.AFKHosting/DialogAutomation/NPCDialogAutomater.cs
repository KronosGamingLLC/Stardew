using KGN.Stardew.Framework;
using KGN.Stardew.Framework.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KGN.Stardew.Framework.StardewAPI;

namespace KGN.Stardew.AFKHosting.DialogAutomation
{
    public class NPCDialogAutomater
    {
        private readonly NPC npc;
        private readonly Stack<DialogAction> dialogActions = new Stack<DialogAction>();
        private readonly IModHelper helper;
        private readonly IMonitor monitor;
        private readonly bool festival;

        private DialogueBox previousDialogBox;
        private IReflectedField<bool> transitioningField;
        private bool previousTransitioningValue;
        private event EventHandler transitionComplete;
        private int waitTickCount = -1;
        private bool wait;
        private Action waitAction;

        public NPCDialogAutomater(string NPCName, bool atFestival, IMonitor monitor, IModHelper helper, DialogAction[] dialogActions)
        {
            this.festival = atFestival;
            this.helper = helper;
            this.monitor = monitor;
            foreach (var action in dialogActions.Reverse())
                this.dialogActions.Push(action);
            npc = GetNPC(NPCName, atFestival);
            GameEvents.EighthUpdateTick += OnUpdate;
        }

        public NPCDialogAutomater(NPC npc)
        {
            this.npc = npc;
        }

        public void OnUpdate(object sender, EventArgs args)
        {
            if (dialogActions.Count < 1 && !wait)
                GameEvents.EighthUpdateTick -= OnUpdate;

            if (wait && waitTickCount >= 0)
                waitTickCount--;
            else
                wait = false;

            if (waitTickCount == 3)
                FinishTextPrinting();

            if (waitTickCount == 0)
            {
                waitAction?.Invoke();
            }

            if(Game1.activeClickableMenu is DialogueBox dialogueBox && !ReferenceEquals(dialogueBox, previousDialogBox))
            {
                previousDialogBox = dialogueBox;
                transitioningField = helper.Reflection.GetField<bool>(Game1.activeClickableMenu, "transitioning");
            }
            else if(Game1.activeClickableMenu == null)
                transitioningField = null;

            if (transitioningField == null) return;

            var transitioning = transitioningField.GetValue();

            if (previousTransitioningValue && !transitioning)
                transitionComplete?.Invoke(this, EventArgs.Empty);

            previousTransitioningValue = transitioning;
        }

        public void TeleportToNPC()
        {
            var location = GetNearestPassableUnoccupiedTile(npc.currentLocation, npc.getTileLocation());
            if (location == null) return;

            if(Game1.currentLocation == npc.currentLocation)
            {
                Game1.player.setTileLocation(new Vector2(location.Value.tile.X, location.Value.tile.Y ));
                Game1.player.FacingDirection = location.Value.facingDirection;
                return;
            }

            TeleportThisPlayer(npc.currentLocation.Name, Convert.ToInt32(location.Value.tile.X),Convert.ToInt32(location.Value.tile.Y));
        }

        public void RunActions()
        {
            Wait(0, RunNextAction);
        }

        private void RunNextAction()
        {
            var action = dialogActions.Pop();
            switch (action.DialogActionType)
            {
                case DialogActionType.Start:
                    transitionComplete += OnTransitionComplete;
                    Wait(2, StartDialog);
                    break;
                case DialogActionType.Next:
                    Wait(2, NextDialog);
                    break;
                case DialogActionType.Answer:
                    Wait(2, () => Answer(action.AnswerIndex.Value));
                    break;
            }
        }

        private void StartDialog()
        {
            var mouseCoords = npc.getTileLocation().ConvertTileToMouseCoords();
            var mouseState = new MouseState(
                Convert.ToInt32(mouseCoords.X), 
                Convert.ToInt32(mouseCoords.Y), 
                0, 
                ButtonState.Released, 
                ButtonState.Released, 
                ButtonState.Pressed, 
                ButtonState.Released, 
                ButtonState.Released);
            Game1.setMousePosition(new Point(Convert.ToInt32(mouseCoords.X), Convert.ToInt32(mouseCoords.Y)));
            Game1.pressActionButton(new KeyboardState(), mouseState, new GamePadState());
        }

        private void FinishTextPrinting()
        {
            if (Game1.activeClickableMenu is DialogueBox dialogBox)
            {
                var characterIndexInDialogue = helper.Reflection.GetField<int>(dialogBox, "characterIndexInDialogue");
                var textMaxIndex = dialogBox.getCurrentString().Length - 1;

                if (characterIndexInDialogue.GetValue() != textMaxIndex)
                    characterIndexInDialogue.SetValue(textMaxIndex);
            }
        }

        private void OnTransitionComplete(object sender, EventArgs args)
        {
            transitionComplete -= OnTransitionComplete;
            Wait(8, RunNextAction);
        }

        private void NextDialog()
        {
            var currentDialog = npc.CurrentDialogue.Peek();
            if (currentDialog.isCurrentDialogueAQuestion())
                monitor.Log("Tried to skip over a question during dialog automation.", LogLevel.Error);

            if (Game1.activeClickableMenu is DialogueBox dialogueBox)
            {
                dialogueBox.receiveLeftClick(0, 0, false);
            }

            Wait(8, RunNextAction);
        }

        private void Answer(int answerIndex)
        {
            if (Game1.activeClickableMenu is DialogueBox dialogueBox)
            {
                var heightForQuestions = helper.Reflection.GetField<int>(dialogueBox, "heightForQuestions").GetValue();
                var height = helper.Reflection.GetField<int>(dialogueBox, "height").GetValue();
                var width = helper.Reflection.GetField<int>(dialogueBox, "width").GetValue();
                var y = helper.Reflection.GetField<int>(dialogueBox, "y").GetValue();

                var responses = helper.Reflection.GetField<List<Response>>(dialogueBox, "responses").GetValue();

                int num = y - (heightForQuestions - height) + SpriteText.getHeightOfString(dialogueBox.getCurrentString(), width - 16) + 48;

                num += SpriteText.getHeightOfString(responses[0].responseText, width - 16);

                for (var i = 0; i < answerIndex; i++)
                {
                    num += SpriteText.getHeightOfString(responses[i].responseText, width - 16) + 16;
                }

                num -= 1;

                Game1.setMousePosition(new Point(0, num));
                dialogueBox.performHoverAction(0, num);
                Wait(8, () => dialogueBox.receiveLeftClick(0, 0, false));
            }
        }

        private void Wait(int ticks, Action action)
        {
            wait = true;
            waitTickCount = ticks + 1;
            waitAction = action;
        }
    }
}
