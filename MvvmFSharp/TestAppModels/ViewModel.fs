namespace TestAppModels

type ViewModel(model: Model) =
    inherit MvvmFSharpLib.ObservableBase()

    let mutable volume: float = model.CurrentSettings.Volume
    let mutable pan: float = model.CurrentSettings.Pan
    let mutable isVolumeConstrained: bool = false

    member x.Model
        with get() = model

    member x.IsVolumeConstrained
        with get() = isVolumeConstrained
        and private set newValue = x.setProperty(&isVolumeConstrained, newValue, <@ x.IsVolumeConstrained @>) |> ignore

    member x.RequestedVolume
        with get() = volume
        and set newValue =
            if x.setProperty(&volume, newValue, <@ x.RequestedVolume @>) then
                match model.ApplySettings {model.LastRequestedSettings with Volume = newValue} with
                | AudioSettingsAsIs(s) -> x.IsVolumeConstrained <- false ; ()
                | AudioSettingsConstrained(s) -> x.IsVolumeConstrained <- true ; ()
                | Error(msg) -> ()

    member x.RequestedPan
        with get() = pan
        and set newValue =
            if x.setProperty(&pan, newValue, <@ x.RequestedPan @>) then
                match model.ApplySettings {model.LastRequestedSettings with Pan = newValue} with
                | AudioSettingsAsIs(s) | AudioSettingsConstrained(s) -> ()
                | Error(msg) -> ()
