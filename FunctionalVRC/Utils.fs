module Utils

let NormalizeIl2CppReferenceArray = fun (refArray: UnhollowerBaseLib.Il2CppReferenceArray<'a>) -> 
    let mutable arr: 'a list = []
    for i in [0..refArray.Length - 1] do
        arr <- List.append arr [refArray.[i]]
    arr
