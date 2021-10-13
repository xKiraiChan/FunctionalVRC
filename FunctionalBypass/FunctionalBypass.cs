using MelonLoader;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: MelonInfo(typeof(FunctionalVRC.FunctionalBypass), "FunctionalBypass", "0.2.0", "Kirai Chan", "github.com/xKiraiChan/FunctionalVRC")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace FunctionalVRC
{
    public class FunctionalBypass : MelonPlugin
    {
        const uint PAGE_EXECUTE_READWRITE = 0x40;

        static readonly IntPtr pBytes;
        static readonly IntPtr pID;

        static FunctionalBypass()
        {
            MelonLogger.Msg("------------------------------");
            MelonLogger.Msg("Integrity Check Information:");

            pID = GetCurrentProcess();
            IntPtr module = GetModuleHandle("bootstrap.dll");

            int size;
            unsafe { size = sizeof(MODULEINFO); }
            MelonLogger.Msg("  Struct Size: " + size);

            GetModuleInformation(pID, module, out MODULEINFO info, (uint)size);

            MelonLogger.Msg("  Module Base: " + module.ToString("X"));
            MelonLogger.Msg("  Module Size: " + info.SizeOfImage);

            short offset = (short)new SigScan(Process.GetCurrentProcess(), module, (int)info.SizeOfImage)
                .FindPattern(new byte[] { // 82 bytes
                    0x66, 0x0F, 0x2F, 0xC7, //                         comisd xmm0, xmm7
                    0x77, 0x1E, //                                     ja bootstrap.7FFF9D5688D6
                    0x66, 0x0F, 0x2F, 0x00, 0x00, 0x00, 0x00, 0x00, // comisd xmm7,qword ptr ds:[7FFF9D583EC8]
                    0x77, 0x14, //                                     ja bootstrap.7FFF9D5688D6
                    0x48, 0x8D, 0x4D, 0xB0, //                         lea rcx,qword ptr ss:[rbp-50]
                    0xE8, 0x00, 0x00, 0x00, 0x00, //                   call bootstrap.7FFF9D567AF0
                    0x49, 0x8B, 0xCF, //                               mov rcx,r15
                    0xFF, 0x15, 0x00, 0x00, 0x00, 0x00, //             call qword ptr ds:[< &mono_image_close >]
                    0xEB, 0x2E, //                                     jmp bootstrap.7FFF9D568904
                    0x48, 0x8D, 0x4D, 0xB0, //                         lea rcx,qword ptr ss:[rbp-50]
                    0xE8, 0x00, 0x00, 0x00, 0x00, //                   call bootstrap.7FFF9D567AF0
                    0x49, 0x8B, 0xCF, //                               mov rcx,r15
                    0xFF, 0x15, 0x00, 0x00, 0x00, 0x00, //             call qword ptr ds:[< &mono_image_close >]
                    0x48, 0x8B, 0x00, 0x00, 0x00, 0x00, 0x00, //       mov rbx, qword ptr ds:[< &mono_raise_exception >]
                    0x48, 0x8B, 0x00, 0x00, 0x00, 0x00, 0x00, //       mov rax, qword ptr ds:[< &mono_get_exception_bad_image_format >]
                    0x48, 0x8D, 0x00, 0x00, 0x00, 0x00, 0x00, //       lea rcx, qword ptr ds:[7FFF9D5820B8]
                    0xFF, 0xD0, //                                     call rax
                    0x48, 0x8B, 0xC8, //                               mov rcx, rax
                    0xFF, 0xD3 //                                      call rbx ; <== PATCHING THIS
                }, "xxxxxxxxx?????xxxxxxx????xxxxx????xxxxxxx????xxxxx????xx?????xx?????xx?????xxxxxxx", 80).ToInt32();

            if (offset == 0)
                MelonLogger.Warning("Failed to find the integrity check");
            else
            {
                MelonLogger.Msg("  Addr Offset: 0x" + offset.ToString("X"));

                pBytes = module + offset + 0x10000;
                MelonLogger.Msg("  Addr pBytes: 0x" + pBytes.ToString("X"));

                BypassIC();
            }
        }

        public override void OnApplicationLateStart()
        {
            if (Directory.Exists("SafeMods"))
            {
                RestoreIC();

                MelonBase prev = null;
                if (MelonHandler.Mods.Count > 0)
                    prev = MelonHandler.Mods[MelonHandler.Mods.Count - 1];

                int loaded = 0;
                foreach (var file in Directory.EnumerateFiles("SafeMods"))
                {
                    try
                    {
                        Assembly asm = Assembly.Load(File.ReadAllBytes(file));
                        MelonInfoAttribute info = asm.GetCustomAttribute<MelonInfoAttribute>();
                        MelonHandler.LoadFromAssembly(asm, file);

                        MelonBase curr = MelonHandler.Mods[MelonHandler.Mods.Count - 1];

                        if (prev != curr) OnApplicationStart();

                    } 
                    catch (Exception ex)
                    {
                        MelonLogger.Error($"Failed to load {file}: {ex}");
                        continue;
                    }

                    loaded++;
                    MelonLogger.Msg("------------------------------");
                    MelonLogger.Msg($"Loaded {Info.Name}");
                    if (!string.IsNullOrEmpty(Info.Author))
                        MelonLogger.Msg($"  by: {Info.Author}");
                    MelonLogger.Msg($"  version: {Info.Version}");
                    if (!string.IsNullOrEmpty(Info.DownloadLink))
                        MelonLogger.Msg($"  from: {Info.DownloadLink}");
                }

                if (loaded > 0)
                    MelonLogger.Msg("------------------------------");
            } 
            else
            {
                MelonLogger.Msg("Functional Loader supports loading safe mods");
                MelonLogger.Msg("Place any mods that require integrity checks");
                MelonLogger.Msg("  into `VRChat/SafeMods`");
            }
        }

        public static void BypassIC()
        {
            VirtualProtectEx(pID, pBytes, (UIntPtr)2, PAGE_EXECUTE_READWRITE, out uint oldProtect);
            MelonLogger.Msg("Removed page protections");

            Marshal.Copy(new byte[2] { 0x90, 0x90 }, 0, pBytes, 2);
            MelonLogger.Msg("Successfully unpatched the integrity check");

            VirtualProtectEx(pID, pBytes, (UIntPtr)2, oldProtect, out _);
            MelonLogger.Msg("Readded page protections");
        }

        public static void RestoreIC()
        {
            VirtualProtectEx(pID, pBytes, (UIntPtr)2, PAGE_EXECUTE_READWRITE, out uint oldProtect);
            MelonLogger.Msg("Removed page protections");

            Marshal.Copy(new byte[2] { 0xFF, 0xD3 }, 0, pBytes, 2);
            MelonLogger.Msg("Successfully repatched the integrity check");

            VirtualProtectEx(pID, pBytes, (UIntPtr)2, oldProtect, out _);
            MelonLogger.Msg("Readded page protections");
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetCurrentProcess();

        [DllImport("psapi.dll", SetLastError = true)]
        static extern bool GetModuleInformation(IntPtr hProcess, IntPtr hModule, out MODULEINFO lpmodinfo, uint cb);

        [StructLayout(LayoutKind.Sequential)]
        public struct MODULEINFO
        {
            public IntPtr lpBaseOfDll;
            public uint SizeOfImage;
            public IntPtr EntryPoint;
        }

        [DllImport("kernel32.dll")]
        static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);
    }
}
