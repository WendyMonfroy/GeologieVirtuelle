//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Valve.VR
{
    using System;
    using UnityEngine;
    
    
    public partial class SteamVR_Actions
    {
        
        private static SteamVR_Input_ActionSet_default p__default;
        
        private static SteamVR_Input_ActionSet_mixedreality p_mixedreality;
        
        private static SteamVR_Input_ActionSet_GeologieVirtuelle p_GeologieVirtuelle;
        
        public static SteamVR_Input_ActionSet_default _default
        {
            get
            {
                return SteamVR_Actions.p__default.GetCopy<SteamVR_Input_ActionSet_default>();
            }
        }
        
        public static SteamVR_Input_ActionSet_mixedreality mixedreality
        {
            get
            {
                return SteamVR_Actions.p_mixedreality.GetCopy<SteamVR_Input_ActionSet_mixedreality>();
            }
        }
        
        public static SteamVR_Input_ActionSet_GeologieVirtuelle GeologieVirtuelle
        {
            get
            {
                return SteamVR_Actions.p_GeologieVirtuelle.GetCopy<SteamVR_Input_ActionSet_GeologieVirtuelle>();
            }
        }
        
        private static void StartPreInitActionSets()
        {
            SteamVR_Actions.p__default = ((SteamVR_Input_ActionSet_default)(SteamVR_ActionSet.Create<SteamVR_Input_ActionSet_default>("/actions/default")));
            SteamVR_Actions.p_mixedreality = ((SteamVR_Input_ActionSet_mixedreality)(SteamVR_ActionSet.Create<SteamVR_Input_ActionSet_mixedreality>("/actions/mixedreality")));
            SteamVR_Actions.p_GeologieVirtuelle = ((SteamVR_Input_ActionSet_GeologieVirtuelle)(SteamVR_ActionSet.Create<SteamVR_Input_ActionSet_GeologieVirtuelle>("/actions/GeologieVirtuelle")));
            Valve.VR.SteamVR_Input.actionSets = new Valve.VR.SteamVR_ActionSet[] {
                    SteamVR_Actions._default,
                    SteamVR_Actions.mixedreality,
                    SteamVR_Actions.GeologieVirtuelle};
        }
    }
}
