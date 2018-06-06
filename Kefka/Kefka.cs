using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using ff14bot;
using ff14bot.AClasses;
using ff14bot.Behavior;
using ff14bot.Managers;
using Kefka.Models;
using Kefka.Routine_Files.General;
using Kefka.Utilities;
using TreeSharp;
using static Kefka.Utilities.Constants;
using HotkeyManager = Kefka.Utilities.HotkeyManager;
using RoutineManager = Kefka.Utilities.RoutineManager;

#pragma warning disable 4014
#pragma warning disable CS1998

namespace Kefka
{
    public class Kefka
    {
        private DateTime pulseLimiter, saveFormTime;
        private bool _inInstance => DutyManager.InInstance;
        internal static bool windowInitialized = false, IsChineseVersion;
        private static readonly string VersionPath = Path.Combine(Environment.CurrentDirectory, @"Routines\Kefka\version.txt");

        public Kefka()
        {

        }

        public void OnInitialize(int version)
        {
            Logger.KefkaLog($"Initializing Version: {File.ReadAllText(VersionPath)}");

            TreeRoot.OnStart += OnBotStart;
            TreeRoot.OnStop += OnBotStop;

            if (version == 2)
            {
                IsChineseVersion = true;
            }

            HookBehaviors();

            var _class = RoutineManager.CurrentClass;
            InterruptManager.ResetInterrupts();
            IgnoreTargetManager.ResetIgnoreTargets();
            TankBusterManager.ResetTankBusters();
            OpenerManager.ResetOpeners();
            CombatHelper.ResetLastUsed();
        }

        public static void OnButtonPress()
        {
            if (!MainSettingsModel.Instance.ReadReportGuide)
            {
                TryAgain:
                var result = MessageBox.Show("If this is your first time opening Kefka, first of all, thank you very, very much! \n\nBefore reporting errors, please generate an Error Report via the Error Report button located in the upper right-hand corner next to the close button on the primary settings interface. Click 'No' to open the settings and dispose of this message for good! The Error Report button will generate a .zip containing all of Kefka's settings and any log(s) you choose to supply. Just follow the instructions and it will automatically upload your complete report to Trello. \n\nIf you cannot open the settings window to generate a report, please include the latest log.txt from /RB/Logs in a message on Discord. Thank you for reading!", "Kefka Run-Once... Hopefully!", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        MessageBox.Show("You didn't read the message...", "Sad Panda...");
                        goto TryAgain;

                    case MessageBoxResult.No:
                        MessageBox.Show("Thank you for reading the message... at least halfway through anyway! Remember, Error Report button and Trello!", "You da bes!");
                        MainSettingsModel.Instance.ReadReportGuide = true;
                        break;

                    case MessageBoxResult.Cancel:
                        MessageBox.Show("You didn't read the message...", "Sad Panda...");
                        goto TryAgain;
                }
            }

            FormManager.OpenForms();
        }

        public void ShutDown()
        {
            TreeRoot.OnStart -= OnBotStart;
            TreeRoot.OnStop -= OnBotStop;

            FormManager.SaveFormInstances();
            HotkeyManager.UnregisterAllHotkeys();
            FormManager.CloseOverlays();
            CombatHelper.ResetLastUsed();

            Logger.KefkaLog("Kefka Shutdown - Complete");
        }

        public void Pulse()
        {
            Monitor.OverlayUpdate();

            if (DateTime.Now < pulseLimiter) return;
            pulseLimiter = DateTime.Now.AddSeconds(1);

            var _class = RoutineManager.CurrentClass;
            if (DateTime.Now > saveFormTime)
            {
                FormManager.SaveFormInstances();

                if (Me.ClassLevel < 70)
                    Logger.KefkaLog("We are currently level synced to level {0}", Me.ClassLevel);

                if (_inInstance && Common_Utils.InActiveInstance())
                    Logger.DebugLog($"Instance Time Remaining: {Common_Utils.InstanceTimeRemaining}");

                saveFormTime = DateTime.Now.AddSeconds(60);
            }

            try
            {
                Group.UpdateAllies();
            }
            catch (Exception e)
            {
                Logger.KefkaLog(e.ToString());
            }
            Monitor.SpellLog();
            AutoDuty.AutoDutyRoot();
            FormManager.Window_Check();
            TargetSelectorManager.UpdatePartyMembers();
            CombatHelper.ResetLastUsed();
        }

        private async void OnBotStart(BotBase bot)
        {
            if (ff14bot.Managers.RoutineManager.Current.Name != "Kefka") return;

            var _class = RoutineManager.CurrentClass;
            OpenerManager.ResetOpeners();
            HotkeyManager.RegisterHotkeys();
            FormManager.ClassChange();
            DeepDive.DeepDiveInterruptOverride();
        }

        private void OnBotStop(BotBase bot)
        {
            if (ff14bot.Managers.RoutineManager.Current.Name != "Kefka") return;

            FormManager.SaveFormInstances();
            HotkeyManager.UnregisterAllHotkeys();
            FormManager.CloseOverlays();
            CombatHelper.ResetLastUsed();
        }

        #region Behavior Composites

        private void HookBehaviors()
        {
            Logger.KefkaLog("Hooking behaviors");
            TreeHooks.Instance.ReplaceHook("Rest", RestBehavior);
            TreeHooks.Instance.ReplaceHook("PreCombatBuff", PreCombatBuffBehavior);
            TreeHooks.Instance.ReplaceHook("Pull", PullBehavior);
            TreeHooks.Instance.ReplaceHook("Heal", HealBehavior);
            TreeHooks.Instance.ReplaceHook("CombatBuff", CombatBuffBehavior);
            TreeHooks.Instance.ReplaceHook("Combat", CombatBehavior);
        }

        public Composite RestBehavior
        {
            get
            {
                return new Decorator(new PrioritySelector(new Decorator(r => WorldManager.InPvP, new ActionRunCoroutine(ctx => RoutineManager.Rotation.PvP())),
                    new ActionRunCoroutine(ctx => RoutineManager.Rotation.Rest())));
            }
        }

        public Composite PreCombatBuffBehavior
        {
            get
            {
                return new Decorator(new PrioritySelector(new Decorator(r => WorldManager.InPvP, new ActionRunCoroutine(ctx => RoutineManager.Rotation.PvP())),
                    new ActionRunCoroutine(ctx => RoutineManager.Rotation.PreCombat())));
            }
        }

        public Composite PullBehavior
        {
            get
            {
                return new Decorator(new PrioritySelector(new Decorator(r => WorldManager.InPvP, new ActionRunCoroutine(ctx => RoutineManager.Rotation.PvP())),
                    new ActionRunCoroutine(ctx => RoutineManager.Rotation.Pull())));
            }
        }

        public Composite HealBehavior
        {
            get
            {
                return new Decorator(new PrioritySelector(new Decorator(r => WorldManager.InPvP, new ActionRunCoroutine(ctx => RoutineManager.Rotation.PvP())),
                    new ActionRunCoroutine(ctx => RoutineManager.Rotation.Heal())));
            }
        }

        public Composite CombatBuffBehavior
        {
            get
            {
                return new Decorator(new PrioritySelector(new Decorator(r => WorldManager.InPvP, new ActionRunCoroutine(ctx => RoutineManager.Rotation.PvP())),
                    new ActionRunCoroutine(ctx => RoutineManager.Rotation.CombatBuff())));
            }
        }

        public Composite CombatBehavior
        {
            get
            {
                return new Decorator(new PrioritySelector(new Decorator(r => WorldManager.InPvP, new ActionRunCoroutine(ctx => RoutineManager.Rotation.PvP())),
                        new ActionRunCoroutine(ctx => RoutineManager.Rotation.Combat())));
            }
        }

        #endregion Behavior Composites
    }
}