namespace TestAppModels

type ViewModel(model: Model) as x =
    inherit MvvmFSharpLib.ObservableBase()

    let requestedVolume = x.MakeField(<@ x.RequestedVolume @>, model.CurrentSettings.Volume)
    let requestedPan = x.MakeField(<@ x.RequestedPan @>, model.CurrentSettings.Pan)
    let isVolumeConstrained = x.MakeField<bool>(<@ x.IsVolumeConstrained @>)

    let resetPanCommand = x.MakeCommand<float>((fun _ -> requestedPan.Value <> 0.0),
                                               (fun _ -> x.RequestedPan <- 0.0),
                                               requestedPan)

    member x.Model with get() = model

    member x.ResetPanCommand with get() = resetPanCommand.ICommand

    member x.IsVolumeConstrained
        with get() = isVolumeConstrained.Value
        and private set newValue = isVolumeConstrained.Value <- newValue

    member x.RequestedVolume
        with get() = requestedVolume.Value
        and set newValue =
            if requestedVolume.Set newValue then
                match model.ApplySettings {model.LastRequestedSettings with Volume = newValue} with
                | AudioSettingsAsIs(s) -> x.IsVolumeConstrained <- false ; ()
                | AudioSettingsConstrained(s) -> x.IsVolumeConstrained <- true ; ()
                | Error(msg) -> ()

    member x.RequestedPan
        with get() = requestedPan.Value
        and set newValue =
            if requestedPan.Set newValue then
                match model.ApplySettings {model.LastRequestedSettings with Pan = newValue} with
                | AudioSettingsAsIs(s) | AudioSettingsConstrained(s) -> ()
                | Error(msg) -> ()
