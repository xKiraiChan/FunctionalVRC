namespace FunctionalVRC

open MelonLoader
open UnityEngine
open FunctionalEnumerator

type FunctionalVRC() = 
    inherit MelonMod()
    override _this.OnApplicationStart() = 
        FunctionalEnumerator([
            fun () -> 
                MelonLogger.Msg("Checking...");
                match GameObject.Find("UserInterface") with
                 | null -> true :> obj
                 | other -> null
            fun () ->
                MelonLogger.Msg("Found UI Manager")
                GameObject.Find("UserInterface").transform
                    .Find("Canvas_QuickMenu(Clone)")
                    .GetComponent<Canvas>()
                    .referencePixelsPerUnit <- 0f
                null
        ]).Start()

[<assembly: MelonInfo(typeof<FunctionalVRC>, "FunctionalVRC", "0.1.0", "Kirai Chan", "github.com/xKiraiChan/FunctionalVRC")>] 
[<assembly: MelonGame("VRChat", "VRChat")>] 
do ()
    