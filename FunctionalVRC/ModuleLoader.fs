module ModuleLoader

open MelonLoader
open System
open System.Reflection

let mutable Modules: Type list = []

let Load = fun () ->
    Modules <- Assembly.GetExecutingAssembly().GetTypes()
    |> Seq.toList
    |> List.filter (fun t -> not (isNull t.Namespace))
    |> List.filter (fun t -> t.Namespace.StartsWith "Modules")
    |> List.filter (fun t -> t.IsAbstract && t.IsSealed)

    Modules |> List.iter (fun t -> 
        MelonLogger.Msg ("Loaded " + t.ToString())
    )

let Init = fun () ->
    Modules |> List.iter (fun t -> 
        t.GetMethod("Init", BindingFlags.Public ||| BindingFlags.Static).Invoke(null, null) |> ignore
    )
