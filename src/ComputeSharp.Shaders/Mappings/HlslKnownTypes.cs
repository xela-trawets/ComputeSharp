﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Numerics;

namespace ComputeSharp.Shaders.Mappings
{
    /// <summary>
    /// A <see langword="class"/> that contains and maps known HLSL type names to common .NET types
    /// </summary>
    internal static class HlslKnownTypes
    {
        private static HashSet<Type> _HlslMappedVectorTypes { get; } = new HashSet<Type>(new[]
        {
            typeof(Bool2), typeof(Bool3), typeof(Bool4),
            typeof(Int2), typeof(Int3), typeof(Int4),
            typeof(UInt2), typeof(UInt3), typeof(UInt4),
            typeof(Float2), typeof(Float3), typeof(Float4),
            typeof(Double2), typeof(Double3), typeof(Double4)
        });

        /// <summary>
        /// Checks whether or not the input type is a known scalar type
        /// </summary>
        /// <param name="type">The input <see cref="Type"/> instance to check</param>
        /// <returns>A <see langword="bool"/> indicating whether the input <see cref="Type"/> is in fact a known HLSL scalar type</returns>
        [Pure]
        public static bool IsKnownScalarType(Type type) => type == typeof(bool) ||
                                                           type == typeof(Bool) ||
                                                           type == typeof(int) ||
                                                           type == typeof(uint) ||
                                                           type == typeof(float) ||
                                                           type == typeof(double);

        /// <summary>
        /// Checks whether or not the input type is a known vector type
        /// </summary>
        /// <param name="type">The input <see cref="Type"/> instance to check</param>
        /// <returns>A <see langword="bool"/> indicating whether the input <see cref="Type"/> is in fact a known HLSL vector type</returns>
        [Pure]
        public static bool IsKnownVectorType(Type type) => _HlslMappedVectorTypes.Contains(type) ||
                                                           type == typeof(Vector2) ||
                                                           type == typeof(Vector3) ||
                                                           type == typeof(Vector4);

        /// <summary>
        /// Checks whether or not the input type is a known buffer type
        /// </summary>
        /// <param name="type">The input <see cref="Type"/> instance to check</param>
        /// <returns>A <see langword="bool"/> indicating whether the input <see cref="Type"/> is in fact a known buffer type</returns>
        [Pure]
        public static bool IsKnownBufferType(Type type) => IsConstantBufferType(type) ||
                                                           IsReadOnlyBufferType(type) ||
                                                           IsReadWriteBufferType(type);

        /// <summary>
        /// Checks whether or not the input type is a <see cref="ConstantBuffer{T}"/> value
        /// </summary>
        /// <param name="type">The input <see cref="Type"/> instance to check</param>
        /// <returns>A <see langword="bool"/> indicating whether the input <see cref="Type"/> is in fact a <see cref="ConstantBuffer{T}"/> instance</returns>
        [Pure]
        public static bool IsConstantBufferType(Type type) => type.IsGenericType &&
                                                              type.GetGenericTypeDefinition() == typeof(ConstantBuffer<>);

        /// <summary>
        /// Checks whether or not the input type is a <see cref="ReadOnlyBuffer{T}"/> value
        /// </summary>
        /// <param name="type">The input <see cref="Type"/> instance to check</param>
        /// <returns>A <see langword="bool"/> indicating whether the input <see cref="Type"/> is in fact a <see cref="ReadOnlyBuffer{T}"/> instance</returns>
        [Pure]
        public static bool IsReadOnlyBufferType(Type type) => type.IsGenericType &&
                                                              type.GetGenericTypeDefinition() == typeof(ReadOnlyBuffer<>);

        /// <summary>
        /// Checks whether or not the input type is a <see cref="ReadWriteBuffer{T}"/> value
        /// </summary>
        /// <param name="type">The input <see cref="Type"/> instance to check</param>
        /// <returns>A <see langword="bool"/> indicating whether the input <see cref="Type"/> is in fact a <see cref="ReadWriteBuffer{T}"/> instance</returns>
        [Pure]
        public static bool IsReadWriteBufferType(Type type) => type.IsGenericType &&
                                                               type.GetGenericTypeDefinition() == typeof(ReadWriteBuffer<>);
    }
}
