namespace TestAppModels

type ValidationResult =
    | Success
    | Error of description: string

type AudioSettings = { Volume: float; Pan: float } with
    member x.Validate() =
        match x.Volume with
        | v when v < 0.0 -> Error("Negative Volume is not supported")
        | _ -> // We'll constrain to 0.0 - 1.0
            match x.Pan with
            | v when v < -1.0 || v > 1.0 -> Error("Invalid Pan newValue")
            | _ -> Success

type AppliedSettingsResult =
    | AudioSettingsAsIs of settings: AudioSettings
    | AudioSettingsConstrained of settings: AudioSettings
    | Error of error: string

type Model(volume: float, pan: float) as x =
    inherit MvvmFSharpLib.ObservableBase()

    let currentSettings = x.MakeField(<@ x.CurrentSettings @>, { Volume = volume; Pan = pan })
    let requestedSettings = x.MakeField(<@ x.LastRequestedSettings @>, { Volume = volume; Pan = pan })
    
    member x.CurrentSettings
        with get() = currentSettings.Value
        and set newValue = currentSettings.Value <- newValue
    member x.LastRequestedSettings
        with get() = requestedSettings.Value
        and set newValue = requestedSettings.Value <- newValue

    member x.ApplySettings (settings: AudioSettings) =
        match settings.Validate() with
        | ValidationResult.Error(msg) -> Error(msg)
        | _ ->
            let constrained = Model.constrain settings
            match constrained with
            | AudioSettingsAsIs(s) | AudioSettingsConstrained(s) ->
                let delayedApplicationSimulation =
                    async {
                        // Not real-world code: these updates can be out-of-sequence.
                        do! Async.Sleep(1000)
                        x.CurrentSettings <- s
                    }
                delayedApplicationSimulation |> Async.Start
                x.LastRequestedSettings <- s
            | Error(desc) -> ()
            constrained

    static member constrain settings =
        if settings.Volume > 1.0 then AudioSettingsConstrained({settings with Volume = 1.0})
        else AudioSettingsAsIs(settings)
