using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// This script is part of the BulletPro package for Unity.
// Author : Simon Albou <albou.simon@gmail.com>

namespace BulletPro.EditorScripts
{
    public static class EmitterProfileUtility
    {
        #region ridding objects of unused references

        public static void RidNewObjectOfUnusedReferences(EmissionParams elem)
        {
            // Find references and set them to null
            bool shouldRebuildChildrenArray = false;
            if (elem is BulletParams)
                shouldRebuildChildrenArray = RidBulletOfReferences(elem as BulletParams);
            else if (elem is ShotParams)
                shouldRebuildChildrenArray = RidShotOfReferences(elem as ShotParams);
            else if (elem is PatternParams)
                shouldRebuildChildrenArray = RidPatternOfReferences(elem as PatternParams);

            // Rebuild "children" array
            if (!shouldRebuildChildrenArray) return;
            List<EmissionParams> children = new List<EmissionParams>();
            for (int i = 0; i < elem.children.Length; i++)
                if (elem.children[i] != null)
                    children.Add(elem.children[i]);

            elem.children = children.ToArray();
        }

        static bool RidBulletOfReferences(BulletParams bp)
        {
            if (!bp.hasPatterns)
            {
                bp.children = new EmissionParams[0];
                bp.patternsShot = new DynamicPattern[0];
            }

            return false;
        }

        static bool RidShotOfReferences(ShotParams sp)
        {
            if (sp.modifiers == null) return false;
            if (sp.modifiers.Count == 0) return false;

            bool hasChanged = false;
            for (int i = 0; i < sp.modifiers.Count; i++)
            {
                ShotModifier sm = sp.modifiers[i];
                if (sm.modifierType == ShotModifierType.SetBulletParams && sm.enabled) continue;
                
                if (IsEmptyDynamicBullet(sm.bulletParams)) continue;
                
                for (int j = 0; j < sp.children.Length; j++)
                {
                    int indexInDynamic = LocateObjectIndexInDynamicBullet(sm.bulletParams, sp.children[j] as BulletParams);
                    if (indexInDynamic > -1)
                    {
                        sp.children[j] = null;
                        hasChanged = true;
                    }
                }
                sm.bulletParams = new DynamicBullet(null);
                sp.modifiers[i] = sm;
            }

            return hasChanged;
        }

        static bool RidPatternOfReferences(PatternParams pp)
        {
            if (pp.instructionLists == null) return false;
            if (pp.instructionLists.Length == 0) return false;

            bool hasChanged = false;
            for (int i = 0; i < pp.instructionLists.Length; i++)
            {
                PatternInstruction[] pil = pp.instructionLists[i].instructions;
                if (pil == null) continue;
                if (pil.Length == 0) continue;
                for (int j = 0; j < pil.Length; j++)
                {
                    PatternInstruction pi = pil[j];
                    if (pi.instructionType == PatternInstructionType.Shoot && pi.enabled) continue;
                    if (IsEmptyDynamicShot(pi.shot)) continue;
                    for (int k = 0; k < pp.children.Length; k++)
                    {
                        int indexInDynamic = LocateObjectIndexInDynamicShot(pi.shot, pp.children[k] as ShotParams);
                        if (indexInDynamic > -1)
                        {
                            pp.children[k] = null;
                            hasChanged = true;
                        }
                    }
                    pi.shot = new DynamicShot(null);
                    pil[j] = pi;
                }
            }

            return hasChanged;
        }

        #endregion

        #region naming utility

        public static string MakeUniqueName(string baseName, EmitterProfile profile)
        {
            if (!HasSubAssetOfMatchingName(baseName, profile))
                return baseName;

            int i = 0;
            while (HasSubAssetOfMatchingName(baseName + " " + i.ToString(), profile))
                i++;

            return baseName + " " + i.ToString();
        }

        static bool HasSubAssetOfMatchingName(string baseName, EmitterProfile profile)
        {
            if (profile.subAssets == null) return false;
            if (profile.subAssets.Length == 0) return false;
            for (int i = 0; i < profile.subAssets.Length; i++)
                if (profile.subAssets[i] != null)
                    if (profile.subAssets[i].name == baseName)
                        return true;

            return false;
        }

        #endregion

        #region child replacement

        public static void ReplaceChild(EmissionParams parentObject, int indexOfChild, EmissionParams newChild)
        {
            if (parentObject is BulletParams)
            {
                if (newChild is BulletParams) return;
                if (newChild is ShotParams) return;
                newChild.parent = parentObject;
                ReplaceChildOfBullet(parentObject as BulletParams, indexOfChild, newChild as PatternParams);
                parentObject.children[indexOfChild] = newChild;
            }

            if (parentObject is ShotParams)
            {
                if (newChild is PatternParams) return;
                if (newChild is ShotParams) return;
                newChild.parent = parentObject;
                ReplaceChildOfShot(parentObject as ShotParams, indexOfChild, newChild as BulletParams);
                parentObject.children[indexOfChild] = newChild;
            }

            if (parentObject is PatternParams)
            {
                if (newChild is BulletParams) return;
                if (newChild is PatternParams) return;
                newChild.parent = parentObject;
                ReplaceChildOfPattern(parentObject as PatternParams, indexOfChild, newChild as ShotParams);
                parentObject.children[indexOfChild] = newChild;
            }
        }

        public static void ReplaceChildOfBullet(BulletParams bp, int indexOfChild, PatternParams newChild)
        {
            if (bp.patternsShot == null) return;
			if (bp.patternsShot.Length == 0) return;
            for (int i = 0; i < bp.patternsShot.Length; i++)
            {
                int indexInDynamic = LocateObjectIndexInDynamicPattern(bp.patternsShot[i], bp.children[indexOfChild] as PatternParams);
                if (indexInDynamic > -1)
                    bp.patternsShot[i].valueTree[indexInDynamic].defaultValue = newChild as PatternParams;
            }
        }

        public static void ReplaceChildOfShot(ShotParams sp, int indexOfChild, BulletParams newChild)
        {
            int indexInDynamic = -1;

            indexInDynamic = LocateObjectIndexInDynamicBullet(sp.bulletParams, sp.children[indexOfChild] as BulletParams);
            if (indexInDynamic > -1)
				sp.bulletParams.valueTree[indexInDynamic].defaultValue = newChild as BulletParams;

            indexInDynamic = -1;
			if (sp.modifiers != null)
				if (sp.modifiers.Count != 0)
					for (int i = 0; i < sp.modifiers.Count; i++)
                    {
                        indexInDynamic = LocateObjectIndexInDynamicBullet(sp.modifiers[i].bulletParams, sp.children[indexOfChild] as BulletParams);
						if (indexInDynamic > -1)
                        {
							ShotModifier sm = sp.modifiers[i];
							sm.bulletParams.valueTree[indexInDynamic].defaultValue = newChild as BulletParams;
							sp.modifiers[i] = sm;
						}
                    }

			if (sp.bulletSpawns != null)
				if (sp.bulletSpawns.Length != 0)
					for (int i = 0; i < sp.bulletSpawns.Length; i++)
						if (sp.bulletSpawns[i].bulletParams == sp.children[indexOfChild])
							sp.bulletSpawns[i].bulletParams = newChild as BulletParams;
        }

        public static void ReplaceChildOfPattern(PatternParams pp, int indexOfChild, ShotParams newChild)
        {
            if (pp.instructionLists == null) return;
            if (pp.instructionLists.Length == 0) return;
            for (int i = 0; i < pp.instructionLists.Length; i++)
            {
                PatternInstruction[] instructions = pp.instructionLists[i].instructions;
                if (instructions == null) continue;
                if (instructions.Length == 0) continue;
                for (int j = 0; j < instructions.Length; j++)
                {
                    int indexInDynamic = LocateObjectIndexInDynamicShot(instructions[j].shot, pp.children[indexOfChild] as ShotParams);
                    if (indexInDynamic > -1)
                        instructions[j].shot.valueTree[indexInDynamic].defaultValue = newChild as ShotParams;
                }
            }
        }

        #endregion

        #region locating an object in a dynamic param

        public static int LocateObjectIndexInDynamicPattern(DynamicPattern dp, PatternParams toLocate, int startingIndex = 1)
        {
            DynamicPatternValue dpv = dp.valueTree[startingIndex];
            if (dpv.settings.valueType == DynamicParameterSorting.Fixed)
            {
                if (dpv.defaultValue == toLocate) return startingIndex;
                else return -1;
            }
            else // if blend
            {
                if (dpv.settings.indexOfBlendedChildren == null) return -1;
                if (dpv.settings.indexOfBlendedChildren.Length == 0) return -1;
                for (int i = 0; i < dpv.settings.indexOfBlendedChildren.Length; i++)
                {
                    int indexOfThisBlend = LocateObjectIndexInDynamicPattern(dp, toLocate, dpv.settings.indexOfBlendedChildren[i]);
                    if (indexOfThisBlend > -1) return indexOfThisBlend;
                }
                return -1;
            }
        }

        public static int LocateObjectIndexInDynamicBullet(DynamicBullet db, BulletParams toLocate, int startingIndex = 1)
        {
            DynamicBulletValue dbv = db.valueTree[startingIndex];
            if (dbv.settings.valueType == DynamicParameterSorting.Fixed)
            {
                if (dbv.defaultValue == toLocate) return startingIndex;
                else return -1;
            }
            else // if blend
            {
                if (dbv.settings.indexOfBlendedChildren == null) return -1;
                if (dbv.settings.indexOfBlendedChildren.Length == 0) return -1;
                for (int i = 0; i < dbv.settings.indexOfBlendedChildren.Length; i++)
                {
                    int indexOfThisBlend = LocateObjectIndexInDynamicBullet(db, toLocate, dbv.settings.indexOfBlendedChildren[i]);
                    if (indexOfThisBlend > -1) return indexOfThisBlend;
                }
                return -1;
            }
        }

        public static int LocateObjectIndexInDynamicShot(DynamicShot ds, ShotParams toLocate, int startingIndex = 1)
        {
            DynamicShotValue dsv = ds.valueTree[startingIndex];
            if (dsv.settings.valueType == DynamicParameterSorting.Fixed)
            {
                if (dsv.defaultValue == toLocate) return startingIndex;
                else return -1;
            }
            else // if blend
            {
                if (dsv.settings.indexOfBlendedChildren == null) return -1;
                if (dsv.settings.indexOfBlendedChildren.Length == 0) return -1;
                for (int i = 0; i < dsv.settings.indexOfBlendedChildren.Length; i++)
                {
                    int indexOfThisBlend = LocateObjectIndexInDynamicShot(ds, toLocate, dsv.settings.indexOfBlendedChildren[i]);
                    if (indexOfThisBlend > -1) return indexOfThisBlend;
                }
                return -1;
            }
        }

        #endregion

        #region finding out if a dynamic object is empty

        public static bool IsEmptyDynamicBullet(DynamicBullet db, int startingIndex = 1)
        {
            DynamicBulletValue dbv = db.valueTree[startingIndex];
            if (dbv.settings.valueType == DynamicParameterSorting.Fixed)
                return dbv.defaultValue == null;
            else // if blend
            {
                if (dbv.settings.indexOfBlendedChildren == null) return true;
                if (dbv.settings.indexOfBlendedChildren.Length == 0) return true;
                for (int i = 0; i < dbv.settings.indexOfBlendedChildren.Length; i++)
                {
                    bool childIsEmpty = IsEmptyDynamicBullet(db, dbv.settings.indexOfBlendedChildren[i]);
                    if (!childIsEmpty) return false;
                }
                return true;
            }
        }

        public static bool IsEmptyDynamicShot(DynamicShot ds, int startingIndex = 1)
        {
            DynamicShotValue dsv = ds.valueTree[startingIndex];
            if (dsv.settings.valueType == DynamicParameterSorting.Fixed)
                return dsv.defaultValue == null;
            else // if blend
            {
                if (dsv.settings.indexOfBlendedChildren == null) return true;
                if (dsv.settings.indexOfBlendedChildren.Length == 0) return true;
                for (int i = 0; i < dsv.settings.indexOfBlendedChildren.Length; i++)
                {
                    bool childIsEmpty = IsEmptyDynamicShot(ds, dsv.settings.indexOfBlendedChildren[i]);
                    if (!childIsEmpty) return false;
                }
                return true;
            }
        }

        public static bool IsEmptyDynamicPattern(DynamicPattern dp, int startingIndex = 1)
        {
            DynamicPatternValue dpv = dp.valueTree[startingIndex];
            if (dpv.settings.valueType == DynamicParameterSorting.Fixed)
                return dpv.defaultValue == null;
            else // if blend
            {
                if (dpv.settings.indexOfBlendedChildren == null) return true;
                if (dpv.settings.indexOfBlendedChildren.Length == 0) return true;
                for (int i = 0; i < dpv.settings.indexOfBlendedChildren.Length; i++)
                {
                    bool childIsEmpty = IsEmptyDynamicPattern(dp, dpv.settings.indexOfBlendedChildren[i]);
                    if (!childIsEmpty) return false;
                }
                return true;
            }
        }

        #endregion

        #region grouped SetParent on whole dynamic object

        // the inspector is passed as reference to call SetParent, it needs serializedObject to work
        public static void SetParentOfBullet(DynamicBullet db, EmissionParams newParent, EmissionParamsInspector insp, int startingIndex = 1)
        {
            DynamicBulletValue dbv = db.valueTree[startingIndex];
            if (dbv.settings.valueType == DynamicParameterSorting.Fixed)
                insp.SetParent(dbv.defaultValue, newParent);
            else // if blend
            {
                if (dbv.settings.indexOfBlendedChildren == null) return;
                if (dbv.settings.indexOfBlendedChildren.Length == 0) return;
                for (int i = 0; i < dbv.settings.indexOfBlendedChildren.Length; i++)
                    SetParentOfBullet(db, newParent, insp, dbv.settings.indexOfBlendedChildren[i]);
            }
        }

        public static void SetParentOfShot(DynamicShot ds, EmissionParams newParent, EmissionParamsInspector insp, int startingIndex = 1)
        {
            DynamicShotValue dsv = ds.valueTree[startingIndex];
            if (dsv.settings.valueType == DynamicParameterSorting.Fixed)
                insp.SetParent(dsv.defaultValue, newParent);
            else // if blend
            {
                if (dsv.settings.indexOfBlendedChildren == null) return;
                if (dsv.settings.indexOfBlendedChildren.Length == 0) return;
                for (int i = 0; i < dsv.settings.indexOfBlendedChildren.Length; i++)
                    SetParentOfShot(ds, newParent, insp, dsv.settings.indexOfBlendedChildren[i]);
            }
        }

        public static void SetParentOfPattern(DynamicPattern dp, EmissionParams newParent, EmissionParamsInspector insp, int startingIndex = 1)
        {
            DynamicPatternValue dpv = dp.valueTree[startingIndex];
            if (dpv.settings.valueType == DynamicParameterSorting.Fixed)
                insp.SetParent(dpv.defaultValue, newParent);
            else // if blend
            {
                if (dpv.settings.indexOfBlendedChildren == null) return;
                if (dpv.settings.indexOfBlendedChildren.Length == 0) return;
                for (int i = 0; i < dpv.settings.indexOfBlendedChildren.Length; i++)
                    SetParentOfPattern(dp, newParent, insp, dpv.settings.indexOfBlendedChildren[i]);
            }
        }

        // is equivalent to any of the three functions above, but works with SerializedProperties. FieldHandler works fine as a reference holding the needed serializedObject.
        public static void SetParentOfHierarchyElementAsSerializedProperty(SerializedProperty sp, EmissionParams newParent, BulletHierarchyFieldHandler fieldHandler, int startingIndex = 1)
        {
            SerializedProperty valueProp = sp.FindPropertyRelative("valueTree").GetArrayElementAtIndex(startingIndex);
            SerializedProperty settings = valueProp.FindPropertyRelative("settings");
            SerializedProperty valueType = settings.FindPropertyRelative("valueType");
            if (valueType.enumValueIndex == (int)DynamicParameterSorting.Fixed)
                fieldHandler.SetParent(valueProp.FindPropertyRelative("defaultValue").objectReferenceValue as EmissionParams, newParent);
            else // if blend
            {
                SerializedProperty indexOfBlendedChildren = settings.FindPropertyRelative("indexOfBlendedChildren");
                if (indexOfBlendedChildren.arraySize == 0) return;
                for (int i = 0; i < indexOfBlendedChildren.arraySize; i++)
                    SetParentOfHierarchyElementAsSerializedProperty(sp, newParent, fieldHandler, indexOfBlendedChildren.GetArrayElementAtIndex(i).intValue);
            }
        }

        #endregion

        #region updates from one version to another

        // Calls UpdateProfile() on every EmitterProfile.
        [MenuItem("Tools/BulletPro/Update Assets")]
        public static void UpdateAllProfiles()
        {
            string[] profileGUIDS = AssetDatabase.FindAssets("t:EmitterProfile");
            if (profileGUIDS == null) return;
            if (profileGUIDS.Length == 0) return;
            for (int i = 0; i < profileGUIDS.Length; i++)
            {
                EmitterProfile profile = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(profileGUIDS[i]), typeof(EmitterProfile)) as EmitterProfile;
                UpdateProfile(profile);
            }

            Debug.Log("BulletPro update: the "+profileGUIDS.Length.ToString()+" Emitter Profile assets found in this project have been updated to the last version of BulletPro.");
        }

        // Ensures a profile matches the current version of BulletPro.
        public static void UpdateProfile(EmitterProfile profile)
        {
            // disable time travel
            if (profile.buildNumber > BulletProSettings.buildNumber)
            {
                Debug.LogWarning("BulletPro Warning: profile\""+profile.name+"\" comes from a superior version of BulletPro. It might not work properly.");
                profile.buildNumber = BulletProSettings.buildNumber;
                EditorUtility.SetDirty(profile);
            }

            // don't do anything if an object is up to date
            if (profile.buildNumber == BulletProSettings.buildNumber) return;

            // from anything (bugged) to V1
            if (profile.buildNumber < 1)
                profile.buildNumber = 1;

            // from V1 to V2
            if (profile.buildNumber == 1)
                profile.buildNumber++;

            // from V2 to V3
            if (profile.buildNumber == 2)
            {
                profile.buildNumber++;
                for (int i = 0; i < profile.subAssets.Length; i++)
                {
                    if (profile.subAssets[i] is ShotParams) continue;
                    if (profile.subAssets[i] is PatternParams) continue;

                    BulletParams bp = profile.subAssets[i] as BulletParams;
                    EditorUtility.SetDirty(bp);
                }
            }

            // from V3 to V4 : making isChildOfEmitter a dynamic param, that defaults to true
            if (profile.buildNumber == 3)
            {
                profile.buildNumber++;
                profile.rootBullet.isChildOfEmitter = new DynamicBool(true);
            }

            // from V4 to V5 : introducing AssetPostProcessor so the default Profile can be changed
            if (profile.buildNumber == 4)
            {
                profile.buildNumber++;
                profile.hasBeenProcessed = true;
            }

            // from V5 to V6 : adding gradient support
            if (profile.buildNumber == 5)
            {
                profile.buildNumber++;
                for (int i = 0; i < profile.subAssets.Length; i++)
                {
                    if (profile.subAssets[i] is ShotParams) continue;
                    if (profile.subAssets[i] is PatternParams) continue;

                    BulletParams bp = profile.subAssets[i] as BulletParams;
                    
                    bp.evolutionBlendWithBaseColor = new DynamicEnum(0);
                    bp.evolutionBlendWithBaseColor.SetEnumType(typeof(ColorBlend));

                    Gradient defaultGrad = new Gradient();
                    GradientAlphaKey[] gak = new GradientAlphaKey[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) };
                    GradientColorKey[] gck = new GradientColorKey[] { new GradientColorKey(Color.black, 0), new GradientColorKey(Color.black, 1) };
                    defaultGrad.SetKeys(gck, gak);

                    bp.colorEvolution = new DynamicGradient(defaultGrad);
                    if (bp.customParameters != null)
                        if (bp.customParameters.Length > 0)
                            for (int j = 0; j < bp.customParameters.Length; j++)
                            {
                                bp.customParameters[j].colorValue = new DynamicColor(Color.black);
                                bp.customParameters[j].gradientValue = new DynamicGradient(defaultGrad);
                            }

                    EditorUtility.SetDirty(bp);
                }
            }

            // from V6 to V7 : the big Pattern revamp, with 70+ possible instructions
            if (profile.buildNumber == 6)
            {
                profile.buildNumber++;
                for (int i = 0; i < profile.subAssets.Length; i++)
                {
                    if (profile.subAssets[i] is BulletParams) continue;
                    if (profile.subAssets[i] is ShotParams) continue;

                    PatternParams pp = profile.subAssets[i] as PatternParams;

                    if (pp.instructionLists == null) pp.instructionLists = new PatternInstructionList[1];
                    if (pp.instructionLists.Length == 0) pp.instructionLists = new PatternInstructionList[1];
                    for (int j = 0; j < pp.instructionLists.Length; j++)
                    {
                        if (pp.instructionLists[j].instructions == null)
                            pp.instructionLists[j].instructions = new PatternInstruction[0];
                        if (pp.instructionLists[j].instructions.Length == 0) continue;
                        for (int k = 0; k < pp.instructionLists[j].instructions.Length; k++)
                        {
                            #region Reassigning enum value since the update scrambled it

                            pp.instructionLists[j].instructions[k].displayName = "Select an Instruction...";
                            pp.instructionLists[j].instructions[k].canBeDoneOverTime = false;
                            int prevEnumValue = (int)pp.instructionLists[j].instructions[k].instructionType;
                            if (prevEnumValue == 0)
                            {
                                pp.instructionLists[j].instructions[k].instructionType = PatternInstructionType.Wait;
                                pp.instructionLists[j].instructions[k].displayName = "Wait";
                            }
                            else if (prevEnumValue == 1)
                            {
                                pp.instructionLists[j].instructions[k].instructionType = PatternInstructionType.Shoot;
                                pp.instructionLists[j].instructions[k].displayName = "Shoot";
                            }
                            else if (prevEnumValue == 2)
                            {
                                pp.instructionLists[j].instructions[k].instructionType = PatternInstructionType.BeginLoop;
                                pp.instructionLists[j].instructions[k].displayName = "Begin Loop";
                            }
                            else if (prevEnumValue == 3)
                            {
                                pp.instructionLists[j].instructions[k].instructionType = PatternInstructionType.EndLoop;
                                pp.instructionLists[j].instructions[k].displayName = "End Loop";
                            }
                            else if (prevEnumValue == 4)
                            {
                                pp.instructionLists[j].instructions[k].instructionType = PatternInstructionType.TranslateGlobal;
                                pp.instructionLists[j].instructions[k].displayName = "Translate (World)";
                                pp.instructionLists[j].instructions[k].canBeDoneOverTime = true;
                            }
                            else if (prevEnumValue == 5)
                            {
                                pp.instructionLists[j].instructions[k].instructionType = PatternInstructionType.TranslateLocal;
                                pp.instructionLists[j].instructions[k].displayName = "Translate (Local)";
                                pp.instructionLists[j].instructions[k].canBeDoneOverTime = true;
                            }
                            else if (prevEnumValue == 6)
                            {
                                pp.instructionLists[j].instructions[k].instructionType = PatternInstructionType.Rotate;
                                pp.instructionLists[j].instructions[k].displayName = "Rotate";
                                pp.instructionLists[j].instructions[k].canBeDoneOverTime = true;
                            }
                            else if (prevEnumValue == 7)
                            {
                                pp.instructionLists[j].instructions[k].instructionType = PatternInstructionType.PlayAudio;
                                pp.instructionLists[j].instructions[k].displayName = "Play Audio";
                            }
                            else if (prevEnumValue == 8)
                            {
                                // flip no longer exists
                                pp.instructionLists[j].instructions[k].instructionType = PatternInstructionType.Wait;
                            }
                            else if (prevEnumValue == 9)
                            {
                                // SetCurve no longer prompts for all curve+period+wrapmode in the same instruction
                                pp.instructionLists[j].instructions[k].instructionType = PatternInstructionType.Wait;
                            }
                            else if (prevEnumValue == 10)
                            {
                                string curveName = "";
                                if (pp.instructionLists[j].instructions[k].curveAffected == PatternCurveType.Speed) curveName = "Speed";
                                else if (pp.instructionLists[j].instructions[k].curveAffected == PatternCurveType.AngularSpeed) curveName = "Ang. Speed";
                                else if (pp.instructionLists[j].instructions[k].curveAffected == PatternCurveType.Scale) curveName = "Scale";
                                else if (pp.instructionLists[j].instructions[k].curveAffected == PatternCurveType.Homing) curveName = "Homing";
                                else if (pp.instructionLists[j].instructions[k].curveAffected == PatternCurveType.Color) curveName = "Color";
                                else if (pp.instructionLists[j].instructions[k].curveAffected == PatternCurveType.Alpha) curveName = "Alpha";
                                else if (pp.instructionLists[j].instructions[k].curveAffected == PatternCurveType.AnimX) curveName = "Anim. X";
                                else if (pp.instructionLists[j].instructions[k].curveAffected == PatternCurveType.AnimY) curveName = "Anim. Y";
                                else if (pp.instructionLists[j].instructions[k].curveAffected == PatternCurveType.AnimAngle) curveName = "Anim. Angle";
                                else if (pp.instructionLists[j].instructions[k].curveAffected == PatternCurveType.AnimScale) curveName = "Anim. Scale";
                                pp.instructionLists[j].instructions[k].displayName = "Set "+curveName+" Curve Ratio";
                                pp.instructionLists[j].instructions[k].instructionType = PatternInstructionType.SetCurveRatio;
                                pp.instructionLists[j].instructions[k].canBeDoneOverTime = true;
                            }
                            else if (prevEnumValue == 11)
                            {
                                pp.instructionLists[j].instructions[k].displayName = "Reboot Pattern";
                                pp.instructionLists[j].instructions[k].instructionType = PatternInstructionType.RebootPattern;
                            }

                            #endregion
                        
                            #region initializing new parameters

                            pp.instructionLists[j].instructions[k].speedValue = new DynamicFloat(0f);
                            pp.instructionLists[j].instructions[k].scaleValue = new DynamicFloat(1f);
                            pp.instructionLists[j].instructions[k].factor = new DynamicFloat(1f);

                            pp.instructionLists[j].instructions[k].vfxToPlay = new DynamicObjectReference(null);
				            pp.instructionLists[j].instructions[k].vfxToPlay.SetNarrowType(typeof(ParticleSystem));

                            pp.instructionLists[j].instructions[k].preferredTarget = new DynamicEnum(0);
                            pp.instructionLists[j].instructions[k].preferredTarget.SetEnumType(typeof(PreferredTarget));
                            pp.instructionLists[j].instructions[k].turnIntensity = new DynamicFloat(1f);
                            pp.instructionLists[j].instructions[k].turnIntensity.EnableSlider(-1f, 1f);
                            pp.instructionLists[j].instructions[k].collisionTagAction = CollisionTagAction.Add;
                            pp.instructionLists[j].instructions[k].collisionTag = new DynamicString("Player");
                            pp.instructionLists[j].instructions[k].patternTag = new DynamicString("");
                            pp.instructionLists[j].instructions[k].patternControlTarget = PatternControlTarget.ThisPattern;
                            
                            pp.instructionLists[j].instructions[k].newCurveValue = new DynamicAnimationCurve(AnimationCurve.Constant(0,1,1));
                            pp.instructionLists[j].instructions[k].newCurveValue.SetForceZeroToOne(true);
                            pp.instructionLists[j].instructions[k].newPeriodValue = new DynamicFloat(1f);
                            pp.instructionLists[j].instructions[k].newWrapMode = new DynamicEnum(0);
                            pp.instructionLists[j].instructions[k].newWrapMode.SetEnumType(typeof(WrapMode));
                            pp.instructionLists[j].instructions[k].curveRawTime = new DynamicFloat(0f);
				
                            pp.instructionLists[j].instructions[k].instructionTiming = InstructionTiming.Instantly;
                            pp.instructionLists[j].instructions[k].instructionDuration = new DynamicFloat(1f);
                            pp.instructionLists[j].instructions[k].operationCurve = new DynamicAnimationCurve(AnimationCurve.EaseInOut(0, 0, 1, 1));
                            pp.instructionLists[j].instructions[k].operationCurve.SetForceZeroToOne(true);

                            #endregion
                        }
                    }

                    EditorUtility.SetDirty(pp);
                }
            }

            // from V7 to V8 : initializing color, gradient, making alpha a slider
            if (profile.buildNumber == 7)
            {
                profile.buildNumber++;
                for (int i = 0; i < profile.subAssets.Length; i++)
                {
                    if (profile.subAssets[i] is BulletParams) continue;
                    if (profile.subAssets[i] is ShotParams) continue;

                    PatternParams pp = profile.subAssets[i] as PatternParams;

                    if (pp.instructionLists == null) pp.instructionLists = new PatternInstructionList[1];
                    if (pp.instructionLists.Length == 0) pp.instructionLists = new PatternInstructionList[1];
                    for (int j = 0; j < pp.instructionLists.Length; j++)
                    {
                        if (pp.instructionLists[j].instructions == null)
                            pp.instructionLists[j].instructions = new PatternInstruction[0];
                        if (pp.instructionLists[j].instructions.Length == 0) continue;
                        for (int k = 0; k < pp.instructionLists[j].instructions.Length; k++)
                        {
                            pp.instructionLists[j].instructions[k].color = new DynamicColor(Color.black);
                            pp.instructionLists[j].instructions[k].alpha = new DynamicSlider01(1f);
                            pp.instructionLists[j].instructions[k].gradient = new DynamicGradient(BulletProExtensions.DefaultGradient());
                        }
                    }

                    EditorUtility.SetDirty(pp);
                }
            }

            // from V8 to V9 : having "plays at bullet birth" default to true
            if (profile.buildNumber == 8)
            {
                profile.buildNumber++;
                for (int i = 0; i < profile.subAssets.Length; i++)
                {
                    if (profile.subAssets[i] is BulletParams) continue;
                    if (profile.subAssets[i] is ShotParams) continue;

                    PatternParams pp = profile.subAssets[i] as PatternParams;

                    pp.playAtBulletBirth = true;

                    EditorUtility.SetDirty(pp);
                }
            }

            // from V9 to V10 : DynamicParameters can now be "Equal to a parameter" instead of Fixed/FromTo/Blend.
            if (profile.buildNumber == 9)
            {
                profile.buildNumber++;
                // nothing to do here, just incrementing build number to avoid seeing enumValue==4 in old versions
                // for (int i = 0; i < profile.subAssets.Length; i++) { }
            }

            // from V10 to V11
            if (profile.buildNumber == 10)
            {
                profile.buildNumber++;

                // Loops twice so patterns get their new DynamicParams, but Bullet update properly overrides it
                for (int i = 0; i < profile.subAssets.Length; i++)
                {
                    if (!(profile.subAssets[i] is PatternParams)) continue;
                    PatternParams pp = profile.subAssets[i] as PatternParams;
                    if (pp.instructionLists == null) continue;
                    if (pp.instructionLists.Length == 0) continue;
                    for (int j = 0; j < pp.instructionLists.Length; j++)
                    {
                        if (pp.instructionLists[j].instructions == null)
                            pp.instructionLists[j].instructions = new PatternInstruction[0];
                        if (pp.instructionLists[j].instructions.Length == 0) continue;
                        for (int k = 0; k < pp.instructionLists[j].instructions.Length; k++)
                        {
                            pp.instructionLists[j].instructions[k].vfxIndex = new DynamicInt(0);
                            pp.instructionLists[j].instructions[k].vfxTag = new DynamicString("");
                            pp.instructionLists[j].instructions[k].vfxFilterType = VFXFilterType.Index;

                            // make room for the newly-created StopVFX instruction
                            int instIndex = (int)pp.instructionLists[j].instructions[k].instructionType;
                            if (instIndex >= (int)PatternInstructionType.StopVFX)
                            {
                                instIndex++;
                                pp.instructionLists[j].instructions[k].instructionType = (PatternInstructionType)instIndex;
                            }
                        }
                    }

                    EditorUtility.SetDirty(pp);
                }

                for (int i = 0; i < profile.subAssets.Length; i++)
                {
                    if (profile.subAssets[i] is BulletParams)
                        UpdateBulletParamsForVFX(profile.subAssets[i] as BulletParams);
                }
            }

            // from V11 to V12
            if (profile.buildNumber == 11)
            {
                profile.buildNumber++;
                for (int i = 0; i < profile.subAssets.Length; i++)
                {
                    if (!(profile.subAssets[i] is PatternParams)) continue;
                    PatternParams pp = profile.subAssets[i] as PatternParams;
                    if (pp.instructionLists == null) continue;
                    if (pp.instructionLists.Length == 0) continue;
                    for (int j = 0; j < pp.instructionLists.Length; j++)
                    {
                        if (pp.instructionLists[j].instructions == null)
                            pp.instructionLists[j].instructions = new PatternInstruction[0];
                        if (pp.instructionLists[j].instructions.Length == 0) continue;
                        for (int k = 0; k < pp.instructionLists[j].instructions.Length; k++)
                        {
                            pp.instructionLists[j].instructions[k].customParamName = "_PowerLevel";
                            pp.instructionLists[j].instructions[k].customInt = new DynamicInt(0);
                            pp.instructionLists[j].instructions[k].customFloat = new DynamicFloat(0f);
                            pp.instructionLists[j].instructions[k].customSlider01 = new DynamicSlider01(0f);
                            pp.instructionLists[j].instructions[k].customVector2 = new DynamicVector2(Vector2.zero);
                            pp.instructionLists[j].instructions[k].customVector3 = new DynamicVector3(Vector3.zero);
                            pp.instructionLists[j].instructions[k].customVector4 = new DynamicVector4(Vector4.zero);
                            pp.instructionLists[j].instructions[k].customColor = new DynamicColor(Color.black);
                            pp.instructionLists[j].instructions[k].customGradient = new DynamicGradient(BulletProExtensions.DefaultGradient());
                            pp.instructionLists[j].instructions[k].customBool = new DynamicBool(false);
                            pp.instructionLists[j].instructions[k].customString = new DynamicString("");
                            pp.instructionLists[j].instructions[k].customAnimationCurve = new DynamicAnimationCurve(AnimationCurve.EaseInOut(0,0,1,1));
                            pp.instructionLists[j].instructions[k].customAnimationCurve.SetForceZeroToOne(false);
                            pp.instructionLists[j].instructions[k].customObjectReference = new DynamicObjectReference(null);
                            pp.instructionLists[j].instructions[k].customRect = new DynamicRect(Rect.zero);
                        }
                    }

                    EditorUtility.SetDirty(pp);
                }
            }

            // from V12 to V13
            /* *
            if (profile.buildNumber == 12)
            {
                profile.buildNumber++;
                for (int i = 0; i < profile.subAssets.Length; i++)
                {
                    
                }
            }
            /* */
            // to be continued (when V14 comes out)

            // in the end, validate everything
            EditorUtility.SetDirty(profile);

            // note to self: when updating BPro, always remember incrementing:
            // - default build number of EmitterProfiles
            // - build number of BulletProSettings
            // - build number of Scene Setup (if it needs to change)
            // - create an update function (here) from N to N+1
        }

        // From V10 to V11 : converts BulletParams so they have VFX Module
        static void UpdateBulletParamsForVFX(BulletParams bp)
        {
            // merge birth/death VFX in the same object, unless they use a different particle system
            bool atBirth = DynamicParameterUtility.GetBool(bp.playVFXOnBirth, true, true);
            bool atDeath = DynamicParameterUtility.GetBool(bp.playVFXOnDeath, true, true);
            bool atLeastOne = atBirth || atDeath;
            bool playsBoth = atBirth && atDeath;
            
            // should we differenciate birth and death effects?
            bool diff = false;
            if (atLeastOne)
            {
                if (playsBoth)
                {
                    bool cBirth = bp.useCustomBirthVFX;
                    bool cDeath = bp.useCustomDeathVFX;

                    if (cBirth || cDeath)
                    {
                        bool hasUniqueBirthFX = false;
                        bool hasUniqueDeathFX = false;

                        if (cBirth)
                        {
                            if (bp.customBirthVFX.root.settings.valueType != DynamicParameterSorting.Fixed)
                                hasUniqueBirthFX = true;
                            else hasUniqueBirthFX = (bp.customBirthVFX.root.defaultValue != null);
                        }
                        if (cDeath)
                        {
                            if (bp.customDeathVFX.root.settings.valueType != DynamicParameterSorting.Fixed)
                                hasUniqueDeathFX = true;
                            else hasUniqueDeathFX = (bp.customDeathVFX.root.defaultValue != null);
                        }

                        diff = (hasUniqueBirthFX || hasUniqueDeathFX);
                    }
                    // else diff stays false
                }
                else if (atBirth)
                {
                    if (bp.useCustomBirthVFX)
                    {
                        if (bp.customBirthVFX.root.settings.valueType != DynamicParameterSorting.Fixed)
                            diff = true;
                        else diff = (bp.customBirthVFX.root.defaultValue != null);
                    }
                    // else diff stays false
                }
                else // if atDeath
                {
                    if (bp.useCustomDeathVFX)
                    {
                        if (bp.customDeathVFX.root.settings.valueType != DynamicParameterSorting.Fixed)
                            diff = true;
                        else diff = (bp.customDeathVFX.root.defaultValue != null);
                    }
                    // else diff stays false
                }
            }
            // else diff stays false (no birth, no death)

            List<DynamicBulletVFXParams> vfxList = new List<DynamicBulletVFXParams>();

            if (!diff)
            {
                DynamicBulletVFXParams vfx = new DynamicBulletVFXParams();
                vfx.tag = "VFX #0";
                vfx.useDefaultParticles = !bp.useCustomBirthVFX;
                vfx.particleSystemPrefab = bp.customBirthVFX;
                vfx.particleSystemPrefab.SetNarrowType(typeof(ParticleSystem));
                vfx.onBulletBirth.enabled = DynamicParameterUtility.GetBool(bp.playVFXOnBirth, false, true);
                vfx.onBulletDeath.enabled = DynamicParameterUtility.GetBool(bp.playVFXOnDeath, false, true);
                vfx.replaceColorWithBulletColor = !bp.useCustomBirthVFX;
                vfx.replaceSizeWithNumber = !bp.useCustomBirthVFX;
                vfx.sizeNewValue = bp.useCustomBirthVFX ? new DynamicFloat(1.0f) : bp.vfxParticleSize;
                vfx.multiplySizeWithNumber = bp.useCustomBirthVFX;
                vfx.sizeMultiplier = bp.useCustomBirthVFX ? bp.vfxParticleSize : new DynamicFloat(1.0f);
                vfx.multiplySizeWithBulletScale = true;
                vfx.multiplySpeedWithBulletScale = false;
                vfx.vfxOverrides = new DynamicBulletVFXOverride[0];
                vfxList.Add(vfx);
            }
            else
            {
                if (atBirth)
                {
                    DynamicBulletVFXParams vfx = new DynamicBulletVFXParams();
                    vfx.tag = "BirthVFX";
                    vfx.useDefaultParticles = !bp.useCustomBirthVFX;
                    vfx.particleSystemPrefab = bp.customBirthVFX;
                    vfx.particleSystemPrefab.SetNarrowType(typeof(ParticleSystem));
                    vfx.onBulletBirth.enabled = DynamicParameterUtility.GetBool(bp.playVFXOnBirth, false, true);
                    vfx.onBulletDeath.enabled = false;
                    vfx.replaceColorWithBulletColor = !bp.useCustomBirthVFX;
                    vfx.replaceSizeWithNumber = !bp.useCustomBirthVFX;
                    vfx.sizeNewValue = bp.useCustomBirthVFX ? new DynamicFloat(1.0f) : bp.vfxParticleSize;
                    vfx.multiplySizeWithNumber = bp.useCustomBirthVFX;
                    vfx.sizeMultiplier = bp.useCustomBirthVFX ? bp.vfxParticleSize : new DynamicFloat(1.0f);
                    vfx.multiplySizeWithBulletScale = true;
                    vfx.multiplySpeedWithBulletScale = false;
                    vfx.vfxOverrides = new DynamicBulletVFXOverride[0];
                    vfxList.Add(vfx);
                }
                
                if (atDeath)
                {
                    DynamicBulletVFXParams vfx = new DynamicBulletVFXParams();
                    vfx.tag = "DeathVFX";
                    vfx.useDefaultParticles = !bp.useCustomDeathVFX;
                    vfx.particleSystemPrefab = bp.customDeathVFX;
                    vfx.particleSystemPrefab.SetNarrowType(typeof(ParticleSystem));
                    vfx.onBulletBirth.enabled = false;
                    vfx.onBulletDeath.enabled = DynamicParameterUtility.GetBool(bp.playVFXOnDeath, false, true);
                    vfx.replaceColorWithBulletColor = !bp.useCustomDeathVFX;
                    vfx.replaceSizeWithNumber = !bp.useCustomDeathVFX;
                    vfx.sizeNewValue = bp.useCustomDeathVFX ? new DynamicFloat(1.0f) : bp.vfxParticleSize;
                    vfx.multiplySizeWithNumber = bp.useCustomDeathVFX;
                    vfx.sizeMultiplier = bp.useCustomDeathVFX ? bp.vfxParticleSize : new DynamicFloat(1.0f);
                    vfx.multiplySizeWithBulletScale = true;
                    vfx.multiplySpeedWithBulletScale = false;
                    vfx.vfxOverrides = new DynamicBulletVFXOverride[0];
                    vfxList.Add(vfx);
                }
            }

            // Finally, if a PatternInstruction says to play a VFX, register it here
            if (bp.children != null)
            {
                if (bp.children.Length > 0)
                {
                    for (int i = 0; i < bp.children.Length; i++)
                    {
                        PatternParams pp = bp.children[i] as PatternParams;
                        if (pp == null) continue;
                        if (pp.instructionLists == null) continue;
                        if (pp.instructionLists.Length == 0) continue;
                        for (int j = 0; j < pp.instructionLists.Length; j++)
                        {
                            PatternInstructionList pil = pp.instructionLists[j];
                            if (pil.instructions == null) continue;
                            if (pil.instructions.Length == 0) continue;
                            for (int k = 0; k < pil.instructions.Length; k++)
                            {
                                pil.instructions[k].vfxIndex = new DynamicInt(0);
                                pil.instructions[k].vfxTag = new DynamicString("");
                                if (pil.instructions[k].instructionType != PatternInstructionType.PlayVFX) continue;

                                // case 1 : instruction uses default particles
                                if (pil.instructions[k].vfxPlayType == VFXPlayType.Default)
                                {
                                    bool shouldCreate = true;
                                    if (vfxList.Count > 0)
                                    {
                                        for (int m = 0; m < vfxList.Count; m++)
                                        {
                                            if (vfxList[m].useDefaultParticles)
                                            {
                                                shouldCreate = false;
                                                pil.instructions[k].vfxIndex = new DynamicInt(m);
                                                break;
                                            }
                                        }
                                    }

                                    if (shouldCreate)
                                    {
                                        DynamicBulletVFXParams vfx = new DynamicBulletVFXParams();
                                        vfx.tag = "Pattern VFX "+vfxList.Count.ToString();
                                        vfx.useDefaultParticles = true;
                                        vfx.particleSystemPrefab = new DynamicObjectReference(null);
                                        vfx.particleSystemPrefab.SetNarrowType(typeof(ParticleSystem));
                                        vfx.onBulletBirth.enabled = false;
                                        vfx.onBulletDeath.enabled = false;
                                        vfx.replaceColorWithBulletColor = true;
                                        vfx.replaceSizeWithNumber = true;
                                        vfx.sizeNewValue = bp.vfxParticleSize;
                                        vfx.multiplySizeWithNumber = false;
                                        vfx.sizeMultiplier = new DynamicFloat(1.0f);
                                        vfx.multiplySizeWithBulletScale = true;
                                        vfx.multiplySpeedWithBulletScale = false;
                                        vfx.vfxOverrides = new DynamicBulletVFXOverride[0];
                                        pil.instructions[k].vfxIndex = new DynamicInt(vfxList.Count);
                                        vfxList.Add(vfx);
                                    }
                                }
                                // case 2 : instruction uses custom VFX
                                else
                                {
                                    // In this rare case, we just consider the VFX should always be created. 
                                    DynamicBulletVFXParams vfx = new DynamicBulletVFXParams();
                                    vfx.tag = "Pattern VFX "+vfxList.Count.ToString();
                                    vfx.useDefaultParticles = false;
                                    vfx.particleSystemPrefab = pil.instructions[k].vfxToPlay;
                                    vfx.onBulletBirth.enabled = false;
                                    vfx.onBulletDeath.enabled = false;
                                    vfx.replaceColorWithBulletColor = false;
                                    vfx.replaceSizeWithNumber = false;
                                    vfx.sizeNewValue = new DynamicFloat(1.0f);
                                    vfx.multiplySizeWithNumber = true;
                                    vfx.sizeMultiplier = bp.vfxParticleSize;
                                    vfx.multiplySizeWithBulletScale = true;
                                    vfx.multiplySpeedWithBulletScale = false;
                                    vfx.vfxOverrides = new DynamicBulletVFXOverride[0];
                                    pil.instructions[k].vfxIndex = new DynamicInt(vfxList.Count);
                                    vfxList.Add(vfx);
                                }
                            }
                        }

                        EditorUtility.SetDirty(pp);
                    }
                }
            }

            vfxList.TrimExcess();
            bp.vfxParams = vfxList.ToArray();

            bp.vfxOpened = 0;
            bp.maxTravellableDistance = new DynamicFloat(5.0f);
            bp.spawnOnTarget = new DynamicFloat(0.0f);
            bp.maxSimultaneousCollisionsPerFrame = 1;
            bp.deathTiming = BulletDeathTiming.Immediately;
            EditorUtility.SetDirty(bp);
        }

        #endregion
    }
}