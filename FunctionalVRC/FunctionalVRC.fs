namespace FunctionalVRC

open MelonLoader
open UnityEngine
open FunctionalEnumerator

type FunctionalVRC() = 
    inherit MelonMod()
    override _.OnApplicationStart() = 
        FunctionalEnumerator([
            fun (_) -> 
                let ui = GameObject.Find("UserInterface")
                match ui with
                 | null -> (null, box true)
                 | _ -> (box ui, null)
            fun (store) ->
                MelonLogger.Msg "Found UI Manager"
                OnUIInit.execute (store :?> GameObject).transform
                (null, box false)
        ]).Start()
    override _.OnSceneWasLoaded(buildIndex, sceneName) =
        if buildIndex = -1 then
            FunctionalEnumerator([
                fun (store) ->
                    let scene = 
                        match store with
                         | null -> SceneManagement.SceneManager.GetSceneByName sceneName
                         | other -> other :?> SceneManagement.Scene

                    let player = 
                        scene.GetRootGameObjects() 
                         |> Utils.NormalizeIl2CppReferenceArray 
                         |> List.tryFind (fun go -> go.name.StartsWith("VRCPlayer[Local]"))

                    match player with
                     | None -> (null, box true)
                     | Some(other) -> (box other, null)
                fun (store) -> 
                    let player = store :?> UnityEngine.GameObject
                    MelonLogger.Msg "Found Local Player"
                    (null, box false)
            ]).Start()

[<assembly: MelonInfo(typeof<FunctionalVRC>, "FunctionalVRC", "0.1.0", "Kirai Chan", "github.com/xKiraiChan/FunctionalVRC")>] 
[<assembly: MelonGame("VRChat", "VRChat")>] 
[<assembly: MelonColor(System.ConsoleColor.DarkMagenta)>]
do ()
    