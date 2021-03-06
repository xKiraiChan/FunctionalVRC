module Modules.UI

open UnityEngine

let _execute = fun (ui: Transform) -> 
    let qm = ui.Find("Canvas_QuickMenu(Clone)");
    let mc = ui.Find("MenuContent");

    qm.GetComponent<Canvas>().referencePixelsPerUnit <- 0f
    mc.GetComponent<Canvas>().referencePixelsPerUnit <- 0f

    let launch_pad = qm.Find("Container/Window/QMParent/Menu_Dashboard/ScrollRect/Viewport/VerticalLayoutGroup");
    launch_pad.Find("VRC+_Banners").gameObject.active <- false;
    launch_pad.Find("Carousel_Banners").gameObject.active <- false;
let execute = _execute

let Init = fun () -> 
    Events.OnUIInit.Add(fun ui -> execute(ui.transform))
