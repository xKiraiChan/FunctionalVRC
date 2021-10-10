module OnGUI

open MelonLoader
open UnityEngine

let _execute = fun () -> 
    //if GUILayout.Button("Test1", null) then
    //    MelonLogger.Msg "Clicked"
    GUILayout.Button("Test2", null) |> ignore
let execute = _execute
