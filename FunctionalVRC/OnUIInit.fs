module OnUIInit

open UnityEngine

let _execute = fun () -> 
    let ui = GameObject.Find("UserInterface").transform;
    let qm = ui.Find("Canvas_QuickMenu(Clone)");

    qm.GetComponent<Canvas>().referencePixelsPerUnit <- 0f

    let launch_pad = qm.Find("Container/Window/QMParent/Menu_Dashboard/ScrollRect/Viewport/VerticalLayoutGroup");
    launch_pad.Find("VRC+_Banners").gameObject.active <- false;
    launch_pad.Find("Carousel_Banners").gameObject.active <- false;
let execute = _execute
