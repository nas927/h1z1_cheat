using System.Runtime.InteropServices;
using System.Numerics;
using System.CodeDom;

namespace MemoryLib;
public class MemoryRead
{
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);
    [DllImport("kernel32.dll")]
    public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, short dwSize, out IntPtr lpNumberOfBytesRead);
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesWritten);

    public const uint PROCESS_VM_READ = 0x0010;
    public const uint PROCESS_VM_WRITE = 0x0020;
    public const uint PROCESS_VM_OPERATION = 0x0008;

    public static string ReadName(IntPtr process, ulong address)
    {
        byte[] buffer = new byte[32];
        IntPtr read;

        ReadProcessMemory(
            process,
            (IntPtr)address,
            buffer,
            32,
            out read
        );

        int length = Array.IndexOf(buffer, (byte)0);
        if (length == -1)
            length = buffer.Length;

        return System.Text.Encoding.UTF8.GetString(buffer, 0, length);
    }

    static Matrix4x4 byteToMatrix(byte[] buffer)
    {
        return new Matrix4x4(
            BitConverter.ToSingle(buffer, 0),
            BitConverter.ToSingle(buffer, 4),
            BitConverter.ToSingle(buffer, 8),
            BitConverter.ToSingle(buffer, 12),

            BitConverter.ToSingle(buffer, 16),
            BitConverter.ToSingle(buffer, 20),
            BitConverter.ToSingle(buffer, 24),
            BitConverter.ToSingle(buffer, 28),

            BitConverter.ToSingle(buffer, 32),
            BitConverter.ToSingle(buffer, 36),
            BitConverter.ToSingle(buffer, 40),
            BitConverter.ToSingle(buffer, 44),

            BitConverter.ToSingle(buffer, 48),
            BitConverter.ToSingle(buffer, 52),
            BitConverter.ToSingle(buffer, 56),
            BitConverter.ToSingle(buffer, 60)
        );
    }
    public static T ReadInt<T>(IntPtr process, ulong address, short size)
    {
        byte[] buffer = new byte[size];
        IntPtr read;

        ReadProcessMemory(
            process,
            (IntPtr)address,
            buffer,
            size,
            out read
        );

        if (size == 64)
        {
            Matrix4x4 matrix = byteToMatrix(buffer);
            return (T)(object)matrix;
        }
        if (size == 12)
        {
            Vector3 vec = new Vector3(
                BitConverter.ToSingle(buffer, 0),
                BitConverter.ToSingle(buffer, 4),
                BitConverter.ToSingle(buffer, 8)
            );

            return (T)(object)vec;
        }
        if (size == 8)
            return (T)(object)BitConverter.ToUInt64(buffer, 0);
        if (size == 4)
            return (T)(object)BitConverter.ToInt32(buffer, 0);
        if (size == 2)
            return (T)(object)BitConverter.ToInt16(buffer, 0);
        return (T)(object)BitConverter.ToUInt64(buffer, 0);
    }

    public static bool WriteMem(IntPtr process, ulong address, byte[] buf, int size)
    {
        IntPtr write;

        bool written = WriteProcessMemory(
            process,
            (IntPtr)address,
            buf,
            size,
            out write
        );

        return written;
    }
}