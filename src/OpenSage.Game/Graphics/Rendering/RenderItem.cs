﻿using System.Collections.Generic;
using System.Numerics;
using LLGfx;
using LLGfx.Effects;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.Rendering
{
    internal delegate void RenderCallback(
        CommandEncoder commandEncoder, 
        Effect effect, 
        EffectPipelineStateHandle pipelineStateHandle,
        RenderInstanceData instanceData);

    internal abstract class RenderItemBase
    {
        public readonly Effect Effect;
        public readonly EffectPipelineStateHandle PipelineStateHandle;
        public readonly RenderCallback RenderCallback;

        protected RenderItemBase(
            Effect effect,
            EffectPipelineStateHandle pipelineStateHandle,
            RenderCallback renderCallback)
        {
            Effect = effect;
            PipelineStateHandle = pipelineStateHandle;
            RenderCallback = renderCallback;
        }
    }

    internal sealed class RenderItem : RenderItemBase
    {
        public readonly RenderableComponent Renderable;

        // Set by RenderPipeline.
        public bool Visible;

        public RenderItem(
            RenderableComponent renderable,
            Effect effect,
            EffectPipelineStateHandle pipelineStateHandle,
            RenderCallback renderCallback)
            : base(effect, pipelineStateHandle, renderCallback)
        {
            Renderable = renderable;
        }
    }

    internal sealed class RenderInstanceData
    {
        public readonly ModelMesh Mesh;
        public readonly List<InstancedRenderable> InstancedRenderables = new List<InstancedRenderable>();

        public DynamicBuffer<Matrix4x3> SkinningBuffer;
        public DynamicBuffer<Matrix4x4> WorldBuffer;

        private Matrix4x3[] _skinningBones;
        private Matrix4x4[] _worldTransforms;

        public uint NumInstances { get; private set; }

        public RenderInstanceData(ModelMesh mesh)
        {
            Mesh = mesh;
        }

        public void Update(GraphicsDevice graphicsDevice)
        {
            NumInstances = 0;

            foreach (var instancedRenderable in InstancedRenderables)
            {
                if (instancedRenderable.Visible)
                {
                    NumInstances += 1;
                }
            }

            if (NumInstances == 0)
            {
                return;
            }

            if (Mesh.Skinned)
            {
                var numElements = (int) (Mesh.NumBones * NumInstances);
                if (SkinningBuffer == null || SkinningBuffer.ElementCount < numElements)
                {
                    SkinningBuffer = DynamicBuffer<Matrix4x3>.CreateArray(graphicsDevice, numElements, BufferUsageFlags.None);

                    _skinningBones = new Matrix4x3[numElements];
                }

                var boneIndex = 0;
                foreach (var instancedRenderable in InstancedRenderables)
                {
                    if (instancedRenderable.Visible)
                    {
                        for (var i = 0; i < Mesh.NumBones; i++)
                        {
                            // Bone matrix should be relative to root bone transform.
                            var rootBoneMatrix = instancedRenderable.Bones[0].LocalToWorldMatrix;
                            var boneMatrix = instancedRenderable.Bones[i].LocalToWorldMatrix;

                            var boneMatrixRelativeToRoot = boneMatrix * Matrix4x4Utility.Invert(rootBoneMatrix);

                            boneMatrixRelativeToRoot.ToMatrix4x3(out _skinningBones[boneIndex++]);
                        }
                    }
                }

                SkinningBuffer.UpdateData(_skinningBones);
            }

            if (WorldBuffer == null || WorldBuffer.ElementCount < NumInstances)
            {
                WorldBuffer = DynamicBuffer<Matrix4x4>.CreateArray(
                    graphicsDevice,
                    (int) NumInstances, 
                    BufferUsageFlags.None);

                _worldTransforms = new Matrix4x4[NumInstances];
            }

            var worldTransformIndex = 0;
            foreach (var instancedRenderable in InstancedRenderables)
            {
                if (instancedRenderable.Visible)
                {
                    _worldTransforms[worldTransformIndex++] = instancedRenderable.Renderable.Transform.LocalToWorldMatrix;
                }
            }

            WorldBuffer.UpdateData(_worldTransforms);
        }
    }

    internal sealed class InstancedRenderItem : RenderItemBase
    {
        public readonly RenderInstanceData InstanceData;

        public InstancedRenderItem(
            RenderInstanceData instanceData,
            Effect effect,
            EffectPipelineStateHandle pipelineStateHandle,
            RenderCallback renderCallback)
            : base(effect, pipelineStateHandle, renderCallback)
        {
            InstanceData = instanceData;
        }
    }

    internal sealed class InstancedRenderable
    {
        public readonly RenderableComponent Renderable;
        public readonly TransformComponent[] Bones;

        // Set by RenderPipeline.
        public bool Visible;

        public InstancedRenderable(RenderableComponent renderable)
        {
            Renderable = renderable;
            if (renderable.MeshBase.Skinned)
            {
                Bones = renderable.Entity.GetComponent<ModelComponent>().Bones;
            }
        }
    }
}
