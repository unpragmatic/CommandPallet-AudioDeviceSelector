// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AudioDeviceSelectorExtension;

internal sealed partial class AudioDeviceSelectorExtensionPage : DynamicListPage
{
    private List<ListItem> allItems;

    public AudioDeviceSelectorExtensionPage()
    {
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Title = "Audio Device Select";
        Name = "Open";

        allItems = [.. Query("")];
    }

    public List<ListItem> Query(string query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var audioDevices = Helper.GetAudioDevices();


        return audioDevices
            .Where(audioDevice =>
                audioDevice.Name.Contains(query, StringComparison.InvariantCultureIgnoreCase) ||
                audioDevice.Description.Contains(query, StringComparison.InvariantCultureIgnoreCase)
            )
            .Select(audioDevice => new ListItem(new SetDefaultAudioDeviceCommand(audioDevice.Id))
            {
                Title = audioDevice.Name,
                Subtitle = audioDevice.Description,
                Icon = IconHelpers.FromRelativePath($"Assets\\ico{audioDevice.IconId}.ico")
            })
            .ToList();
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        allItems = [.. Query(newSearch)];
        RaiseItemsChanged(0);
    }

    public override IListItem[] GetItems() => [.. allItems];
}
