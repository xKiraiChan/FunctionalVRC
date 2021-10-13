module FunctionalVRC.FunctionalLoader

open MelonLoader
open System.IO
open System.Net.Http
open System.Reflection

let SHA256 = fun (bytes: byte[]) ->
    let mutable a = ""
    bytes 
    |> (new System.Security.Cryptography.SHA256Managed()).ComputeHash 
    |> Seq.map (fun bite -> bite.ToString("X2")) 
    |> Seq.toArray
    |> Seq.iter (fun char -> a <- a + char)
    a

let http = new HttpClient()

type FunctionalVRC() = 
    inherit MelonPlugin()

    static do 
        MelonLogger.Msg "------------------------------"

        let mutable bytes, oldHash, newHash = (null, null, null)
        let local = System.Environment.CommandLine.ToLower().Contains("--local-fvrc-lib")
        
        if File.Exists "Dependencies/FunctionalLib.dll" then
            bytes <- File.ReadAllBytes "Dependencies/FunctionalLib.dll"
            oldHash <- SHA256(bytes)
            if not local then
                newHash <- Async.RunSynchronously (Async.AwaitTask (http.GetStringAsync("https://github.com/xKiraiChan/FunctionalVRC/blob/master/FunctionalLib/Dist/FunctionalLib.dll.hash?raw=true")))

        if (isNull bytes || not (oldHash.Equals newHash)) && not local then
            bytes <- Async.RunSynchronously (Async.AwaitTask (http.GetByteArrayAsync("https://github.com/xKiraiChan/FunctionalVRC/blob/master/FunctionalLib/Dist/FunctionalLib.dll?raw=true")))
            newHash <- SHA256(bytes)

        match isNull oldHash with
         | true -> 
            MelonLogger.Msg "Downloaded FunctionalLib"
            MelonLogger.Msg ("Hash: " + newHash)
         | false ->
            match oldHash.Equals newHash || isNull newHash with
             | true -> 
                MelonLogger.Msg "Loaded FunctionalLib"
                MelonLogger.Msg ("Hash: " + oldHash)
             | false -> 
                MelonLogger.Msg "Updated FunctionalLib"
                MelonLogger.Msg ("Old Hash: " + oldHash)
                MelonLogger.Msg ("New Hash: " + newHash)

        MelonLogger.Msg "------------------------------"

        try
            Assembly.Load(bytes) |> ignore
        with ex -> 
            MelonLogger.Error ("Failed to load FunctionalLib: " + ex.ToString())
        ()


[<assembly: MelonInfo(typeof<FunctionalVRC>, "FunctionalLoader", "0.1.1", "Kirai Chan", "github.com/xKiraiChan/FunctionalVRC")>] 
[<assembly: MelonGame("VRChat", "VRChat")>] 
[<assembly: MelonColor(System.ConsoleColor.DarkMagenta)>]
do ()
    