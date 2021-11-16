// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from um/wincodec.h in the Windows SDK for Windows 10.0.20348.0
// Original source is Copyright © Microsoft. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TerraFX.Interop.Windows
{
    [Guid("30989668-E1C9-4597-B395-458EEDB808DF")]
    [NativeTypeName("struct IWICMetadataQueryReader : IUnknown")]
    [NativeInheritance("IUnknown")]
    public unsafe partial struct IWICMetadataQueryReader : IWICMetadataQueryReader.Interface
    {
        public void** lpVtbl;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [VtblIndex(0)]
        public HRESULT QueryInterface([NativeTypeName("const IID &")] Guid* riid, void** ppvObject)
        {
            return ((delegate* unmanaged<IWICMetadataQueryReader*, Guid*, void**, int>)(lpVtbl[0]))((IWICMetadataQueryReader*)Unsafe.AsPointer(ref this), riid, ppvObject);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [VtblIndex(1)]
        [return: NativeTypeName("ULONG")]
        public uint AddRef()
        {
            return ((delegate* unmanaged<IWICMetadataQueryReader*, uint>)(lpVtbl[1]))((IWICMetadataQueryReader*)Unsafe.AsPointer(ref this));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [VtblIndex(2)]
        [return: NativeTypeName("ULONG")]
        public uint Release()
        {
            return ((delegate* unmanaged<IWICMetadataQueryReader*, uint>)(lpVtbl[2]))((IWICMetadataQueryReader*)Unsafe.AsPointer(ref this));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [VtblIndex(3)]
        public HRESULT GetContainerFormat(Guid* pguidContainerFormat)
        {
            return ((delegate* unmanaged<IWICMetadataQueryReader*, Guid*, int>)(lpVtbl[3]))((IWICMetadataQueryReader*)Unsafe.AsPointer(ref this), pguidContainerFormat);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [VtblIndex(4)]
        public HRESULT GetLocation(uint cchMaxLength, [NativeTypeName("WCHAR *")] ushort* wzNamespace, uint* pcchActualLength)
        {
            return ((delegate* unmanaged<IWICMetadataQueryReader*, uint, ushort*, uint*, int>)(lpVtbl[4]))((IWICMetadataQueryReader*)Unsafe.AsPointer(ref this), cchMaxLength, wzNamespace, pcchActualLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [VtblIndex(5)]
        public HRESULT GetMetadataByName([NativeTypeName("LPCWSTR")] ushort* wzName, PROPVARIANT* pvarValue)
        {
            return ((delegate* unmanaged<IWICMetadataQueryReader*, ushort*, PROPVARIANT*, int>)(lpVtbl[5]))((IWICMetadataQueryReader*)Unsafe.AsPointer(ref this), wzName, pvarValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [VtblIndex(6)]
        public HRESULT GetEnumerator(IEnumString** ppIEnumString)
        {
            return ((delegate* unmanaged<IWICMetadataQueryReader*, IEnumString**, int>)(lpVtbl[6]))((IWICMetadataQueryReader*)Unsafe.AsPointer(ref this), ppIEnumString);
        }

        public interface Interface : IUnknown.Interface
        {
            [VtblIndex(3)]
            HRESULT GetContainerFormat(Guid* pguidContainerFormat);

            [VtblIndex(4)]
            HRESULT GetLocation(uint cchMaxLength, [NativeTypeName("WCHAR *")] ushort* wzNamespace, uint* pcchActualLength);

            [VtblIndex(5)]
            HRESULT GetMetadataByName([NativeTypeName("LPCWSTR")] ushort* wzName, PROPVARIANT* pvarValue);

            [VtblIndex(6)]
            HRESULT GetEnumerator(IEnumString** ppIEnumString);
        }

        public partial struct Vtbl
        {
            [NativeTypeName("HRESULT (const IID &, void **) __attribute__((stdcall))")]
            public delegate* unmanaged<IWICMetadataQueryReader*, Guid*, void**, int> QueryInterface;

            [NativeTypeName("ULONG () __attribute__((stdcall))")]
            public delegate* unmanaged<IWICMetadataQueryReader*, uint> AddRef;

            [NativeTypeName("ULONG () __attribute__((stdcall))")]
            public delegate* unmanaged<IWICMetadataQueryReader*, uint> Release;

            [NativeTypeName("HRESULT (GUID *) __attribute__((stdcall))")]
            public delegate* unmanaged<IWICMetadataQueryReader*, Guid*, int> GetContainerFormat;

            [NativeTypeName("HRESULT (UINT, WCHAR *, UINT *) __attribute__((stdcall))")]
            public delegate* unmanaged<IWICMetadataQueryReader*, uint, ushort*, uint*, int> GetLocation;

            [NativeTypeName("HRESULT (LPCWSTR, PROPVARIANT *) __attribute__((stdcall))")]
            public delegate* unmanaged<IWICMetadataQueryReader*, ushort*, PROPVARIANT*, int> GetMetadataByName;

            [NativeTypeName("HRESULT (IEnumString **) __attribute__((stdcall))")]
            public delegate* unmanaged<IWICMetadataQueryReader*, IEnumString**, int> GetEnumerator;
        }
    }
}
