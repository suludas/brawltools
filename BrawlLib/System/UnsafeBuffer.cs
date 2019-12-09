﻿using System.Runtime.InteropServices;

namespace System
{
    public class UnsafeBuffer : IDisposable
    {
        private VoidPtr _data;
        public VoidPtr Address { get { return _data; } }

        private int _length;
        public int Length { get { return _length; } set { _length = value; } }

        public unsafe UnsafeBuffer(int size) {
            _data = Marshal.AllocHGlobal(size); 
            _length = size; 
            //Diagnostics.Debug.WriteLine($"allocated to {(ulong)_data.address:X} {size:X} bytes"); 
        }
        ~UnsafeBuffer() { Dispose(); }

        public VoidPtr this[int count, int stride] { get { return _data[count, stride]; } }

        public unsafe void Dispose()
        {
            //Diagnostics.Debug.WriteLine($"DEallocated to {(ulong)_data.address:X} {_length:X} bytes");
            try
            {
                if (_data)
                {
                    Marshal.FreeHGlobal(_data);
                    _data = null;
                    GC.SuppressFinalize(this);
                }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
    }
}
