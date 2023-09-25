using System;
using System.Collections.Generic;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using RotationMaster.Utils;
using RotationMaster.Windows;
using RotationMaster.Windows.Viewer;

namespace RotationMaster
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Rotation Master";

        public readonly WindowSystem WindowSystem = new("RotationMaster");

        public static IDataManager DataManager { get; private set; }
        public static UiBuilder UiBuilder { get; private set; }
        public static ITextureProvider TextureProvider { get; private set; }
        public static DalamudPluginInterface PluginInterface { get; private set; }
        public static ChatGui ChatGui { get; private set; }
        public static RotationViewerWindow RotationViewerWindow { get; private set; }
        private ICommandManager CommandManager { get; init; }
        public Configuration Configuration { get; init; }
        private Dictionary<string, (Window window, string helpMessage)> Commands { get; init; }

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] ICommandManager commandManager,
            [RequiredVersion("1.0")] ChatGui chatGui,
            [RequiredVersion("1.0")] ITextureProvider textureProvider,
            [RequiredVersion("1.0")] IDataManager dataManager)
        {
            this.CommandManager = commandManager;
            PluginInterface = pluginInterface;
            DataManager = dataManager;
            ChatGui = chatGui;
            UiBuilder = pluginInterface.UiBuilder;
            TextureProvider = textureProvider;

            this.Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(PluginInterface);

            Fonts.Init(UiBuilder);
            Images.Init(pluginInterface);

            RotationViewerWindow = new RotationViewerWindow();

            WindowSystem.AddWindow(RotationViewerWindow);

            Commands = new Dictionary<string, (Window window, string helpMessage)>()
            {
                { "/rtm", (new ConfigWindow(this), "Configuration") },
                { "/rtb", (new RotationBuilderWindow(this), "Open Rotation Master") },
            };

            foreach (var (command, (window, helpMessage)) in Commands)
            {
                WindowSystem.AddWindow(window);
                this.CommandManager.AddHandler(command, new CommandInfo(OnCommand) { HelpMessage = helpMessage });
            }

            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        public void Dispose()
        {
            this.WindowSystem.RemoveAllWindows();

            foreach (var (command, (window, _)) in Commands)
            {
                if (window is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                this.CommandManager.RemoveHandler(command);
            }
        }

        private void OnCommand(string command, string args)
        {
            if (Commands.TryGetValue(command, out var commandData))
            {
                commandData.window.IsOpen = true;
            }
        }

        private void DrawUI()
        {
            this.WindowSystem.Draw();
        }

        public void DrawConfigUI()
        {
            Commands["/cfg"].window.IsOpen = true;
        }
    }
}
