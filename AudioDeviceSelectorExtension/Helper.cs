using System;
using System.Collections.Generic;
using Windows.Win32.Media.Audio;
using Windows.Win32;
using Windows.Win32.System.Com;
using Windows.Win32.UI.Shell.PropertiesSystem;
using Windows.Win32.System.Com.StructuredStorage;
using Windows.Win32.Foundation;
using System.Runtime.InteropServices;
using System.Linq;


namespace AudioDeviceSelectorExtension
{
    internal static class Helper
    {
        internal static string PropVariantToString(in PROPVARIANT prop)
        {
            if ((VarEnum)prop.Anonymous.Anonymous.vt == VarEnum.VT_LPWSTR)
            {
                return prop.Anonymous.Anonymous.Anonymous.pwszVal.ToString();
            }
            return null;
        }

        internal static IMMDeviceEnumerator CreateMMDeviceEnumerator()
        {
            PInvoke.CoCreateInstance(
                new Guid("BCDE0395-E52F-467C-8E3D-C4579291692E"),
                null,
                CLSCTX.CLSCTX_INPROC_SERVER,
                typeof(IMMDeviceEnumerator).GUID,
                out var dataSourceObj
            ).ThrowOnFailure();
            return (IMMDeviceEnumerator)dataSourceObj;
        }

        internal static IPolicyConfigVista CreatePolicyConfigVista()
        {
            PInvoke.CoCreateInstance(
                new Guid("294935CE-F637-4E7C-A41B-AB255460B862"),
                null,
                CLSCTX.CLSCTX_INPROC_SERVER,
                new Guid("568b9108-44bf-40b4-9006-86afe5b5a620"),
                out var dataSourceObj
            ).ThrowOnFailure();
            return (IPolicyConfigVista)dataSourceObj;
        }

        internal unsafe static List<AudioDevice> GetAudioDevices()
        {
            PROPERTYKEY PKEY_DeviceClassIconPath = new()
            {
                fmtid = new Guid("259abffc-50a7-47ce-af08-68c9a7d73366"), // DEVPKEY_DeviceClass_Icon
                pid = 12
            };
            PROPERTYKEY PKEY_Device_FriendlyName = new()
            {
                fmtid = new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"),
                pid = 14
            };

            PROPERTYKEY PKEY_DeviceInterface_FriendlyName = new()
            {
                fmtid = new Guid("026e516e-b814-414b-83cd-856d6fef4822"),
                pid = 2
            };

            IMMDeviceEnumerator enumerator = CreateMMDeviceEnumerator();
            enumerator.EnumAudioEndpoints(EDataFlow.eRender, DEVICE_STATE.DEVICE_STATE_ACTIVE, out IMMDeviceCollection devices);

            devices.GetCount(out uint count);
            Console.WriteLine(count);

            List<AudioDevice> result = new List<AudioDevice>();
            for (uint i = 0; i < count; i++)
            {
                devices.Item(i, out IMMDevice device);
                device.OpenPropertyStore(STGM.STGM_READ, out IPropertyStore store);
                store.GetValue(&PKEY_DeviceClassIconPath, out PROPVARIANT iconPathVar);
                store.GetValue(&PKEY_Device_FriendlyName, out PROPVARIANT nameVar);
                store.GetValue(&PKEY_DeviceInterface_FriendlyName, out PROPVARIANT descVar);

                PWSTR id = new PWSTR();
                device.GetId(&id);
                string iconPath = PropVariantToString(iconPathVar);
                string name = PropVariantToString(nameVar);
                string desc = PropVariantToString(descVar);

                result.Add(new AudioDevice
                {
                    Id = id.ToString(),
                    Name = name,
                    Description = desc,
                    IconId = iconPath.Split("-").Last()
                });
            }

            return result;
        }

        internal unsafe static void SetDefaultAudioDevice(string deviceId)
        {
            var policyConfig = CreatePolicyConfigVista();

            // Set for all roles
            policyConfig.SetDefaultEndpoint(deviceId, ERole.eConsole);
            policyConfig.SetDefaultEndpoint(deviceId, ERole.eMultimedia);
            policyConfig.SetDefaultEndpoint(deviceId, ERole.eCommunications);
        }

        internal class AudioDevice
        {
            public required string Id { get; set; }
            public required string Name { get; set; }
            public required string Description { get; set; }

            public required string IconId { get; set; }
        }
    }

    [Guid("568b9108-44bf-40b4-9006-86afe5b5a620"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IPolicyConfigVista
    {
        int Unused1(); // 0
        int Unused2(); // 1
        int Unused3(); // 2
        int Unused4(); // 3
        int Unused5(); // 4
        int Unused6(); // 5
        int Unused7(); // 6
        int Unused8(); // 7
        int Unused9(); // 8
        int SetDefaultEndpoint([MarshalAs(UnmanagedType.LPWStr)] string wszDeviceId, ERole role); // 9
    }
}
