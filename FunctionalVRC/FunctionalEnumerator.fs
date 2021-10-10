module FunctionalEnumerator

open MelonLoader

type FunctionalEnumerator(Lambdas: list<unit -> obj>) =
    let mutable position: int = 0
    let mutable result: obj = null

    member _.Position 
        with get() = position
        and set(value) = position <- value

    member _.Result
        with get() = result
        and set(value) = result <- value

    member _.Finished
        with get() = result <> null

    member private _.Token = null

    member this.Start() = this.Token = MelonCoroutines.Start this |> ignore
    member this.Stop() = MelonCoroutines.Stop(this.Token)

    interface System.Collections.IEnumerator with
        member this.Current
            with get() = this.Result

        member this.MoveNext() = 
            match this.Position < Lambdas.Length with
             | true -> 
                match Lambdas.[this.Position]() with
                 | :? bool as repeat ->
                    if not repeat then
                       this.Position <- this.Position + 1
                    repeat
                 | other -> 
                    this.Position <- this.Position + 1
                    other = null
             | false -> false

        member this.Reset() = this.Position <- 0
