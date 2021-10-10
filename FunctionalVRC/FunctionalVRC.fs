namespace FunctionalVRC

open MelonLoader
open UnityEngine
open FunctionalEnumerator

type FunctionalVRC() = 
    inherit MelonMod()
    override _this.OnApplicationStart() = 
        FunctionalEnumerator([
            fun () -> 
                match GameObject.Find("UserInterface") with
                 | null -> true :> obj
                 | other -> null
            fun () ->
                MelonLogger.Msg("Found UI Manager")
                OnUIInit.execute()
                null
        ]).Start()

[<assembly: MelonInfo(typeof<FunctionalVRC>, "FunctionalVRC", "0.1.0", "Kirai Chan", "github.com/xKiraiChan/FunctionalVRC")>] 
[<assembly: MelonGame("VRChat", "VRChat")>] 
[<assembly: MelonColor(System.ConsoleColor.DarkMagenta)>]
do ()
    