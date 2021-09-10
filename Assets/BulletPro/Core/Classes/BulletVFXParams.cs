using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script is part of the BulletPro package for Unity.
// Author : Simon Albou <albou.simon@gmail.com>

namespace BulletPro
{
    // Contains every info a bullet needs to play a dynamic VFX at any given moment
    [System.Serializable]
    public struct DynamicBulletVFXParams
    {
        public string tag;
        public bool attachToBulletTransform;
        public bool useDefaultParticles;
        public DynamicObjectReference particleSystemPrefab;

        // Triggers
        public BulletVFXTrigger onBulletBirth;
        public BulletVFXTrigger onVisible;
        public BulletVFXTrigger onInvisible;
        public BulletVFXTrigger onCollision;
        public BulletVFXTrigger onPatternShoot;
        public BulletVFXTrigger onBulletDeath;
        // Important for future updates :
        // if more triggers get created, don't forget to copy them in DynamicParameterSolver.cs

        // Shortcuts for common overrides, creates elements for vfxOverrides at solving
        public bool replaceColorWithBulletColor;
        public bool replaceSizeWithNumber;
        public DynamicFloat sizeNewValue;
        public bool multiplySizeWithNumber;
        public DynamicFloat sizeMultiplier;
        public bool multiplySizeWithBulletScale;
        public bool multiplySpeedWithBulletScale;
        
        // Advanced overrides
        public DynamicBulletVFXOverride[] vfxOverrides;

        #if UNITY_EDITOR
        public static DynamicBulletVFXParams GetDefaultParams()
        {
            DynamicBulletVFXParams result = new DynamicBulletVFXParams();
            result = new DynamicBulletVFXParams();
			result.tag = "VFX #0";
			result.useDefaultParticles = true;
			result.particleSystemPrefab = new DynamicObjectReference(null);
			result.particleSystemPrefab.SetNarrowType(typeof(ParticleSystem));
			//result.onBulletBirth.enabled = true; // removed by default, visually too noisy
			result.onBulletDeath.enabled = true;
			result.replaceColorWithBulletColor = true;
			result.replaceSizeWithNumber = false;
			result.sizeNewValue = new DynamicFloat(1.0f);
			result.multiplySizeWithNumber = false;
			result.sizeMultiplier = new DynamicFloat(1.0f);
			result.multiplySizeWithBulletScale = false;
			result.multiplySpeedWithBulletScale = false;
			result.vfxOverrides = new DynamicBulletVFXOverride[0];
            return result;
        }
        #endif
    }

    // Solved version of this set of parameters. Serializable just in case, but shouldn't be.
    [System.Serializable]
    public struct BulletVFXParams
    {
        // Tag helps identify it, remember stuff in inspector, and can be used to quick-call this VFX
        public string tag;
        public bool attachToBulletTransform;
        public bool useDefaultParticles;
        public ParticleSystem particleSystemPrefab;

        // Triggers
        public BulletVFXTrigger onBulletBirth;
        public BulletVFXTrigger onVisible;
        public BulletVFXTrigger onInvisible;
        public BulletVFXTrigger onCollision;
        public BulletVFXTrigger onPatternShoot;
        public BulletVFXTrigger onBulletDeath;
        
        // Overrides (both simple and advanced)
        public List<BulletVFXOverride> vfxOverrides;
    }

    // Represents an event (say, onBulletDeath) and the associated wanted behaviour.
    [System.Serializable]
    public struct BulletVFXTrigger
    {
        public bool enabled;
        public BulletVFXBehaviour behaviour;
        public bool unless;
        public BulletVFXCanceller canceller;

        // Example : "on bullet collision, play this VFX, unless the renderer is disabled"
    }
    public enum BulletVFXBehaviour { Play, Stop }
    [System.Flags]
    public enum BulletVFXCanceller
    {
        BulletGraphicsAreDisabled = (1 << 0),
        BulletGraphicsAreEnabled = (1 << 1)
    }

    // VFXOverrides contain dynamic parameters, allowing the aspect of a VFX to change based on bullet params.
    // Most importantly, it allows any parameter of the ParticleSystem to get the same level of modularity as DynamicParameters,
    // such as checking Bullet Hierarchy state to change, say, color or speed.
    [System.Serializable]
    public struct DynamicBulletVFXOverride
    {
        // Choose what particle system parameter should be overriden, among a looong enum
        public BulletVFXParameterType parameterToOverride;

        // Choose in what mode the parameter should be
        public ParticleSystemCurveMode curveMode;
        public ParticleSystemGradientMode gradientMode;

        // In "random between two" mode, choose what value to override
        public MinMaxOverrideMode minMaxOverrideMode;

        // Do we take the Bullet's color or a custom one? And so on.
        public VFXReferenceColor referenceColor;
        public VFXReferenceGradient referenceGradient;
        public VFXReferenceFloat referenceFloat;

        // Blend modes
        public VFXNumberOverrideMode numberOverrideMode;
        public VFXColorOverrideMode colorOverrideMode;
        public VFXStringOverrideMode stringOverrideMode;

        // Actual values
        public DynamicAnimationCurve curveValue;
        public DynamicColor colorValue;
        public DynamicGradient gradientValue;
        public DynamicFloat floatValue;
        public DynamicInt intValue;
        public DynamicString stringValue;
        public DynamicEnum enumValue;
        public DynamicBool boolValue;
        public DynamicVector2 vector2Value;
        public DynamicVector3 vector3Value;
        public DynamicObjectReference objectReferenceValue;
    }

    // Solved version of this override. Serializable just in case, but shouldn't be.
    [System.Serializable]
    public struct BulletVFXOverride
    {
        // For comments, see the Dynamic counterpart of this struct.
        public BulletVFXParameterType parameterToOverride;

        public ParticleSystemCurveMode curveMode;
        public ParticleSystemGradientMode gradientMode;

        public MinMaxOverrideMode minMaxOverrideMode;

        public VFXReferenceColor referenceColor;
        public VFXReferenceGradient referenceGradient;
        public VFXReferenceFloat referenceFloat;

        public VFXNumberOverrideMode numberOverrideMode;
        public VFXColorOverrideMode colorOverrideMode;
        public VFXStringOverrideMode stringOverrideMode;

        // Solved values
        public AnimationCurve curveValue;
        public Color colorValue;
        public Gradient gradientValue;
        public float floatValue;
        public int intValue; // also used for enums
        public string stringValue;
        public bool boolValue;
        public Vector2 vector2Value;
        public Vector3 vector3Value;
        public Object objectReferenceValue;

        // Helper function for various editors and cases, helps group values that behave similarly
        public static BulletVFXAtomicParameterType GetAtomicParameterType(BulletVFXParameterType parameterType)
        {
            switch (parameterType)
            {
                case BulletVFXParameterType.MainModuleStartLifetime:
                case BulletVFXParameterType.MainModuleStartSpeed:
                case BulletVFXParameterType.MainModuleStartSize:
                case BulletVFXParameterType.MainModuleStartSizeX:
                case BulletVFXParameterType.MainModuleStartSizeY:
                case BulletVFXParameterType.MainModuleStartSizeZ:
                case BulletVFXParameterType.MainModuleStartDelay:
                case BulletVFXParameterType.MainModuleGravityModifier:
                case BulletVFXParameterType.MainModuleStartRotation:
                case BulletVFXParameterType.MainModuleStartRotationX:
                case BulletVFXParameterType.MainModuleStartRotationY:
                case BulletVFXParameterType.MainModuleStartRotationZ:
                case BulletVFXParameterType.EmissionModuleRateOverTime:
                case BulletVFXParameterType.EmissionModuleRateOverDistance:
                case BulletVFXParameterType.VelocityOverLifetimeModuleOrbitalOffsetX:
                case BulletVFXParameterType.VelocityOverLifetimeModuleOrbitalOffsetY:
                case BulletVFXParameterType.VelocityOverLifetimeModuleOrbitalOffsetZ:
                case BulletVFXParameterType.VelocityOverLifetimeModuleOrbitalX:
                case BulletVFXParameterType.VelocityOverLifetimeModuleOrbitalY:
                case BulletVFXParameterType.VelocityOverLifetimeModuleOrbitalZ:
                case BulletVFXParameterType.VelocityOverLifetimeModuleX:
                case BulletVFXParameterType.VelocityOverLifetimeModuleY:
                case BulletVFXParameterType.VelocityOverLifetimeModuleZ:
                case BulletVFXParameterType.VelocityOverLifetimeModuleRadial:
                case BulletVFXParameterType.VelocityOverLifetimeModuleSpeedModifier:
                case BulletVFXParameterType.LimitVelocityOverLifetimeModuleDrag:
                case BulletVFXParameterType.LimitVelocityOverLifetimeModuleLimit:
                case BulletVFXParameterType.LimitVelocityOverLifetimeModuleLimitX:
                case BulletVFXParameterType.LimitVelocityOverLifetimeModuleLimitY:
                case BulletVFXParameterType.LimitVelocityOverLifetimeModuleLimitZ:
                case BulletVFXParameterType.InheritVelocityModuleCurve:
                case BulletVFXParameterType.ForceOverLifetimeModuleX:
                case BulletVFXParameterType.ForceOverLifetimeModuleY:
                case BulletVFXParameterType.ForceOverLifetimeModuleZ:
                case BulletVFXParameterType.SizeOverLifetimeModuleSize:
                case BulletVFXParameterType.SizeOverLifetimeModuleX:
                case BulletVFXParameterType.SizeOverLifetimeModuleY:
                case BulletVFXParameterType.SizeOverLifetimeModuleZ:
                case BulletVFXParameterType.SizeBySpeedModuleX:
                case BulletVFXParameterType.SizeBySpeedModuleY:
                case BulletVFXParameterType.SizeBySpeedModuleZ:
                case BulletVFXParameterType.RotationOverLifetimeModuleX:
                case BulletVFXParameterType.RotationOverLifetimeModuleY:
                case BulletVFXParameterType.RotationOverLifetimeModuleZ:
                case BulletVFXParameterType.RotationBySpeedModuleX:
                case BulletVFXParameterType.RotationBySpeedModuleY:
                case BulletVFXParameterType.RotationBySpeedModuleZ:
                case BulletVFXParameterType.NoiseModuleScrollSpeed:
                case BulletVFXParameterType.NoiseModuleSizeAmount:
                case BulletVFXParameterType.NoiseModuleRemap:
                case BulletVFXParameterType.NoiseModuleRemapX:
                case BulletVFXParameterType.NoiseModuleRemapY:
                case BulletVFXParameterType.NoiseModuleRemapZ:
                case BulletVFXParameterType.NoiseModuleStrength:
                case BulletVFXParameterType.NoiseModuleStrengthX:
                case BulletVFXParameterType.NoiseModuleStrengthY:
                case BulletVFXParameterType.NoiseModuleStrengthZ:
                case BulletVFXParameterType.CollisionModuleBounce:
                case BulletVFXParameterType.CollisionModuleDampen:
                case BulletVFXParameterType.CollisionModuleLifetimeLoss:
                case BulletVFXParameterType.TextureSheetAnimationModuleFrameOverTime:
                case BulletVFXParameterType.TextureSheetAnimationModuleStartFrame:
                case BulletVFXParameterType.LightsModuleIntensity:
                case BulletVFXParameterType.LightsModuleRange:
                case BulletVFXParameterType.TrailModuleLifetime:
                case BulletVFXParameterType.TrailModuleWidthOverTrail:
                    return BulletVFXAtomicParameterType.MinMaxCurve;

                case BulletVFXParameterType.MainModuleStartColor:
                case BulletVFXParameterType.ColorOverLifetimeModule:
                case BulletVFXParameterType.ColorBySpeedModuleColor:
                case BulletVFXParameterType.TrailModuleColorOverLifetime:
                case BulletVFXParameterType.TrailModuleColorOverTrail:
                    return BulletVFXAtomicParameterType.MinMaxGradient;

                case BulletVFXParameterType.MainModuleSystemDuration:
                case BulletVFXParameterType.MainModuleSimulationSpeed:
                case BulletVFXParameterType.MainModuleFlipRotation:
                case BulletVFXParameterType.ShapeModuleArc:
                case BulletVFXParameterType.ShapeModuleLength:
                case BulletVFXParameterType.ShapeModuleNormalOffset:
                case BulletVFXParameterType.ShapeModuleRadius:
                case BulletVFXParameterType.ShapeModuleRandomPositionAmount:
                case BulletVFXParameterType.ShapeModuleRandomDirectionAmount:
                case BulletVFXParameterType.ShapeModuleSphericalDirectionAmount:
                case BulletVFXParameterType.LimitVelocityOverLifetimeModuleDampen:
                case BulletVFXParameterType.ExternalForcesModuleMultiplier:
                case BulletVFXParameterType.NoiseModuleFrequency:
                case BulletVFXParameterType.NoiseModuleOctaveMultiplier:
                case BulletVFXParameterType.NoiseModuleOctaveScale:
                case BulletVFXParameterType.CollisionModuleMinKillSpeed:
                case BulletVFXParameterType.CollisionModuleMaxKillSpeed:
                case BulletVFXParameterType.CollisionModuleRadiusScale:
                case BulletVFXParameterType.TriggersModuleRadiusScale:
                case BulletVFXParameterType.LightsModuleRatio:
                case BulletVFXParameterType.TrailModuleRatio:
                case BulletVFXParameterType.RendererMinParticleSize:
                case BulletVFXParameterType.RendererMaxParticleSize:
                    return BulletVFXAtomicParameterType.ConstantFloat;

                case BulletVFXParameterType.MainModuleMaxParticles:
                case BulletVFXParameterType.ShapeModuleMeshMaterialIndex:
                case BulletVFXParameterType.NoiseModuleOctaveCount:
                case BulletVFXParameterType.TextureSheetAnimationModuleCycleCount:
                case BulletVFXParameterType.TextureSheetAnimationModuleRowIndex:
                case BulletVFXParameterType.RendererSortingOrder:
                    return BulletVFXAtomicParameterType.ConstantInt;                    

                case BulletVFXParameterType.MainModuleSimulationSpace:
                case BulletVFXParameterType.MainModuleScalingMode:
                case BulletVFXParameterType.ShapeModuleShapeType:
                case BulletVFXParameterType.ShapeModuleMeshShapeType:
                case BulletVFXParameterType.VelocityOverLifetimeModuleSpace:
                case BulletVFXParameterType.LimitVelocityOverLifetimeModuleSpace:
                case BulletVFXParameterType.InheritVelocityModuleMode:
                case BulletVFXParameterType.ForceOverLifetimeModuleSpace:
                case BulletVFXParameterType.NoiseModuleQuality:
                    return BulletVFXAtomicParameterType.Enum;

                case BulletVFXParameterType.ColorBySpeedModuleRange:
                case BulletVFXParameterType.SizeBySpeedModuleRange:
                case BulletVFXParameterType.RotationBySpeedModuleRange:
                    return BulletVFXAtomicParameterType.Vector2;                    

                case BulletVFXParameterType.ShapeModuleScale:
                case BulletVFXParameterType.ShapeModulePosition:
                case BulletVFXParameterType.ShapeModuleRotation:
                case BulletVFXParameterType.RendererFlip:
                case BulletVFXParameterType.RendererPivot:
                    return BulletVFXAtomicParameterType.Vector3;                    

                case BulletVFXParameterType.MainModuleLooping:
                case BulletVFXParameterType.MainModulePrewarm:
                case BulletVFXParameterType.ShapeModuleAlignToDirection:
                case BulletVFXParameterType.NoiseModuleDamping:
                    return BulletVFXAtomicParameterType.Bool;                    

                case BulletVFXParameterType.RendererSortingLayerName:
                    return BulletVFXAtomicParameterType.String;                    

                case BulletVFXParameterType.MainModuleCustomSimulationSpace:
                case BulletVFXParameterType.ShapeModuleMesh:
                case BulletVFXParameterType.ShapeModuleSkinnedMeshRenderer:
                case BulletVFXParameterType.LightsModuleLight:
                case BulletVFXParameterType.RendererMaterial:
                case BulletVFXParameterType.RendererTrailMaterial:
                    return BulletVFXAtomicParameterType.Object;
            }

            // default, shouldn't happen
            return BulletVFXAtomicParameterType.Error;
        }
    }

    // All the enums that drive settings for BulletVFXOverrides
    public enum MinMaxOverrideMode { OverrideMin, OverrideMax, OverrideBoth }
    public enum VFXReferenceColor { BulletColor, CustomValue }
    public enum VFXReferenceGradient { BulletLifetimeGradient, CustomValue }
    public enum VFXReferenceFloat { BulletScale, BulletSpeed, BulletLifetime, CustomValue }
    public enum VFXNumberOverrideMode { ReplaceWith, MultiplyBy, Add }
    public enum VFXColorOverrideMode { ReplaceWith, MultiplyBy, Add, Subtract, AlphaBlend, Average }
    public enum VFXStringOverrideMode { ReplaceWith, Append }

    public enum BulletVFXParameterType
    {
        // Main module - common stuff
        MainModuleStartColor,
        MainModuleSystemDuration,
        MainModuleStartLifetime,
        MainModuleStartSpeed,
        MainModuleStartSize,
        MainModuleStartSizeX,
        MainModuleStartSizeY,
        MainModuleStartSizeZ,
        MainModuleSimulationSpace,
        MainModuleSimulationSpeed,        
        MainModuleMaxParticles,

        // Main module - rare stuff
        MainModulePrewarm,
        MainModuleLooping,
        MainModuleStartDelay,
        MainModuleGravityModifier,
        MainModuleStartRotation,
        MainModuleStartRotationX,
        MainModuleStartRotationY,
        MainModuleStartRotationZ,
        MainModuleFlipRotation,
        MainModuleScalingMode,
        MainModuleCustomSimulationSpace,

        // Emission module
        EmissionModuleRateOverTime,
        EmissionModuleRateOverDistance,

        // Shape module - dimension
        ShapeModuleScale,
        ShapeModuleAlignToDirection,
        ShapeModuleArc,
        ShapeModuleLength,
        ShapeModuleNormalOffset,
        ShapeModulePosition,
        ShapeModuleRotation,
        ShapeModuleRadius,
        ShapeModuleRandomPositionAmount,
        ShapeModuleRandomDirectionAmount,
        ShapeModuleSphericalDirectionAmount,

        // Shape module - geometry
        ShapeModuleMesh,
        ShapeModuleMeshMaterialIndex,
        ShapeModuleMeshShapeType,
        ShapeModuleShapeType,
        ShapeModuleSkinnedMeshRenderer,

        // Velocity over lifetime
        VelocityOverLifetimeModuleOrbitalOffsetX,
        VelocityOverLifetimeModuleOrbitalOffsetY,
        VelocityOverLifetimeModuleOrbitalOffsetZ,
        VelocityOverLifetimeModuleOrbitalX,
        VelocityOverLifetimeModuleOrbitalY,
        VelocityOverLifetimeModuleOrbitalZ,
        VelocityOverLifetimeModuleRadial,
        VelocityOverLifetimeModuleSpace,
        VelocityOverLifetimeModuleSpeedModifier,
        VelocityOverLifetimeModuleX,
        VelocityOverLifetimeModuleY,
        VelocityOverLifetimeModuleZ,

        // Limit velocity over lifetime
        LimitVelocityOverLifetimeModuleDampen,
        LimitVelocityOverLifetimeModuleDrag,
        LimitVelocityOverLifetimeModuleLimit,
        LimitVelocityOverLifetimeModuleLimitX,
        LimitVelocityOverLifetimeModuleLimitY,
        LimitVelocityOverLifetimeModuleLimitZ,
        LimitVelocityOverLifetimeModuleSpace,

        // Inherit velocity
        InheritVelocityModuleCurve,
        InheritVelocityModuleMode,

        // Force over lifetime
        ForceOverLifetimeModuleX,
        ForceOverLifetimeModuleY,
        ForceOverLifetimeModuleZ,
        ForceOverLifetimeModuleSpace,

        // Color over lifetime
        ColorOverLifetimeModule,

        // Color by speed
        ColorBySpeedModuleColor,
        ColorBySpeedModuleRange,

        // Size over lifetime
        SizeOverLifetimeModuleSize,
        SizeOverLifetimeModuleX,
        SizeOverLifetimeModuleY,
        SizeOverLifetimeModuleZ,

        // Size by speed
        SizeBySpeedModuleRange,
        SizeBySpeedModuleSize,
        SizeBySpeedModuleX,
        SizeBySpeedModuleY,
        SizeBySpeedModuleZ,

        // Rotation over lifetime
        RotationOverLifetimeModuleX,
        RotationOverLifetimeModuleY,
        RotationOverLifetimeModuleZ,

        // Rotation by speed
        RotationBySpeedModuleRange,
        RotationBySpeedModuleX,
        RotationBySpeedModuleY,
        RotationBySpeedModuleZ,

        // External forces
        ExternalForcesModuleMultiplier,

        // Noise
        NoiseModuleDamping,
        NoiseModuleFrequency,
        NoiseModuleOctaveCount,
        NoiseModuleOctaveScale,
        NoiseModuleOctaveMultiplier,
        NoiseModuleQuality,
        NoiseModuleRemap,
        NoiseModuleRemapX,
        NoiseModuleRemapY,
        NoiseModuleRemapZ,
        NoiseModuleScrollSpeed,
        NoiseModuleSizeAmount,
        NoiseModuleStrength,
        NoiseModuleStrengthX,
        NoiseModuleStrengthY,
        NoiseModuleStrengthZ,

        // Collision
        CollisionModuleBounce,
        CollisionModuleDampen,
        CollisionModuleLifetimeLoss,
        CollisionModuleMinKillSpeed,
        CollisionModuleMaxKillSpeed,
        CollisionModuleRadiusScale,

        // Triggers
        TriggersModuleRadiusScale,

        // (Skipping Sub-emitter module)
        // Texture Sheet Animation
        TextureSheetAnimationModuleCycleCount,
        TextureSheetAnimationModuleFrameOverTime,
        TextureSheetAnimationModuleRowIndex,
        TextureSheetAnimationModuleStartFrame,

        // Lights module
        LightsModuleIntensity,
        LightsModuleLight,
        LightsModuleRange,
        LightsModuleRatio,

        // Trail module
        TrailModuleRatio,
        TrailModuleLifetime,
        TrailModuleWidthOverTrail,
        TrailModuleColorOverLifetime,
        TrailModuleColorOverTrail,

        // Renderer
        RendererSortingOrder,
        RendererSortingLayerName,
        RendererMaterial,
        RendererTrailMaterial,
        RendererMinParticleSize,
        RendererMaxParticleSize,
        RendererFlip,
        RendererPivot
    }

    // Helps sorting BulletVFXParameterTypes into categories
    public enum BulletVFXAtomicParameterType
    {
        MinMaxCurve,
        MinMaxGradient,
        ConstantFloat,
        ConstantInt,
        Enum,
        Bool,
        Vector2,
        Vector3,
        String,
        Object,
        Error // if Error gets returned, a value has been forgotten
    }
}