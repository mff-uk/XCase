using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using XCase.Model;

namespace XCase.Evolution
{
    public static class ChangesLookupManager
    {
        private static Dictionary<EChangeScope, List<Type>> changesByScope;
        private static Dictionary<EEditType, List<Type>> changesByEditType;
        private static List<Type> changesNotRequiringRevalidation;

        private static void CathegorizeChanges(out Dictionary<EChangeScope, List<Type>> changesByScope, out Dictionary<EEditType, List<Type>> changesByEditType)
        {
            changesByScope = new Dictionary<EChangeScope, List<Type>>();
            changesByEditType = new Dictionary<EEditType, List<Type>>();
            changesNotRequiringRevalidation = new List<Type>();
            
            detectMethods = new Dictionary<Type, MethodInfo>();
            foreach (Type type in Assembly.GetAssembly(typeof(EvolutionChange)).GetTypes())
            {
                if (type.IsSubclassOf(typeof(EvolutionChange)) && !type.IsAbstract)
                {
                    object[] customAttributes = type.GetCustomAttributes(typeof(ChangePropertiesAttribute), true);
                    foreach (object customAttribute in customAttributes)
                    {
                        ChangePropertiesAttribute csp = (ChangePropertiesAttribute)customAttribute;

                        int flagMask = 1 << 30; // start with high-order bit...
                        while (flagMask != 0)   // loop terminates once all flags have been compared
                        {
                            // switch on only a single bit...
                            if ((csp.Scope & (EChangeScope)flagMask) == (EChangeScope)flagMask)
                            {
                                if (!changesByScope.ContainsKey((EChangeScope)flagMask))
                                    changesByScope[(EChangeScope)flagMask] = new List<Type>();
                                if (!changesByScope[(EChangeScope)flagMask].Contains(type))
                                    changesByScope[(EChangeScope)flagMask].Add(type);
                            }

                            flagMask >>= 1;  // bit-shift the flag value one bit to the right
                        }

                        if (!changesByEditType.ContainsKey(csp.EditType))
                            changesByEditType[csp.EditType] = new List<Type>();
                        if (!changesByEditType[csp.EditType].Contains(type))
                            changesByEditType[csp.EditType].Add(type);

                        detectMethods[type] = type.GetMethod("Detect");

                        if (!csp.MayRequireRevalidation)
                        {
                            changesNotRequiringRevalidation.Add(type);
                        }
                    }
                }
            }
            
        }

        private static Dictionary<Type, MethodInfo> detectMethods;

        public static void DetectLocalChanges(ChangesDetectorContext context)
        {
            if (changesByScope == null)
                CathegorizeChanges(out changesByScope, out changesByEditType);

            foreach (Type changeType in changesByScope[context.Scope])
            {
                bool isChangeSedentery = changesByEditType[EEditType.Sedentary].Contains(changeType);
                bool isChangeMigratory = changesByEditType[EEditType.Migratory].Contains(changeType);
                bool isChangeAddition = changesByEditType[EEditType.Addition].Contains(changeType);
                bool isChangeRemoval = changesByEditType[EEditType.Removal].Contains(changeType);

                int flagMask = 1 << 30; // start with high-order bit...
                while (flagMask != 0)   // loop terminates once all flags have been compared
                {
                    IVersionedElement contextElement = null; 

                    // switch on only a single bit...
                    switch (context.Scope & (EChangeScope)flagMask)
                    {
                        case EChangeScope.Class:
                            contextElement = context.CurrentClass;
                            break;
                        case EChangeScope.ClassUnion:
                            contextElement = context.CurrentClassUnion;
                            break;
                        case EChangeScope.Association:
                            contextElement = context.CurrentAssociation; 
                            break;
                        case EChangeScope.ContentChoice:
                            contextElement = context.CurrentContentChoice;
                            break;
                        case EChangeScope.ContentContainer:
                            contextElement = context.CurrentContentContainer;
                            break;
                        case EChangeScope.AttributeContainer:
                            contextElement = context.CurrentAttributeContainer;
                            break;
                        case EChangeScope.Attribute:
                            contextElement = context.CurrentAttribute;
                            break;
                        case EChangeScope.Diagram:
                            contextElement = context.Diagram;
                            break;
                        default:
                            break;
                    }

                    if (contextElement == null)
                    {
                        flagMask >>= 1;
                        continue;
                    }
                    
                    IVersionedElement contextElementOldVersion = contextElement.GetInVersion(context.OldVersion);

                    if ((isChangeRemoval || isChangeMigratory || isChangeSedentery) && contextElementOldVersion == null)
                    {
                        flagMask >>= 1;
                        continue;
                    }

                    List<object> args = new List<object>();
                    args.Clear();
                    args.Add(context.OldVersion);
                    args.Add(context.NewVersion);
                    args.Add(contextElement);

                    IList<EvolutionChange> localChanges = (IList<EvolutionChange>)detectMethods[changeType].Invoke(null, args.ToArray());
                    Debug.Assert(localChanges.All(change => change.GetType() == changeType));
                    Debug.Assert(localChanges.All(change => { change.Verify(); return true; }));
                    context.DetectedChanges.AddRange(localChanges);
                    flagMask >>= 1;  // bit-shift the flag value one bit to the right
                }
            }

        }

        public static bool MayRequireRevalidation(EvolutionChange change)
        {
            if (changesNotRequiringRevalidation == null)
            {
                CathegorizeChanges(out changesByScope, out changesByEditType);
                Debug.Assert(changesNotRequiringRevalidation != null);
            }

            return !changesNotRequiringRevalidation.Contains(change.GetType());
        }
    }
}