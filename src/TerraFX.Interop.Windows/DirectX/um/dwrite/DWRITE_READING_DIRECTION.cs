// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from um/dwrite.h in the Windows SDK for Windows 10.0.20348.0
// Original source is Copyright © Microsoft. All rights reserved.

namespace TerraFX.Interop.DirectX
{
    public enum DWRITE_READING_DIRECTION
    {
        DWRITE_READING_DIRECTION_LEFT_TO_RIGHT = 0,
        DWRITE_READING_DIRECTION_RIGHT_TO_LEFT = 1,
        DWRITE_READING_DIRECTION_TOP_TO_BOTTOM = 2,
        DWRITE_READING_DIRECTION_BOTTOM_TO_TOP = 3,
    }
}
