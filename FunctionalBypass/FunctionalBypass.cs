using MelonLoader;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

[assembly: MelonInfo(typeof(FunctionalVRC.FunctionalBypass), "FunctionalBypass", "0.1.0", "Kirai Chan", "github.com/xKiraiChan/FunctionalVRC")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace FunctionalVRC
{
    public class FunctionalBypass : MelonPlugin
    {
        const uint PAGE_EXECUTE_READWRITE = 0x40;

        static FunctionalBypass()
        {
            MelonLogger.Msg("------------------------------");

            IntPtr process = GetCurrentProcess();
            IntPtr module = GetModuleHandle("bootstrap.dll");

            int size;
            unsafe { size = sizeof(MODULEINFO); }
            MelonLogger.Msg("Struct Size: " + size);

            GetModuleInformation(process, module, out MODULEINFO info, (uint)size);

            MelonLogger.Msg("Module Base: " + module.ToString("X"));
            MelonLogger.Msg("Module Size: " + info.SizeOfImage);

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
                MelonLogger.Msg("Addr Offset: 0x" + offset.ToString("X"));

                var real = module + offset + 0x10000;
                MelonLogger.Msg("IntChk Addr: 0x" + real.ToString("X"));

                VirtualProtectEx(process, real, (UIntPtr)2, PAGE_EXECUTE_READWRITE, out uint oldProtect);
                MelonLogger.Msg("Removed page protections");

                Marshal.Copy(new byte[2] { 0x90, 0x90 }, 0, real, 2);
                MelonLogger.Msg("Successfully unpatched the integrity check");

                VirtualProtectEx(process, real, (UIntPtr)2, oldProtect, out _);
                MelonLogger.Msg("Readded page protections");
            }

            MelonLogger.Msg("------------------------------");
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
