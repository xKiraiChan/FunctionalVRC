module Modules.Flight

open UnityEngine
open UnityEngine.XR
open VRC.Animation

let mutable private state = false
let mutable private stored = false
let mutable private oGravity: Option<Vector3> = None
let mutable private local: Transform = null

let mutable public speed = 8f

let Execute = fun (s: bool) (player: Transform) ->
    state <- s
    local <- player
    match s with
     | true ->
        stored <- 
            match stored with
             | true -> 
                stored
             | false -> 
                oGravity <- Some(Physics.gravity)
                true
        Physics.gravity <- Vector3(0f, 0f, 0f)
     | false ->
        stored <- false
        if oGravity.IsSome then
            Physics.gravity <- oGravity.Value

    try 
        player.GetComponent<VRCMotionState>().Method_Private_Void_0()
    with _ -> ()

let OnUpdate = fun () ->
    if Input.GetKeyDown KeyCode.F then
        Execute (not state) local
    elif state && (not (local.Equals null)) then
        let w,x,y,z = 
            match XRDevice.isPresent with
             | true -> (0f,
                        Input.GetAxis("Horizontal"),
                        Input.GetAxis("Oculus_CrossPlatform_SecondaryThumbstickVertical"),
                        Input.GetAxis("Vertical"))
             | false -> 
                let get = fun (a: KeyCode) (b: KeyCode) ->
                    match Input.GetKey a with
                     | true -> 1f
                     | false -> 0f
                    +
                    match Input.GetKey b with
                     | true -> -1f
                     | false -> 0f
                let w = 
                    match Input.GetKey KeyCode.LeftShift with
                     | true -> 8f
                     | false -> 1f
                (w,
                 get KeyCode.D KeyCode.A,
                 get KeyCode.E KeyCode.Q,
                 get KeyCode.W KeyCode.S)
        local.position <- local.position
            + local.transform.right   * speed * Time.deltaTime * x * w
            + local.transform.forward * speed * Time.deltaTime * z * w
            + local.transform.up      * speed * Time.deltaTime * y * w
        if Physics.gravity.y <> 0f then
            oGravity <- Some(Physics.gravity)
            Physics.gravity <- Vector3(0f, 0f, 0f)
        VRC.SDKBase.Networking.LocalPlayer.SetVelocity(new Vector3(0f, 0f, 0f));

let Init = fun () ->
    Events.OnLocalLoad.Add(fun player -> player.transform |> Execute state)
    Events.OnUpdate.Add(OnUpdate)
