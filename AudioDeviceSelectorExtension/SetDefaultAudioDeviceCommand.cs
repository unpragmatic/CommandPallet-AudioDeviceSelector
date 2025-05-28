using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.Foundation;

namespace AudioDeviceSelectorExtension
{
    internal sealed partial class SetDefaultAudioDeviceCommand : InvokableCommand
    {
        private readonly string _deviceId;

        public override string Name => "Set Default Audio Device";
        public override IconInfo Icon => new("\uE8A7");

        public SetDefaultAudioDeviceCommand(string deviceId)
        {
            this._deviceId = deviceId;
        }

        public override ICommandResult Invoke(object sender)
        {
            Helper.SetDefaultAudioDevice(_deviceId);

            return CommandResult.GoHome();
        }
    }
}
