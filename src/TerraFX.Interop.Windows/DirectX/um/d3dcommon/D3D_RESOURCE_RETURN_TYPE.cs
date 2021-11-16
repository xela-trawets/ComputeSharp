// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from um/d3dcommon.h in the Windows SDK for Windows 10.0.20348.0
// Original source is Copyright © Microsoft. All rights reserved.

namespace TerraFX.Interop.DirectX
{
    public enum D3D_RESOURCE_RETURN_TYPE
    {
        D3D_RETURN_TYPE_UNORM = 1,
        D3D_RETURN_TYPE_SNORM = 2,
        D3D_RETURN_TYPE_SINT = 3,
        D3D_RETURN_TYPE_UINT = 4,
        D3D_RETURN_TYPE_FLOAT = 5,
        D3D_RETURN_TYPE_MIXED = 6,
        D3D_RETURN_TYPE_DOUBLE = 7,
        D3D_RETURN_TYPE_CONTINUED = 8,
        D3D10_RETURN_TYPE_UNORM = D3D_RETURN_TYPE_UNORM,
        D3D10_RETURN_TYPE_SNORM = D3D_RETURN_TYPE_SNORM,
        D3D10_RETURN_TYPE_SINT = D3D_RETURN_TYPE_SINT,
        D3D10_RETURN_TYPE_UINT = D3D_RETURN_TYPE_UINT,
        D3D10_RETURN_TYPE_FLOAT = D3D_RETURN_TYPE_FLOAT,
        D3D10_RETURN_TYPE_MIXED = D3D_RETURN_TYPE_MIXED,
        D3D11_RETURN_TYPE_UNORM = D3D_RETURN_TYPE_UNORM,
        D3D11_RETURN_TYPE_SNORM = D3D_RETURN_TYPE_SNORM,
        D3D11_RETURN_TYPE_SINT = D3D_RETURN_TYPE_SINT,
        D3D11_RETURN_TYPE_UINT = D3D_RETURN_TYPE_UINT,
        D3D11_RETURN_TYPE_FLOAT = D3D_RETURN_TYPE_FLOAT,
        D3D11_RETURN_TYPE_MIXED = D3D_RETURN_TYPE_MIXED,
        D3D11_RETURN_TYPE_DOUBLE = D3D_RETURN_TYPE_DOUBLE,
        D3D11_RETURN_TYPE_CONTINUED = D3D_RETURN_TYPE_CONTINUED,
    }
}
