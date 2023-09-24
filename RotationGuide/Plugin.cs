using System;
using System.Collections.Generic;
using System.IO;
using Dalamud.Data;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Interface;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ImGuiNET;
using ImGuiScene;
using RotationGuide.Utils;
using RotationGuide.Windows;

namespace RotationGuide
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Rotation Guide";

        public readonly WindowSystem WindowSystem = new("RotationGuide");

        public static IDataManager DataManager { get; private set; }
        public static UiBuilder UiBuilder { get; private set; }
        public static ITextureProvider TextureProvider { get; private set; }
        public static DalamudPluginInterface PluginInterface { get; private set; }
        public static ChatGui ChatGui { get; private set; }

        public static TextureWrap PullBarImage;
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

            Commands = new Dictionary<string, (Window window, string helpMessage)>()
            {
                { "/rtm", (new ConfigWindow(this), "Configuration") },
                { "/rtg", (new RotationWindow(this), "The rotation runner") },
                { "/rtb", (new RotationBuilderWindow(this), "Open rotation builder") },
            };

            foreach (var (command, (window, helpMessage)) in Commands)
            {
                WindowSystem.AddWindow(window);
                this.CommandManager.AddHandler(command, new CommandInfo(OnCommand) { HelpMessage = helpMessage });
            }

            var imagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "pullBar.png");
            PullBarImage = PluginInterface.UiBuilder.LoadImage(imagePath);

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
