using System.Collections.Generic;
using System.Threading.Tasks;
using Sidekick.Apis.Poe;
using Sidekick.Common;
using Sidekick.Common.Blazor.Views;
using Sidekick.Common.Extensions;
using Sidekick.Common.Platform;
using Sidekick.Common.Settings;

namespace Sidekick.Modules.Trade.Keybinds
{
    public class PriceCheckItemKeybindHandler : IKeybindHandler
    {
        private readonly IViewLocator viewLocator;
        private readonly IClipboardProvider clipboardProvider;
        private readonly IProcessProvider processProvider;
        private readonly ISettings settings;
        private readonly IItemParser itemParser;
        private readonly IKeyboardProvider keyboard;

        public PriceCheckItemKeybindHandler(
            IViewLocator viewLocator,
            IClipboardProvider clipboardProvider,
            IProcessProvider processProvider,
            ISettings settings,
            IItemParser itemParser,
            IKeyboardProvider keyboard)
        {
            this.viewLocator = viewLocator;
            this.clipboardProvider = clipboardProvider;
            this.processProvider = processProvider;
            this.settings = settings;
            this.itemParser = itemParser;
            this.keyboard = keyboard;
        }

        public List<string> GetKeybinds() => new() { settings.Trade_Key_Check };

        public bool IsValid(string _) => processProvider.IsPathOfExileInFocus;

        public async Task Execute(string keybind)
        {
            var text = await clipboardProvider.Copy();
            if (text == null)
            {
                await keyboard.PressKey(keybind);
                return;
            }

            var advancedItemText = await clipboardProvider.CopyAdvanced();
            var originalItem = itemParser.ParseOriginalItem(advancedItemText);

            await viewLocator.CloseAllOverlays();
            await viewLocator.Open($"/trade/{originalItem?.ToString().EncodeBase64Url()}/{text.EncodeBase64Url()}");
        }
    }
}
