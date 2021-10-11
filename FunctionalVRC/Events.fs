module Events

open UnityEngine

let private onUIInit = new Event<GameObject>()
let private onLocalLoad = new Event<GameObject>()
let private onUpdate = new Event<unit>()

[<CLIEvent>]
let OnUIInit = onUIInit.Publish

[<CLIEvent>] 
let OnLocalLoad = onLocalLoad.Publish

[<CLIEvent>] 
let OnUpdate = onUpdate.Publish

let UIInit obj = onUIInit.Trigger obj
let LocalLoad obj = onLocalLoad.Trigger obj
let Update = onUpdate.Trigger
