﻿
// sigScan C# Implementation - Written by atom0s [aka Wiccaan]
// Class Version: 2.0.0
//
// [ CHANGE LOG ] -------------------------------------------------------------------------
//
//      2.0.0
//          - Updated to no longer require unsafe or fixed code.
//          - Removed unneeded methods and code.
//
//      1.0.0
//          - First version written and release.
//
// [ CREDITS ] ----------------------------------------------------------------------------
//
// sigScan is based on the FindPattern code written by
// dom1n1k and Patrick at GameDeception.net (no longer exists
//
// Full credit to them for the purpose of this code. I, atom0s, simply
// take credit for converting it to C#.
//
// [ USAGE ] ------------------------------------------------------------------------------
//
// Examples:
//
//      SigScan _sigScan = new SigScan();
//      _sigScan.Process = someProc;
//      _sigScan.Address = new IntPtr(0x123456);
//      _sigScan.Size = 0x1000;
//      IntPtr pAddr = _sigScan.FindPattern(new byte[]{ 0xFF, 0xFF, 0xFF, 0xFF, 0x51, 0x55, 0xFC, 0x11 }, "xxxx?xx?", 12);
//
//      SigScan _sigScan = new SigScan(someProc, new IntPtr(0x123456), 0x1000);
//      IntPtr pAddr = _sigScan.FindPattern(new byte[]{ 0xFF, 0xFF, 0xFF, 0xFF, 0x51, 0x55, 0xFC, 0x11 }, "xxxx?xx?", 12);
//
// ----------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FunctionalVRC
{
    public class SigScan
    {
        /// <summary>
        /// ReadProcessMemory
        ///
        ///     API import definition for ReadProcessMemory.
        /// </summary>
        /// <param name="hProcess">Handle to the process we want to read from.</param>
        /// <param name="lpBaseAddress">The base address to start reading from.</param>
        /// <param name="lpBuffer">The return buffer to write the read data to.</param>
        /// <param name="dwSize">The size of data we wish to read.</param>
        /// <param name="lpNumberOfBytesRead">The number of bytes successfully read.</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out()] byte[] lpBuffer,
            int dwSize,
            out int lpNumberOfBytesRead
            );

        /// <summary>
        /// m_vDumpedRegion
        ///
        ///     The memory dumped from the external process.
        /// </summary>
        private byte[] m_vDumpedRegion;

        /// <summary>
        /// m_vProcess
        ///
        ///     The process we want to read the memory of.
        /// </summary>
        private Process m_vProcess;

        /// <summary>
        /// m_vAddress
        ///
        ///     The starting address we want to begin reading at.
        /// </summary>
        private IntPtr m_vAddress;

        /// <summary>
        /// m_vSize
        ///
        ///     The number of bytes we wish to read from the process.
        /// </summary>
        private int m_vSize;


        #region "sigScan Class Construction"
        /// <summary>
        /// SigScan
        ///
        ///     Main class constructor that uses no params.
        ///     Simply initializes the class properties and
        ///     expects the user to set them later.
        /// </summary>
        public SigScan()
        {
            m_vProcess = null;
            m_vAddress = IntPtr.Zero;
            m_vSize = 0;
            m_vDumpedRegion = null;
        }
        /// <summary>
        /// SigScan
        ///
        ///     Overloaded class constructor that sets the class
        ///     properties during construction.
        /// </summary>
        /// <param name="proc">The process to dump the memory from.</param>
        /// <param name="addr">The started address to begin the dump.</param>
        /// <param name="size">The size of the dump.</param>
        public SigScan(Process proc, IntPtr addr, int size)
        {
            m_vProcess = proc;
            m_vAddress = addr;
            m_vSize = size;
        }
        #endregion

        #region "sigScan Class Private Methods"
        /// <summary>
        /// DumpMemory
        ///
        ///     Internal memory dump function that uses the set class
        ///     properties to dump a memory region.
        /// </summary>
        /// <returns>Boolean based on RPM results and valid properties.</returns>
        private bool DumpMemory()
        {
            try
            {
                // Checks to ensure we have valid data.
                if (m_vProcess == null)
                    return false;
                if (m_vProcess.HasExited == true)
                    return false;
                if (m_vAddress == IntPtr.Zero)
                    return false;
                if (m_vSize == 0)
                    return false;

                // Create the region space to dump into.
                m_vDumpedRegion = new byte[m_vSize];

                bool bReturn = false;
                int nBytesRead = 0;

                // Dump the memory.
                bReturn = ReadProcessMemory(
                    m_vProcess.Handle, m_vAddress, m_vDumpedRegion, m_vSize, out nBytesRead
                    );

                // Validation checks.
                if (bReturn == false || nBytesRead != m_vSize)
                    return false;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// MaskCheck
        ///
        ///     Compares the current pattern byte to the current memory dump
        ///     byte to check for a match. Uses wildcards to skip bytes that
        ///     are deemed unneeded in the compares.
        /// </summary>
        /// <param name="nOffset">Offset in the dump to start at.</param>
        /// <param name="btPattern">Pattern to scan for.</param>
        /// <param name="strMask">Mask to compare against.</param>
        /// <returns>Boolean depending on if the pattern was found.</returns>
        private bool MaskCheck(int nOffset, byte[] btPattern, string strMask)
        {
            // Loop the pattern and compare to the mask and dump.
            for (int x = 0; x < btPattern.Length; x++)
            {
                // If the mask char is a wildcard, just continue.
                if (strMask[x] == '?')
                    continue;

                // If the mask char is not a wildcard, ensure a match is made in the pattern.
                if ((strMask[x] == 'x') && (btPattern[x] != m_vDumpedRegion[nOffset + x]))
                    return false;
            }

            // The loop was successful so we found the pattern.
            return true;
        }
        #endregion

        #region "sigScan Class Public Methods"
        /// <summary>
        /// FindPattern
        ///
        ///     Attempts to locate the given pattern inside the dumped memory region
        ///     compared against the given mask. If the pattern is found, the offset
        ///     is added to the located address and returned to the user.
        /// </summary>
        /// <param name="btPattern">Byte pattern to look for in the dumped region.</param>
        /// <param name="strMask">The mask string to compare against.</param>
        /// <param name="nOffset">The offset added to the result address.</param>
        /// <returns>IntPtr - zero if not found, address if found.</returns>
        public IntPtr FindPattern(byte[] btPattern, string strMask, int nOffset)
        {
            try
            {
                // Dump the memory region if we have not dumped it yet.
                if (m_vDumpedRegion == null || m_vDumpedRegion.Length == 0)
                {
                    if (!DumpMemory())
                        return IntPtr.Zero;
                }

                // Ensure the mask and pattern lengths match.
                if (strMask.Length != btPattern.Length)
                    return IntPtr.Zero;

                // Loop the region and look for the pattern.
                for (int x = 0; x < m_vDumpedRegion.Length; x++)
                {
                    if (MaskCheck(x, btPattern, strMask))
                    {
                        // The pattern was found, return it.
                        return new IntPtr((int)m_vAddress + (x + nOffset));
                    }
                }

                // Pattern was not found.
                return IntPtr.Zero;
            }
            catch
            {
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// ResetRegion
        ///
        ///     Resets the memory dump array to nothing to allow
        ///     the class to redump the memory.
        /// </summary>
        public void ResetRegion()
        {
            this.m_vDumpedRegion = null;
        }
        #endregion

        #region "sigScan Class Properties"
        public Process Process
        {
            get { return m_vProcess; }
            set { m_vProcess = value; }
        }
        public IntPtr Address
        {
            get { return m_vAddress; }
            set { m_vAddress = value; }
        }
        public Int32 Size
        {
            get { return m_vSize; }
            set { m_vSize = value; }
        }
        #endregion

    }
}