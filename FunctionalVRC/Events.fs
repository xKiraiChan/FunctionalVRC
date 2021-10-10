module Events

open UnityEngine

let private onUIInit = new Event<GameObject>()

[<CLIEvent>] 
let OnUIInit = onUIInit.Publish

let UIInit obj =
    onUIInit.Trigger obj
